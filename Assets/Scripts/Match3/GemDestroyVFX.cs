using System;
using UnityEngine;

namespace Match3
{
    [RequireComponent(typeof(ParticleSystem))]
    public class GemDestroyVFX : MonoBehaviour, IPoolable
    {
        public static event Action<GemDestroyVFX> OnVFXStopped;

        [SerializeField] private PoolType thisPoolType = PoolType.GemVFXBlue;

        private ParticleSystem vfxParticles;

        public PoolType GetPoolType
        {
            get => thisPoolType;
            set { }
        }

        public void OnSpawn()
        {
            PlayVFX();
        }

        public void OnDespawn() { }

        private void Awake()
        {
            vfxParticles = GetComponent<ParticleSystem>();
        }

        private void PlayVFX()
        {
            vfxParticles.Play();
        }

        public void OnParticleSystemStopped()
        {
            OnVFXStopped?.Invoke(this);
        }
    }
}
