using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to Dreamnailing
    /// </summary>
    public static class DreamnailUtilities
	{
		static DreamnailUtilities_I _impl;

		static DreamnailUtilities_I Impl => _impl ??= ImplFinder.GetImplementation<DreamnailUtilities_I>();

        /// <summary>
        /// Displays an enemy dreamnail message
        /// </summary>
        /// <param name="convoAmount">The amount of possible messages that is associated with this <paramref name="convoTitle"/>. Set to 0 if there is only one message</param>
        /// <param name="convoTitle">The language convo title to get the dreamnail messages from</param>
		public static void DisplayEnemyDreamnailMessage(int convoAmount, string convoTitle, string sheetTitle = "Enemy Dreams")
		{
			Impl.DisplayEnemyDreamMessage(convoAmount, convoTitle, sheetTitle);
        }

        /// <summary>
        /// Displays an enemy dreamnail message
        /// </summary>
        /// <param name="message">The message to display</param>
        public static void DisplayEnemyDreamnailMessage(string message)
        {
            DisplayEnemyDreamnailMessage(new string[] { message });
        }

        /// <summary>
        /// Displays an enemy dreamnail message
        /// </summary>
        /// <param name="possibleOptions">The possible messages to display</param>
        public static void DisplayEnemyDreamnailMessage(params string[] possibleOptions)
        {
            DisplayEnemyDreamnailMessage((IEnumerable<string>)possibleOptions);
        }

        /// <summary>
        /// Displays an enemy dreamnail message
        /// </summary>
        /// <param name="possibleOptions">The possible messages to display</param>
        public static void DisplayEnemyDreamnailMessage(IEnumerable<string> possibleOptions)
        {
            int convoAmount = possibleOptions.Count();
            string convoTitle = Guid.NewGuid().ToString();

            string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
            {
                if (sheetTitle == "Enemy Dreams" && key.StartsWith(convoTitle))
                {
                    if (int.TryParse(key.Replace($"{convoTitle}_", ""), out var result))
                    {
                        if (result > 0 && result <= convoAmount)
                        {
                            return possibleOptions.ElementAt(result - 1);
                        }
                    }
                }

                return orig;
            }

            IEnumerator TemporaryOverrideRoutine()
            {
                yield return new WaitForSeconds(0.25f);
                ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
            }

            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;

            UnboundCoroutine.Start(TemporaryOverrideRoutine());

            Impl.DisplayEnemyDreamMessage(convoAmount, convoTitle, "Enemy Dreams");
        }


        /// <summary>
        /// Displays a regular dreamnail conversation. Note: Be sure to also call <see cref="PlayDreamnailEffects(Vector3)"/> to play effects
        /// </summary>
        /// <param name="convoAmount">The amount of possible messages that is associated with this <paramref name="convoTitle"/>. Set to 0 if there is only one message</param>
        /// <param name="convoTitle">The language convo title to get the dreamnail messages from. If null, will use the default sheetTitle of "Minor NPC"</param>
        public static void DisplayRegularDreamnailMessage((string convoTitle, string sheetTitle) langPack)
        {
            if (string.IsNullOrEmpty(langPack.sheetTitle))
            {
                langPack.sheetTitle = "Minor NPC";
            }
            Impl.DisplayRegularDreamMessage(langPack.convoTitle, langPack.sheetTitle);
        }

        /// <summary>
        /// Displays a regular dreamnail conversation. Note: Be sure to also call <see cref="PlayDreamnailEffects(Vector3)"/> to play effects
        /// </summary>
        /// <param name="message">The message to display</param>
        public static void DisplayRegularDreamnailMessage(string message)
        {
            DisplayRegularDreamnailMessage(new string[] { message });
        }

        /// <summary>
        /// Displays a regular dreamnail conversation. Note: Be sure to also call <see cref="PlayDreamnailEffects(Vector3)"/> to play effects
        /// </summary>
        /// <param name="possibleOptions">The possible messages to display</param>
        public static void DisplayRegularDreamnailMessage(params string[] possibleOptions)
        {
            DisplayRegularDreamnailMessage((IEnumerable<string>)possibleOptions);
        }

        /// <summary>
        /// Displays a regular dreamnail conversation. Note: Be sure to also call <see cref="PlayDreamnailEffects(Vector3)"/> to play effects
        /// </summary>
        /// <param name="possibleOptions">The possible messages to display</param>
        public static void DisplayRegularDreamnailMessage(IEnumerable<string> possibleOptions)
        {
            int convoAmount = possibleOptions.Count();
            string convoTitle = Guid.NewGuid().ToString();
            string pickedOption = possibleOptions.ElementAt(UnityEngine.Random.Range(0,convoAmount));

            string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
            {
                if (sheetTitle == "Minor NPC" && key == convoTitle)
                {
                    return pickedOption;
                }

                return orig;
            }

            IEnumerator TemporaryOverrideRoutine()
            {
                yield return new WaitForSeconds(1f);
                ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
            }

            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;

            UnboundCoroutine.Start(TemporaryOverrideRoutine());

            Impl.DisplayRegularDreamMessage(convoTitle, "Minor NPC");
        }

        /// <summary>
        /// Plays dreamnail hit effects at the specified position
        /// </summary>
        /// <param name="position">The position to play the effects at</param>
        public static void PlayDreamnailEffects(Vector3 position)
        {
            Impl.PlayDreamnailEffects(position);
        }

        /// <summary>
        /// If a regular dreamnail message is being displayed, then cancel it
        /// </summary>
        public static void CancelRegularDreamnailMessage()
        {
            Impl.CancelRegularDreamnailMessage();
        }

        /// <summary>
        /// Checks if a layer can respond to dreamnail hit events
        /// </summary>
        /// <param name="layer">The layer to check</param>
        /// <returns>Returns true if the layer responds to dreamnail hits</returns>
        public static bool IsLayerDreamnailable(int layer)
		{
			return !Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Attack"), layer);
		}
	}
}
