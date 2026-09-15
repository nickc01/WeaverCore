using Modding;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to custom inventory items.
    /// </summary>
    public static class InventoryUtilities
    {
        public enum VanillaInventoryItem
        {
            Dash,
            Walljump,
            SuperDash,
            DoubleJump,
            AcidArmour,
            Lantern,
            MapAndQuill,
            KingsBrand,
            TramPass,
            CityKey,
            StoreKey,
            WhiteKey,
            LoveKey,
            XunFlower,
            SimpleKey,
            Ore,
            RancidEgg
        }

        enum VanillaItemType
        {
            Normal,
            OneTwoBoth,
            Counted,
            Flower
        }

        struct VanillaItemData
        {
            public VanillaItemType Type;
            public string PlayerDataBool1;
            public string PlayerDataBool2;
            public string PlayerDataInt;
            public string NameKey;
            public string AlternateNameKey;
            public string BothNameKey;
            public string DescriptionKey;
            public string AlternateDescriptionKey;
            public string BothDescriptionKey;
            public string FlowerOtherSourceDescriptionKey;
            public string FlowerBrokenOtherSourceDescriptionKey;
        }

        const string Prefix = "WEAVERCORE_INVENTORY_ITEM_";

        static readonly Dictionary<string, InventoryItem> addedCustomItems = new Dictionary<string, InventoryItem>();
        static readonly Dictionary<InventoryItem, string> itemToID = new Dictionary<InventoryItem, string>();
        static readonly HashSet<InventoryItem> disabledItems = new HashSet<InventoryItem>();
        static readonly Dictionary<VanillaInventoryItem, VanillaItemData> vanillaItems = new Dictionary<VanillaInventoryItem, VanillaItemData>
        {
            [VanillaInventoryItem.Dash] = OneTwoBoth("hasDash", "hasShadowDash", "INV_NAME_DASH", "INV_NAME_SHADOWDASH", "INV_NAME_SHADOWDASH", "INV_DESC_DASH", "INV_DESC_SHADOWDASH", "INV_DESC_SHADOWDASH"),
            [VanillaInventoryItem.Walljump] = Normal("hasWalljump", "INV_NAME_WALLJUMP", "INV_DESC_WALLJUMP"),
            [VanillaInventoryItem.SuperDash] = Normal("hasSuperDash", "INV_NAME_SUPERDASH", "INV_DESC_SUPERDASH"),
            [VanillaInventoryItem.DoubleJump] = Normal("hasDoubleJump", "INV_NAME_DOUBLEJUMP", "INV_DESC_DOUBLEJUMP"),
            [VanillaInventoryItem.AcidArmour] = Normal("hasAcidArmour", "INV_NAME_ACIDARMOUR", "INV_DESC_ACIDARMOUR"),
            [VanillaInventoryItem.Lantern] = Normal("hasLantern", "INV_NAME_LANTERN", "INV_DESC_LANTERN"),
            [VanillaInventoryItem.MapAndQuill] = OneTwoBoth("hasMap", "hasQuill", "INV_NAME_MAP", "INV_NAME_QUILL", "INV_NAME_MAPQUILL", "INV_DESC_MAP", "INV_DESC_QUILL", "INV_DESC_MAPQUILL"),
            [VanillaInventoryItem.KingsBrand] = Normal("hasKingsBrand", "INV_NAME_KINGSBRAND", "INV_DESC_KINGSBRAND"),
            [VanillaInventoryItem.TramPass] = Normal("hasTramPass", "INV_NAME_TRAM_PASS", "INV_DESC_TRAM_PASS"),
            [VanillaInventoryItem.CityKey] = Normal("hasCityKey", "INV_NAME_CITYKEY", "INV_DESC_CITYKEY"),
            [VanillaInventoryItem.StoreKey] = Normal("hasSlykey", "INV_NAME_STOREKEY", "INV_DESC_STOREKEY"),
            [VanillaInventoryItem.WhiteKey] = Normal("hasWhiteKey", "INV_NAME_WHITEKEY", "INV_DESC_WHITEKEY"),
            [VanillaInventoryItem.LoveKey] = Normal("hasLoveKey", "INV_NAME_LOVEKEY", "INV_DESC_LOVEKEY"),
            [VanillaInventoryItem.XunFlower] = Flower("hasXunFlower", "extraFlowerAppear", "xunFlowerBroken", "INV_NAME_FLOWER", "INV_NAME_FLOWER_BROKEN", "INV_DESC_FLOWER", "INV_DESC_FLOWER_BROKEN", "INV_DESC_FLOWER_QG", "INV_DESC_FLOWER_BROKEN_QG"),
            [VanillaInventoryItem.SimpleKey] = Counted("simpleKeys", "INV_NAME_SIMPLEKEY", "INV_DESC_SIMPLEKEY"),
            [VanillaInventoryItem.Ore] = Counted("ore", "INV_NAME_ORE", "INV_DESC_ORE"),
            [VanillaInventoryItem.RancidEgg] = Counted("rancidEggs", "INV_NAME_RANCIDEGG", "INV_DESC_RANCIDEGG")
        };

#if !UNITY_EDITOR
        static Assembly sfCoreAssembly;
        static MethodInfo addNormalItem;
        static MethodInfo addOneTwoItem;
        static MethodInfo addOneTwoBothItem;
        static MethodInfo addCountedItem;
        static MethodInfo addFlowerItem;
#endif

        /// <summary>
        /// Registers a custom inventory item with SFCore.
        /// </summary>
        public static void RegisterItem(InventoryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (ItemRegistered(item))
            {
                throw new Exception($"The inventory item \"{item.Name}\" : {item.GetType()} is being added multiple times");
            }

            var itemID = GetSafeItemID(item);
            if (addedCustomItems.TryGetValue(itemID, out var existing) && existing != item)
            {
                throw new Exception($"The inventory item ID \"{itemID}\" is already registered by {existing.GetType().FullName}");
            }

#if !UNITY_EDITOR
            EnsureSFCoreLoaded();
            RegisterWithSFCore(item, itemID);
#endif

            addedCustomItems.Add(itemID, item);
            itemToID.Add(item, itemID);
            WeaverLog.Log($"Registered Inventory Item {item.GetType().FullName} - {itemID}");
        }

        public static bool ItemRegistered(InventoryItem item)
        {
            return item != null && itemToID.ContainsKey(item);
        }

        public static string GetCustomItemID(InventoryItem item)
        {
            item = ResolveToRegisteredItem(item);
            if (item != null && itemToID.TryGetValue(item, out var id))
            {
                return id;
            }

            return null;
        }

        public static InventoryItem ResolveToRegisteredItem(InventoryItem item)
        {
            if (item == null)
            {
                return null;
            }

            if (itemToID.ContainsKey(item))
            {
                return item;
            }

            var itemID = GetSafeItemID(item);
            if (addedCustomItems.TryGetValue(itemID, out var registeredItem))
            {
                return registeredItem;
            }

            return item;
        }

        public static IEnumerable<InventoryItem> GetLoadedItems()
        {
            foreach (var pair in addedCustomItems)
            {
                if (!disabledItems.Contains(pair.Value))
                {
                    yield return pair.Value;
                }
            }
        }

        public static IEnumerable<T> GetLoadedItems<T>()
        {
            foreach (var pair in addedCustomItems)
            {
                if (!disabledItems.Contains(pair.Value) && pair.Value is T item)
                {
                    yield return item;
                }
            }
        }

        public static bool TryGetLoadedItem<T>(out T value)
        {
            foreach (var pair in addedCustomItems)
            {
                if (!disabledItems.Contains(pair.Value) && pair.Value is T item)
                {
                    value = item;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static int GetLoadedItemCount()
        {
            int count = 0;
            foreach (var pair in addedCustomItems)
            {
                if (!disabledItems.Contains(pair.Value))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool ItemDisabled(InventoryItem item)
        {
            return disabledItems.Contains(item);
        }

        /// <summary>
        /// Returns true when the vanilla inventory entry would be visible in SFCore's equipment inventory page.
        /// </summary>
        public static bool HasVanillaItem(VanillaInventoryItem item)
        {
            var data = GetVanillaItemData(item);
            var playerData = PlayerData.instance;

            switch (data.Type)
            {
                case VanillaItemType.Normal:
                    return playerData.GetBool(data.PlayerDataBool1);
                case VanillaItemType.OneTwoBoth:
                    return playerData.GetBool(data.PlayerDataBool1) || playerData.GetBool(data.PlayerDataBool2);
                case VanillaItemType.Counted:
                    return playerData.GetInt(data.PlayerDataInt) > 0;
                case VanillaItemType.Flower:
                    return playerData.GetBool(data.PlayerDataBool1) && !(playerData.GetBool(data.PlayerDataBool2) && playerData.GetBool(data.PlayerDataInt));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the count for a counted vanilla item. Non-counted entries return 1 if visible, otherwise 0.
        /// </summary>
        public static int GetVanillaItemCount(VanillaInventoryItem item)
        {
            var data = GetVanillaItemData(item);
            if (data.Type == VanillaItemType.Counted)
            {
                return PlayerData.instance.GetInt(data.PlayerDataInt);
            }

            return HasVanillaItem(item) ? 1 : 0;
        }

        /// <summary>
        /// Gets the active language key for a vanilla item's displayed name.
        /// </summary>
        public static string GetVanillaItemNameKey(VanillaInventoryItem item)
        {
            var data = GetVanillaItemData(item);
            var playerData = PlayerData.instance;

            switch (data.Type)
            {
                case VanillaItemType.OneTwoBoth:
                    if (playerData.GetBool(data.PlayerDataBool1) && !playerData.GetBool(data.PlayerDataBool2))
                    {
                        return data.NameKey;
                    }
                    if (!playerData.GetBool(data.PlayerDataBool1) && playerData.GetBool(data.PlayerDataBool2))
                    {
                        return data.AlternateNameKey;
                    }
                    return data.BothNameKey;
                case VanillaItemType.Flower:
                    return playerData.GetBool(data.PlayerDataInt) ? data.AlternateNameKey : data.NameKey;
                default:
                    return data.NameKey;
            }
        }

        /// <summary>
        /// Gets the active language key for a vanilla item's displayed description.
        /// </summary>
        public static string GetVanillaItemDescriptionKey(VanillaInventoryItem item)
        {
            var data = GetVanillaItemData(item);
            var playerData = PlayerData.instance;

            switch (data.Type)
            {
                case VanillaItemType.OneTwoBoth:
                    if (playerData.GetBool(data.PlayerDataBool1) && !playerData.GetBool(data.PlayerDataBool2))
                    {
                        return data.DescriptionKey;
                    }
                    if (!playerData.GetBool(data.PlayerDataBool1) && playerData.GetBool(data.PlayerDataBool2))
                    {
                        return data.AlternateDescriptionKey;
                    }
                    return data.BothDescriptionKey;
                case VanillaItemType.Flower:
                    if (playerData.GetBool(data.PlayerDataInt))
                    {
                        return playerData.GetBool(data.PlayerDataBool2) ? data.FlowerBrokenOtherSourceDescriptionKey : data.AlternateDescriptionKey;
                    }
                    return playerData.GetBool(data.PlayerDataBool2) ? data.FlowerOtherSourceDescriptionKey : data.DescriptionKey;
                default:
                    return data.DescriptionKey;
            }
        }

        /// <summary>
        /// Gets a vanilla inventory item's main PlayerData bool field, if it has one.
        /// </summary>
        public static bool TryGetVanillaItemPlayerDataBool(VanillaInventoryItem item, out string playerDataBool)
        {
            playerDataBool = GetVanillaItemData(item).PlayerDataBool1;
            return !string.IsNullOrEmpty(playerDataBool);
        }

        /// <summary>
        /// Gets a vanilla inventory item's secondary PlayerData bool field, if it has one.
        /// </summary>
        public static bool TryGetVanillaItemSecondaryPlayerDataBool(VanillaInventoryItem item, out string playerDataBool)
        {
            playerDataBool = GetVanillaItemData(item).PlayerDataBool2;
            return !string.IsNullOrEmpty(playerDataBool);
        }

        /// <summary>
        /// Gets a vanilla inventory item's PlayerData int field, if it has one.
        /// </summary>
        public static bool TryGetVanillaItemPlayerDataInt(VanillaInventoryItem item, out string playerDataInt)
        {
            playerDataInt = GetVanillaItemData(item).PlayerDataInt;
            return !string.IsNullOrEmpty(playerDataInt);
        }

        public static bool GiveItemToPlayer(InventoryItem item, int countedAmount = 1)
        {
            item = ResolveToRegisteredItem(item);
            if (item == null)
            {
                return false;
            }

            bool alreadyCollected;
            if (item.Type == InventoryItem.ItemType.Counted)
            {
                alreadyCollected = item.Count > 0;
                item.Count += countedAmount;
            }
            else
            {
                alreadyCollected = item.Acquired;
                item.Acquired = true;
            }

            return alreadyCollected;
        }

        [OnFeatureLoad(priority: int.MinValue)]
        static void OnInventoryItemLoad(InventoryItem item)
        {
            if (disabledItems.Contains(item))
            {
                disabledItems.Remove(item);
            }
            else
            {
                try
                {
                    RegisterItem(item);
                }
                catch (Exception e)
                {
                    WeaverLog.LogError($"Error: Failed to instantiate inventory item \"{item.GetType().FullName}\".");
                    WeaverLog.LogException(e);
                }
            }
        }

        [OnFeatureUnload(priority: int.MinValue)]
        static void OnInventoryItemUnload(InventoryItem item)
        {
            disabledItems.Add(item);
        }

        [OnRuntimeInit]
        static void OnInit()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
            ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;
        }

        static bool TryParseItemKey(string key, string suffix, out InventoryItem item)
        {
            if (key.StartsWith(Prefix) && key.EndsWith(suffix))
            {
                var itemID = key.Substring(Prefix.Length, key.Length - Prefix.Length - suffix.Length);
                if (addedCustomItems.TryGetValue(itemID, out item))
                {
                    return true;
                }
            }

            item = null;
            return false;
        }

        static string GetSafeItemID(InventoryItem item)
        {
            if (!string.IsNullOrEmpty(item.ItemID))
            {
                return item.ItemID;
            }

            return item.GetType().FullName;
        }

        static string Key(string itemID, string suffix)
        {
            return $"{Prefix}{itemID}{suffix}";
        }

        static VanillaItemData GetVanillaItemData(VanillaInventoryItem item)
        {
            if (vanillaItems.TryGetValue(item, out var data))
            {
                return data;
            }

            throw new ArgumentOutOfRangeException(nameof(item), item, null);
        }

        static VanillaItemData Normal(string playerDataBool, string nameKey, string descriptionKey)
        {
            return new VanillaItemData
            {
                Type = VanillaItemType.Normal,
                PlayerDataBool1 = playerDataBool,
                NameKey = nameKey,
                DescriptionKey = descriptionKey
            };
        }

        static VanillaItemData OneTwoBoth(string playerDataBool1, string playerDataBool2, string nameKey, string alternateNameKey, string bothNameKey, string descriptionKey, string alternateDescriptionKey, string bothDescriptionKey)
        {
            return new VanillaItemData
            {
                Type = VanillaItemType.OneTwoBoth,
                PlayerDataBool1 = playerDataBool1,
                PlayerDataBool2 = playerDataBool2,
                NameKey = nameKey,
                AlternateNameKey = alternateNameKey,
                BothNameKey = bothNameKey,
                DescriptionKey = descriptionKey,
                AlternateDescriptionKey = alternateDescriptionKey,
                BothDescriptionKey = bothDescriptionKey
            };
        }

        static VanillaItemData Counted(string playerDataInt, string nameKey, string descriptionKey)
        {
            return new VanillaItemData
            {
                Type = VanillaItemType.Counted,
                PlayerDataInt = playerDataInt,
                NameKey = nameKey,
                DescriptionKey = descriptionKey
            };
        }

        static VanillaItemData Flower(string acquiredBool, string fromOtherSourceBool, string brokenBool, string nameKey, string alternateNameKey, string descriptionKey, string alternateDescriptionKey, string otherSourceDescriptionKey, string brokenOtherSourceDescriptionKey)
        {
            return new VanillaItemData
            {
                Type = VanillaItemType.Flower,
                PlayerDataBool1 = acquiredBool,
                PlayerDataBool2 = fromOtherSourceBool,
                PlayerDataInt = brokenBool,
                NameKey = nameKey,
                AlternateNameKey = alternateNameKey,
                DescriptionKey = descriptionKey,
                AlternateDescriptionKey = alternateDescriptionKey,
                FlowerOtherSourceDescriptionKey = otherSourceDescriptionKey,
                FlowerBrokenOtherSourceDescriptionKey = brokenOtherSourceDescriptionKey
            };
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (TryParseItemKey(key, "_NAME", out var item))
            {
                return ItemDisabled(item) ? $"DISABLED - {item.Name}" : item.Name;
            }

            if (TryParseItemKey(key, "_DESCRIPTION", out item))
            {
                return ItemDisabled(item) ? $"{item.Description} - This item is currently disabled" : item.Description;
            }

            if (TryParseItemKey(key, "_ALT_NAME", out item))
            {
                return ItemDisabled(item) ? $"DISABLED - {item.AlternateName}" : item.AlternateName;
            }

            if (TryParseItemKey(key, "_ALT_DESCRIPTION", out item))
            {
                return ItemDisabled(item) ? $"{item.AlternateDescription} - This item is currently disabled" : item.AlternateDescription;
            }

            if (TryParseItemKey(key, "_BOTH_NAME", out item))
            {
                return ItemDisabled(item) ? $"DISABLED - {item.BothName}" : item.BothName;
            }

            if (TryParseItemKey(key, "_BOTH_DESCRIPTION", out item))
            {
                return ItemDisabled(item) ? $"{item.BothDescription} - This item is currently disabled" : item.BothDescription;
            }

            if (TryParseItemKey(key, "_FLOWER_OTHER_SOURCE_DESCRIPTION", out item))
            {
                return ItemDisabled(item) ? $"{item.FlowerOtherSourceDescription} - This item is currently disabled" : item.FlowerOtherSourceDescription;
            }

            if (TryParseItemKey(key, "_FLOWER_BROKEN_OTHER_SOURCE_DESCRIPTION", out item))
            {
                return ItemDisabled(item) ? $"{item.FlowerBrokenOtherSourceDescription} - This item is currently disabled" : item.FlowerBrokenOtherSourceDescription;
            }

            return orig;
        }

        private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
        {
            if (TryParseItemKey(name, "_ACQUIRED", out var item))
            {
                return !ItemDisabled(item) && item.Acquired;
            }

            if (TryParseItemKey(name, "_SECOND_ACQUIRED", out item))
            {
                return !ItemDisabled(item) && item.SecondAcquired;
            }

            if (TryParseItemKey(name, "_FLOWER_FROM_OTHER_SOURCE", out item))
            {
                return !ItemDisabled(item) && item.FlowerFromOtherSource;
            }

            if (TryParseItemKey(name, "_FLOWER_BROKEN", out item))
            {
                return !ItemDisabled(item) && item.FlowerBroken;
            }

            return orig;
        }

        private static bool ModHooks_SetPlayerBoolHook(string name, bool newValue)
        {
            if (TryParseItemKey(name, "_ACQUIRED", out var item))
            {
                if (!ItemDisabled(item))
                {
                    item.Acquired = newValue;
                }
                return newValue;
            }

            if (TryParseItemKey(name, "_SECOND_ACQUIRED", out item))
            {
                if (!ItemDisabled(item))
                {
                    item.SecondAcquired = newValue;
                }
                return newValue;
            }

            if (TryParseItemKey(name, "_FLOWER_FROM_OTHER_SOURCE", out item))
            {
                if (!ItemDisabled(item))
                {
                    item.FlowerFromOtherSource = newValue;
                }
                return newValue;
            }

            if (TryParseItemKey(name, "_FLOWER_BROKEN", out item))
            {
                if (!ItemDisabled(item))
                {
                    item.FlowerBroken = newValue;
                }
                return newValue;
            }

            return newValue;
        }

        private static int ModHooks_GetPlayerIntHook(string name, int orig)
        {
            if (TryParseItemKey(name, "_COUNT", out var item))
            {
                return ItemDisabled(item) ? 0 : item.Count;
            }

            return orig;
        }

        private static int ModHooks_SetPlayerIntHook(string name, int newValue)
        {
            if (TryParseItemKey(name, "_COUNT", out var item))
            {
                if (!ItemDisabled(item))
                {
                    item.Count = newValue;
                }
                return newValue;
            }

            return newValue;
        }

#if !UNITY_EDITOR
        static void EnsureSFCoreLoaded()
        {
            if (sfCoreAssembly != null)
            {
                return;
            }

            sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
            if (sfCoreAssembly == null)
            {
                throw new Exception("Attempting to add an inventory item without SFCore installed. Install SFCore to fix this issue");
            }

            var itemHelper = sfCoreAssembly.GetType("SFCore.ItemHelper");
            if (itemHelper == null)
            {
                throw new Exception("Unable to find the ItemHelper class");
            }

            addNormalItem = GetSFCoreMethod(itemHelper, "AddNormalItem");
            addOneTwoItem = GetSFCoreMethod(itemHelper, "AddOneTwoItem");
            addOneTwoBothItem = GetSFCoreMethod(itemHelper, "AddOneTwoBothItem");
            addCountedItem = GetSFCoreMethod(itemHelper, "AddCountedItem");
            addFlowerItem = GetSFCoreMethod(itemHelper, "AddFlowerItem");
        }

        static MethodInfo GetSFCoreMethod(Type itemHelper, string methodName)
        {
            var method = itemHelper.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                throw new Exception($"Unable to find the {methodName} method on the ItemHelper class");
            }

            return method;
        }

        static void RegisterWithSFCore(InventoryItem item, string itemID)
        {
            switch (item.Type)
            {
                case InventoryItem.ItemType.Normal:
                    addNormalItem.Invoke(null, new object[]
                    {
                        item.Sprite,
                        Key(itemID, "_ACQUIRED"),
                        Key(itemID, "_NAME"),
                        Key(itemID, "_DESCRIPTION")
                    });
                    break;
                case InventoryItem.ItemType.OneTwo:
                    addOneTwoItem.Invoke(null, new object[]
                    {
                        item.Sprite,
                        item.AlternateSprite,
                        Key(itemID, "_ACQUIRED"),
                        Key(itemID, "_SECOND_ACQUIRED"),
                        Key(itemID, "_NAME"),
                        Key(itemID, "_ALT_NAME"),
                        Key(itemID, "_DESCRIPTION"),
                        Key(itemID, "_ALT_DESCRIPTION")
                    });
                    break;
                case InventoryItem.ItemType.OneTwoBoth:
                    addOneTwoBothItem.Invoke(null, new object[]
                    {
                        item.Sprite,
                        item.AlternateSprite,
                        item.BothSprite,
                        Key(itemID, "_ACQUIRED"),
                        Key(itemID, "_SECOND_ACQUIRED"),
                        Key(itemID, "_NAME"),
                        Key(itemID, "_ALT_NAME"),
                        Key(itemID, "_BOTH_NAME"),
                        Key(itemID, "_DESCRIPTION"),
                        Key(itemID, "_ALT_DESCRIPTION"),
                        Key(itemID, "_BOTH_DESCRIPTION")
                    });
                    break;
                case InventoryItem.ItemType.Counted:
                    addCountedItem.Invoke(null, new object[]
                    {
                        item.Sprite,
                        Key(itemID, "_COUNT"),
                        Key(itemID, "_NAME"),
                        Key(itemID, "_DESCRIPTION")
                    });
                    break;
                case InventoryItem.ItemType.Flower:
                    addFlowerItem.Invoke(null, new object[]
                    {
                        item.Sprite,
                        item.AlternateSprite,
                        Key(itemID, "_ACQUIRED"),
                        Key(itemID, "_FLOWER_FROM_OTHER_SOURCE"),
                        Key(itemID, "_FLOWER_BROKEN"),
                        Key(itemID, "_NAME"),
                        Key(itemID, "_ALT_NAME"),
                        Key(itemID, "_DESCRIPTION"),
                        Key(itemID, "_ALT_DESCRIPTION"),
                        Key(itemID, "_FLOWER_OTHER_SOURCE_DESCRIPTION"),
                        Key(itemID, "_FLOWER_BROKEN_OTHER_SOURCE_DESCRIPTION")
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
    }
}
