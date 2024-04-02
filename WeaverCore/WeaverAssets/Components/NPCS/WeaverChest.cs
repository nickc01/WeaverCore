using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Settings;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// WeaverCore's implementation of chests
    /// </summary>
    public class WeaverChest : MonoBehaviour
    {
        [Header("Geo")]
        [SerializeField]
        [Tooltip("The amount of small geo to spawn")]
        protected int smallGeo = 0;

        [SerializeField]
        [Tooltip("The amount of medium geo to spawn")]
        protected int mediumGeo = 0;

        [SerializeField]
        [Tooltip("The amount of large geo to spawn")]
        protected int largeGeo = 0;


        [Header("Persistent Data")]
        [SerializeField]
        [Tooltip("The save specific settings object used to persistently store if the chest was opened")]
        protected SaveSpecificSettings chestSaveSettings;

        [SerializeField]
        [Tooltip("The name of the boolean field in the Save Specific Settings that stores whether the chest has already been opened")]
        [SaveSpecificFieldName(typeof(bool), nameof(chestSaveSettings))]
        protected string chestOpenedSaveField;

        [Header("Effects")]
        [SerializeField]
        System.Collections.Generic.List<AudioClip> chestOpenSounds = new System.Collections.Generic.List<AudioClip>();

        public UnityEvent OnChestOpen;

        bool _opened_internal = false;

        protected virtual void Awake()
        {
            StartCoroutine(MainRoutine());
        }

        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

        WeaverAnimationPlayer _mainAnimator;
        public WeaverAnimationPlayer MainAnimator => _mainAnimator ??= GetComponent<WeaverAnimationPlayer>();

        public bool StruckByNail { get; private set; }

        IEnumerator MainRoutine()
        {
            yield return null;

            var bouncer = transform.Find("Bouncer");
            var openedObj = transform.Find("Opened");
            var itemHolder = transform.Find("PUT ITEMS IN HERE");
            var idleGlow = transform.Find("Idle Glow");
            var openEffects = transform.Find("Open Effects");

            if (IsOpened)
            {
                OnChestOpen?.Invoke();
                transform.GetComponent<Collider2D>().enabled = false;
                for (int i = 0; i < itemHolder.childCount; i++)
                {
                    itemHolder.GetChild(i).gameObject.SetActive(true);
                }
                idleGlow.GetComponent<ParticleSystem>().Stop();
                MainRenderer.enabled = false;
                openedObj.gameObject.SetActive(true);
                bouncer.gameObject.SetActive(false);
            }

            yield return new WaitUntil(() => StruckByNail);

            foreach (var sound in chestOpenSounds)
            {
                WeaverAudio.PlayAtPoint(sound, transform.position);
            }

            OnChestOpen?.Invoke();

            GameManager.instance.FreezeMoment(1);

            idleGlow.GetComponent<ParticleSystem>().Stop();
            CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);
            IsOpened = true;

            Pooling.Instantiate(EffectAssets.NailStrikePrefab, transform.position, Quaternion.identity);
            yield return MainAnimator.PlayAnimationTillDone("Open");

            openEffects.gameObject.SetActive(true);
            for (int i = 0; i < itemHolder.childCount; i++)
            {
                itemHolder.GetChild(i).gameObject.SetActive(true);
            }
            //TODO - FLING GEO

            WeaverGeo.FlingSmall(smallGeo, transform.position);
            WeaverGeo.FlingMedium(mediumGeo, transform.position);
            WeaverGeo.FlingLarge(largeGeo, transform.position);

            transform.GetComponent<Collider2D>().enabled = false;
            MainRenderer.enabled = false;
            openedObj.gameObject.SetActive(true);
            bouncer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns true if the chest is already opened. Returns false if the chest is closed
        /// </summary>
        public virtual bool IsOpened
        {
            get
            {
                if (chestSaveSettings == null)
                {
                    return _opened_internal;
                }
                else
                {
                    return chestSaveSettings.TryGetFieldValue<bool>(chestOpenedSaveField, out var result) && result;
                }
            }
            protected set
            {
                if (chestSaveSettings == null)
                {
                    _opened_internal = value;
                }
                else
                {
                    chestSaveSettings.SetFieldValue(chestOpenedSaveField, value);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsOpened && collision.tag == "Nail Attack")
            {
                StruckByNail = true;
            }
        }

        /// <summary>
        /// Opens up the chest
        /// </summary>
        public void OpenChest()
        {
            StruckByNail = true;
        }

        /// <summary>
        /// Called when the chest is opened
        /// </summary>
        protected virtual void OnOpen()
        {

        }
    }
}
