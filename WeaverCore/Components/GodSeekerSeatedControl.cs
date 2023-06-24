using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    public class GodSeekerSeatedControl : MonoBehaviour
	{
        public Transform Target { get; set; } = null;


        WeaverAnimationPlayer animator;
        SpriteRenderer spriteRenderer;

        float rightX;
        float leftX;

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

        IEnumerator LookUpRoutine()
        {
            yield return new WaitForSeconds(0.25f);
            animator.PlayAnimation("LookUp");
        }

        IEnumerator MainRoutine()
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

        IEnumerator LeftRoutine()
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

        IEnumerator RightRoutine()
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
