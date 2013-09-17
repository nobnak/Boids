using UnityEngine;
using System.Collections;
using Nobnak.Extension;
using System.Linq;

public class GridController : MonoBehaviour {
	public ParticleSystem shuriken;
	public Marker[] markers;
	
	private IUniformGrid _grid;
	private ParticleSystem.Particle[] _particles;
	private Vector3[] _positions;
	private int[] _ids;

	// Use this for initialization
	void Start () {
		_grid = new UniformGrid2DFixed();
		_particles = new ParticleSystem.Particle[0];
		_positions = new Vector3[0];
		_ids = new int[0];
	}
	
	// Update is called once per frame
	void Update () {
		var nParticles = shuriken.GetParticlesInLargetArray(ref _particles);
		if (nParticles < 10) 
			return;
		
		UpdateVariables(nParticles);
		_grid.Build(_positions, _ids, nParticles);
		var startColor = (Color32)shuriken.startColor;
		for (int i = 0; i < nParticles; i++)
			_particles[i].color = startColor;
		foreach (var m in markers) {
			var color = m.color;
			foreach (var i in _grid.GetNeighbors(m.target.position, m.radius)) {
				_particles[i].color = color;
			}
		}
		
		shuriken.SetParticles(_particles, nParticles);
	}

	void UpdateVariables (int nParticles) {
		if (_positions.Length < nParticles) {
			_positions = new Vector3[_particles.Length];
			_ids = new int[_particles.Length];
		}
		for (int i = 0; i < nParticles; i++) {
			_positions[i] = _particles[i].position;
			_ids[i] = i;
		}
	}
}

[System.Serializable]
public class Marker {
	public Transform target;
	public float radius;
	public Color32 color;
}