using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Causes splash effects to occur when certain objects collide with this object. Usually used with water or acid
    /// </summary>
    public class WeaverSurfaceObjectSplasher : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The collision mask used to determine what large-sized objects can splash")]
        LayerMask largeCollisionMask;

        [SerializeField]
        [Tooltip("The collision mask used to determine what small-sized objects can splash")]
        LayerMask smallCollisionMask;

        [SerializeField]
        AudioClip splashSound;

        [SerializeField]
        GameObject splashOutParticles;

        [SerializeField]
        GameObject spatterParticles;

        private void Reset()
        {
            largeCollisionMask = LayerMask.GetMask("Corpse");
            smallCollisionMask = LayerMask.GetMask("Tinker");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.name.Contains("TRIGGER WORKAROUND") && (largeCollisionMask & (1 << collision.gameObject.layer)) != 0)
            {
                CorpseCollision(collision.gameObject, true);
            }

            if (!collision.gameObject.name.Contains("TRIGGER WORKAROUND") && (smallCollisionMask & (1 << collision.gameObject.layer)) != 0)
            {
                CorpseCollision(collision.gameObject, false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.name.Contains("TRIGGER WORKAROUND") && (largeCollisionMask & (1 << collision.gameObject.layer)) != 0)
            {
                CorpseCollision(collision.gameObject, true);
            }

            if (!collision.gameObject.name.Contains("TRIGGER WORKAROUND") && (smallCollisionMask & (1 << collision.gameObject.layer)) != 0)
            {
                CorpseCollision(collision.gameObject, false);
            }
        }

        void CorpseCollision(GameObject corpse, bool large)
        {
            if (large)
            {
                if (splashSound != null)
                {
                    var instance = WeaverAudio.PlayAtPoint(splashSound, corpse.transform.position);
                    instance.AudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                }

                if (splashOutParticles != null)
                {
                    Pooling.Instantiate(splashOutParticles, corpse.transform.position, Quaternion.identity);
                }

                if (spatterParticles != null)
                {
                    FlingUtilities.SpawnRandomObjectsPooled(spatterParticles, corpse.transform.position + new Vector3(0, 1), new Vector2Int(12, 16), new Vector2(12, 20), new Vector2(70, 110), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                if (spatterParticles != null)
                {
                    FlingUtilities.SpawnRandomObjectsPooled(spatterParticles, corpse.transform.position + new Vector3(0, 1), new Vector2Int(3, 5), new Vector2(8, 15), new Vector2(70, 110), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}
