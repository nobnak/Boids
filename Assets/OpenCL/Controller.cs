using UnityEngine;
using System.Collections;
using System.Linq;

public class Controller : MonoBehaviour {
	public int nBoids = 1024;
	public float positionRange = 0.1f;
	public float velocityRange = 0.01f;
	public GPGPU_UniformGrid.FlockInfo frInfo = new GPGPU_UniformGrid.FlockInfo(){
		sepRange = 0.5f, sepPower = 0.5f, 
		alnRange = 1.0f, alnPower = 2f, 
		cohRange = 1.0f, cohPower = 0.2f };
	
	private GPGPU_UniformGrid _gpgpu;
	private Vector3[] _positions;
	private Vector3[] _velocities;

	private ParticleSystem _system;
	private ParticleSystem.Particle[] _particles;

	// Use this for initialization
	void Start () {
		_gpgpu = GetComponent<GPGPU_UniformGrid>();
		_positions = (from i in Enumerable.Range(0, nBoids) select Random.insideUnitSphere * positionRange).ToArray();
		_velocities = (from i in Enumerable.Range(0, nBoids) select Random.onUnitSphere * velocityRange).ToArray();

		_system = GetComponent<ParticleSystem>();
		_system.Emit(nBoids);
		_particles = new ParticleSystem.Particle[nBoids];
	}
	
	// Update is called once per frame
	void Update () {
		_system.GetParticles(_particles);
		
		_gpgpu.Calc(_positions, _velocities, frInfo);		
		for (int i = 0; i < nBoids; i++) {
			_particles[i].position = _positions[i];
		}
		
		_system.SetParticles(_particles, nBoids);
	}
}
