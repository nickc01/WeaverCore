using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to control a white shockwave that scrolls across the screen
    /// </summary>
    public class Shockwave : MonoBehaviour, IOnPool
    {

        private static CachedPrefab<Shockwave> shockwaveSmall = new CachedPrefab<Shockwave>();
        /// <summary>
        /// The default prefab for a small shockwave
        /// </summary>
        public static Shockwave ShockwaveSmall
        {
            get
            {
                if (shockwaveSmall.Value == null)
                {
                    shockwaveSmall.Value = WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Small").GetComponent<Shockwave>();
                }
                return shockwaveSmall.Value;
            }
        }

        private static CachedPrefab<Shockwave> shockwaveShort = new CachedPrefab<Shockwave>();
        /// <summary>
        /// The default prefab for a short shockwave
        /// </summary>
        public static Shockwave ShockwaveShort
        {
            get
            {
                if (shockwaveShort.Value == null)
                {
                    shockwaveShort.Value = WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Short").GetComponent<Shockwave>();
                }
                return shockwaveShort.Value;
            }
        }

        private static Shockwave shockwaveZ;
        /// <summary>
        /// The default prefab for a large shockwave
        /// </summary>
        public static Shockwave ShockwaveZ
        {
            get
            {
                if (shockwaveZ == null)
                {
                    shockwaveZ = WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Z").GetComponent<Shockwave>();
                }
                return shockwaveZ;
            }
        }

        //private static RaycastHit2D[] hitCache = new RaycastHit2D[1];

        private enum CollisionType
        {
            None,
            Hit,
            Wall
        }

        [SerializeField]
        [Tooltip("Prefab for the left spurt of the shockwave. Used only if the shockwave is travelling to the left")]
        private ShockwaveSpurt spurtLeftPrefab;

        [SerializeField]
        [Tooltip("Prefab for the right spurt of the shockwave. Used only if the shockwave is travelling to the right")]
        private ShockwaveSpurt spurtRightPrefab;

        [SerializeField]
        [Tooltip("Determines whether spurts should be spawned along with the shockwave.")]
        private bool spawnSpurts = false;

        [NonSerialized]
        private Coroutine spawnWaveRoutine;

        [NonSerialized]
        private Rigidbody2D rb;

        [NonSerialized]
        private CollisionType hitType;

        [NonSerialized]
        private System.Collections.Generic.List<ParticleSystem> particles = new System.Collections.Generic.List<ParticleSystem>();

        [NonSerialized]
        private float speed = 1f;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            if (particles.Count == 0)
            {
                gameObject.GetComponentsInChildren(particles);
            }
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
            StopAllCoroutines();
            StartCoroutine(MainRoutine());
        }

        /// <summary>
        /// Main coroutine for the shockwave behavior.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MainRoutine()
        {
            foreach (var particle in particles)
            {
                particle.Play();
            }

            yield return null;
            var scale = transform.GetLocalScaleXY();

            var rocksStomp = transform.Find("Burst Rocks Stomp");

            Vector2 raycastFrom;
            ShockwaveSpurt spurtPrefab;

            if (scale.x >= 0)
            {
                raycastFrom = Vector2.one;
                spurtPrefab = spurtRightPrefab;
            }
            else
            {
                raycastFrom = new Vector2(-1f, 0f);
                speed *= -1f;
                spurtPrefab = spurtLeftPrefab;
            }

            float incrementer = speed * 2f;
            speed *= 0.025f;

            if (spawnSpurts)
            {
                spawnWaveRoutine = StartCoroutine(SpawnRoutine(spurtPrefab, scale.x, scale.y));
            }

            hitType = CollisionType.None;

            while (hitType == CollisionType.None)
            {
                speed += incrementer;
                rb.velocity = rb.velocity.With(x: speed);

                var hitCache = HitCache.GetSingleCachedArray();

                if (Physics2D.RaycastNonAlloc((Vector2)transform.position + raycastFrom, Vector2.left, hitCache, 2f, 8) > 0)
                {
                    hitType = CollisionType.Hit;
                }

                yield return new WaitForFixedUpdate();
            }

            float spawnTimer = 0;
            for (float t = 0; t < 0.15; t += Time.deltaTime)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= 0.005f)
                {
                    spawnTimer = 0;

                    var instance = Pooling.Instantiate(spurtPrefab, transform.position, transform.rotation);
                    instance.transform.SetLocalScaleXY(scale.x, scale.y);
                }

                yield return null;
            }

            foreach (var particle in particles)
            {
                particle.Stop();
            }

            if (spawnWaveRoutine != null)
            {
                StopCoroutine(spawnWaveRoutine);
                spawnWaveRoutine = null;
            }

            yield return new WaitForSeconds(1f);

            speed = 1;
            Pooling.Destroy(this);

            //yield return new WaitForSeconds(0.15f);
        }

        /// <summary>
        /// Coroutine for spawning spurts along with the shockwave.
        /// </summary>
        /// <param name="prefab">Prefab of the spurt.</param>
        /// <param name="scaleX">X scale of the spurt.</param>
        /// <param name="scaleY">Y scale of the spurt.</param>
        /// <returns></returns>
        private IEnumerator SpawnRoutine(ShockwaveSpurt prefab, float scaleX, float scaleY)
        {
            while (true)
            {
                var instance = Pooling.Instantiate(prefab, transform.position, transform.rotation);
                instance.transform.SetLocalScaleXY(scaleX, scaleY);
                yield return new WaitForSeconds(0.005f);
            }
        }

        /// <summary>
        /// Called when the collider enters another collider trigger.
        /// </summary>
        /// <param name="collision">The other Collider2D involved in this collision.</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 8)
            {
                hitType = CollisionType.Wall;
            }
        }

        /// <summary>
        /// Called when the object is retrieved from the object pool.
        /// </summary>
        public void OnPool()
        {
            hitType = CollisionType.None;
            StopAllCoroutines();
            spawnWaveRoutine = null;
            speed = 1;

            if (transform.localScale.x < 0)
            {
                transform.SetXLocalScale(-transform.GetXLocalScale());
            }
        }

        /// <summary>
        /// Spawns a shockwave instance.
        /// </summary>
        /// <param name="prefab">Prefab of the shockwave.</param>
        /// <param name="position">Position of the shockwave.</param>
        /// <param name="faceRight">Determines the initial facing direction.</param>
        /// <param name="speed">Speed of the shockwave.</param>
        /// <returns>The spawned shockwave instance.</returns>
        public static Shockwave Spawn(Shockwave prefab, Vector3 position, bool faceRight = true, float speed = 1f)
        {
            var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);
            instance.transform.localScale = prefab.transform.localScale;
            instance.speed = speed;
            if (!faceRight && instance.transform.localScale.x >= 0f)
            {
                instance.transform.SetXLocalScale(-instance.transform.GetXLocalScale());
            }
            return instance;
        }
    }
}
