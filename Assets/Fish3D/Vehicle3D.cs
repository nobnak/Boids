using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vehicle3D : MonoBehaviour {
	public float easing;
	public float rotateSpeed;
	public float maxSpeed;
	public float minSpeed;
	
	public Vector3 position;
	public Vector3 velocity;
	public Vector3 forward;
	
	void Awake() {
		forward = Vector3.up;
	}
	
	void Update() {
		var dt = Time.deltaTime;
		
		position += velocity * dt;
		transform.position = position;
		if (velocity.sqrMagnitude > 1e-2f) {
			var targetRotation = Quaternion.FromToRotation(Vector3.up, velocity);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, easing);
			forward = velocity.normalized;
		}
	}
}

public static class Vehicle3DExtension {
	public static IEnumerable<Vehicle3D> FindInRadius(this IEnumerable<Vehicle3D> neighbors, Vector3 center, float radius) {
		var sqrRadius = radius * radius;
		foreach (var b in neighbors) {
			if ((b.position - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
	public static IEnumerable<Vehicle3D> FindInRadius(this IEnumerable<Vehicle3D> neighbors, Vector3 center, float radius, int nNeighbors) {
		var sqrRadius = radius * radius;
		var count = 0;
		foreach (var b in neighbors) {
			if (count++ >= nNeighbors)
				yield break;
			if ((b.position - center).sqrMagnitude < sqrRadius)
				yield return b;
		}
	}
}