using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SteeringBehaviours {
	
	public static Vector3 AntiPenetrate(Vehicle3D me, Vehicle3D[] neighbors, int nNeighbors, float radius) {
		var v = Vector3.zero;
		if (neighbors.FindInRadius(me.position, radius, nNeighbors).Count() > 0) {
			v = Random.insideUnitCircle * me.maxSpeed;
		}
		return v;
	}
	
	public static Vector3 SphereAvoidance(Vehicle3D me, ISphere[] spheres, float cylinderLength, float cylinderRadius) {
		var closestIntersectionX = float.MaxValue;
		ISphere closestSphere = null;
		
		foreach (var sp in spheres) {
			var sqrEffectiveRadius = cylinderLength * cylinderLength + sp.radius * sp.radius;
			var me2sp = sp.position - me.position;
			if (sqrEffectiveRadius < me2sp.sqrMagnitude)
				continue;
			var localSphereCenterX = Vector3.Dot(me.forward, me2sp);
			if (localSphereCenterX <= 0)
				continue;
			var sqrLocalSphereCenterY = Vector3.Cross(me.forward, me2sp).sqrMagnitude;
			var outerRadius = sp.radius + cylinderRadius;
			if ((outerRadius * outerRadius) < sqrLocalSphereCenterY)
				continue;
			var d = Mathf.Sqrt(sp.radius * sp.radius - sqrLocalSphereCenterY);
			var intersectionX = localSphereCenterX - d;
			if (intersectionX < 0)
				intersectionX = localSphereCenterX + d;
			if (intersectionX < closestIntersectionX) {
				closestIntersectionX = intersectionX;
				closestSphere = sp;
			}
		}
		if (closestIntersectionX == float.MaxValue)
			return Vector3.zero;
		
		var toSp = closestSphere.position - me.position;
		var distMult = 1.0f + (cylinderLength - closestIntersectionX) / cylinderLength;
		var sideForce = Vector3.Dot(me.forward, toSp) * me.forward - toSp;
		var sideForceMag = sideForce.magnitude;
		sideForce *= (closestSphere.radius + cylinderRadius - sideForceMag) / sideForceMag * distMult;
		
		var breakForce = -0.2f * (cylinderLength - closestIntersectionX) * toSp.normalized;
		
		return sideForce + breakForce;
	}
	
	public static Vector3 Seek(Vehicle3D me, Vector3 position) {
		var toDest = position - me.position;
		var desiredVelocity = toDest.normalized * me.maxSpeed;
		return desiredVelocity - me.velocity;
	}
	
	public static Vector3 Arrive(Vehicle3D me, Vector3 position, float baseSpeed) {
		var toDest = position - me.position;
		var dist = toDest.magnitude;
		if (dist < 1e-3f)
			return Vector3.zero;
		
		var speed = Mathf.Min(me.maxSpeed, dist * baseSpeed);
		return toDest * (speed / dist);
	}
	
	public static Vector3 Follow(Vehicle3D me, Vector3 destination) {
		var toDest = destination - me.position;
		return toDest;
	}
	
	#region BOID
	public static Vector3  Separate(Vehicle3D me, Vehicle3D[] neighbors, int nNeighbors, float radius) {
		var v = Vector3.zero;
		foreach (var f in neighbors.FindInRadius(me.position, radius, nNeighbors)) {
			var distvec = f.position - me.position;
			v -= distvec;
		}
		return v;
	}
	public static Vector3 Align(Vehicle3D me, Vehicle3D[] neighbors, int nNeighbors, float radius, float minRadius) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = minRadius * minRadius;
		foreach (var f in neighbors.FindInRadius(me.position, radius, nNeighbors)) {
			var distvec = f.position - me.position;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.velocity;
			count++;
		}
		if (count > 0) {
			v = v / count - me.velocity;
		}
		return v;
	}	
	public static Vector3 Cohere(Vehicle3D me, Vehicle3D[] neighbors, int nNeighbors, float radius, float minRadius) {
		var v = Vector3.zero;
		var count = 0;
		var sqrMinRadius = minRadius * minRadius;
		foreach (var f in neighbors.FindInRadius(me.position, radius, nNeighbors)) {
			var distvec = f.position - me.position;
			if (distvec.sqrMagnitude < sqrMinRadius)
				continue;
			v += f.position;
			count++;
		}
		if (count > 0) {
			v = v / count - me.position;
		}
		return v;
	}
	#endregion
	
	public interface ISphere {
		Vector3 position { get; }
		float radius { get; }
	}
}
