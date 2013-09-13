using UnityEngine;
using System.Collections.Generic;

public class PikminFacade : MonoBehaviour {
	public Pikmin pikminfab;
	public int nPikmins;
	public List<Pikmin> pikmins { get; private set; }
	public TrailTracker tracker;
	
	private PikminGrid _partition;
	
	void Awake() {
		pikmins = new List<Pikmin>();
	}

	// Use this for initialization
	void Start () {
		_partition = GetComponent<PikminGrid>();
		
		for (int i = 0; i < nPikmins; i++) {
			var pos = pikminfab.transform.position;
			pos.x = Random.Range(-10f, 10f);
			pos.y = Random.Range(-10f, 10f);
			var go = (GameObject)Instantiate(pikminfab.gameObject, pos, Quaternion.identity);
			go.transform.parent = transform;
			var pikmin = go.GetComponent<Pikmin>();
			pikmins.Add(pikmin);
			pikmin.traker = tracker;
			pikmin.partition = _partition;
		}
		
		_partition.Construct(pikmins);
	}
	
	// Update is called once per frame
	void Update () {
		_partition.Construct(pikmins);
		foreach (var pik in pikmins)
			pik.UpdatePikmin();
	}
}
