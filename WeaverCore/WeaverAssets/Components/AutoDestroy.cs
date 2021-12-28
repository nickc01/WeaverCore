using System.Collections;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// This component is used to automatically destroy an object after a set condition. This can be setup to:
    /// 1. Destroy after a set amount of time has passed
    /// 2. Destroy when the particle system on an object stops
    /// 3. Destroy when an animation is done (via an animation event)
    /// 
    /// The destroy behaviour can also be customized
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        ParticleSystem particles;
        ParticleSystem.MainModule main;

        [SerializeField]
        private OnDoneBehaviour DestroyBehaviour = OnDoneBehaviour.DestroyOrPool;

        [SerializeField]
        private bool destroyAfterTime;

        [Tooltip("Used only if Destroy After Time is set to true")]
        [SerializeField]
        private float lifeTime;

        private void Start()
        {
            particles = GetComponent<ParticleSystem>();

            main = particles.main;

            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnEnable()
        {
            if (destroyAfterTime)
            {
                base.StartCoroutine(Waiter());
            }
        }

        private void OnDisable()
        {
            base.StopAllCoroutines();
        }

        private IEnumerator Waiter()
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy();
            yield break;
        }

        /// <summary>
        /// Destroys the object
        /// </summary>
        public void Destroy()
        {
            DestroyBehaviour.DoneWithObject(this);
        }

        private void OnParticleSystemStopped()
        {
            Destroy();
        }
    }
}
