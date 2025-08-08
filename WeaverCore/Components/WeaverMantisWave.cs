using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverMantisWave : MonoBehaviour
    {
        static WeaverMantisWave _defaultPrefab;

        public static WeaverMantisWave DefaultPrefab
        {
            get
            {
                if (_defaultPrefab == null)
                {
                    _defaultPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Mega Mantis Wave").GetComponent<WeaverMantisWave>();
                }

                return _defaultPrefab;
            }
        }

        [SerializeField]
        List<Vector3> hurtBoxPositions;

        [SerializeField]
        List<Vector3> hurtBoxRotations;

        [SerializeField]
        List<Vector3> hurtBoxScales;

        [SerializeField]
        List<Sprite> slashCoreFrames;

        [SerializeField]
        float slashCoreFPS = 20;

        [SerializeField]
        string slashCoreAnimLoop = "Mantis Loop";

        public float ActiveTime = 5f;

        public bool Ending { get; private set; }

        [NonSerialized]
        WeaverAnimationPlayer slashCore;

        [NonSerialized]
        Transform hurtbox;

        [SerializeField]
        OnDoneBehaviour onDone = OnDoneBehaviour.DestroyOrPool;

        void Awake()
        {
            slashCore = transform.Find("slash_core").GetComponent<WeaverAnimationPlayer>();
            hurtbox = slashCore.transform.Find("hurtbox");
            PlayBeginning();
        }


        void PlayBeginning()
        {
            slashCore.StopCurrentAnimation();
            StopAllCoroutines();
            StartCoroutine(BeginRoutine());
        }

        void PlaySlashLoop()
        {
            StopAllCoroutines();
            slashCore.PlayAnimation(slashCoreAnimLoop);
            StartCoroutine(Wait());
        }

        void PlayEnding()
        {
            if (Ending)
            {
                return;
            }
            Ending = true;
            slashCore.StopCurrentAnimation();
            StopAllCoroutines();
            StartCoroutine(EndRoutine());
        }

        public void End()
        {
            PlayEnding();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(ActiveTime);
            PlayEnding();
        }

        IEnumerator BeginRoutine()
        {
            for (int i = 0; i < hurtBoxPositions.Count; i++)
            {
                hurtbox.localPosition = hurtBoxPositions[i];
                hurtbox.localRotation = Quaternion.Euler(hurtBoxRotations[i]);
                hurtbox.localEulerAngles = hurtBoxScales[i];
                slashCore.SpriteRenderer.sprite = slashCoreFrames[i];
                yield return new WaitForSeconds(1f / slashCoreFPS);
            }
            PlaySlashLoop();
        }

        IEnumerator EndRoutine()
        {
            //for (int i = 0; i < hurtBoxPositions.Count; i++)
            for (int i = hurtBoxPositions.Count - 1; i >= 0; i--)
            {
                hurtbox.localPosition = hurtBoxPositions[i];
                hurtbox.localRotation = Quaternion.Euler(hurtBoxRotations[i]);
                hurtbox.localEulerAngles = hurtBoxScales[i];
                slashCore.SpriteRenderer.sprite = slashCoreFrames[i];
                yield return new WaitForSeconds(1f / slashCoreFPS);
            }

            onDone.DoneWithObject(this);
        }

        public static WeaverMantisWave Spawn(Vector3 position, Vector3 scale, Vector2 velocity, WeaverMantisWave prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);
            if (instance.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = velocity;
            }

            instance.transform.localScale = scale;

            return instance;
        }
    }
}