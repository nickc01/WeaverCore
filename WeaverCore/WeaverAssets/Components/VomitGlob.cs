using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{

    public class VomitGlob : MonoBehaviour, IOnPool
    {
        static VomitGlob prefab;

        [SerializeField]
        Vector2 randomZRange = new Vector2(0.0041f,0.00499f);

        [SerializeField]
        Vector2 randomScaleRange = new Vector2(1f,1.75f);

        [field: SerializeField]
        public Vector2 lifeTimeRange { get; set; } = new Vector2(1.75f,2.0f);

        bool initialized = false;

        Rigidbody2D rb;
        public Rigidbody2D RB => rb ??= GetComponent<Rigidbody2D>();

        SpriteRenderer mainRenderer;
        public SpriteRenderer MainRenderer => mainRenderer ??= GetComponent<SpriteRenderer>();

        Collider2D mainCollider;
        public Collider2D MainCollider => mainCollider ??= GetComponent<Collider2D>();

        WeaverAnimationPlayer animator;
        public WeaverAnimationPlayer Animator => animator ??= GetComponent<WeaverAnimationPlayer>();

        [SerializeField]
        Collider2D puddleCollider;

        [SerializeField]
        ParticleSystem airSteamParticles;

        [SerializeField]
        ParticleSystem steamParticles;

        [SerializeField]
        AudioClip landSound;

        [SerializeField]
        float shrinkTime = 0.25f;

        public bool Grounded { get; private set; } = false;

        public bool PlaySounds { get; set; } = true;

        PlayerDamager damager;

        int oldDamage = 0;

        bool forceDisappear = false;
        bool hasLifeTime = true;
        public bool HasLifeTime
        {
            get => hasLifeTime;
            set
            {
                if (hasLifeTime != value)
                {
                    hasLifeTime = value;
                    if (value && Grounded && lifeTimeRoutine == null)
                    {
                        lifeTimeRoutine = StartCoroutine(OnTerrainWait(UnityEngine.Random.Range(lifeTimeRange.x, lifeTimeRange.y)));
                    }

                    if (!value && lifeTimeRoutine != null)
                    {
                        StopCoroutine(lifeTimeRoutine);
                        lifeTimeRoutine = null;
                    }
                }
            }
        }


        Coroutine lifeTimeRoutine = null;
        Coroutine scaleCoroutine = null;

        IEnumerator MaxLifetimeRoutine(float time)
        {
            yield return new WaitForSeconds(time);
            Pooling.Destroy(this);
        }

        private void Start()
        {
            if (damager == null)
            {
                damager = GetComponent<PlayerDamager>();
            }
            if (!initialized)
            {
                Init();
            }

            StartCoroutine(MaxLifetimeRoutine(10));
        }

        void OnTerrainLand(Collision2D collision)
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
            OnOtherLand(collision, false);
            if (HasLifeTime)
            {
                lifeTimeRoutine = StartCoroutine(OnTerrainWait(UnityEngine.Random.Range(lifeTimeRange.x, lifeTimeRange.y)));
            }
            else if (forceDisappear)
            {
                StartCoroutine(EndRoutine());
            }
        }

        void OnOtherLand(Collision2D collision, bool finish)
        {
            oldDamage = damager.damageDealt;
            transform.SetZPosition(randomZRange.RandomInRange());
            RB.velocity = default;
            airSteamParticles.Stop();


            var contact = collision.GetContact(0);
            var normal = contact.normal;

            var normalAngle = MathUtilities.CartesianToPolar(normal).x - 90f;

            var bounds = MainCollider.bounds;

            transform.RotateAround(bounds.center,Vector3.forward, normalAngle);

            MainCollider.enabled = false;
            //transform.rotation = Quaternion.Euler(0f, 0f, normalAngle);

            transform.Translate(new Vector3(0f, 0.25f), Space.Self);
            steamParticles.Play();
            puddleCollider.gameObject.SetActive(true);
            puddleCollider.enabled = true;
            Animator.PlayAnimation("Land");
            if (PlaySounds)
            {
                WeaverAudio.PlayAtPoint(landSound, transform.position);
            }
            /*if (finish)
            {
                MainCollider.enabled = false;
                damager.damageDealt
            }*/
            rb.isKinematic = true;

            if (finish)
            {
                StartCoroutine(EndRoutine());
            }
        }

        IEnumerator OnTerrainWait(float waitTime)
        {
            /*for (float t = 0; t < waitTime; t += Time.deltaTime)
            {
                if (HasLifeTime)
                {
                    yield return null;
                }
                else
                {
                    yield break;
                }
            }*/
            yield return new WaitForSeconds(waitTime);
            yield return EndRoutine();
        }

        public void ForceDisappear()
        {
            if (!forceDisappear)
            {
                forceDisappear = true;
                HasLifeTime = false;
                if (Grounded)
                {
                    StartCoroutine(EndRoutine());
                }
            }
        }

        public void ForceDisappear(float time)
        {
            IEnumerator Wait(float time)
            {
                yield return new WaitForSeconds(time);
                ForceDisappear();
            }

            StartCoroutine(Wait(time));
        }

        IEnumerator EndRoutine()
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
            //var oldDamage = damager.damageDealt;
            //damager.damageDealt = 0;
            //damager.damageDealt = 0;
            puddleCollider.enabled = false;

            Vector3 oldScale = transform.localScale;
            Vector3 newScale = new Vector3(0.1f,0.1f);

            for (float t = 0; t < shrinkTime; t += Time.deltaTime)
            {
                transform.SetLocalScaleXY(Vector2.Lerp(oldScale, newScale, t / shrinkTime));
                yield return null;
            }

            damager.damageDealt = oldDamage;

            steamParticles.Stop();
            MainRenderer.enabled = false;

            yield return new WaitForSeconds(1f);

            Pooling.Destroy(this);
        }

        void Init()
        {
            initialized = true;

            var scale = UnityEngine.Random.Range(randomScaleRange.x, randomScaleRange.y);
            transform.SetLocalScaleXY(scale, scale);

            puddleCollider.gameObject.SetActive(false);
            puddleCollider.enabled = true;
            MainCollider.enabled = true;
        }

        public void SetScale(float scale)
        {
            transform.SetLocalScaleXY(scale,scale);
        }

        IEnumerator ScaleRoutine(float oldScale, float newScale, AnimationCurve curve, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                SetScale(Mathf.Lerp(oldScale,newScale, curve.Evaluate(t / time)));
                yield return null;
            }

            SetScale(newScale);
            scaleCoroutine = null;
        }

        public void SetScaleGradually(float newScale, AnimationCurve curve, float time = 0.5f)
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            scaleCoroutine = StartCoroutine(ScaleRoutine(transform.localScale.x, newScale, curve, time));
        }

        public static VomitGlob Spawn(Vector3 position, Vector2 velocity, float gravityScale = 0.7f, bool playSounds = true)
        {
            if (prefab == null)
            {
                prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Vomit Glob").GetComponent<VomitGlob>();
            }

            var instance = Pooling.Instantiate(prefab);
            instance.Init();

            instance.airSteamParticles.Play();

            instance.transform.position = position;
            instance.RB.velocity = velocity;
            instance.RB.gravityScale = gravityScale;
            instance.PlaySounds = playSounds;

            return instance;
        }

        public void OnPool()
        {
            MainRenderer.enabled = true;
            initialized = false;
            Grounded = false;
            rb.velocity = default;
            rb.isKinematic = false;
            forceDisappear = false;
            hasLifeTime = true;
            lifeTimeRoutine = null;
            scaleCoroutine = null;
            StopAllCoroutines();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Grounded)
            {
                return;
            }
            Grounded = true;
            if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                OnTerrainLand(collision);
                //collidedObjects.Add(collision.gameObject);
            }
            else
            {
                OnOtherLand(collision, true);
            }
        }

        /*private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                collidedObjects.Add(collision.gameObject);
            }
        }*/
    }
}
