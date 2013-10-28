using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WanderRigidbody : MonoBehaviour {
	public Transform wanderTarget;
	public Collider tank;
	public Transform[] haptics;
	
	public float wanderRadius;
	public float wanderDistance;
	public float wanderWeigt;
	public float wanderNoise;
	
	public float wallAvoidWeight;
	
	public float maxSpeed;
	public float minSpeed;
	public float maxRotationSpeed;
	
	private Vector2 _targetLocal;

	// Use this for initialization
	void Start () {
		rigidbody.velocity = minSpeed * transform.up;
		_targetLocal = Vector2.up;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var dt = Time.fixedDeltaTime;
		
		_targetLocal += dt * wanderNoise * new Vector2(Random.value - 0.5f, Random.value - 0.5f); 
		_targetLocal = wanderRadius * _targetLocal.normalized;
		
		var targetWorld = transform.TransformPoint(_targetLocal + wanderDistance * Vector2.up);
		wanderTarget.position = targetWorld;
		var wanderForce = (Vector2)(targetWorld - transform.position);
		rigidbody.AddForce(wanderWeigt * wanderForce);
		
		var wallAvoidForce = Vector2.zero;
		foreach (var hap in haptics) {
			var posHap = hap.position;
			if (tank.bounds.Contains(posHap))
				continue;
			wallAvoidForce += (Vector2)(tank.ClosestPointOnBounds(posHap) - posHap);
		}
		rigidbody.AddForce(wallAvoidWeight * wallAvoidForce);

		ClampSpeed();		
		UpdateRotation(dt);
	}

	void ClampSpeed () {
		if ((maxSpeed * maxSpeed) < rigidbody.velocity.sqrMagnitude) {
			rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, maxSpeed);
		} else if (rigidbody.velocity.sqrMagnitude < 1e-3f) {
			rigidbody.velocity = minSpeed * (((Vector2)rigidbody.velocity).normalized + Random.insideUnitCircle);
		}
	}

	void UpdateRotation (float dt) 	{
		if (1e-4f < rigidbody.velocity.sqrMagnitude) {
			var speed = rigidbody.velocity.magnitude;
			var forward = rigidbody.velocity / speed;
			var to = Quaternion.FromToRotation(Vector3.up, forward);
			rigidbody.MoveRotation(Quaternion.RotateTowards(rigidbody.rotation, to, speed * maxRotationSpeed * dt));
		}
	}
}
