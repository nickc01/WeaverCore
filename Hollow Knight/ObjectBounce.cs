using UnityEngine;


public class ObjectBounce : MonoBehaviour
{
    public delegate void BounceEvent();

    public float bounceFactor;

    public float speedThreshold = 1f;

    public bool playSound;

    public AudioClip[] clips;

    public int chanceToPlay = 100;

    public float pitchMin = 1f;

    public float pitchMax = 1f;

    public bool playAnimationOnBounce;

    public string animationName;

    public float animPause = 0.5f;

    public bool sendFSMEvent;

    private float speed;

    private float animTimer;

    //private tk2dSpriteAnimator animator;

    //private PlayMakerFSM fsm;

    private Vector2 velocity;

    private Vector2 lastPos;

    private Rigidbody2D rb;

    private new AudioSource audio;

    private int chooser;

    private bool bouncing = true;

    private int stepCounter;

    public event BounceEvent OnBounce;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
        //animator = GetComponent<tk2dSpriteAnimator>();
        /*if (sendFSMEvent)
        {
            fsm = GetComponent<PlayMakerFSM>();
        }*/
    }

    private void FixedUpdate()
    {
        if (bouncing)
        {
            if (stepCounter >= 3)
            {
                Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.y);
                velocity = vector - lastPos;
                lastPos = vector;
                speed = (rb ? rb.velocity.magnitude : 0f);
                stepCounter = 0;
            }
            else
            {
                stepCounter++;
            }
        }
    }

    private void Update()
    {
        if (animTimer > 0f)
        {
            animTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!rb || rb.isKinematic || !bouncing || !(speed > speedThreshold))
        {
            return;
        }
        //Vector3 inNormal = col.GetSafeContact().Normal;
        //Vector3 inNormal = col.GetContacts().

        Vector3 inNormal;

        if (col.contactCount >= 1)
        {
            var contactPoint = col.GetContact(0);
            inNormal = contactPoint.normal;
        }
        else
        {
            Vector2 sourceOrigin = col.collider.transform.TransformPoint(col.collider.offset);
            Vector2 otherOrigin = col.otherCollider.transform.TransformPoint(col.otherCollider.offset);

            inNormal = (otherOrigin - sourceOrigin).normalized;
        }

        Vector3 normalized = Vector3.Reflect(velocity.normalized, inNormal).normalized;
        rb.velocity = new Vector2(normalized.x, normalized.y) * (speed * (bounceFactor * Random.Range(0.8f, 1.2f)));
        if (playSound)
        {
            chooser = Random.Range(1, 100);
            int num = Random.Range(0, clips.Length - 1);
            AudioClip clip = clips[num];
            if (chooser <= chanceToPlay)
            {
                float pitch = Random.Range(pitchMin, pitchMax);
                audio.pitch = pitch;
                audio.PlayOneShot(clip);
            }
        }
        /*if (playAnimationOnBounce && animTimer <= 0f)
        {
            animator.Play(animationName);
            animator.PlayFromFrame(0);
            animTimer = animPause;
        }
        if (sendFSMEvent && (bool)fsm)
        {
            fsm.SendEvent("BOUNCE");
        }*/
        if (this.OnBounce != null)
        {
            this.OnBounce();
        }
    }

    public void StopBounce()
    {
        bouncing = false;
    }

    public void StartBounce()
    {
        bouncing = true;
    }

    public void SetBounceFactor(float value)
    {
        bounceFactor = value;
    }

    public void SetBounceAnimation(bool set)
    {
        playAnimationOnBounce = set;
    }
}
