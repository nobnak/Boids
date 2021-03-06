using System.Collections.Generic;
using UnityEngine;

public class UniformGrid2D : IPositionUniverse {
	public readonly static Vector3 SMALL_AMOUNT = 1e-4f * Vector3.one;
	
	private int _nX, _nY;
	private Vector3 _worldMin;
	private Vector3 _rCellSize;
	private List<int>[,] _cells;
	private HashSet<int> _tmpFound;
	
	public UniformGrid2D() {
		_tmpFound = new HashSet<int>();
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
		_cells = new List<int>[_nX, _nY];
		
		int ix, iy;
		for (var i = 0; i < length; i++) {
			GetIndices(positions[i], out ix, out iy);
			var list = _cells[ix, iy];
			if (list == null) {
				list = new List<int>(1);
				_cells[ix, iy] = list;
			}
			list.Add(ids[i]);
		}
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
		_tmpFound.Clear();
		int minix, miniy;
		int maxix, maxiy;
		GetIndices(house.min, out minix, out miniy);
		GetIndices(house.max, out maxix, out maxiy);
		for (var ix = minix; ix <= maxix; ix++) {
			for (var iy = miniy; iy <= maxiy; iy++) {
				var residents = _cells[ix, iy];
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
	
	public IEnumerable<int> GetNeighbors(Vector3 center, float radius) {
		var house = new Bounds(center, 2 * radius * Vector3.one);
		return GetNeighbors(house);
	}
}
