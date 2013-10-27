using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {
	public Transform target;
	public float zoomSpeed;
	public float rotationSpeed;
	
	private float _dist;
	private Quaternion _rot;

	// Use this for initialization
	void Start () {
		var view = target.position - transform.position;
		_rot = Quaternion.LookRotation(view);
		_dist = view.magnitude;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount == 0)
			return;
		
		var touch = Input.touches[0];
		
	}
}
