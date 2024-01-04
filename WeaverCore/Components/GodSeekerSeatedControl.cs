using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// This class controls the Godseeker, and makes Godseeker look left or right towards a specified target.
    /// </summary>
    public class GodSeekerSeatedControl : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the target for Godseeker to look towards.
        /// </summary>
        public Transform Target { get; set; } = null;

        private WeaverAnimationPlayer animator;
        private SpriteRenderer spriteRenderer;

        private float rightX;
        private float leftX;

        private void Awake()
        {
            animator = GetComponent<WeaverAnimationPlayer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            var eventRegister = GetComponent<EventRegister>();

            if (eventRegister == null)
            {
                eventRegister = gameObject.AddComponent<EventRegister>();
            }

            eventRegister.SwitchEvent("LOOK UP");
            eventRegister.OnReceivedEvent += EventRegister_OnReceivedEvent;

            StartCoroutine(MainRoutine());
        }

        private void EventRegister_OnReceivedEvent()
        {
            StopAllCoroutines();
            StartCoroutine(LookUpRoutine());
        }

        /// <summary>
        /// Coroutine to make Godseeker look up.
        /// </summary>
        private IEnumerator LookUpRoutine()
        {
            yield return new WaitForSeconds(0.25f);
            animator.PlayAnimation("LookUp");
        }

        /// <summary>
        /// Main coroutine for controlling Godseeker's behavior.
        /// </summary>
        private IEnumerator MainRoutine()
        {
            yield return new WaitUntil(() => Player.Player1 != null);

            if (Target == null)
            {
                Target = Player.Player1.transform;
            }

            yield return new WaitForSeconds(0.5f);

            rightX = transform.position.x + 4f;
            leftX = transform.position.x - 4f;

            while (true)
            {
                if (Target.GetPositionX() >= rightX)
                {
                    yield return RightRoutine();
                }
                else if (Target.GetPositionX() <= leftX)
                {
                    yield return LeftRoutine();
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Coroutine for making Godseeker look left.
        /// </summary>
        private IEnumerator LeftRoutine()
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            animator.PlayAnimation("LookL");

            while (true)
            {
                if (Target.GetPositionX() > leftX)
                {
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    animator.PlayAnimation("ReturnL");
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Coroutine for making Godseeker look right.
        /// </summary>
        private IEnumerator RightRoutine()
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            animator.PlayAnimation("LookR");

            while (true)
            {
                if (Target.GetPositionX() < rightX)
                {
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    animator.PlayAnimation("ReturnR");
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}