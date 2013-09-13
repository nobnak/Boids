using UnityEngine;
using System.Collections.Generic;

public class Olimar : MonoBehaviour {
	public float speed;
	public float rotationSpeed;

	void Awake() {
	}
	
	// Update is called once per frame
	void Update () {
		Move();

	}


	void Move () {
		var dt = Time.deltaTime;
		
		if (Input.GetKey(KeyCode.W)) {
			transform.position += speed * dt * transform.up;
		}
		
		if (Input.GetKey(KeyCode.A)) {
			transform.localRotation *= Quaternion.AngleAxis(rotationSpeed * dt, Vector3.forward);
		}
		if (Input.GetKey(KeyCode.D)) {
			transform.localRotation *= Quaternion.AngleAxis(-rotationSpeed * dt, Vector3.forward);
		}
	}
}
