using UnityEngine;
using System.Collections;

namespace Nobnak.Extension {
	public static class ParticleSystemExtension {
		public static int GetParticlesInLargetArray(this ParticleSystem shuriken, ref ParticleSystem.Particle[] particles) {
			if (particles == null || particles.Length < shuriken.particleCount) {
				particles = new ParticleSystem.Particle[shuriken.particleCount * 2];
			}
			return shuriken.GetParticles(particles);
		}
	}
}
