using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Crowd2D : MonoBehaviour {
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
	
	private List<Boid2D> _fishes;
	private Bounds _fieldBounds;
	private IPositionUniverse _grid;
	private Vector3[] _positions;
	private int[] _ids;

	// Use this for initialization
	void Start () {
		_fishes = new List<Boid2D>();
		for (int i = 0; i < nFishes; i++) {
			var f = (GameObject)Instantiate(fishfab);
			f.transform.parent = transform;
			var b = f.GetComponent<Boid2D>();
			b.velocity = maxSpeed * Random.insideUnitCircle.normalized;
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
		var dvs = new Vector2[_fishes.Count];
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
	
	Vector2 AntiPenetrate(Boid2D me, Boid2D[] neighbors) {
		var v = Vector2.zero;
		if (FindInRadius(me.position, radiuses[INDEX_ANTI_PENET], neighbors).Count() > 0) {
			v = Random.insideUnitCircle * maxSpeed;
		}
		return weights[INDEX_ANTI_PENET] * v;
	}
	
	Vector2 Separate(Boid2D me, Boid2D[] neighbors) {
		var v = Vector2.zero;
		foreach (var f in FindInRadius(me.position, radiuses[INDEX_SEPARATE], neighbors)) {
			var distvec = f.position - me.position;
			v -= distvec;
		}
		return weights[INDEX_SEPARATE] * v;
	}
	
	Vector2 Alignment(Boid2D me, Boid2D[] neighbors) {
		var v = Vector2.zero;
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
	
	Vector2 Cohesion(Boid2D me, Boid2D[] neighbors) {
		var v = Vector2.zero;
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
		var fieldBase = (Vector2)_fieldBounds.min;
		var fieldSize = (Vector2)_fieldBounds.size;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			if (_fieldBounds.Contains(fish.position))
				continue;
			var relativePos = fish.position - fieldBase;
			var t = new Vector2(Mathf.Repeat(relativePos.x / fieldSize.x, 1f), Mathf.Repeat(relativePos.y / fieldSize.y, 1f));
			fish.position = Vector2.Scale(t, fieldSize) + fieldBase;
		}
	}
	
	IEnumerable<Boid2D> FindInRadius(Vector2 center, float radius, Boid2D[] neighbors) {
		var sqrRadius = radius * radius;
		foreach (var b in neighbors) {
			if ((b.position - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
}
