using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Crowd3D : MonoBehaviour {
	public const int INDEX_ANTI_PENET = 0;
	public const int INDEX_SEPARATE = 1;
	public const int INDEX_ALIGNMENT = 2;
	public const int INDEX_COHESION = 3;
	
	public GameObject field;
	public GameObject fishfab;
	public int nFishes;
	public float minSpeed;
	public float maxSpeed;
	public float maxForce;
	public float[] weights;
	public float[] radiuses;
	public float acceleration;
	
	private List<Vehicle3D> _fishes;
	private Bounds _fieldBounds;
	private IPositionUniverse _grid;
	private Vector3[] _positions;
	private int[] _ids;

	// Use this for initialization
	void Start () {
		_fishes = new List<Vehicle3D>();
		for (int i = 0; i < nFishes; i++) {
			var f = (GameObject)Instantiate(fishfab);
			f.transform.parent = transform;
			var b = f.GetComponent<Vehicle3D>();
			b.currentVelocity = maxSpeed * Random.insideUnitSphere.normalized;
			_fishes.Add(b);
		}
		
		_fieldBounds = field.collider.bounds;
		
		_positions = new Vector3[nFishes];
		_ids = new int[_fishes.Count];
		for (int i = 0; i < nFishes; i++) {
			_positions[i] = _fishes[i].currentPosition;
			_ids[i] = i;
		}
		_grid = new UniformGrid2DFixed();
		_grid.Build(_positions, _ids, nFishes);
	}

	// Update is called once per frame
	void Update () {
		var dvs = new Vector3[_fishes.Count];
		var dt = Time.deltaTime;
		
		boundPosition ();
		for (int i = 0; i < nFishes; i++)
			_positions[i] = _fishes[i].currentPosition;
		_grid.Build(_positions, _ids, nFishes);
		
		var sqrMaxSpeed = maxSpeed * maxSpeed;
		var sqrMinSpeed = minSpeed * minSpeed;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			var neighborIndices = _grid.GetNeighbors(fish.currentPosition, radiuses[INDEX_COHESION]).ToArray();
			var neighbors = System.Array.ConvertAll(neighborIndices, (iNeighbor) => _fishes[iNeighbor]);
			var velocityAntiPenetrate = AntiPenetrate(fish, neighbors);
			var velocitySeparate = Separate(fish, neighbors);
			var velocityAlignment = Alignment(fish, neighbors);
			var velocityCohesion = Cohesion(fish, neighbors);
			dvs[i] = velocityAntiPenetrate + velocitySeparate + velocityAlignment + velocityCohesion;
			
			fish.currentVelocity += dvs[i];
			var sqrSpeed = fish.currentVelocity.sqrMagnitude;
			if (sqrMaxSpeed < sqrSpeed)
				fish.currentVelocity = fish.currentVelocity.normalized * maxSpeed;
			else if (sqrSpeed < sqrMinSpeed)
				fish.currentVelocity *= 1f + acceleration * dt;
		}
	}
	
	Vector3 AntiPenetrate(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		if (FindInRadius(me.currentPosition, radiuses[INDEX_ANTI_PENET], neighbors).Count() > 0) {
			v = Random.insideUnitCircle * maxSpeed;
		}
		return weights[INDEX_ANTI_PENET] * v;
	}
	
	Vector3 Separate(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		foreach (var f in FindInRadius(me.currentPosition, radiuses[INDEX_SEPARATE], neighbors)) {
			var distvec = f.currentPosition - me.currentPosition;
			v -= distvec;
		}
		return weights[INDEX_SEPARATE] * v;
	}
	
	Vector3 Alignment(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_SEPARATE] * radiuses[INDEX_SEPARATE];
		foreach (var f in FindInRadius(me.currentPosition, radiuses[INDEX_ALIGNMENT], neighbors)) {
			var distvec = f.currentPosition - me.currentPosition;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.currentVelocity;
			count++;
		}
		if (count > 0) {
			v = v / count - me.currentVelocity;
		}
		return weights[INDEX_ALIGNMENT] * v;
	}
	
	Vector3 Cohesion(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_ALIGNMENT] * radiuses[INDEX_ALIGNMENT];
		foreach (var f in FindInRadius(me.currentPosition, radiuses[INDEX_COHESION], neighbors)) {
			var distvec = f.currentPosition - me.currentPosition;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.currentPosition;
			count++;
		}
		if (count > 0) {
			v = v / count - me.currentPosition;
		}
		return weights[INDEX_COHESION] * v;
	}
	
	void boundPosition ()
	{
		var fieldBase = _fieldBounds.min;
		var fieldSize = _fieldBounds.size;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			if (_fieldBounds.Contains(fish.currentPosition))
				continue;
			var relativePos = fish.currentPosition - fieldBase;
			var t = new Vector3(Mathf.Repeat(relativePos.x / fieldSize.x, 1f), 
				Mathf.Repeat(relativePos.y / fieldSize.y, 1f), 
				Mathf.Repeat(relativePos.z / fieldSize.z, 1f));
			fish.currentPosition = Vector3.Scale(t, fieldSize) + fieldBase;
		}
	}
	
	IEnumerable<Vehicle3D> FindInRadius(Vector3 center, float radius, Vehicle3D[] neighbors) {
		var sqrRadius = radius * radius;
		foreach (var b in neighbors) {
			if ((b.currentPosition - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
}

public interface IBehaviour {
	Vector3 Calculate(Vehicle3D me, Vehicle3D[] neighbors);
}