using Modding;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used for providing simple strings for a Boss Staru's Name and Description
    /// </summary>
    [RequireComponent(typeof(WeaverBossStatue))]
    public class BossStatueLanguageProvider : MonoBehaviour
    {
        /// <summary>
        /// The localized name value for the boss.
        /// </summary>
        [SerializeField]
        [Tooltip("The localized name value for the boss.")]
        string nameValue;

        /// <summary>
        /// The localized description value for the boss.
        /// </summary>
        [SerializeField]
        [Tooltip("The localized description value for the boss.")]
        string descValue;

        /// <summary>
        /// The localized dream boss name value.
        /// </summary>
        [SerializeField]
        [Tooltip("The localized dream boss name value.")]
        string dreamNameValue;

        /// <summary>
        /// The localized dream boss description value.
        /// </summary>
        [SerializeField]
        [Tooltip("The localized dream boss description value.")]
        string dreamDescValue;

        /// <summary>
        /// The locked text displayed when the statue hasn't been unlocked yet.
        /// </summary>
        [SerializeField]
        [Tooltip("The locked text displayed when the statue hasn't been unlocked yet.")]
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

        /// <summary>
        /// Handles the language get hook and returns the localized text based on the provided key and sheet title.
        /// </summary>
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