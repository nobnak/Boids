using System.Collections.Generic;
using UnityEngine;

public class UniformGrid2DFixed : IPositionUniverse {
	public readonly static Vector3 SMALL_AMOUNT = 1e-4f * Vector3.one;
	public int nEntitiesPerCell = 4;
	
	private int _nX, _nY;
	private Vector3 _worldMin;
	private Vector3 _rCellSize;
	private int[] _cells;
	private int[] _counters;
	
	public UniformGrid2DFixed() {
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
		var cellSize = new Vector2(size.x / _nX, size.y / _nY);
		_rCellSize = new Vector2(1f / cellSize.x, 1f / cellSize.y);
		if (_counters.Length < (_nX * _nY)) {
			_cells = new int[nEntitiesPerCell * _nX * _nY];
			_counters = new int[_nX * _nY];
		} else {
			System.Array.Clear(_counters, 0, _counters.Length);
		}
		
		int ix, iy;
		for (var i = 0; i < length; i++) {
			GetIndices(positions[i], out ix, out iy);
			var cellIndex = XY2CellBaseIndex(ix, iy);
			var count = _counters[cellIndex];
			if (count < nEntitiesPerCell) {
				_cells[nEntitiesPerCell * cellIndex + count] = ids[i];
				_counters[cellIndex] = ++count;
			}				
		}
	}
	
	public int XY2CellBaseIndex(int ix, int iy) {
		return (ix + _nX * iy);
	}
	
	public Bounds Encapsulate(Vector3[] positions, int length) {
		float minx = float.MaxValue, miny = float.MaxValue;
		float maxx = float.MinValue, maxy = float.MinValue;
		for (int i = 0; i < length; i++) {
			var p = positions[i];
			if (p.x < minx) minx = p.x;
			else if (maxx < p.x) maxx = p.x;
			if (p.y < miny) miny = p.y;
			else if (maxy < p.y) maxy = p.y;
		}
		
		var center = new Vector3(0.5f * (minx + maxx), 0.5f * (miny + maxy), 0f);
		var size = new Vector3(1.1f * (maxx - minx), 1.1f * (maxy - miny), 0f);
		return new Bounds(center, size + SMALL_AMOUNT);
	}
	
	public void GetIndices(Vector3 pos, out int ix, out int iy) {
		var relativePos = pos - _worldMin;
		ix = (int)(relativePos.x * _rCellSize.x); ix = ix < 0 ? 0 : (ix >= _nX ? _nX - 1 : ix);
		iy = (int)(relativePos.y * _rCellSize.y); iy = iy < 0 ? 0 : (iy >= _nY ? _nY - 1 : iy);
	}

	public IEnumerable<int> GetNeighbors(Bounds house) {
		int minix, miniy;
		int maxix, maxiy;
		GetIndices(house.min, out minix, out miniy);
		GetIndices(house.max, out maxix, out maxiy);
		for (var ix = minix; ix <= maxix; ix++) {
			for (var iy = miniy; iy <= maxiy; iy++) {
				var cellIndex = XY2CellBaseIndex(ix, iy);
				var count = _counters[cellIndex];
				for (var offset = 0; offset < count; offset++) {
					var p = _cells[nEntitiesPerCell * cellIndex + offset];
					yield return p;
				}
			}
		}
	}
	
	public IEnumerable<int> GetNeighbors(Vector3 center, float radius) {
		var house = new Bounds(center, 2 * radius * Vector3.one);
		return GetNeighbors(house);
	}
}
