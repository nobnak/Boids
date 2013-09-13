using UnityEngine;
using System.Collections;
using Cloo;
using System.Linq;
using System.Runtime.InteropServices;

public class GPGPU_UniformGrid : MonoBehaviour {
	public int nGridPartitions = 64;
	public int maxIndices = 4;
	public Bounds gridbounds = new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 1000f));

	private string clProgramPath = "Assets/UniformGrid.cl";
	private string clUpdateGridKernelName = "updateGrid";
	private string clUpdateBoidsKernelName = "updateBoids";
	private string clBoundaryKernelName = "boundary";
	
	private ComputeContext _context;
	private ComputeCommandQueue _queue;
	private ComputeProgram _program;
	private ComputeKernel _updateGridKernel, _updateBoidsKernel, _boundaryKernel;
	private ComputeEventList _events;
	
	private int[] _pointCounters;
	private int[] _pointIndices;
	private Cloo.ComputeBuffer<int> _pointCountersBuffer;
	private Cloo.ComputeBuffer<int> _pointIndicesBuffer;
	private GridInfo _gridInfo;
	
	// Use this for initialization
	void Awake () {
		var platform = ComputePlatform.Platforms[0];
		_context = new ComputeContext(ComputeDeviceTypes.Cpu,
			new ComputeContextPropertyList(platform), null, System.IntPtr.Zero);
		_queue = new ComputeCommandQueue(_context, _context.Devices[0], ComputeCommandQueueFlags.None);
		string clSource = System.IO.File.ReadAllText(clProgramPath);
		_program = new ComputeProgram(_context, clSource);
		try {
			_program.Build(null, null, null, System.IntPtr.Zero);
		} catch(BuildProgramFailureComputeException) {
			Debug.Log(_program.GetBuildLog(_context.Devices[0]));
			throw;
		}
		_events = new ComputeEventList();
		_updateGridKernel = _program.CreateKernel(clUpdateGridKernelName);
		_updateBoidsKernel = _program.CreateKernel(clUpdateBoidsKernelName);
		_boundaryKernel = _program.CreateKernel(clBoundaryKernelName);

		_pointCounters = new int[nGridPartitions * nGridPartitions * nGridPartitions];
		_pointIndices = new int[_pointCounters.Length * maxIndices];

		_pointCountersBuffer = new Cloo.ComputeBuffer<int>(
			_context, ComputeMemoryFlags.WriteOnly, _pointCounters.Length);
		_pointIndicesBuffer = new Cloo.ComputeBuffer<int>(
			_context, ComputeMemoryFlags.WriteOnly, _pointIndices.Length);
		
		_gridInfo = new GridInfo() {
			worldOrigin = gridbounds.min,
			worldSize = gridbounds.size,
			cellSize = gridbounds.size * (1f / nGridPartitions),
			nGridPartitions = nGridPartitions,
			maxIndices = maxIndices
		};
		
		_boundaryKernel.SetValueArgument(1, _gridInfo);

		_updateGridKernel.SetMemoryArgument(1, _pointCountersBuffer);
		_updateGridKernel.SetMemoryArgument(2, _pointIndicesBuffer);
		_updateGridKernel.SetValueArgument(3, _gridInfo);
		
		_updateBoidsKernel.SetMemoryArgument(2, _pointCountersBuffer);
		_updateBoidsKernel.SetMemoryArgument(3, _pointIndicesBuffer);
		_updateBoidsKernel.SetValueArgument(4, _gridInfo);		
	}
	
	void OnDestroy() {
		Debug.Log("On Destroy called");
		_pointCountersBuffer.Dispose();
		_pointIndicesBuffer.Dispose();
		_boundaryKernel.Dispose();
		_updateGridKernel.Dispose();
		_updateBoidsKernel.Dispose();
		_program.Dispose();
		_queue.Dispose();
		_context.Dispose();
	}
	
	public void Calc(Vector3[] positions, Vector3[] velocities, FlockInfo frInfo) {
		System.Array.Clear(_pointCounters, 0, _pointCounters.Length);
		System.Array.Clear(_pointIndices, 0, _pointIndices.Length);
		_queue.WriteToBuffer(_pointCounters, _pointCountersBuffer, false, _events);
		_queue.WriteToBuffer(_pointIndices, _pointIndicesBuffer, false, _events);
		
		var positionsBuffer = new Cloo.ComputeBuffer<Vector3>(
			_context, ComputeMemoryFlags.CopyHostPointer, positions);
		var velocitiesBuffer = new Cloo.ComputeBuffer<Vector3>(
			_context, ComputeMemoryFlags.CopyHostPointer, velocities);
		
		_boundaryKernel.SetMemoryArgument(0, positionsBuffer);
		
		_updateGridKernel.SetMemoryArgument(0, positionsBuffer);
		
		_updateBoidsKernel.SetMemoryArgument(0, positionsBuffer);
		_updateBoidsKernel.SetMemoryArgument(1, velocitiesBuffer);
		_updateBoidsKernel.SetValueArgument(5, frInfo);
		
		//var startTime = Time.realtimeSinceStartup;
		_queue.Execute(_boundaryKernel, null, new long[]{ positions.Length }, null, _events);
		_queue.Execute(_updateGridKernel, null, new long[]{ positions.Length }, null, _events);
		_queue.Execute(_updateBoidsKernel, null, new long[]{ positions.Length }, null, _events);
		_queue.ReadFromBuffer(_pointCountersBuffer, ref _pointCounters, false, _events);
		_queue.ReadFromBuffer(_pointIndicesBuffer, ref _pointIndices, false, _events);
		_queue.ReadFromBuffer(positionsBuffer, ref positions, false, _events);
		_queue.ReadFromBuffer(velocitiesBuffer, ref velocities, false, _events);
		_queue.Finish();
		//Debug.Log("Elapsed: " + (Time.realtimeSinceStartup - startTime));
		
#if false
		var counterSum = _pointCounters.Sum();
		if (positions.Length != counterSum)
			Debug.Log(string.Format("Counter sum must be {0} but {1}", positions.Length, counterSum));
		for (int i = 0; i < positions.Length; i++) {
			var p = positions[i];
			var cellPos = new Vector3(
				(p.x - _gridInfo.worldOrigin.x) / _gridInfo.cellSize.x,
				(p.y - _gridInfo.worldOrigin.y) / _gridInfo.cellSize.y,
				(p.z - _gridInfo.worldOrigin.z) / _gridInfo.cellSize.z);
			var cellId = (int)(cellPos.x) + _gridInfo.nGridPartitions 
				* ((int)(cellPos.y) + _gridInfo.nGridPartitions * (int)(cellPos.z));
			if (!Enumerable.Range(cellId * _gridInfo.maxIndices, _gridInfo.maxIndices).Any(iter => _pointIndices[iter] == i))
				Debug.Log(string.Format("Index is wrong at {0}", i));
		}
#endif
	}
	
	void Update() {
		if (Input.GetKey(KeyCode.R)) {
			var nBoids = 10000;
			var positions = new Vector3[nBoids];
			var velocities = new Vector3[nBoids];
			for (int i = 0; i < positions.Length; i++) {
				positions[i] = Random.insideUnitSphere * 400f;
				velocities[i] = Vector3.zero;
			}
			Calc(positions, velocities, new FlockInfo());
		}
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct GridInfo {
		public Vector3 worldOrigin;
		public Vector3 worldSize;
		public Vector3 cellSize;
		public int nGridPartitions;
		public int maxIndices;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct FlockInfo {
	    public float sepRange;
	    public float sepPower;
	    public float alnRange;
	    public float alnPower;
	    public float cohRange;
	    public float cohPower;
	}
}