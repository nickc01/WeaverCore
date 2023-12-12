using System;
using System.Collections;
using UnityEngine;

public class GeoControl : MonoBehaviour
{
    [Serializable]
    public struct Size
    {
        public string idleAnim;

        public string airAnim;

        public int value;

        public Size(string idleAnim, string airAnim, int value)
        {
            this.idleAnim = idleAnim;
            this.airAnim = airAnim;
            this.value = value;
        }
    }

    public Size[] sizes = new Size[3]
    {
        new Size("Small Idle", "Small Air", 1),
        new Size("Med Idle", "Med Air", 5),
        new Size("Large Idle", "Large Air", 25)
    };

    public int type;

    private Size size;

    [Space]
    public AudioClip[] pickupSounds;

    //[Space]
    //public VibrationData pickupVibration;

    [Space]
    public ParticleSystem acidEffect;

    public GameObject getterBug;

    private Coroutine getterRoutine;

    private HeroController hero;

    private Transform player;

    private bool activated;

    private bool attracted;

    private const float pickupStartDelay = 0.25f;

    private float pickupStartTime;

    private float defaultGravity;

    //private tk2dSpriteAnimator anim;

    private AudioSource audioSource;

    private Renderer rend;

    private Rigidbody2D body;

    private BoxCollider2D boxCollider;

    //private SpriteFlash spriteFlash;

    private void Awake()
    {
        //anim = GetComponent<tk2dSpriteAnimator>();
        audioSource = GetComponent<AudioSource>();
        rend = GetComponent<Renderer>();
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        //spriteFlash = GetComponent<SpriteFlash>();
        defaultGravity = body.gravityScale;
    }

    private void Start()
    {
        hero = HeroController.instance;
    }

    private void OnEnable()
    {
        if (BossSceneController.IsBossScene)
        {
            //base.gameObject.Recycle();
            GameObject.Destroy(gameObject);
            return;
        }
        SetSize(type);
        base.transform.SetPositionZ(UnityEngine.Random.Range(0.001f, 0.002f));
        activated = false;
        attracted = false;
        body.gravityScale = defaultGravity;
        if ((bool)rend)
        {
            rend.enabled = true;
        }
        if ((bool)getterBug)
        {
            getterBug.SetActive(value: false);
        }
        if ((bool)acidEffect)
        {
            acidEffect.gameObject.SetActive(value: false);
        }
        boxCollider.isTrigger = false;
        if (!(GameManager.instance.sm != null && GameManager.instance.GetCurrentMapZone() == "COLOSSEUM") && !(GameManager.instance.sceneName == "Crossroads_38"))
        {
            if (GameManager.instance.GetPlayerDataBool("equippedCharm_1"))
            {
                getterRoutine = StartCoroutine(Getter());
            }
            pickupStartTime = Time.time + 0.25f;
        }
    }

    private void FixedUpdate()
    {
        if (attracted)
        {
            Vector2 vector = new Vector2(hero.transform.position.x - base.transform.position.x, hero.transform.position.y - 0.5f - base.transform.position.y);
            vector = Vector2.ClampMagnitude(vector, 1f);
            vector = new Vector2(vector.x * 150f, vector.y * 150f);
            body.AddForce(vector);
            Vector2 velocity = body.velocity;
            velocity = Vector2.ClampMagnitude(velocity, 20f);
            body.velocity = velocity;
        }
    }

    public void SetSize(int index)
    {
        if (index >= sizes.Length)
        {
            index = sizes.Length - 1;
        }
        else if (index < 0)
        {
            index = 0;
        }
        size = sizes[index];
        /*if ((bool)anim)
        {
            anim.Play(size.airAnim);
        }*/
    }

    public void SetFlashing()
    {
        /*if ((bool)spriteFlash)
        {
            spriteFlash.GeoFlash();
        }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if ((bool)anim)
        {
            tk2dSpriteAnimationClip clipByName = anim.GetClipByName(size.idleAnim);
            if (clipByName != null)
            {
                anim.PlayFromFrame(clipByName, UnityEngine.Random.Range(0, clipByName.frames.Length));
            }
        }*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated || Time.time < pickupStartTime)
        {
            return;
        }
        bool flag = false;
        float num = 0f;
        if (collision.tag == "HeroBox")
        {
            hero.AddGeo(size.value);
            //VibrationManager.PlayVibrationClipOneShot(pickupVibration);
            num = Mathf.Max(num, PlayCollectSound());
            flag = true;
        }
        else if (collision.tag == "Acid")
        {
            if ((bool)acidEffect)
            {
                acidEffect.gameObject.SetActive(value: true);
                num = Mathf.Max(num, acidEffect.main.duration + acidEffect.main.startLifetime.constant);
            }
            flag = true;
        }
        if (flag)
        {
            if (getterRoutine != null)
            {
                StopCoroutine(getterRoutine);
            }
            Disable(num);
        }
    }

    private float PlayCollectSound()
    {
        if ((bool)audioSource && pickupSounds.Length != 0)
        {
            AudioClip audioClip = pickupSounds[UnityEngine.Random.Range(0, pickupSounds.Length)];
            if ((bool)audioClip)
            {
                audioSource.PlayOneShot(audioClip);
                return audioClip.length;
            }
            Debug.LogError("GeoControl encountered missing audio!", this);
        }
        return 0f;
    }

    public void Disable(float waitTime)
    {
        activated = true;
        if ((bool)rend)
        {
            rend.enabled = false;
        }
        if ((bool)getterBug)
        {
            getterBug.SetActive(value: false);
        }
        StartCoroutine(DisableAfterTime(waitTime));
    }

    private IEnumerator DisableAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //base.gameObject.Recycle();
        GameObject.Destroy(gameObject);
    }

    private IEnumerator Getter()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 1.7f));
        if ((bool)getterBug)
        {
            getterBug.SetActive(value: true);
            Vector3 destination = new Vector3(-0.06624349f, 0.1932119f, -0.001f);
            Vector3 source = destination + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1.5f), 0f);
            float easeTime = UnityEngine.Random.Range(0.3f, 0.5f);
            for (float timer = 0f; timer < easeTime; timer += Time.deltaTime)
            {
                float t = Mathf.Sin(timer / easeTime * ((float)Math.PI / 2f));
                getterBug.transform.localPosition = Vector3.Lerp(source, destination, t);
                yield return null;
            }
            getterBug.transform.localPosition = destination;
            boxCollider.isTrigger = true;
            body.gravityScale = 0f;
            attracted = true;
        }
    }
}
