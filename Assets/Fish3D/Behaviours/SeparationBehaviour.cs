using UnityEngine;
using System.Collections;

public class SeparationBehaviour : MonoBehaviour, IBehaviour {
	public float radius;
	public float weight;
	
	public Vector3 Calculate(Vehicle3D me, Vehicle3D[] neighbors) {
		var v = Vector3.zero;
		foreach (var f in neighbors.FindInRadius(me.currentPosition, radius)) {
			var distvec = f.currentPosition - me.currentPosition;
			v -= distvec;
		}
		return weight * v;
	}
}
