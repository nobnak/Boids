using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WanderRigidbody : MonoBehaviour {
	public Transform wanderTarget;
	
	public float radius;
	public float distance;
	public float power;
	public float noise;
	
	public float maxSpeed;
	public float minSpeed;
	
	private Vector2 _targetLocal;

	// Use this for initialization
	void Start () {
		rigidbody.velocity = minSpeed * transform.up;
		_targetLocal = Vector2.up;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var dt = Time.fixedDeltaTime;
		
		_targetLocal += dt * noise * Random.insideUnitCircle;
		_targetLocal = radius * _targetLocal.normalized;
		
		var targetWorld = transform.TransformPoint(_targetLocal + distance * Vector2.up);
		wanderTarget.position = targetWorld;
		var force = (Vector2)(targetWorld - transform.position);
		rigidbody.AddForce(power * force);

		if ((maxSpeed * maxSpeed) < rigidbody.velocity.sqrMagnitude) {
			rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, maxSpeed);
		} else if (rigidbody.velocity.sqrMagnitude < 1e-3f) {
			rigidbody.velocity = minSpeed * (((Vector2)rigidbody.velocity).normalized + Random.insideUnitCircle);
		}
		
		if (1e-4f < rigidbody.velocity.sqrMagnitude) {
			var forward = rigidbody.velocity.normalized;
			rigidbody.MoveRotation(Quaternion.FromToRotation(Vector3.up, forward));
		}
	}
}
