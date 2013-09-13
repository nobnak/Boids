using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Crowd : MonoBehaviour {
	public const int INDEX_ANTI_PENET = 0;
	public const int INDEX_SEPARATE = 1;
	public const int INDEX_ALIGNMENT = 2;
	public const int INDEX_COHESION = 3;
	
	public GameObject field;
	public GameObject fishfab;
	public int nFishes;
	public float maxSpeed;
	public float maxForce;
	public float[] weights;
	public float[] radiuses;
	public float acceleration;
	
	private List<Boid> _fishes;
	private Bounds _fieldBounds;

	// Use this for initialization
	void Start () {
		_fishes = new List<Boid>();
		for (int i = 0; i < nFishes; i++) {
			var f = (GameObject)Instantiate(fishfab);
			f.transform.parent = transform;
			var b = f.GetComponent<Boid>();
			b.velocity = maxSpeed * Random.insideUnitCircle.normalized;
			_fishes.Add(b);
		}
		
		_fieldBounds = field.collider.bounds;
	}

	// Update is called once per frame
	void Update () {
		var dvs = new Vector2[_fishes.Count];
		var dt = Time.deltaTime;
		
		boundPosition ();
		
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			var velocityAntiPenetrate = AntiPenetrate(fish);
			var velocitySeparate = Separate(fish);
			var velocityAlignment = Alignment(fish);
			var velocityCohesion = Cohesion(fish);
			dvs[i] = velocityAntiPenetrate + velocitySeparate + velocityAlignment + velocityCohesion;
			fish.velocity = Vector3.ClampMagnitude(fish.velocity + (fish.velocity * acceleration * dt + dvs[i]), maxSpeed);
		}
	}
	
	Vector2 AntiPenetrate(Boid me) {
		var v = Vector2.zero;
		if (GetNeigbhors(me, radiuses[INDEX_ANTI_PENET]).Count() > 0) {
			v = Random.insideUnitCircle * maxSpeed;
		}
		return weights[INDEX_ANTI_PENET] * v;
	}
	
	Vector2 Separate(Boid me) {
		var v = Vector2.zero;
		foreach (var f in GetNeigbhors(me, radiuses[INDEX_SEPARATE])) {
			var distvec = f.position - me.position;
			v -= distvec;
		}
		return weights[INDEX_SEPARATE] * v;
	}
	
	Vector2 Alignment(Boid me) {
		var v = Vector2.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_SEPARATE] * radiuses[INDEX_SEPARATE];
		foreach (var f in GetNeigbhors(me, radiuses[INDEX_ALIGNMENT])) {
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
	
	Vector2 Cohesion(Boid me) {
		var v = Vector2.zero;
		var count = 0;
		var sqrMinRadius = radiuses[INDEX_ALIGNMENT] * radiuses[INDEX_ALIGNMENT];
		foreach (var f in GetNeigbhors(me, radiuses[INDEX_COHESION])) {
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
	
	IEnumerable<Boid> GetNeigbhors(Boid me, float radius) {
		var sqrRadius = radius * radius;
		
		foreach (var f in _fishes) {
			var distVec = f.position - me.position;
			if (f != me && distVec.sqrMagnitude < sqrRadius)
				yield return f;
		}
	}
}
