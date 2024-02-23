using System;
using System.Collections;
using UnityEngine;

public class SoulOrb : MonoBehaviour
{
    //public RandomAudioClipTable soulOrbCollectSounds;

    public ParticleSystem getParticles;

    public bool awardSoul = true;

    [HideInInspector]
    public bool dontRecycle;

    private Transform target;

    private float speed;

    private float acceleration;

    private SpriteRenderer sprite;

    private TrailRenderer trail;

    private Rigidbody2D body;

    private AudioSource source;

    private Coroutine zoomRoutine;

    public float stretchFactor = 2f;

    public float stretchMinY = 1f;

    public float stretchMaxX = 2f;

    public float scaleModifier;

    public float scaleModifierMin = 1f;

    public float scaleModifierMax = 2f;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        body = GetComponent<Rigidbody2D>();
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        base.transform.SetPositionZ(UnityEngine.Random.Range(-0.001f, -0.1f));
        target = HeroController.instance.transform;
    }

    private void OnDisable()
    {
        if ((bool)sprite)
        {
            sprite.enabled = false;
        }
        if ((bool)trail)
        {
            trail.enabled = false;
        }
        if ((bool)body)
        {
            body.isKinematic = true;
        }
        GameManager.instance.UnloadingLevel -= SceneLoading;
    }

    private void OnEnable()
    {
        if ((bool)sprite)
        {
            sprite.enabled = true;
        }
        if ((bool)trail)
        {
            trail.enabled = true;
        }
        if ((bool)body)
        {
            body.isKinematic = false;
        }
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
        }
        zoomRoutine = null;
        GameManager.instance.UnloadingLevel += SceneLoading;
        scaleModifier = UnityEngine.Random.Range(scaleModifierMin, scaleModifierMax);
    }

    private void Update()
    {
        if ((bool)body && body.velocity.magnitude < 2.5f && zoomRoutine == null)
        {
            zoomRoutine = StartCoroutine(Zoom());
        }
        FaceAngle();
        ProjectileSquash();
    }

    private void SceneLoading()
    {
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
        }
        zoomRoutine = StartCoroutine(Zoom(doZoom: false));
    }

    private IEnumerator Zoom(bool doZoom = true)
    {
        if (doZoom)
        {
            speed = 0f;
            while (true)
            {
                if ((bool)target)
                {
                    speed += acceleration;
                    speed = Mathf.Clamp(speed, 0f, 30f);
                    acceleration += 0.07f;
                    FireAtTarget();
                    if (Vector2.Distance(target.position, base.transform.position) < 0.8f)
                    {
                        break;
                    }
                    yield return null;
                    continue;
                }
                Debug.LogError("Soul orb could not get player target!");
                break;
            }
        }
        body.velocity = Vector2.zero;
        /*if ((bool)soulOrbCollectSounds)
        {
            soulOrbCollectSounds.PlayOneShot(source);
        }*/
        if ((bool)getParticles)
        {
            getParticles.Play();
        }
        if ((bool)sprite)
        {
            sprite.enabled = false;
        }
        if (awardSoul)
        {
            HeroController.instance.AddMPCharge(2);
        }
        //SpriteFlash component = HeroController.instance.gameObject.GetComponent<SpriteFlash>();
        /*if ((bool)component)
        {
            component.flashSoulGet();
        }*/
        yield return new WaitForSeconds(0.4f);
        if (dontRecycle)
        {
            base.gameObject.SetActive(value: false);
        }
        /*else
        {
            base.gameObject.Recycle();
        }*/
    }

    private void FireAtTarget()
    {
        float y = target.position.y - base.transform.position.y;
        float x = target.position.x - base.transform.position.x;
        float num = Mathf.Atan2(y, x) * (180f / (float)Math.PI);
        Vector2 velocity = default(Vector2);
        velocity.x = speed * Mathf.Cos(num * ((float)Math.PI / 180f));
        velocity.y = speed * Mathf.Sin(num * ((float)Math.PI / 180f));
        body.velocity = velocity;
    }

    private void FaceAngle()
    {
        Vector2 velocity = body.velocity;
        float z = Mathf.Atan2(velocity.y, velocity.x) * (180f / (float)Math.PI);
        base.transform.localEulerAngles = new Vector3(0f, 0f, z);
    }

    private void ProjectileSquash()
    {
        float num = 1f - body.velocity.magnitude * stretchFactor * 0.01f;
        float num2 = 1f + body.velocity.magnitude * stretchFactor * 0.01f;
        if (num2 > stretchMaxX)
        {
            num2 = stretchMaxX;
        }
        if (num < stretchMinY)
        {
            num = stretchMinY;
        }
        num *= scaleModifier;
        num2 *= scaleModifier;
        base.transform.localScale = new Vector3(num2, num, base.transform.localScale.z);
    }
}
