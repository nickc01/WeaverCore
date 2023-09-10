using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Breakable : MonoBehaviour//, IHitResponder
{
    [Serializable]
    public class FlingObject
    {
        public GameObject referenceObject;

        [Space]
        public int spawnMin;

        public int spawnMax;

        public float speedMin;

        public float speedMax;

        public float angleMin;

        public float angleMax;

        public Vector2 originVariation;

        public FlingObject()
        {
            spawnMin = 25;
            spawnMax = 30;
            speedMin = 9f;
            speedMax = 20f;
            angleMin = 20f;
            angleMax = 160f;
            originVariation = new Vector2(0.5f, 0.5f);
        }

        public void Fling(Vector3 origin)
        {
            if (!referenceObject)
            {
                return;
            }
            int num = UnityEngine.Random.Range(spawnMin, spawnMax + 1);
            for (int i = 0; i < num; i++)
            {
                //GameObject gameObject = referenceObject.Spawn();
                GameObject gameObject = GameObject.Instantiate(referenceObject, Vector3.zero, Quaternion.identity);
                if ((bool)gameObject)
                {
                    gameObject.transform.position = origin + new Vector3(UnityEngine.Random.Range(0f - originVariation.x, originVariation.x), UnityEngine.Random.Range(0f - originVariation.y, originVariation.y), 0f);
                    float num2 = UnityEngine.Random.Range(speedMin, speedMax);
                    float num3 = UnityEngine.Random.Range(angleMin, angleMax);
                    float x = num2 * Mathf.Cos(num3 * ((float)Math.PI / 180f));
                    float y = num2 * Mathf.Sin(num3 * ((float)Math.PI / 180f));
                    Vector2 force = new Vector2(x, y);
                    Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
                    if ((bool)component)
                    {
                        component.AddForce(force, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    private Collider2D bodyCollider;

    [Tooltip("Renderer which presents the undestroyed object.")]
    [SerializeField]
    private Renderer wholeRenderer;

    [Tooltip("List of child game objects which also represent the whole object.")]
    [SerializeField]
    private GameObject[] wholeParts;

    [Tooltip("List of child game objects which represent remnants that remain static after destruction.")]
    [SerializeField]
    private GameObject[] remnantParts;

    [SerializeField]
    private List<GameObject> debrisParts;

    [SerializeField]
    private float angleOffset = -60f;

    [Tooltip("Breakables behind this threshold are inert.")]
    [SerializeField]
    private float inertBackgroundThreshold;

    [Tooltip("Breakables in front of this threshold are inert.")]
    [SerializeField]
    private float inertForegroundThreshold;

    [Tooltip("Breakable effects are spawned at this offset.")]
    [SerializeField]
    private Vector3 effectOffset;

    [HideInInspector]
    [Tooltip("Prefab to spawn for audio.")]
    [SerializeField]
    private AudioSource audioSourcePrefab;

    [Tooltip("Table of audio clips to play upon break.")]
    [SerializeField]
    private AudioEvent breakAudioEvent;

    //[Tooltip("Table of audio clips to play upon break.")]
    //[SerializeField]
    //private RandomAudioClipTable breakAudioClipTable;

    [Tooltip("Prefab to spawn when hit from a non-down angle.")]
    [SerializeField]
    private Transform dustHitRegularPrefab;

    [Tooltip("Prefab to spawn when hit from a down angle.")]
    [SerializeField]
    private Transform dustHitDownPrefab;

    [Tooltip("Prefab to spawn when hit from a down angle.")]
    [SerializeField]
    private float flingSpeedMin;

    [Tooltip("Prefab to spawn when hit from a down angle.")]
    [SerializeField]
    private float flingSpeedMax;

    [Tooltip("Strike effect prefab to spawn.")]
    [SerializeField]
    private Transform strikeEffectPrefab;

    [Tooltip("Nail hit prefab to spawn.")]
    [SerializeField]
    private Transform nailHitEffectPrefab;

    [Tooltip("Spell hit effect prefab to spawn.")]
    [SerializeField]
    private Transform spellHitEffectPrefab;

    [Tooltip("Legacy flag that was set but has always been broken but is no longer used?")]
    [SerializeField]
    private bool preventParticleRotation;

    [Tooltip("Object to send HIT event to.")]
    [SerializeField]
    private GameObject hitEventReciever;

    [Tooltip("Forward break effect to sibling FSMs.")]
    [SerializeField]
    private bool forwardBreakEvent;

    [Space]
    public Probability.ProbabilityGameObject[] containingParticles;

    public FlingObject[] flingObjectRegister;

    private bool isBroken;

    protected void Reset()
    {
        wholeRenderer = gameObject.GetComponent<Renderer>();

        debrisParts = gameObject.GetComponentsInChildren<ParticleSystem>(true).Where(p => p.gameObject != gameObject).Select(p => p.gameObject).ToList();

        var remnants = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.ToLower().Contains("base"))
            {
                remnants.Add(child.gameObject);
            }
        }

        remnantParts = remnants.ToArray();


        inertBackgroundThreshold = 1f;
        inertForegroundThreshold = -1f;
        effectOffset = new Vector3(0f, 0.5f, 0f);
        flingSpeedMin = 10f;
        flingSpeedMax = 17f;
    }

    protected void Awake()
    {
        bodyCollider = GetComponent<Collider2D>();
        /*PersistentBoolItem component = GetComponent<PersistentBoolItem>();
        if (!(component != null))
        {
            return;
        }
        component.OnGetSaveState += delegate (ref bool val)
        {
            val = isBroken;
        };
        component.OnSetSaveState += delegate (bool val)
        {
            isBroken = val;
            if (isBroken)
            {
                SetStaticPartsActivation(broken: true);
            }
        };*/
    }

    protected void Start()
    {
        CreateAdditionalDebrisParts(debrisParts);
        float z = base.transform.position.z;
        if (z > inertBackgroundThreshold || z < inertForegroundThreshold)
        {
            BoxCollider2D component = GetComponent<BoxCollider2D>();
            if (component != null)
            {
                component.enabled = false;
            }
            UnityEngine.Object.Destroy(this);
            return;
        }
        for (int i = 0; i < remnantParts.Length; i++)
        {
            GameObject gameObject = remnantParts[i];
            if (gameObject != null && gameObject.activeSelf)
            {
                gameObject.SetActive(value: false);
            }
        }
        angleOffset *= Mathf.Sign(base.transform.localScale.x);
    }

    protected virtual void CreateAdditionalDebrisParts(List<GameObject> debrisParts)
    {
    }

    public void BreakSelf()
    {
        if (!isBroken)
        {
            Break(70f, 110f, 1f);
        }
    }

    protected void ManageHit(float direction, float magnitudeMultiplier, int attackType, GameObject source)
    {
        if (isBroken)
        {
            return;
        }
        float impactAngle = direction;
        float num = magnitudeMultiplier;
        if (attackType == 2)
        {
            GameObject.Instantiate(spellHitEffectPrefab, base.transform.position, Quaternion.identity).SetPositionZ(0.0031f);
            //spellHitEffectPrefab.Spawn(base.transform.position).SetPositionZ(0.0031f);
        }
        else
        {
            if (attackType != 0 && attackType != 1)
            {
                impactAngle = 90f;
                num = 1f;
            }
            //strikeEffectPrefab.Spawn(base.transform.position);
            GameObject.Instantiate(strikeEffectPrefab, base.transform.position, Quaternion.identity);
            Vector3 position = (source.transform.position + base.transform.position) * 0.5f;
            SpawnNailHitEffect(nailHitEffectPrefab, position, impactAngle);
        }
        int cardinalDirection = GetCardinalDirection(direction);
        Transform transform = dustHitRegularPrefab;
        float flingAngleMin;
        float flingAngleMax;
        Vector3 euler;
        switch (cardinalDirection)
        {
            case 2:
                angleOffset *= -1f;
                flingAngleMin = 120f;
                flingAngleMax = 160f;
                euler = new Vector3(180f, 90f, 270f);
                break;
            case 0:
                flingAngleMin = 30f;
                flingAngleMax = 70f;
                euler = new Vector3(0f, 90f, 270f);
                break;
            case 1:
                angleOffset = 0f;
                flingAngleMin = 70f;
                flingAngleMax = 110f;
                num *= 1.5f;
                euler = new Vector3(270f, 90f, 270f);
                break;
            default:
                angleOffset = 0f;
                flingAngleMin = 160f;
                flingAngleMax = 380f;
                transform = dustHitDownPrefab;
                euler = new Vector3(-72.5f, -180f, -180f);
                break;
        }
        if (transform != null)
        {
            //transform.Spawn(base.transform.position + effectOffset, Quaternion.Euler(euler));
            GameObject.Instantiate(transform, base.transform.position + effectOffset, Quaternion.Euler(euler));
        }
        Break(flingAngleMin, flingAngleMax, num);
    }

    /*public void Hit(HitInstance damageInstance)
    {
        if (isBroken)
        {
            return;
        }
        float impactAngle = damageInstance.Direction;
        float num = damageInstance.MagnitudeMultiplier;
        if (damageInstance.AttackType == AttackTypes.Spell)
        {
            GameObject.Instantiate(spellHitEffectPrefab, base.transform.position, Quaternion.identity).SetPositionZ(0.0031f);
            //spellHitEffectPrefab.Spawn(base.transform.position).SetPositionZ(0.0031f);
        }
        else
        {
            if (damageInstance.AttackType != 0 && damageInstance.AttackType != AttackTypes.Generic)
            {
                impactAngle = 90f;
                num = 1f;
            }
            //strikeEffectPrefab.Spawn(base.transform.position);
            GameObject.Instantiate(strikeEffectPrefab, base.transform.position, Quaternion.identity);
            Vector3 position = (damageInstance.Source.transform.position + base.transform.position) * 0.5f;
            SpawnNailHitEffect(nailHitEffectPrefab, position, impactAngle);
        }
        int cardinalDirection = GetCardinalDirection(damageInstance.Direction);
        Transform transform = dustHitRegularPrefab;
        float flingAngleMin;
        float flingAngleMax;
        Vector3 euler;
        switch (cardinalDirection)
        {
            case 2:
                angleOffset *= -1f;
                flingAngleMin = 120f;
                flingAngleMax = 160f;
                euler = new Vector3(180f, 90f, 270f);
                break;
            case 0:
                flingAngleMin = 30f;
                flingAngleMax = 70f;
                euler = new Vector3(0f, 90f, 270f);
                break;
            case 1:
                angleOffset = 0f;
                flingAngleMin = 70f;
                flingAngleMax = 110f;
                num *= 1.5f;
                euler = new Vector3(270f, 90f, 270f);
                break;
            default:
                angleOffset = 0f;
                flingAngleMin = 160f;
                flingAngleMax = 380f;
                transform = dustHitDownPrefab;
                euler = new Vector3(-72.5f, -180f, -180f);
                break;
        }
        if (transform != null)
        {
            //transform.Spawn(base.transform.position + effectOffset, Quaternion.Euler(euler));
            GameObject.Instantiate(transform, base.transform.position + effectOffset, Quaternion.Euler(euler));
        }
        Break(flingAngleMin, flingAngleMax, num);
    }*/

    static int NegSafeMod(int val, int len)
    {
        return (val % len + len) % len;
    }

    static int GetCardinalDirection(float degrees)
    {
        return NegSafeMod(Mathf.RoundToInt(degrees / 90f), 4);
    }

    private static Transform SpawnNailHitEffect(Transform nailHitEffectPrefab, Vector3 position, float impactAngle)
    {
        if (nailHitEffectPrefab == null)
        {
            return null;
        }
        int cardinalDirection = GetCardinalDirection(impactAngle);
        float y = 1.5f;
        float minInclusive;
        float maxInclusive;
        switch (cardinalDirection)
        {
            case 3:
                minInclusive = 250f;
                maxInclusive = 290f;
                break;
            case 1:
                minInclusive = 70f;
                maxInclusive = 110f;
                break;
            default:
                minInclusive = 340f;
                maxInclusive = 380f;
                break;
        }
        float x = ((cardinalDirection == 2) ? (-1.5f) : 1.5f);
        //Transform obj = nailHitEffectPrefab.Spawn(position);
        Transform obj = GameObject.Instantiate(nailHitEffectPrefab, position, Quaternion.identity);
        Vector3 eulerAngles = obj.eulerAngles;
        eulerAngles.z = UnityEngine.Random.Range(minInclusive, maxInclusive);
        obj.eulerAngles = eulerAngles;
        Vector3 localScale = obj.localScale;
        localScale.x = x;
        localScale.y = y;
        obj.localScale = localScale;
        return obj;
    }

    private void SetStaticPartsActivation(bool broken)
    {
        if (wholeRenderer != null)
        {
            wholeRenderer.enabled = !broken;
        }
        for (int i = 0; i < wholeParts.Length; i++)
        {
            GameObject gameObject = wholeParts[i];
            if (gameObject == null)
            {
                Debug.LogErrorFormat(this, "Unassigned whole part in {0}", this);
            }
            else
            {
                gameObject.SetActive(!broken);
            }
        }
        for (int j = 0; j < remnantParts.Length; j++)
        {
            GameObject gameObject2 = remnantParts[j];
            if (gameObject2 == null)
            {
                Debug.LogErrorFormat(this, "Unassigned remnant part in {0}", this);
            }
            else
            {
                gameObject2.SetActive(broken);
            }
        }
        if (hitEventReciever != null)
        {
            //TODO
            //FSMUtility.SendEventToGameObject(hitEventReciever, "HIT");
        }
        if ((bool)bodyCollider)
        {
            bodyCollider.enabled = !broken;
        }
    }

    public void Break(float flingAngleMin, float flingAngleMax, float impactMultiplier)
    {
        if (isBroken)
        {
            return;
        }
        SetStaticPartsActivation(broken: true);
        for (int i = 0; i < debrisParts.Count; i++)
        {
            GameObject gameObject = debrisParts[i];
            if (gameObject == null)
            {
                Debug.LogErrorFormat(this, "Unassigned debris part in {0}", this);
                continue;
            }
            gameObject.SetActive(value: true);
            gameObject.transform.SetRotationZ(gameObject.transform.localEulerAngles.z + angleOffset);
            Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
            if (component != null)
            {
                float num = UnityEngine.Random.Range(flingAngleMin, flingAngleMax);
                Vector2 vector = new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f)));
                float num2 = UnityEngine.Random.Range(flingSpeedMin, flingSpeedMax) * impactMultiplier;
                component.velocity = vector * num2;
            }
        }
        if (containingParticles.Length != 0)
        {
            GameObject gameObject2 = Probability.GetRandomGameObjectByProbability(containingParticles);
            if ((bool)gameObject2)
            {
                if (gameObject2.transform.parent != base.transform)
                {
                    FlingObject flingObject = null;
                    FlingObject[] array = flingObjectRegister;
                    foreach (FlingObject flingObject2 in array)
                    {
                        if (flingObject2.referenceObject == gameObject2)
                        {
                            flingObject = flingObject2;
                            break;
                        }
                    }
                    if (flingObject != null)
                    {
                        flingObject.Fling(base.transform.position);
                    }
                    else
                    {
                        //gameObject2 = gameObject2.Spawn(base.transform.position);
                        gameObject2 = GameObject.Instantiate(gameObject2, transform.position, Quaternion.identity);
                    }
                }
                gameObject2.SetActive(value: true);
            }
        }
        breakAudioEvent.SpawnAndPlayOneShot(audioSourcePrefab, base.transform.position);
        //breakAudioClipTable.SpawnAndPlayOneShot(audioSourcePrefab, base.transform.position);

        //var selectedSound = 

        //var playSoundMethod = WeaverTypeHelpers.GetWeaverMethod("WeaverCore.WeaverAudio", "PlayAtPoint");

        if (hitEventReciever != null)
        {
            //TODO
            //FSMUtility.SendEventToGameObject(hitEventReciever, "HIT");
        }
        if (forwardBreakEvent)
        {
            //TODO
            //FSMUtility.SendEventToGameObject(base.gameObject, "BREAK");
        }
        GameObject gameObject3 = GameObject.FindGameObjectWithTag("CameraParent");
        if (gameObject3 != null)
        {
            //TODO
            /*PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(gameObject3, "CameraShake");
            if (playMakerFSM != null)
            {
                playMakerFSM.SendEvent("EnemyKillShake");
            }*/
        }
        bodyCollider.enabled = false;
        isBroken = true;
    }
}
