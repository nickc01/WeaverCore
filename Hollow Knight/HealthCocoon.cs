using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCocoon : MonoBehaviour
{
    [Serializable]
    public class FlingPrefab
    {
        public GameObject prefab;

        private List<GameObject> pool = new List<GameObject>();

        public int minAmount;

        public int maxAmount;

        public Vector2 originVariation = new Vector2(0.5f, 0.5f);

        public float minSpeed;

        public float maxSpeed;

        public float minAngle;

        public float maxAngle;

        public void SetupPool(Transform parent)
        {
            if ((bool)prefab)
            {
                pool.Capacity = maxAmount;
                for (int i = pool.Count; i < maxAmount; i++)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent);
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.SetActive(value: false);
                    pool.Add(gameObject);
                }
            }
        }

        public GameObject Spawn()
        {
            foreach (GameObject item in pool)
            {
                if (!item.activeSelf)
                {
                    item.SetActive(value: true);
                    return item;
                }
            }
            return null;
        }
    }

    [Header("Behaviour")]
    public GameObject[] slashEffects;

    public GameObject[] spellEffects;

    public Vector3 effectOrigin = new Vector3(0f, 0.8f, 0f);

    [Space]
    public FlingPrefab[] flingPrefabs;

    [Space]
    public GameObject[] enableChildren;

    public GameObject[] disableChildren;

    public Collider2D[] disableColliders;

    [Space]
    public Rigidbody2D cap;

    public float capHitForce = 10f;

    [Space]
    public AudioClip deathSound;

    private bool activated;

    [Header("Animation")]
    public string idleAnimation = "Cocoon Idle";

    public string sweatAnimation = "Cocoon Sweat";

    public AudioClip moveSound;

    public float waitMin = 2f;

    public float waitMax = 6f;

    private Coroutine animRoutine;

    private AudioSource source;

    //private tk2dSpriteAnimator animator;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        //animator = GetComponent<tk2dSpriteAnimator>();
        /*PersistentBoolItem component = GetComponent<PersistentBoolItem>();
        if (!component)
        {
            return;
        }
        component.OnGetSaveState += delegate (ref bool value)
        {
            value = activated;
        };
        component.OnSetSaveState += delegate (bool value)
        {
            activated = value;
            if (activated)
            {
                SetBroken();
            }
        };*/
    }

    private void Start()
    {
        animRoutine = StartCoroutine(Animate());
        FlingPrefab[] array = flingPrefabs;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetupPool(base.transform);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if ((bool)source && (bool)clip)
        {
            source.PlayOneShot(clip);
        }
    }

    private IEnumerator Animate()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(waitMin, waitMax));
            PlaySound(moveSound);
            /*if ((bool)animator)
            {
                tk2dSpriteAnimationClip clipByName = animator.GetClipByName(sweatAnimation);
                animator.Play(clipByName);
                yield return new WaitForSeconds((float)clipByName.frames.Length / clipByName.fps);
                animator.Play(idleAnimation);
            }*/
        }
    }

    /*bool TryGetHitDirection(Collider2D collision)
    {
        if (collision != null)
        {
            var components = collision.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component.GetType().FullName == "PlayMakerFSM")
                {

                }
            }
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
        {
            return;
        }
        bool flag = false;

        if (collision.tag == "Nail Attack")
        {
            flag = true;

            //TODO
            /*float value = PlayMakerFSM.FindFsmOnGameObject(collision.gameObject, "damages_enemy").FsmVariables.FindFsmFloat("direction").Value;
            float z = 0f;
            Vector2 vector = new Vector2(1.5f, 1.5f);
            if (value < 45f)
            {
                z = UnityEngine.Random.Range(340, 380);
            }
            else if (value < 135f)
            {
                z = UnityEngine.Random.Range(340, 380);
            }
            else if (value < 225f)
            {
                vector.x *= -1f;
                z = UnityEngine.Random.Range(70, 110);
            }
            else if (value < 360f)
            {
                z = UnityEngine.Random.Range(250, 290);
            }*/

            Vector2 vector = new Vector2(-1.5f, 1.5f);
            float z = UnityEngine.Random.Range(70, 110);
            GameObject[] array = slashEffects;
            for (int i = 0; i < array.Length; i++)
            {
                //GameObject obj = array[i].Spawn(base.transform.position + effectOrigin);
                GameObject obj = GameObject.Instantiate(array[i], base.transform.position + effectOrigin, Quaternion.identity);

                //TODO : FIX THIS
                obj.transform.eulerAngles = new Vector3(0f, 0f, z);
                obj.transform.localScale = vector;
            }
        }
        if (collision.tag == "Hero Spell")
        {
            flag = true;
            GameObject[] array = spellEffects;
            for (int i = 0; i < array.Length; i++)
            {
                //GameObject obj2 = array[i].Spawn(base.transform.position + effectOrigin);
                GameObject obj2 = GameObject.Instantiate(array[i], base.transform.position + effectOrigin, Quaternion.identity);
                Vector3 position = obj2.transform.position;
                position.z = 0.0031f;
                obj2.transform.position = position;
            }
        }
        if (flag)
        {
            activated = true;
            GameObject[] array = enableChildren;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetActive(value: true);
            }
            if ((bool)cap)
            {
                cap.gameObject.SetActive(value: true);
                Vector3 vector2 = base.transform.position - collision.transform.position;
                vector2.Normalize();
                cap.AddForce(capHitForce * vector2, ForceMode2D.Impulse);
            }
            FlingPrefab[] array2 = flingPrefabs;
            foreach (FlingPrefab fling in array2)
            {
                FlingObjects(fling);
            }
            PlaySound(deathSound);
            SetBroken();
            GameManager.instance.AddToCocoonList();
            //GameCameras gameCameras = UnityEngine.Object.FindObjectOfType<GameCameras>();
            /*if ((bool)gameCameras)
            {
                
                //gameCameras.cameraShakeFSM.SendEvent("EnemyKillShake");
            }*/
        }
    }

    private void SetBroken()
    {
        StopCoroutine(animRoutine);
        GetComponent<MeshRenderer>().enabled = false;
        GameObject[] array = disableChildren;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetActive(value: false);
        }
        Collider2D[] array2 = disableColliders;
        for (int i = 0; i < array2.Length; i++)
        {
            array2[i].enabled = false;
        }
    }

    private void FlingObjects(FlingPrefab fling)
    {
        if (!fling.prefab)
        {
            return;
        }
        int num = UnityEngine.Random.Range(fling.minAmount, fling.maxAmount + 1);
        Vector2 velocity = default(Vector2);
        for (int i = 1; i <= num; i++)
        {
            GameObject obj = fling.Spawn();
            obj.transform.position += new Vector3(fling.originVariation.x * UnityEngine.Random.Range(-1f, 1f), fling.originVariation.y * UnityEngine.Random.Range(-1f, 1f));
            float num2 = UnityEngine.Random.Range(fling.minSpeed, fling.maxSpeed);
            float num3 = UnityEngine.Random.Range(fling.minAngle, fling.maxAngle);
            float x = num2 * Mathf.Cos(num3 * ((float)Math.PI / 180f));
            float y = num2 * Mathf.Sin(num3 * ((float)Math.PI / 180f));
            velocity.x = x;
            velocity.y = y;
            Rigidbody2D component = obj.GetComponent<Rigidbody2D>();
            if ((bool)component)
            {
                component.velocity = velocity;
            }
        }
    }

    public void SetScuttlerAmount(int amount)
    {
        FlingPrefab[] array = flingPrefabs;
        foreach (FlingPrefab flingPrefab in array)
        {
            if (flingPrefab.prefab.name == "Health Scuttler")
            {
                flingPrefab.minAmount = (flingPrefab.maxAmount = amount);
                flingPrefab.SetupPool(base.transform);
                break;
            }
        }
    }
}
