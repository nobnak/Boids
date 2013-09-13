using UnityEngine;
using System.Collections;

public class MouseMove : MonoBehaviour {
	public Vector3 speedRotation = Vector3.one;
	public float speedWheel = 3f;
	
	private Transform _tr;
	private Camera _cam;
	private Vector2 _mousePos;
	private Vector3 _rotation;
	private Vector3 _position;

	// Use this for initialization
	void Start () {
		_tr = transform;
		_rotation = _tr.eulerAngles;
		
		_cam = GetComponentInChildren<Camera>();
		if (!!_cam) {
			_position = _cam.transform.localPosition;
		}
	}
	
	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		
		if (Input.GetMouseButtonDown(0)) {
			_mousePos = Input.mousePosition;
			return;
		}
			
		if (Input.GetMouseButton(0)) {
			Vector2 dx = (Vector2)Input.mousePosition - _mousePos;
			_mousePos = Input.mousePosition;
			_rotation.y += dx.x * (speedRotation.x * dt);
			_rotation.x -= dx.y * (speedRotation.y * dt);
			_tr.localRotation = Quaternion.Euler(_rotation);
			
			return;
		}
		
		float wheel = Input.GetAxis("Mouse ScrollWheel");
		if (!!_cam && (wheel < - Mathf.Epsilon || Mathf.Epsilon < wheel)) {
			_position.z += (speedWheel * dt) * wheel;
			_cam.transform.localPosition = _position;
		}
	}
}
