using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour {
	public float easing;
	
	public Vector2 position {
		get { return transform.position; }
		set { transform.position = value; }
	}
	
	public Vector2 velocity;
	public float rotateSpeed;
	
	void Update() {
		var dt = Time.deltaTime;
		
		transform.position += (Vector3)(velocity * dt);
		if (velocity.sqrMagnitude > 1e-2f) {
			var targetRotation = Quaternion.FromToRotation(Vector3.up, velocity);
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, easing);
		}
	}
}
