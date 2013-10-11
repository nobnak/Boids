using UnityEngine;
using System.Collections;

public class Wander : MonoBehaviour {
	public float radius;
	public float distance;
	public float power;
	public float noise;
	
	public float maxSpeed;
	public float minSpeed;
	
	private Vector2 _forward;
	private Vector2 _velocity;
	private Vector2 _targetLocal;

	// Use this for initialization
	void Start () {
		_forward = transform.up;
		_velocity = minSpeed * _forward;
		_targetLocal = Vector2.up;
	}
	
	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		
		_targetLocal += dt * noise * Random.insideUnitCircle;
		_targetLocal = radius * _targetLocal.normalized;
		Debug.Log("Target Local : " + _targetLocal);
		
		var targetWorld = transform.TransformPoint(_targetLocal + distance * Vector2.up);
		var force = (Vector2)(targetWorld - transform.position);
		_velocity += dt * power * force;
		if ((maxSpeed * maxSpeed) < _velocity.sqrMagnitude) {
			_velocity = Vector2.ClampMagnitude(_velocity, maxSpeed);
		} else if (_velocity.sqrMagnitude < 1e-3f) {
			_velocity = minSpeed * (_velocity.normalized + Random.insideUnitCircle);
		}
		
		transform.position += (Vector3)(dt * _velocity);
		if (1e-4f < _velocity.sqrMagnitude) {
			_forward = _velocity.normalized;
			transform.localRotation = Quaternion.FromToRotation(Vector3.up, _forward);
		}
	}
}
