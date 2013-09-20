using System.Collections.Generic;
using UnityEngine;

public class UniformGrid : IPositionUniverse {
	public readonly static Vector3 SMALL_AMOUNT = 1e-4f * Vector3.one;
	
	private int _nX, _nY, _nZ;
	private Vector3 _worldMin;
	private Vector3 _rCellSize;
	private List<int>[,,] _cells;
	private HashSet<int> _tmpFound;
	
	public UniformGrid() {
		_tmpFound = new HashSet<int>();
	}
	
	public void Build(Vector3[] positions, int[] ids, int length) {
		var world = Encapsulate(positions, length);
		_worldMin = world.min;
		var size = world.size + SMALL_AMOUNT;
		var scale = Mathf.Pow((float)length  / (size.x * size.y * size.z), 0.3333f);
		_nX = Mathf.Max(1, (int)(size.x * scale));
		_nY = Mathf.Max(1, (int)(size.y * scale));
		_nZ = Mathf.Max(1, (int)(size.z * scale));
		var cellSize = new Vector3(size.x / _nX, size.y / _nY, size.z / _nZ);
		_rCellSize = new Vector3(1f / cellSize.x, 1f / cellSize.y, 1f / cellSize.z);
		_cells = new List<int>[_nX, _nY, _nZ];
		
		int ix, iy, iz;
		for (var i = 0; i < length; i++) {
			GetIndices(positions[i], out ix, out iy, out iz);
			var list = _cells[ix, iy, iz];
			if (list == null) {
				list = new List<int>(1);
				_cells[ix, iy, iz] = list;
			}
			list.Add(ids[i]);
		}
	}
	
	public Bounds Encapsulate(Vector3[] positions, int length) {
		float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;
		float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;
		for (int i = 0; i < length; i++) {
			var p = positions[i];
			if (p.x < minx) minx = p.x;
			else if (maxx < p.x) maxx = p.x;
			if (p.y < miny) miny = p.y;
			else if (maxy < p.y) maxy = p.y;
			if (p.z < minz) minz = p.z;
			else if (maxz < p.z) maxz = p.z;
		}
		
		var center = new Vector3(0.5f * (minx + maxx), 0.5f * (miny + maxy), 0.5f * (minz + maxz));
		var size = new Vector3(1.1f * (maxx - minx), 1.1f * (maxy - miny), 1.1f * (maxz - minz));
		return new Bounds(center, size + SMALL_AMOUNT);
	}
	
	public void GetIndices(Vector3 pos, out int ix, out int iy, out int iz) {
		var relativePos = pos - _worldMin;
		ix = (int)(relativePos.x * _rCellSize.x); ix = ix < 0 ? 0 : (ix >= _nX ? _nX - 1 : ix);
		iy = (int)(relativePos.y * _rCellSize.y); iy = iy < 0 ? 0 : (iy >= _nY ? _nY - 1 : iy);
		iz = (int)(relativePos.z * _rCellSize.z); iz = iz < 0 ? 0 : (iz >= _nZ ? _nZ - 1 : iz);
	}

	public IEnumerable<int> GetNeighbors(Bounds house) {
		_tmpFound.Clear();
		int minix, miniy, miniz;
		int maxix, maxiy, maxiz;
		GetIndices(house.min, out minix, out miniy, out miniz);
		GetIndices(house.max, out maxix, out maxiy, out maxiz);
		var nGrid = 0;
		for (var ix = minix; ix <= maxix; ix++) {
			for (var iy = miniy; iy <= maxiy; iy++) {
				for (var iz = miniz; iz <= maxiz; iz++) {
					nGrid++;
					var residents = _cells[ix, iy, iz];
					if (residents == null)
						continue;
					foreach (var p in residents) {
						if (_tmpFound.Contains(p))
							continue;
						_tmpFound.Add(p);
						yield return p;
					}
				}
			}
		}
		Debug.Log("nGrid per GetNeighbors " + nGrid);
	}
	
	public IEnumerable<int> GetNeighbors(Vector3 center, float radius) {
		var house = new Bounds(center, 2 * radius * Vector3.one);
		return GetNeighbors(house);
	}
}


public interface IPositionUniverse {
	void Build(Vector3[] positions, int[] ids, int length);
	IEnumerable<int> GetNeighbors(Bounds house);
	IEnumerable<int> GetNeighbors(Vector3 center, float radius);
}

public struct LR {
	public int left;
	public int right;
	public LR(int left, int right) {
		this.left = left;
		this.right = right;
	}
}
	