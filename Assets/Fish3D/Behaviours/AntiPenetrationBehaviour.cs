using UnityEngine;
using System.Collections;
using System.Linq;

public class AntiPenetrationBehaviour : MonoBehaviour, IBehaviour {
	public float weight;
	public float radius;
	
	public Vector3 Calculate(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		if (neighbors.FindInRadius(me.currentPosition, radius).Count() > 0) {
			v = Random.insideUnitCircle * me.maxSpeed;
		}
		return weight * v;
	}
}
