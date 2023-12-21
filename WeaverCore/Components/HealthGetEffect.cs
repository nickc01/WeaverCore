using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to play effects when the player picks up blue health pickups
    /// </summary>
    public class HealthGetEffect : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("An extra pause before the health get effect activates")]
        float pause = 0f;

        [SerializeField]
        [Tooltip("When set to true, this will set the localPosition of the effect to the configured initPosition")]
        bool reposition = false;

        [SerializeField]
        [Tooltip("The init position used when \"reposition\" is set to true")]
        Vector2 initPosition = default;

        [SerializeField]
        [Tooltip("When set to true, this will set the Z localRotation of the effect to the configured initRotation")]
        bool resetRotation = false;

        [SerializeField]
        [Tooltip("The init rotation used when \"resetRotation\" is set to true")]
        float initRotation = default;

        [SerializeField]
        [Tooltip("If set to true, then the effect will be unparented")]
        bool unparent = false;


        Transform storedParent = null;
        Vector3 storedLocalScale;

        [NonSerialized]
        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

        [NonSerialized]
        WeaverAnimationPlayer _animator;
        public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

        private void OnEnable()
        {
            gameObject.ActivateAllChildren(true);
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            if (pause > 0)
            {
                MainRenderer.enabled = false;
                yield return new WaitForSeconds(pause);
            }
            MainRenderer.enabled = true;
            if (reposition)
            {
                transform.localPosition = initPosition;
            }

            if (resetRotation)
            {
                transform.localRotation = Quaternion.Euler(0f,0f,initRotation);
            }

            if (unparent)
            {
                storedLocalScale = transform.localScale;
                var worldScale = transform.lossyScale;
                storedParent = transform.parent;
                transform.parent = null;
                transform.localScale = worldScale;
            }
            if (Animator.PlayingGUID == default)
            {
                Animator.PlayAnimation("Health Get");
            }

            yield return Animator.WaitforClipToFinish();

            if (unparent)
            {
                transform.parent = storedParent;
                transform.localScale = storedLocalScale;
            }

            gameObject.ActivateAllChildren(false);
            StopAllCoroutines();
        }
    }
}