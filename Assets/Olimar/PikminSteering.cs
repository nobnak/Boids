using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PikminSteering : MonoBehaviour {
	public float maxForce;
	public float decelerationFollowPath;
	public float radiusNeighbor;
	public float weightFollowPath;
	public float weightSeparation;
	[HideInInspector]
	public TrailTracker tracker;
	[HideInInspector]
	public Pikmin me;
	
	public Vector2 Calculate() {
		var force = Vector2.zero;
		var maxSqrForce = maxForce * maxForce;
		
		force += FollowPath() * weightFollowPath;
		if (maxSqrForce < force.sqrMagnitude) {
			return force.normalized * maxForce;
		}
		
		force += Separation(me.partition.GetNeighbors(me.Vehicle.position, radiusNeighbor)) * weightSeparation;
		if (maxSqrForce < force.sqrMagnitude) {
			return force.normalized * maxForce;
		}
		
		return Vector2.ClampMagnitude(force, maxForce);
	}
	
	public Vector2 FollowPath() {
		var minSqrDist = float.MaxValue;
		LinkedListNode<Transform> nearest = tracker.Markers.First;
		while (nearest.Next != null) {
			var next = nearest.Next;
			var dist2next = (Vector2)next.Value.position - me.Vehicle.position;
			if (minSqrDist < dist2next.sqrMagnitude)
				break;
			minSqrDist = dist2next.sqrMagnitude;
			nearest = next;
		}
		if (nearest.Previous == null) {
			return Arrive(nearest.Value.position, decelerationFollowPath);
		}
		return Seek(nearest.Previous.Value.position);
	}
	
	public Vector2 Seek(Vector2 position) {
		var toDest = position - me.Vehicle.position;
		var desiredVelocity = toDest.normalized * me.Vehicle.maxSpeed;
		return desiredVelocity - me.Vehicle.velocity;
	}
	
	public Vector2 Arrive(Vector2 position, float deceleration) {
		var toDest = position - me.Vehicle.position;
		var dist = toDest.magnitude;
		var desiredVelocity = Vector2.zero;
		if (1e-3f < dist) {
			var speed = Mathf.Min(me.Vehicle.maxSpeed, dist * deceleration);
			desiredVelocity = toDest * (speed / dist);
		}
		return desiredVelocity - me.Vehicle.velocity;
	}
	
	public Vector2 Separation(IEnumerable<Pikmin> neighbors) {
		var force = Vector2.zero;
		foreach (var pik in neighbors) {
			if (pik == me)
				continue;
			var toNeighbor = pik.Vehicle.position - me.Vehicle.position;
			force -= toNeighbor / toNeighbor.sqrMagnitude;
		}
		return force;
	}
}
