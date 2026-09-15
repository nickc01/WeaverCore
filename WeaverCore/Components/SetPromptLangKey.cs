using UnityEngine;
using WeaverCore.Assets.Components;

namespace WeaverCore.Components
{
    public class SetPromptLangKey : MonoBehaviour
    {
        [SerializeField]
        string interactLangKey = "INTERACT";

        [SerializeField]
        string sheetTitle = "Prompts";

        void Awake()
        {
            var prompt = GetComponentInChildren<WeaverArrowPrompt>();
            if (prompt != null)
            {
                prompt.SetLabelTextLang(interactLangKey, sheetTitle);
            }
        }
    }
}
