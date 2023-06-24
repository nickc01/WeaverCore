using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class DarknessRegion : MonoBehaviour
    {
        [SerializeField]
        int darknessWithinRegion = -1;


        int previousDarkness;

        bool playerInRegion = false;

        GameObject vignette;
        SceneManager sceneManager;

        private void Awake()
        {

            gameObject.layer = LayerMask.NameToLayer("Hero Detector");
        }

        private void Start()
        {
            sceneManager = GameObject.FindObjectOfType<SceneManager>();
            vignette = GameObject.Find("Vignette");

            if (sceneManager != null)
            {
                previousDarkness = sceneManager.GetDarknessLevel();
            }

            StartCoroutine(DarknessRoutine());
        }

        IEnumerator DarknessRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => playerInRegion);

                HeroController.instance.SetDarkness(darknessWithinRegion);
                if (vignette != null)
                {
                    PlayMakerUtilities.SetFsmInt(vignette, "Darkness Control", "Darkness Level", darknessWithinRegion);

                    EventManager.SendEventToGameObject("SCENE RESET", vignette, gameObject);
                }

                yield return new WaitUntil(() => !playerInRegion);

                HeroController.instance.SetDarkness(previousDarkness);
                if (vignette != null)
                {
                    PlayMakerUtilities.SetFsmInt(vignette, "Darkness Control", "Darkness Level", previousDarkness);

                    EventManager.SendEventToGameObject("SCENE RESET", vignette, gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            playerInRegion = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            playerInRegion = false;
        }
    }
}
