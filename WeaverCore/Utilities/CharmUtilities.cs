using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to Charms
    /// </summary>
    public static class CharmUtilities
    {
        static Dictionary<int, IWeaverCharm> addedCustomCharms =
            new Dictionary<int, IWeaverCharm>();

        static Dictionary<IWeaverCharm, int> charmsToID = new Dictionary<IWeaverCharm, int>();

        static HashSet<IWeaverCharm> disabledCharms = new HashSet<IWeaverCharm>();

        static Assembly sfCoreAssembly;

#if UNITY_EDITOR
        static int editor_counter = 40;
#else
        static Type charmHelper;
        delegate List<int> AddSpritesDelegate(params Sprite[] charmSprites);

        static AddSpritesDelegate AddSprites;
        static FieldInfo customSprites;
#endif

        public static Assembly SFCoreAssembly =>
            sfCoreAssembly ??= ReflectionUtilities.FindLoadedAssembly("SFCore");

        /// <summary>
        ///
        /// </summary>
        /// <param name="charm">The charm to add</param>
        /// <param name="charmSprite">The sprite of the charm</param>
        /// <param name="acquiredByDefault">Is the charm acquired by the player by default?</param>
        /// <param name="equippedByDefault">Is the charm equipped by the player by default?</param>
        /// <returns>Returns the ID of the registered charm</returns>
        /// <exception cref="System.Exception">Throws if the charm is already registered</exception>
        public static int RegisterCharm(IWeaverCharm charm, Sprite charmSprite)
        {
            if (CharmRegistered(charm))
            {
                throw new System.Exception(
                    $"The charm \"{charm.Name}\" : {charm.GetType()} is being added multiple times"
                );
            }
#if UNITY_EDITOR
            int id = ++editor_counter;

            addedCustomCharms.Add(id, charm);
            charmsToID.Add(charm, id);

            WeaverLog.Log($"Registered Charm {charm.GetType().FullName} - {id}");
            return id;
#else
            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
                    throw new System.Exception(
                        "Attempting to add a charm without SFCore installed. Install SFCore to fix this issue"
                    );
                }
            }

            if (charmHelper == null)
            {
                charmHelper = sfCoreAssembly.GetType("SFCore.CharmHelper");

                if (charmHelper == null)
                {
                    throw new System.Exception("Unable to find the CharmHelper class");
                }
            }

            if (AddSprites == null)
            {
                var addSpritesMethod = charmHelper.GetMethod("AddSprites");

                if (addSpritesMethod == null)
                {
                    throw new System.Exception(
                        "Unable to find the AddSprites method on the CharmHelper class"
                    );
                }

                AddSprites = ReflectionUtilities.MethodToDelegate<AddSpritesDelegate>(
                    addSpritesMethod
                );
            }

            var ids = AddSprites(charmSprite);

            addedCustomCharms.Add(ids[0], charm);
            charmsToID.Add(charm, ids[0]);
            WeaverLog.Log($"Registered Charm {charm.GetType().FullName} - {ids[0]}");

            return ids[0];
#endif
        }

        public static IEnumerable<int> GetAllActiveSFCoreCharmIDs()
        {
            var startCharmID = 40;
            int endCharmID;
#if UNITY_EDITOR
            endCharmID = editor_counter;
#else
            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
                    throw new System.Exception(
                        "Attempting to add a charm without SFCore installed. Install SFCore to fix this issue"
                    );
                }
            }

            if (charmHelper == null)
            {
                charmHelper = sfCoreAssembly.GetType("SFCore.CharmHelper");

                if (charmHelper == null)
                {
                    throw new System.Exception("Unable to find the CharmHelper class");
                }
            }

            if (customSprites == null)
            {
                customSprites = charmHelper.GetField(
                    "CustomSprites",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public
                );

                if (customSprites == null)
                {
                    throw new System.Exception(
                        "Unable to find the CustomSprites field on the CharmHelper class"
                    );
                }
            }

            endCharmID = startCharmID + (customSprites.GetValue(null) as List<Sprite>).Count + 1;
