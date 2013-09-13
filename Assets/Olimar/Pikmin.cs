using UnityEngine;
using System.Collections;
using System.Linq;

public class Pikmin : MonoBehaviour {
	public enum State { Friend, Relation, Other }
	
	public float arrivalDistance;
	[HideInInspector]
	public TrailTracker traker;
	[HideInInspector]
	public PikminGrid partition;

	public Steering Steering { get; private set; }
	public Vehicle Vehicle { get; private set; }
	
	public State state;
	
	void Awake() {
		state = State.Other;
	}

	// Use this for initialization
	void Start () {
		Steering = GetComponent<Steering>();
		Vehicle = GetComponent<Vehicle>();
		Steering.tracker = traker;
		Steering.me = this;
		Vehicle.position = transform.position;
		Vehicle.me = this;
	}
	
	// Update is called once per frame
	public void UpdatePikmin() {
		if (state == State.Other) {
			var dt = Time.deltaTime;
			var force = Steering.Calculate();
			
			Vehicle.velocity = Vector2.ClampMagnitude(Vehicle.velocity + force * dt, Vehicle.maxSpeed);
		} else {
			Vehicle.velocity *= 0.9f;
		}
		
	}
}
