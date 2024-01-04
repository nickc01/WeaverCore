using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Main class for handling breakable globs in WeaverCore.
    /// </summary>
    public class WeaverGlobControl : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The renderer component for the glob.
        /// </summary>
        public Renderer rend;

        [Space]
        /// <summary>
        /// The minimum scale of the glob.
        /// </summary>
        [Tooltip("The minimum scale of the glob.")]
        public float minScale = 0.6f;

        /// <summary>
        /// The maximum scale of the glob.
        /// </summary>
        [Tooltip("The maximum scale of the glob.")]
        public float maxScale = 1.6f;

        [Space]
        /// <summary>
        /// Animation name for landing.
        /// </summary>
        [Tooltip("Animation name for landing.")]
        public string landAnim = "Glob Land";

        /// <summary>
        /// Animation name for wobbling.
        /// </summary>
        [Tooltip("Animation name for wobbling.")]
        public string wobbleAnim = "Glob Wobble";

        /// <summary>
        /// Animation name for breaking.
        /// </summary>
        [Tooltip("Animation name for breaking.")]
        public string breakAnim = "Glob Break";

        [Space]
        /// <summary>
        /// Prefab for the audio player.
        /// </summary>
        [HideInInspector]
        [Tooltip("Prefab for the audio player.")]
        public AudioSource audioPlayerPrefab;

        /// <summary>
        /// Audio event for the break sound.
        /// </summary>
        [Tooltip("Audio event for the break sound.")]
        public AudioEvent breakSound;

        /// <summary>
        /// Color override for blood.
        /// </summary>
        [Tooltip("Color override for blood.")]
        public Color bloodColorOverride = new Color(1f, 0.537f, 0.188f);

        [Space]
        /// <summary>
        /// Child GameObject for displaying splat.
        /// </summary>
        public GameObject splatChild;

        [Space]
        /// <summary>
        /// Collider for detecting ground.
        /// </summary>
        public Collider2D groundCollider;

        private bool landed;

        private bool broken;

        //private tk2dSpriteAnimator anim;
        WeaverAnimationPlayer anim;

        [HideInInspector]
        [SerializeField]
        AudioClip breakSound_Clip;

        [HideInInspector]
        [SerializeField]
        float breakSound_PitchMin;

        [HideInInspector]
        [SerializeField]
        float breakSound_PitchMax;

        [HideInInspector]
        [SerializeField]
        float breakSound_Volume;

        private void Awake()
        {
            anim = GetComponent<WeaverAnimationPlayer>();
            audioPlayerPrefab = GG_Internal.AudioPlayerPrefab;
        }

        private void OnEnable()
        {
            float num = Random.Range(minScale, maxScale);
            base.transform.localScale = new Vector3(num, num, 1f);
            if ((bool)splatChild)
            {
                splatChild.SetActive(value: false);
            }
            landed = false;
            broken = false;
        }

        private void Start()
        {
            CollisionEnterEvent collision = GetComponent<CollisionEnterEvent>();
            if ((bool)collision)
            {
                collision.OnCollisionEnteredDirectional += delegate (CollisionEnterEvent.Direction direction, Collision2D col)
                {
                    if (!landed)
                    {
                        if (direction == CollisionEnterEvent.Direction.Bottom)
                        {
                            landed = true;
                            collision.doCollisionStay = false;
                            if (CheckForGround())
                            {
                                anim.PlayAnimation(landAnim);
                                var rigidBody = GetComponent<Rigidbody2D>();
                                rigidBody.velocity = rigidBody.velocity.With(x: 0f);
                            }
                            else
                            {
                                StartCoroutine(Break());
                            }
                        }
                        else
                        {
                            collision.doCollisionStay = true;
                        }
                    }
                };
            }
            TriggerEnterEvent componentInChildren = GetComponentInChildren<TriggerEnterEvent>();
            if (!componentInChildren)
            {
                return;
            }
            componentInChildren.OnTriggerEntered += delegate (Collider2D col, GameObject sender)
            {
                if (landed && !broken && col.gameObject.layer == 11)
                {
                    anim.PlayAnimation(wobbleAnim);
                }
            };
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (landed && !broken)
            {
                if (col.tag == "Nail Attack")
                {
                    StartCoroutine(Break());
                }
                else if (col.tag == "HeroBox")
                {
                    anim.PlayAnimation(wobbleAnim);
                }
            }
        }

        private IEnumerator Break()
        {
            broken = true;
#if UNITY_EDITOR
            var soundInstance = WeaverAudio.PlayAtPoint(breakSound.Clip, transform.position, breakSound.Volume);
            soundInstance.AudioSource.pitch = UnityEngine.Random.Range(breakSound.PitchMin, breakSound.PitchMax);
#else
            breakSound.SpawnAndPlayOneShot(audioPlayerPrefab, base.transform.position);
#endif



            //var playMethod = WeaverTypeHelpers.GetWeaverMethod("WeaverCore.WeaverAudio", "PlayAtPoint");

            //playMethod.Invoke(null, new object[] { breakSound, transform.position });
            //WeaverAudio.PlayAtPoint(breakSound)

            //GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position, 4, 5, 5f, 20f, 80f, 100f, bloodColorOverride);
            Blood.SpawnBlood(transform.position, new Blood.BloodSpawnInfo(4,5,5f, 20f, 80f, 100f, bloodColorOverride));
            if ((bool)splatChild)
            {
                splatChild.SetActive(value: true);
            }
            yield return anim.PlayAnimationTillDone(breakAnim);
            if ((bool)rend)
            {
                rend.enabled = false;
            }
            yield break;
        }

        private bool CheckForGround()
        {
            if (!groundCollider)
            {
                return true;
            }
            Vector2 origin = groundCollider.bounds.min;
            Vector2 origin2 = groundCollider.bounds.max;
            float num = origin2.y - origin.y;
            origin.y = origin2.y;
            origin.x += 0.1f;
            origin2.x -= 0.1f;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.down, num + 0.25f, 256);
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin2, Vector2.down, num + 0.25f, 256);
            if (raycastHit2D.collider != null)
            {
                return raycastHit2D2.collider != null;
            }
            return false;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            breakSound_Clip = breakSound.Clip;
            breakSound_PitchMin = breakSound.PitchMin;
            breakSound_PitchMax = breakSound.PitchMax;
            breakSound_Volume = breakSound.Volume;
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            breakSound = new AudioEvent()
            {
                Clip = breakSound_Clip,
                PitchMin = breakSound_PitchMin,
                PitchMax = breakSound_PitchMax,
                Volume = breakSound_Volume
            };
#endif
        }
    }
}
