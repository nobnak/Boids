using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour {
	public float maxSpeed;
	public float personalSpaceRadius;
	public Vector2 position;
	public Vector2 heading;
	public Vector2 velocity;
	[HideInInspector]
	public Pikmin me;
	
	void Start() {
		UpdateMotion();
	}
	
	void Update() {
		UpdateMotion();
		EnforceNoPenetration ();
	}	
	
	void UpdateMotion () {
		var dt = Time.deltaTime;
		position += velocity * dt;
		transform.position = position;
		if (1f < velocity.sqrMagnitude) {
			heading = velocity.normalized;
			transform.localRotation = Quaternion.FromToRotation(Vector3.up, heading);
		}
	}

	void EnforceNoPenetration () {
		var minSqrDist = float.MaxValue;
		Pikmin nearest = null;
		foreach (var p in me.partition.GetNeighbors(position, personalSpaceRadius)) {
			if (p == me)
				continue;
			var toNeighbor = p.Vehicle.position - position;
			if (minSqrDist < toNeighbor.sqrMagnitude)
				continue;
			minSqrDist = toNeighbor.sqrMagnitude;
			nearest = p;
		}
		if (nearest == null)
			return;
		
		var toNearest = nearest.Vehicle.position - position;
		var dist = toNearest.magnitude;
		var penetrationAmount = personalSpaceRadius - dist;
		if (penetrationAmount > 0f) {
			if (dist > 1e-1f)
				position -= toNearest * (penetrationAmount / dist);
			else
				position -= Random.insideUnitCircle.normalized * penetrationAmount;
			transform.position = position;
		}
	}
}