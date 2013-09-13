using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : MonoBehaviour {
	public PikminFacade pikmins;
	public TrailTracker traker;
	public float arrivalDistance;
	public float hysteresis;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var destination = (Vector2)traker.Markers.First.Value.position;
		
		var arrived = new Queue<Pikmin>();
		var candidates = new LinkedList<Pikmin>();
		foreach (var pik in pikmins.pikmins) {
			var toDest = destination - (Vector2)pik.transform.position;
			var hys = pik.state != Pikmin.State.Other ? + hysteresis : - hysteresis;
			if (toDest.sqrMagnitude < arrivalDistance * arrivalDistance + hys) {
				pik.state = Pikmin.State.Friend;
				arrived.Enqueue(pik);
			} else {
				pik.state = Pikmin.State.Other;
				candidates.AddLast(pik);
			}
		}
		
		while (arrived.Count > 0) {
			var pik = arrived.Dequeue();
			var cand = candidates.First;
			while (cand != null) {
				var toArrived = (Vector2)pik.transform.position - (Vector2)cand.Value.transform.position;
				var hys = cand.Value.state != Pikmin.State.Other ? + hysteresis : - hysteresis;
				if (toArrived.sqrMagnitude < arrivalDistance * arrivalDistance + hys) {
					arrived.Enqueue(cand.Value);
					candidates.Remove(cand);
					cand.Value.state = Pikmin.State.Relation;
				}
				cand = cand.Next;
			}
		}
	}
}
