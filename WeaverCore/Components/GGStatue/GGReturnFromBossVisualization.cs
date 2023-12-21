using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Internal;

namespace WeaverCore.Components.GGStatue
{
    /// <summary>
    /// Handles the visualization when returning from a boss encounter.
    /// </summary>
    public class GGReturnFromBossVisualization : MonoBehaviour
    {
        [SerializeField]
        GameObject inspect;

        /// <summary>
        /// Called when the object is awakened.
        /// </summary>
        private void Awake()
        {
            if (HeroController.instance.isHeroInPosition)
            {
                StartCoroutine(VisualizationRoutine());
            }
            else
            {
                HeroController.instance.heroInPosition += Instance_heroInPosition;
            }
        }

        private void Instance_heroInPosition(bool forceDirect)
        {
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
            StartCoroutine(VisualizationRoutine());
        }

        /// <summary>
        /// Coroutine for the visualization routine.
        /// </summary>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        IEnumerator VisualizationRoutine()
        {
            var doorEntry = gameObject.name;

            var doorEntered = HeroController.instance.GetEntryGateName();

            if (doorEntry == doorEntered)
            {
                PlayerData.instance.SetString("bossReturnEntryGate", "");
            }
            else
            {
                yield break;
            }

            inspect.SetActive(false);

            HeroController.instance.ClearMPSendEvents();

            EventRegister.SendEvent("K HATCHLING END");

            yield return new WaitForSeconds(0.25f);

            GameObject transitions = null;

            if (GG_Internal.ggBattleTransitions != null)
            {
                if (transitions == null)
                {
                    transitions = GameObject.Instantiate(GG_Internal.ggBattleTransitions);
                }

                EventManager.SendEventToGameObject("GG TRANSITION OUT INSTANT", transitions, gameObject);
            }

            var hudCamera = GameObject.FindObjectOfType<HUDCamera>()?.gameObject;
            if (hudCamera != null)
            {
                var hudCanvas = hudCamera.transform.Find("Hud Canvas")?.gameObject;

                if (hudCanvas != null)
                {
                    EventManager.SendEventToGameObject("IN", hudCanvas, gameObject);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (transitions != null)
            {
                EventManager.SendEventToGameObject("GG TRANSITION IN STATUE", transitions, gameObject);
            }

            yield return new WaitForSeconds(1f);

            inspect.SetActive(true);

            yield break;
        }
    }
}