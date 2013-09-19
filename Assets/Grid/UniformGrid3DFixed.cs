using System.Collections.Generic;
using UnityEngine;

public class UniformGrid3DFixed : IUniformGrid {
	public readonly static Vector3 SMALL_AMOUNT = 1e-4f * Vector3.one;
	public int nEntitiesPerCell = 4;
	
	private int _nX, _nY, _nZ;
	private Vector3 _worldMin;
	private Vector3 _rCellSize;
	private int[] _cells;
	private int[] _counters;
	
	public UniformGrid3DFixed() {
		_cells = new int[0];
		_counters = new int[0];
	}
	
	public void Build(Vector3[] positions, int[] ids, int length) {
		var world = Encapsulate(positions, length);
		_worldMin = world.min;
		var size = world.size + SMALL_AMOUNT;
		var scale = Mathf.Sqrt((float)length  / (size.x * size.y));
		_nX = Mathf.Max(1, (int)(size.x * scale));
		_nY = Mathf.Max(1, (int)(size.y * scale));
		_nZ = Mathf.Max(1, (int)(size.z * scale));
		var cellSize = new Vector3(size.x / _nX, size.y / _nY, size.z / _nZ);
		_rCellSize = new Vector3(1f / cellSize.x, 1f / cellSize.y, 1f / cellSize.z);
		if (_counters.Length < (_nX * _nY * _nZ)) {
			_cells = new int[nEntitiesPerCell * _nX * _nY * _nZ];
			_counters = new int[_nX * _nY * _nZ];
		} else {
			System.Array.Clear(_counters, 0, _counters.Length);
		}
		
		int ix, iy, iz;
		for (var i = 0; i < length; i++) {
			GetIndices(positions[i], out ix, out iy, out iz);
			var cellIndex = XYZ2CellBaseIndex(ix, iy, iz);
			var count = _counters[cellIndex];
			if (count < nEntitiesPerCell) {
				_cells[nEntitiesPerCell * cellIndex + count] = ids[i];
				_counters[cellIndex] = ++count;
			}				
		}
	}
	
	public int XYZ2CellBaseIndex(int ix, int iy, int iz) {
		return (ix + _nX * iy + _nX * _nY * iz);
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
		var size = new Vector3(1.01f * (maxx - minx), 1.01f * (maxy - miny), 1.01f * (maxz - minz));
		return new Bounds(center, size + SMALL_AMOUNT);
	}
	
	public void GetIndices(Vector3 pos, out int ix, out int iy, out int iz) {
		var relativePos = pos - _worldMin;
		ix = (int)(relativePos.x * _rCellSize.x); ix = ix < 0 ? 0 : (ix >= _nX ? _nX - 1 : ix);
		iy = (int)(relativePos.y * _rCellSize.y); iy = iy < 0 ? 0 : (iy >= _nY ? _nY - 1 : iy);
		iz = (int)(relativePos.z * _rCellSize.z); iz = iz < 0 ? 0 : (iz >= _nZ ? _nZ - 1 : iz);
	}

	public IEnumerable<int> GetNeighbors(Bounds house) {
		int minix, miniy, miniz;
		int maxix, maxiy, maxiz;
		GetIndices(house.min, out minix, out miniy, out miniz);
		GetIndices(house.max, out maxix, out maxiy, out maxiz);
		for (var ix = minix; ix <= maxix; ix++) {
			for (var iy = miniy; iy <= maxiy; iy++) {
				for (var iz = miniz; iz <= maxiz; iz++) {
					var cellIndex = XYZ2CellBaseIndex(ix, iy, iz);
					var count = _counters[cellIndex];
					for (var offset = 0; offset < count; offset++) {
						var p = _cells[nEntitiesPerCell * cellIndex + offset];
						yield return p;
					}
				}
			}
		}
	}
	
	public IEnumerable<int> GetNeighbors(Vector3 center, float radius) {
		var house = new Bounds(center, 2 * radius * Vector3.one);
		return GetNeighbors(house);
	}
}
