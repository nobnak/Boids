using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Crowd3D : MonoBehaviour {
	public const int INDEX_ANTI_PENET = 0;
	public const int INDEX_SEPARATE = 1;
	public const int INDEX_ALIGNMENT = 2;
	public const int INDEX_COHESION = 3;
	
	public GameObject field;
	public Transform leader;
	public GameObject fishfab;
	public int nFishes;
	public float minSpeed;
	public float maxSpeed;
	public float maxForce;
	public float acceleration;
	public Transform[] spheres;

	public float antiPenetrateRadius = 0.01f;
	public float antiPenetrateWeight = 0.01f;
	public float separateRadius = 0.5f;
	public float separateWeight = 0.5f;
	public float alignRadius = 0.001f;
	public float alignWeight = 1f;
	public float cohereRadius = 0.1f;
	public float cohereWeight = 1.5f;
	public float avoidanceWeight = 1f;
	public float avoidanceCylinderLength = 3f;
	public float avoidanceCylinderRadius = 0.5f;
	public float followLeaderWeight = 1f;
	
	private List<Vehicle3D> _fishes;
	private Bounds _fieldBounds;
	private IPositionUniverse _grid;
	private Vector3[] _positions;
	private int[] _ids;
	private Sphere[] _spheres;

	// Use this for initialization
	void Start () {
		_fishes = new List<Vehicle3D>();
		for (int i = 0; i < nFishes; i++) {
			var f = (GameObject)Instantiate(fishfab);
			f.transform.parent = transform;
			var b = f.GetComponent<Vehicle3D>();
			b.velocity = maxSpeed * Random.insideUnitSphere.normalized;
			_fishes.Add(b);
		}
		
		if (field != null)
			_fieldBounds = field.collider.bounds;
		
		_positions = new Vector3[nFishes];
		_ids = new int[_fishes.Count];
		for (int i = 0; i < nFishes; i++) {
			_positions[i] = _fishes[i].position;
			_ids[i] = i;
		}
		_grid = new UniformGrid3DFixed();
		_grid.Build(_positions, _ids, nFishes);
		
		_spheres = System.Array.ConvertAll(spheres, (t) => new Sphere(){ position = t.position, radius = t.localScale.x * 0.5f });
	}

	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		
		if (field != null)
			boundPosition ();
		
		for (int i = 0; i < nFishes; i++)
			_positions[i] = _fishes[i].position;
		_grid.Build(_positions, _ids, nFishes);
		
		var sqrMaxSpeed = maxSpeed * maxSpeed;
		var sqrMinSpeed = minSpeed * minSpeed;
		for (int i = 0; i < _fishes.Count; i++) {
			var fish = _fishes[i];
			var indices = _grid.GetNeighbors(fish.position, cohereRadius).ToArray();
			var neighbors = System.Array.ConvertAll(indices, (iNeighbor) => _fishes[iNeighbor]);
			var nNeighbors = neighbors.Length;
			
			var forceAccumulated = CalculateSteering (fish, neighbors, nNeighbors);			
			fish.velocity += forceAccumulated;
			var sqrSpeed = fish.velocity.sqrMagnitude;
			if (sqrMaxSpeed < sqrSpeed)
				fish.velocity = fish.velocity.normalized * maxSpeed;
			else if (sqrSpeed < sqrMinSpeed)
				fish.velocity *= 1f + acceleration * dt;
		}
	}

	Vector3 CalculateSteering (Vehicle3D fish, Vehicle3D[] neighbors, int nNeighbors) {
		var sqrMaxForce = maxForce * maxForce;
		var v = Vector3.zero;
		
		v += antiPenetrateWeight * SteeringBehaviours.AntiPenetrate(fish, neighbors, nNeighbors, antiPenetrateRadius);
		if (sqrMaxForce < v.sqrMagnitude)
			return v;
		
		if (_spheres.Length > 0) {
			v += avoidanceWeight * SteeringBehaviours.SphereAvoidance(fish, _spheres, avoidanceCylinderLength, avoidanceCylinderRadius);
			if (sqrMaxForce < v.sqrMagnitude)
				return v;
		}

		if (leader != null) {
			v += followLeaderWeight * SteeringBehaviours.Follow(fish, leader.position);
			if (sqrMaxForce < v.sqrMagnitude)
				return v;
		}
		
		v += separateWeight * SteeringBehaviours.Separate(fish, neighbors, nNeighbors, separateRadius);
		if (sqrMaxForce < v.sqrMagnitude)
			return v;
		
		v += alignWeight * SteeringBehaviours.Align(fish, neighbors, nNeighbors, alignRadius, separateRadius);
		if (sqrMaxForce < v.sqrMagnitude)
			return v;
		
		v += cohereWeight * SteeringBehaviours.Cohere(fish, neighbors, nNeighbors, cohereRadius, separateRadius);
		if (sqrMaxForce < v.sqrMagnitude)
			return v;

		
		return v;
		
	}
	
	void boundPosition () {
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
	
	public class Sphere : SteeringBehaviours.ISphere {
		#region ISphere implementation
		public Vector3 position { get; set; }
		public float radius { get; set; }
		#endregion
	}
}