#endif
            List<int> disabledCharmIDs = disabledCharms
                .Select(c => c != null && charmsToID.TryGetValue(c, out var charmID) ? charmID : -1)
                .ToList();
            for (int i = startCharmID + 1; i < endCharmID; i++)
            {
                if (!disabledCharmIDs.Contains(i))
                {
                    yield return i;
                }
            }
        }

        public static Sprite GetSpriteForCharmID(int id)
        {
#if UNITY_EDITOR
            var charm = GetLoadedCharms().FirstOrDefault(c => c != null && charmsToID.TryGetValue(c, out var cID) && cID == id);

            if (charm != null)
            {
                return charm.CharmSprite;
            }

            return null;
#else
            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
                    throw new System.Exception(
                        "Attempting to add a charm without SFCore installed. Install SFCore to fix this issue"
                    );
                }
            }

            if (charmHelper == null)
            {
                charmHelper = sfCoreAssembly.GetType("SFCore.CharmHelper");

                if (charmHelper == null)
                {
                    throw new System.Exception("Unable to find the CharmHelper class");
                }
            }

            if (customSprites == null)
            {
                customSprites = charmHelper.GetField(
                    "CustomSprites",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public
                );

                if (customSprites == null)
                {
                    throw new System.Exception(
                        "Unable to find the CustomSprites field on the CharmHelper class"
                    );
                }
            }
            var customSpritesList = customSprites.GetValue(null) as List<Sprite>;

            var customIndex = id - 41;
            if (customSpritesList != null && customIndex >= 0 && customIndex < customSpritesList.Count)
            {
                return customSpritesList[customIndex];
            }

            return null;
