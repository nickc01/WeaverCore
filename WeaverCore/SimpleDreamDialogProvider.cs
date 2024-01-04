using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore
{
    /// <summary>
    /// Used to map a convo title with a set of dreamnail messages
    /// </summary>
    public class SimpleDreamDialogProvider : MonoBehaviour
	{
        [SerializeField]
        string convoTitle;

        [SerializeField]
        List<string> dreamnailDialogs = new List<string>();

        private void OnEnable()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private void OnDisable()
        {
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (key.StartsWith($"{convoTitle}_"))
            {
                if (int.TryParse(key.Replace($"{convoTitle}_", ""), out var number))
                {
                    if (number > 0 && number <= dreamnailDialogs.Count)
                    {
                        return dreamnailDialogs[number - 1];
                    }
                }
            }

            return orig;
        }
    }
}
