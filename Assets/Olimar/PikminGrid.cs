using UnityEngine;
using System.Collections.Generic;

public class PikminGrid : MonoBehaviour {
	private int _nGrid;
	private Bounds _world;
	private Vector3 _cellSize;
	private LinkedList<Pikmin>[,,] _cells;
	private HashSet<Pikmin> _tmpFound;
	
	void Start() {
		_tmpFound = new HashSet<Pikmin>();
	}
	
	public void Construct(IList<Pikmin> pikmins) {
		_world = Encapsulate(pikmins, 0, pikmins.Count - 1);
		_nGrid = Mathf.Max(1, (int)Mathf.Pow(pikmins.Count, 0.333f));
		_cellSize = _world.extents / _nGrid;
		_cells = new LinkedList<Pikmin>[_nGrid, _nGrid, _nGrid];
		
		int ix, iy, iz;
		foreach (var p in pikmins) {
			GetIndices(p.transform.position, out ix, out iy, out iz);
			LinkedList<Pikmin> list = _cells[ix, iy, iz];
			if (list == null) {
				list = new LinkedList<Pikmin>();
				_cells[ix, iy, iz] = list;
			}
			list.AddLast(p);
		}
	}
	
	public Bounds Encapsulate(IList<Pikmin> pikmins, int left, int right) {
		var count = right - left + 1;
		if (count == 1)
			return new Bounds(pikmins[left].transform.position, Vector3.zero);
		if (count == 2) {
			var pLeft = pikmins[left].transform.position;
			var pRight = pikmins[right].transform.position;
			return new Bounds(0.5f * (pLeft + pRight), pRight - pLeft);
		}
		
		var center = left + (right - left) / 2;
		var bLeft = Encapsulate(pikmins, left, center -1);
		var bRight = Encapsulate(pikmins, center, right);
		bLeft.Encapsulate(bRight);
		return bLeft;
	}
	
	public void GetIndices(Vector3 pos, out int ix, out int iy, out int iz) {
		var relativePos = pos - _world.min;
		ix = (int)(relativePos.x / _cellSize.x); ix = ix < 0 ? 0 : (ix >= _nGrid ? _nGrid - 1 : ix);
		iy = (int)(relativePos.y / _cellSize.y); iy = iy < 0 ? 0 : (iy >= _nGrid ? _nGrid - 1 : iy);
		iz = (int)(relativePos.z / _cellSize.z); iz = iz < 0 ? 0 : (iz >= _nGrid ? _nGrid - 1 : iz);
	}

	public IEnumerable<Pikmin> GetNeighbors(Bounds house) {
		_tmpFound.Clear();
		int minix, miniy, miniz;
		int maxix, maxiy, maxiz;
		GetIndices(house.min, out minix, out miniy, out miniz);
		GetIndices(house.max, out maxix, out maxiy, out maxiz);
		for (var ix = minix; ix <= maxix; ix++) {
			for (var iy = miniy; iy <= maxiy; iy++) {
				for (var iz = miniz; iz <= maxiz; iz++) {
					LinkedList<Pikmin> residents = _cells[ix, iy, iz];
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
	}
	
	public IEnumerable<Pikmin> GetNeighbors(Vector2 center, float radius) {
		var sqrRadius = radius * radius;
		var house = new Bounds(center, 2 * radius * Vector3.one);
		foreach (var p in GetNeighbors(house)) {
			var toPik = p.Vehicle.position - center;
			if (toPik.sqrMagnitude < sqrRadius)
				yield return p;
		}
	}
}
