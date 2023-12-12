using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class Shockwave : MonoBehaviour, IOnPool
    {
        static Shockwave shockwaveSmall;
        public static Shockwave ShockwaveSmall => shockwaveSmall ??= WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Small").GetComponent<Shockwave>();

        static Shockwave shockwaveShort;
        public static Shockwave ShockwaveShort => shockwaveShort ??= WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Short").GetComponent<Shockwave>();

        static Shockwave shockwaveZ;
        public static Shockwave ShockwaveZ => shockwaveZ ??= WeaverAssets.LoadWeaverAsset<GameObject>("Shockwave Wave Z").GetComponent<Shockwave>();

        static RaycastHit2D[] hitCache = new RaycastHit2D[1];

        enum CollisionType
        {
            None,
            Hit,
            Wall
        }

        [SerializeField]
        ShockwaveSpurt spurtLeftPrefab;

        [SerializeField]
        ShockwaveSpurt spurtRightPrefab;

        [SerializeField]
        bool spawnSpurts = false;

        [NonSerialized]
        Coroutine spawnWaveRoutine;

        [NonSerialized]
        Rigidbody2D rb;

        [NonSerialized]
        CollisionType hitType;

        [NonSerialized]
        List<ParticleSystem> particles = new List<ParticleSystem>();

        float speed = 1f;

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

        IEnumerator MainRoutine()
        {
            foreach (var particle in particles)
            {
                particle.Play();
            }

            yield return null;
            var scale = transform.GetLocalScaleXY();

            var rocksStomp = transform.Find("Burst Rocks Stomp");

            Vector2 raycastFrom;
            float angleMin, angleMax, direction;
            ShockwaveSpurt spurtPrefab;

            if (scale.x >= 0)
            {
                raycastFrom = Vector2.one;
                angleMin = 95f;
                angleMax = 105f;
                direction = 0;
                spurtPrefab = spurtRightPrefab;
            }
            else
            {
                raycastFrom = new Vector2(-1f,0f);
                angleMin = 75f;
                angleMax = 85f;
                direction = 180f;
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

                if (Physics2D.RaycastNonAlloc((Vector2)transform.position + raycastFrom, Vector2.left,hitCache, 2f,8) > 0)
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

        IEnumerator SpawnRoutine(ShockwaveSpurt prefab, float scaleX, float scaleY)
        {
            while (true)
            {
                var instance = Pooling.Instantiate(prefab, transform.position, transform.rotation);
                instance.transform.SetLocalScaleXY(scaleX, scaleY);
                yield return new WaitForSeconds(0.005f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 8)
            {
                hitType = CollisionType.Wall;
            }
        }

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