#endif
        }

        public static bool CharmRegistered(IWeaverCharm charm)
        {
            return addedCustomCharms.ContainsValue(charm);
        }

        public static int GetCustomCharmID(IWeaverCharm charm)
        {
            charm = ResolveToRegisteredCharm(charm);
            if (charm != null && charmsToID.TryGetValue(charm, out var id))
            {
                return id;
            }
            return -1;
        }

        public static IWeaverCharm ResolveToRegisteredCharm(IWeaverCharm charm)
        {
            if (charm == null)
            {
                return null;
            }

            if (charmsToID.ContainsKey(charm))
            {
                return charm;
            }

            var charmType = charm.GetType();
            foreach (var registeredCharm in Registry.GetAllFeatures<IWeaverCharm>())
            {
                if (registeredCharm != null && registeredCharm.GetType() == charmType)
                {
                    return registeredCharm;
                }
            }

            return charm;
        }

        public static IEnumerable<IWeaverCharm> GetLoadedCharms()
        {
            foreach (var pair in addedCustomCharms)
            {
                if (!disabledCharms.Contains(pair.Value))
                {
                    yield return pair.Value;
                }
            }
        }

        public static IEnumerable<T> GetLoadedCharms<T>()
        {
            foreach (var pair in addedCustomCharms)
            {
                if (!disabledCharms.Contains(pair.Value) && pair.Value is T)
                {
                    yield return (T)pair.Value;
                }
            }
        }

        public static bool TryGetLoadedCharm<T>(out T value)
        {
            foreach (var pair in addedCustomCharms)
            {
                if (!disabledCharms.Contains(pair.Value) && pair.Value is T)
                {
                    value = (T)pair.Value;
                    return true;
                }
            }
            value = default;

            return false;
        }

        public static int GetLoadedCharmCount()
        {
            int count = 0;
            foreach (var pair in addedCustomCharms)
            {
                if (!disabledCharms.Contains(pair.Value))
                {
                    count++;
                }
            }
            return count;
        }

        [OnFeatureLoad(priority: int.MinValue)]
        static void OnCharmLoad(IWeaverCharm charm)
        {
            if (disabledCharms.Contains(charm))
            {
                disabledCharms.Remove(charm);
            }
            else
            {
                try
                {
                    RegisterCharm(charm, charm.CharmSprite);
                }
                catch (Exception e)
                {
                    WeaverLog.LogError(
                        $"Error: Failed to instantiate charm \"{charm.GetType().FullName}\". Make sure it has a default constructor"
                    );
                    WeaverLog.LogException(e);
                }
            }
        }

        [OnFeatureUnload(priority: int.MinValue)]
        static void OnCharmUnload(IWeaverCharm charm)
        {
            disabledCharms.Add(charm);
        }

        [OnRuntimeInit]
        static void OnInit()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        }

        public static bool CharmDisabled(IWeaverCharm charm)
        {
            return disabledCharms.Contains(charm);
        }

        static bool TryParseCharm(
            string key,
            string strToFind,
            out int charmNumber,
            out IWeaverCharm charm
        )
        {
            if (
                key.TryFind(strToFind, out _, out var end)
                && int.TryParse(key.Substring(end), out charmNumber)
                && addedCustomCharms.TryGetValue(charmNumber, out charm)
            )
            {
                return true;
            }
            charmNumber = -1;
            charm = null;
            return false;
        }

        private static int ModHooks_GetPlayerIntHook(string name, int orig)
        {
            IWeaverCharm charm;

            if (TryParseCharm(name, "charmCost_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return 1;
                }
                return charm.NotchCost;
            }

            return orig;
        }

        private static bool ModHooks_SetPlayerBoolHook(string name, bool newValue)
        {
            IWeaverCharm charm;

            if (name.Contains("equippedCharm_"))
            {
                WeaverLog.Log("RECEIVED EQUIPPED CHARM CALL = " + name + " with value = " + newValue);
            }

            if (TryParseCharm(name, "gotCharm_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return newValue;
                }
                if (charm.Acquired != newValue)
                {
                    charm.Acquired = newValue;
                }
                return newValue;
            }

            if (TryParseCharm(name, "newCharm_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return newValue;
                }
                if (charm.NewlyCollected != newValue)
                {
                    charm.NewlyCollected = newValue;
                }
                return newValue;
            }

            if (TryParseCharm(name, "equippedCharm_", out _, out charm))
            {
                WeaverLog.Log($"Setting Equipped Charm = [Name = {name}, Type = equippedCharm_, charm = {charm.GetType().FullName}]");
                if (CharmDisabled(charm))
                {
                    return newValue;
                }
                if (charm.Equipped != newValue)
                {
                    charm.Equipped = newValue;
                }
                return newValue;
            }

            return newValue;
        }

        private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
        {
            IWeaverCharm charm;

            if (TryParseCharm(name, "gotCharm_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return false;
                }
                return charm.Acquired;
            }

            if (TryParseCharm(name, "newCharm_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return false;
                }
                return charm.NewlyCollected;
            }

            if (TryParseCharm(name, "equippedCharm_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return false;
                }
                return charm.Equipped;
            }

            return orig;
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            IWeaverCharm charm;

            if (TryParseCharm(key, "CHARM_NAME_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return $"DISABLED - {charm.Name}";
                }
                return charm.Name;
            }

            if (TryParseCharm(key, "CHARM_DESC_", out _, out charm))
            {
                if (CharmDisabled(charm))
                {
                    return $"{charm.Description} - This charm is currently disabled";
                }
                return charm.Description;
            }

            return orig;
        }

        public static bool GiveCharmToPlayer(IWeaverCharm charm, bool displayCollectMessage = true)
        {
            charm = ResolveToRegisteredCharm(charm);
            var charmID = GetCustomCharmID(charm);
            return GiveCharmToPlayer(charmID, displayCollectMessage);
        }

        public static bool GiveCharmToPlayer(int charmID, bool displayCollectMessage = true)
        {
            bool alreadyCollected = false;
            if (PlayerData.instance.GetBool($"gotCharm_{charmID}") == true)
            {
                alreadyCollected = true;
            }
            else
            {
                GameManager.instance.IncrementPlayerDataInt("charmsOwned");
            }

            //WeaverLog.Log("ALREADY COLLECTED = " + alreadyCollected);

            //WeaverLog.Log("HAS CHARM = " + GameManager.instance.GetPlayerDataBool($"gotCharm_{charmID}"));

            PlayerData.instance.SetBool($"gotCharm_{charmID}", true);
            GameManager.instance.StoryRecord_acquired($"gotCharm_{charmID}");

            if (!GameManager.instance.GetPlayerDataBool("hasCharm"))
            {
                PlayerData.instance.SetBool("hasCharm", true);
            }

            if (displayCollectMessage)
            {
                ItemGetMessage.SpawnCharm(charmID);
            }
            return alreadyCollected;
        }
    }
}
