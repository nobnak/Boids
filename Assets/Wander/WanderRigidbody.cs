using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WanderRigidbody : MonoBehaviour {
	public float wanderRadius;
	public float wanderDistance;
	public float wanderWeight;
	public float wanderNoise;
	
	public float wallWeight;
	
	public float maxSpeed;
	public float minSpeed;
	
	public Collider watertank;
	
	private Vector2 _targetLocal;

	// Use this for initialization
	void Start () {
		rigidbody.velocity = minSpeed * transform.up;
		_targetLocal = Vector2.up;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var dt = Time.fixedDeltaTime;
		var posTr = transform.position;
		
		if (!watertank.bounds.Contains(posTr)) {
			var wallPosLocal = transform.InverseTransformPoint(watertank.ClosestPointOnBounds(posTr));
			_targetLocal += dt* wallWeight * (Vector2)wallPosLocal;
		}

		_targetLocal += dt * wanderNoise * Random.insideUnitCircle;
		_targetLocal = wanderRadius * _targetLocal.normalized;
		
		var targetWorld = transform.TransformPoint(_targetLocal + wanderDistance * Vector2.up);
		var wanderForce = (Vector2)(targetWorld - posTr);
		rigidbody.AddForce(wanderWeight * wanderForce);

		ClampSpeed ();
		
		UpdateRotation ();
	}

	void ClampSpeed () {
		if ((maxSpeed * maxSpeed) < rigidbody.velocity.sqrMagnitude) {
			rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, maxSpeed);
		} else if (rigidbody.velocity.sqrMagnitude < 1e-3f) {
			rigidbody.velocity = minSpeed * (((Vector2)rigidbody.velocity).normalized + Random.insideUnitCircle);
		}
	}

	void UpdateRotation () {
		if (1e-4f < rigidbody.velocity.sqrMagnitude) {
			var forward = rigidbody.velocity.normalized;
			forward.z = 0f;
			rigidbody.MoveRotation(Quaternion.FromToRotation(Vector3.up, forward));
		}
	}
}
