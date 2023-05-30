using Modding;
using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public abstract class DroppedItem : MonoBehaviour
	{
        [SerializeField]
        SaveSpecificSettings settingsStorage;

        [SerializeField]
        [Tooltip("The field in Settings Storage that will store whether or not this item has already been collected. Leave this field empty if this item can always be collected.")]
        string settingsField;

        [field: SerializeField]
        [field: Tooltip("If set to false, then this item will not spawn in the world")]
        public bool ItemActive { get; set; } = true;

        [field: SerializeField]
        [field: Tooltip("If set to true, the item will start by flinging itself upwards")]
        public bool FlingOnStart { get; set; } = true;

        [field: SerializeField]
        [field: Tooltip("The speed range the object will be flung at")]
        public Vector2 flingSpeedMinMax = new Vector2(20,20);

        [field: SerializeField]
        [field: Tooltip("The angle range the object will be flung at")]
        public Vector2 flingAngleMinMax = new Vector2(82, 86);

        InspectRegion inspectRegion;

        Rigidbody2D rb;

        public Rigidbody2D RB => rb ??= GetComponent<Rigidbody2D>();

        protected virtual void Awake()
        {
            inspectRegion = GetComponentInChildren<InspectRegion>();
            if (inspectRegion == null)
            {
                throw new Exception("A dropped item requires an inspect region");
            }
            inspectRegion.Inspectable = false;

            inspectRegion.OnInspect.AddListener(GiveItem);

            StartCoroutine(DroppedItemRoutine());
        }

        IEnumerator DroppedItemRoutine()
        {
            transform.rotation = Quaternion.identity;

            if (!ItemActive || (settingsStorage.HasField<bool>(settingsField) && settingsStorage.GetFieldValue<bool>(settingsField))) {
                Destroy(gameObject);
                yield break;
            }

            if (FlingOnStart)
            {
                var particleTrail = GetComponentInChildren<ParticleSystem>();
                particleTrail.Play();
                RB.gravityScale = 0.85f;

                float speed = flingSpeedMinMax.RandomInRange();
                float angle = flingAngleMinMax.RandomInRange();
                Vector2 velocity = new Vector2(speed * Mathf.Cos(angle * Mathf.Deg2Rad), speed * Mathf.Sin(angle * Mathf.Deg2Rad));
                RB.velocity = velocity;

                //yield return new WaitUntil(() => RB.velocity.magnitude < 1f);


                float idleCounter = 0f;

                while (true)
                {
                    if (RB.velocity.y == 0f)
                    {
                        idleCounter += Time.deltaTime;
                    }
                    if (idleCounter >= 0.25f)
                    {
                        break;
                    }
                    yield return null;
                }

                RB.gravityScale = 0f;
                RB.velocity = default;
                particleTrail.Stop();
            }

            if (gameObject.TryGetComponent<ObjectBounce>(out var bouncer))
            {
                bouncer.StopBounce();
            }

            inspectRegion.Inspectable = true;
        }

        public abstract void GiveItem();

        public void GiveCharm(int charmID)
        {
            PlayerData.instance.SetBool($"gotCharm_{charmID}", true);
            GameManager.instance.StoryRecord_acquired($"gotCharm_{charmID}");
            GameManager.instance.IncrementPlayerDataInt("charmsOwned");

            if (!GameManager.instance.GetPlayerDataBool("hasCharm"))
            {
                PlayerData.instance.SetBool("hasCharm", true);
            }

            GetCharmMessage.Spawn(charmID);

            GetComponent<SpriteRenderer>().enabled = false;

            ItemActive = false;

            if (settingsStorage.HasField<bool>(settingsField))
            {
                settingsStorage.SetFieldValue(settingsField, true);
            }
        }

        /*IEnumerator Finish()
        {
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                //ABORT IF HERO IS DAMAGED
                yield return null;
            }
        }*/
    }
}
