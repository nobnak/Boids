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
	
	private List<Boid3D> _fishes;
	private Bounds _fieldBounds;
	private IUniformGrid _grid;
	private Vector3[] _positions;
	private int[] _ids;

	// Use this for initialization
	void Start () {
		_fishes = new List<Boid3D>();
		for (int i = 0; i < nFishes; i++) {
			var f = (GameObject)Instantiate(fishfab);
			f.transform.parent = transform;
			var b = f.GetComponent<Boid3D>();
			b.velocity = maxSpeed * Random.insideUnitSphere.normalized;
			_fishes.Add(b);
		}
		
		_fieldBounds = field.collider.bounds;
		
		_positions = new Vector3[nFishes];
		_ids = new int[_fishes.Count];
		for (int i = 0; i < nFishes; i++) {
			_positions[i] = _fishes[i].position;
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
			_positions[i] = _fishes[i].position;
		_grid.Build(_positions, _ids, nFishes);
		
		var sqrMaxSpeed = maxSpeed * maxSpeed;
		var sqrMinSpeed = minSpeed * minSpeed;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			var neighborIndices = _grid.GetNeighbors(fish.position, radiuses[INDEX_COHESION]).ToArray();
			var neighbors = System.Array.ConvertAll(neighborIndices, (iNeighbor) => _fishes[iNeighbor]);
			var velocityAntiPenetrate = AntiPenetrate(fish, neighbors);
			var velocitySeparate = Separate(fish, neighbors);
			var velocityAlignment = Alignment(fish, neighbors);
			var velocityCohesion = Cohesion(fish, neighbors);
			dvs[i] = velocityAntiPenetrate + velocitySeparate + velocityAlignment + velocityCohesion;
			
			fish.velocity += dvs[i];
			var sqrSpeed = fish.velocity.sqrMagnitude;
			if (sqrMaxSpeed < sqrSpeed)
				fish.velocity = fish.velocity.normalized * maxSpeed;
			else if (sqrSpeed < sqrMinSpeed)
				fish.velocity *= 1f + acceleration * dt;
		}
	}
	
	Vector3 AntiPenetrate(Boid3D me, Boid3D[] neighbors) {
		var v = Vector3.zero;
		if (FindInRadius(me.position, radiuses[INDEX_ANTI_PENET], neighbors).Count() > 0) {
			v = Random.insideUnitCircle * maxSpeed;
		}
		return weights[INDEX_ANTI_PENET] * v;
	}
	
	Vector3 Separate(Boid3D me, Boid3D[] neighbors) {
		var v = Vector3.zero;
		foreach (var f in FindInRadius(me.position, radiuses[INDEX_SEPARATE], neighbors)) {
			var distvec = f.position - me.position;
			v -= distvec;
		}
		return weights[INDEX_SEPARATE] * v;
	}
	
	Vector3 Alignment(Boid3D me, Boid3D[] neighbors) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_SEPARATE] * radiuses[INDEX_SEPARATE];
		foreach (var f in FindInRadius(me.position, radiuses[INDEX_ALIGNMENT], neighbors)) {
			var distvec = f.position - me.position;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.velocity;
			count++;
		}
		if (count > 0) {
			v = v / count - me.velocity;
		}
		return weights[INDEX_ALIGNMENT] * v;
	}
	
	Vector3 Cohesion(Boid3D me, Boid3D[] neighbors) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_ALIGNMENT] * radiuses[INDEX_ALIGNMENT];
		foreach (var f in FindInRadius(me.position, radiuses[INDEX_COHESION], neighbors)) {
			var distvec = f.position - me.position;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.position;
			count++;
		}
		if (count > 0) {
			v = v / count - me.position;
		}
		return weights[INDEX_COHESION] * v;
	}
	
	void boundPosition ()
	{
		var fieldBase = _fieldBounds.min;
		var fieldSize = _fieldBounds.size;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			if (_fieldBounds.Contains(fish.position))
				continue;
			var relativePos = fish.position - fieldBase;
			var t = new Vector3(Mathf.Repeat(relativePos.x / fieldSize.x, 1f), 
				Mathf.Repeat(relativePos.y / fieldSize.y, 1f), 
				Mathf.Repeat(relativePos.z / fieldSize.z, 1f));
			fish.position = Vector3.Scale(t, fieldSize) + fieldBase;
		}
	}
	
	IEnumerable<Boid3D> FindInRadius(Vector3 center, float radius, Boid3D[] neighbors) {
		var sqrRadius = radius * radius;
		foreach (var b in neighbors) {
			if ((b.position - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
}
