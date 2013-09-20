using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vehicle3D : MonoBehaviour {
	public float easing;
	public float rotateSpeed;
	public float maxSpeed;
	public float minSpeed;
	
	public Vector3 currentPosition;
	public Vector3 currentVelocity;
	
	void Update() {
		var dt = Time.deltaTime;
		
		currentPosition += currentVelocity * dt;
		transform.position = currentPosition;
		if (currentVelocity.sqrMagnitude > 1e-2f) {
			var targetRotation = Quaternion.FromToRotation(Vector3.up, currentVelocity);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, easing);
		}
	}
}

public static class Vehicle3DExtension {
	public static IEnumerable<Vehicle3D> FindInRadius(this IEnumerable<Vehicle3D> neighbors, Vector3 center, float radius) {
		var sqrRadius = radius * radius;
		foreach (var b in neighbors) {
			if ((b.currentPosition - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
}