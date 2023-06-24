using Modding;
using UnityEngine;

namespace WeaverCore.Components
{
    [RequireComponent(typeof(WeaverBossStatue))]
    public class BossStatueLanguageProvider : MonoBehaviour
    {
        /*[Header("Boss Name")]
        [Space]
        [SerializeField]
        string nameKey;

        [SerializeField]
        string nameSheet;*/

        [SerializeField]
        string nameValue;

        /*[Header("Boss Description")]
        [Space]
        [SerializeField]
        string descKey;

        [SerializeField]
        string descSheet;*/

        [SerializeField]
        string descValue;


        /*[Header("Dream Boss Name")]
        [Space]
        [SerializeField]
        string dreamNameKey;

        [SerializeField]
        string dreamNameSheet;*/

        [SerializeField]
        string dreamNameValue;

        /*[Header("Dream Boss Description")]
        [Space]
        [SerializeField]
        string dreamDescKey;

        [SerializeField]
        string dreamDescSheet;*/

        [SerializeField]
        string dreamDescValue;

        [SerializeField]
        string lockedText;


        BossStatue statue;
        DialogueInspectRegion lockedInspect;

        private void Awake()
        {
            statue = GetComponent<BossStatue>();

            lockedInspect = transform.Find("Inspect_Locked").GetComponent<DialogueInspectRegion>();
        }

        private void OnEnable()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (key == statue.bossDetails.nameKey && sheetTitle == statue.bossDetails.nameSheet)
            {
                return nameValue;
            }
            else if (key == statue.bossDetails.descriptionKey && sheetTitle == statue.bossDetails.descriptionSheet)
            {
                return descValue;
            }
            else if (key == statue.dreamBossDetails.nameKey && sheetTitle == statue.dreamBossDetails.nameSheet)
            {
                return dreamNameValue;
            }
            else if (key == statue.dreamBossDetails.descriptionKey && sheetTitle == statue.dreamBossDetails.descriptionSheet)
            {
                return dreamDescValue;
            }
            else if (key == lockedInspect.TextConvo && sheetTitle == lockedInspect.TextSheet)
            {
                return lockedText;
            }
            else
            {
                return orig;
            }
        }

        private void OnDisable()
        {
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        }

        private void OnDestroy()
        {
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        }
    }
}
