using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailTracker : MonoBehaviour {
	public Transform olimar;
	public int nTrailMarkers;
	public float distanceBetween;
	public GameObject markerfab;
	public float markerOffset;
	
	public LinkedList<Transform> Markers { get; private set; }
	private Transform _headerMarker;
	private Vector3 _nextCandidatePosition;
	
	void Awake() {
		Markers = new LinkedList<Transform>();
	}

	// Use this for initialization
	void Start () {
		InitMarkers();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateHeader ();
		var dist2header = _headerMarker.position - _nextCandidatePosition;
		if (distanceBetween * distanceBetween < dist2header.sqrMagnitude) {
			AddMarker ();
		}	
	}
	

	void AddMarker () {
		var pos = _headerMarker.position;
		_headerMarker.position = _nextCandidatePosition;
		_nextCandidatePosition = pos;
		if (Markers.Count < nTrailMarkers) {
			_headerMarker = ((GameObject)Instantiate(markerfab, pos, Quaternion.identity)).transform;
			_headerMarker.parent = transform;
		} else {
			_headerMarker = Markers.Last.Value;
			Markers.RemoveLast();
			_headerMarker.position = pos;
		}
		Markers.AddFirst(_headerMarker);
		UpdateMarkerName ();
	}	

	void InitMarkers () {
		var pos = olimar.position - markerOffset * olimar.forward;
		_nextCandidatePosition = pos;
		_headerMarker = ((GameObject)Instantiate(markerfab, pos, Quaternion.identity)).transform;
		_headerMarker.parent = transform;
		Markers.AddFirst(_headerMarker);
		UpdateMarkerName ();
	}

	void UpdateHeader () {
		_headerMarker.position = olimar.position - markerOffset * olimar.forward;
	}
	 
	void UpdateMarkerName () {
		var i = 0;
		foreach (var mk in Markers) {
			mk.gameObject.name = string.Format("Marker {0}", i++);
		}
	}
}
