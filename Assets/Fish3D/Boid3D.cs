using UnityEngine;
using System.Collections;

public class Boid3D : MonoBehaviour {
	public float easing;
	
	public Vector3 position;
	public Vector3 velocity;
	public float rotateSpeed;
	
	void Update() {
		var dt = Time.deltaTime;
		
		position += velocity * dt;
		transform.position = position;
		if (velocity.sqrMagnitude > 1e-2f) {
			var targetRotation = Quaternion.FromToRotation(Vector3.up, velocity);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, easing);
		}
	}
}
