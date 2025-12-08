using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Reflective helper for interacting with ItemChanger when it is installed, without creating a hard dependency.
    /// </summary>
    public static class ItemChangerUtilities
    {
        private const string AssemblyName = "ItemChanger";
        private const string FinderTypeName = "ItemChanger.Finder";
        private const string ModTypeName = "ItemChanger.ItemChangerMod";
        private const string PlacementConflictTypeName = "ItemChanger.PlacementConflictResolution";
        private const string AbstractPlacementTypeName = "ItemChanger.AbstractPlacement";

        private static readonly Lazy<Assembly> _assembly = new Lazy<Assembly>(() => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == AssemblyName));
        private static readonly Lazy<Type> _finderType = new Lazy<Type>(() => ItemChangerAssembly?.GetType(FinderTypeName));
        private static readonly Lazy<Type> _modType = new Lazy<Type>(() => ItemChangerAssembly?.GetType(ModTypeName));
        private static readonly Lazy<Type> _placementConflictType = new Lazy<Type>(() => ItemChangerAssembly?.GetType(PlacementConflictTypeName));
        private static readonly Lazy<Type> _abstractPlacementType = new Lazy<Type>(() => ItemChangerAssembly?.GetType(AbstractPlacementTypeName));
        private static readonly Lazy<Type> _settingsType = new Lazy<Type>(() => ItemChangerAssembly?.GetType("ItemChanger.Settings"));
        private static readonly Lazy<Type> _itemNamesType = new Lazy<Type>(() => ItemChangerAssembly?.GetType("ItemChanger.ItemNames"));
        private static readonly Lazy<Type> _locationNamesType = new Lazy<Type>(() => ItemChangerAssembly?.GetType("ItemChanger.LocationNames"));
        private static readonly Lazy<Type> _sceneNamesType = new Lazy<Type>(() => ItemChangerAssembly?.GetType("ItemChanger.SceneNames"));

        private static Assembly ItemChangerAssembly => _assembly.Value;
        private static Type FinderType => _finderType.Value;
        private static Type ItemChangerModType => _modType.Value;
        private static Type PlacementConflictType => _placementConflictType.Value;
        private static Type AbstractPlacementType => _abstractPlacementType.Value;
        private static Type SettingsType => _settingsType.Value;

        /// <summary>
        /// True when ItemChanger is detected and the core entry types are available.
        /// </summary>
        public static bool IsAvailable => ItemChangerAssembly != null && FinderType != null && ItemChangerModType != null;

        public static string[] GetAllItemNames()
        {
            return GetAllConstValues(_itemNamesType.Value);
        }

        public static string[] GetAllLocationNames()
        {
            return GetAllConstValues(_locationNamesType.Value);
        }

        public static string[] GetAllSceneNames()
        {
            return GetAllConstValues(_sceneNamesType.Value);
        }

        static string[] GetAllConstValues(Type type)
        {
            if (!IsAvailable)
            {
                return new string[]
                {
                    "Demo 1",
                    "Demo 2",
                    "Demo 3"  
                };
            }

            return type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue())
            .ToArray();
        }

        /// <summary>
        /// Ensures ItemChanger has a settings profile so placements can be added.
        /// </summary>
        /// <param name="overwriteExisting">Pass true to force a fresh profile if the current one is not loaded.</param>
        /// <param name="includeDefaultModules">Whether to include ItemChanger's default modules when creating a profile.</param>
        public static bool TryEnsureSettingsProfile(bool overwriteExisting = false, bool includeDefaultModules = true)
        {
            if (!IsAvailable)
            {
                return false;
            }

            try
            {
                var createSettings = ItemChangerModType!.GetMethod("CreateSettingsProfile", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool), typeof(bool) }, null)
                                     ?? ItemChangerModType.GetMethod("CreateSettingsProfile", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null);

                if (createSettings == null)
                {
                    WeaverLog.LogWarning("ItemChanger CreateSettingsProfile method could not be located.");
                    return false;
                }

                if (createSettings.GetParameters().Length == 2)
                {
                    createSettings.Invoke(null, new object[] { overwriteExisting, includeDefaultModules });
                }
                else
                {
                    createSettings.Invoke(null, new object[] { overwriteExisting });
                }

                return true;
            }
            catch (TargetInvocationException e)
            {
                WeaverLog.LogError($"ItemChanger CreateSettingsProfile threw an exception:\n{e.InnerException ?? e}");
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger CreateSettingsProfile failed:\n{e}");
            }

            return false;
        }

        /// <summary>
        /// Attempts to clone an ItemChanger item by name via Finder.GetItem.
        /// </summary>
        static bool TryGetItem(string itemName, out object item)
        {
            //WeaverLog.Log("TRY GET ITEM = " + itemName);
            item = null;
            if (!IsAvailable || string.IsNullOrEmpty(itemName))
            {
                //WeaverLog.Log("RETURNING FALSE");
                return false;
            }

            try
            {
                var method = FinderType!.GetMethod("GetItem", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                item = method?.Invoke(null, new object[] { itemName });
                //WeaverLog.Log("ITEM = " + item);
                //WeaverLog.Log("FINAL RETURN = " + (item != null));
                return item != null;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger GetItem failed for \"{itemName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to clone an ItemChanger location by name via Finder.GetLocation.
        /// </summary>
        static bool TryGetLocation(string locationName, out object location)
        {
            location = null;
            if (!IsAvailable || string.IsNullOrEmpty(locationName))
            {
                return false;
            }

            try
            {
                var method = FinderType!.GetMethod("GetLocation", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                location = method?.Invoke(null, new object[] { locationName });
                return location != null;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger GetLocation failed for \"{locationName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Wraps a location into a placement using the location's Wrap method.
        /// </summary>
        static bool TryWrapPlacement(object location, out object placement)
        {
            placement = null;
            if (location == null)
            {
                return false;
            }

            try
            {
                var wrapMethod = location.GetType().GetMethod("Wrap", BindingFlags.Public | BindingFlags.Instance);
                placement = wrapMethod?.Invoke(location, null);
                return placement != null;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger location wrap failed for type \"{location.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Adds an item to a placement by calling its Add method.
        /// </summary>
        static bool TryAddItemToPlacement(object placement, object item)
        {
            if (placement == null || item == null)
            {
                return false;
            }

            try
            {
                var addMethod = placement.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);

                addMethod?.Invoke(placement, new[] { item });
                return addMethod != null;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger Add item failed on placement type \"{placement.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        public static bool TryAddPlacements(IEnumerable<ItemChangerPlacement> placements, string conflictResolution = "MergeKeepingNew", bool ensureSettings = true)
        {
            return TryAddPlacements(placements.Select(p => p.InternalPlacement), conflictResolution, ensureSettings);
        }

        /// <summary>
        /// Adds placements to ItemChanger via ItemChangerMod.AddPlacements.
        /// </summary>
        static bool TryAddPlacements(IEnumerable<object> placements, string conflictResolution = "MergeKeepingNew", bool ensureSettings = true)
        {
            if (!IsAvailable)
            {
                return false;
            }

            var placementArray = BuildPlacementArray(placements);
            if (placementArray == null)
            {
                return false;
            }

            if (ensureSettings)
            {
                TryEnsureSettingsProfile();
            }

            try
            {
                var method = ItemChangerModType!.GetMethod("AddPlacements", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    WeaverLog.LogWarning("ItemChanger AddPlacements method could not be located.");
                    return false;
                }

                var conflictValue = GetConflictResolution(conflictResolution);

                method.Invoke(null, new[] { (object)placementArray, conflictValue });
                return true;
            }
            catch (TargetInvocationException e)
            {
                WeaverLog.LogError($"ItemChanger AddPlacements threw an exception:\n{e.InnerException ?? e}");
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger AddPlacements failed:\n{e}");
            }

            return false;
        }

        /// <summary>
        /// Convenience helper that fetches an item and location, wraps the placement, and adds it to ItemChanger.
        /// </summary>
        public static bool TryCreateAndAddPlacement(string itemName, string locationName, string conflictResolution = "MergeKeepingNew", bool ensureSettings = true)
        {
            if (!IsAvailable)
            {
                return false;
            }

            if (ensureSettings)
            {
                TryEnsureSettingsProfile();
            }

            if (!TryGetItem(itemName, out var item) || item == null)
            {
                return false;
            }

            if (!TryGetLocation(locationName, out var location) || location == null)
            {
                return false;
            }

            if (!TryWrapPlacement(location, out var placement) || placement == null)
            {
                return false;
            }

            if (!TryAddItemToPlacement(placement, item))
            {
                return false;
            }

            return TryAddPlacements(new[] { placement }, conflictResolution, false);
        }

        private static Array BuildPlacementArray(IEnumerable<object> placements)
        {
            if (AbstractPlacementType == null)
            {
                WeaverLog.LogWarning("ItemChanger AbstractPlacement type could not be located.");
                return null;
            }

            var placementList = placements?.Where(p => p != null).ToList();
            if (placementList == null || placementList.Count == 0)
            {
                return null;
            }

            var arr = Array.CreateInstance(AbstractPlacementType, placementList.Count);
            for (int i = 0; i < placementList.Count; i++)
            {
                var placement = placementList[i];
                if (!AbstractPlacementType.IsInstanceOfType(placement))
                {
                    WeaverLog.LogWarning($"Object at index {i} is not an ItemChanger placement ({placement?.GetType().FullName ?? "null"}).");
                    return null;
                }

                arr.SetValue(placement, i);
            }

            return arr;
        }

        private static object GetConflictResolution(string conflictResolution)
        {
            if (PlacementConflictType == null)
            {
                return null;
            }

            try
            {
                return Enum.Parse(PlacementConflictType, conflictResolution);
            }
            catch
            {
                WeaverLog.LogWarning($"Unknown ItemChanger PlacementConflictResolution \"{conflictResolution}\". Falling back to MergeKeepingNew.");
                return Enum.Parse(PlacementConflictType, "MergeKeepingNew");
            }
        }

        /// <summary>
        /// Attempts to create a geo Cost via ItemChanger.Cost.NewGeoCost.
        /// </summary>
        static bool TryCreateGeoCost(int amount, out object cost)
        {
            cost = null;
            if (!IsAvailable)
            {
                return false;
            }

            try
            {
                var costType = ItemChangerAssembly?.GetType("ItemChanger.Cost");
                var newGeoCost = costType?.GetMethod("NewGeoCost", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);
                cost = newGeoCost?.Invoke(null, new object[] { amount });
                return cost != null;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger NewGeoCost failed:\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to call AddItemWithCost on a placement, if present.
        /// </summary>
        static bool TryAddItemWithCost(object placement, object item, object cost)
        {
            if (placement == null || item == null)
            {
                return false;
            }

            try
            {
                var method = placement.GetType().GetMethod("AddItemWithCost", BindingFlags.Public | BindingFlags.Instance, null, new[] { item.GetType(), cost?.GetType() ?? typeof(object) }, null)
                             ?? placement.GetType().GetMethod("AddItemWithCost", BindingFlags.Public | BindingFlags.Instance);

                if (method == null)
                {
                    return false;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 2)
                {
                    method.Invoke(placement, new[] { item, cost });
                }
                else
                {
                    method.Invoke(placement, new[] { item });
                }

                return true;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"ItemChanger AddItemWithCost failed on placement type \"{placement.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to set a property on an object by name.
        /// </summary>
        static bool TrySetProperty(object target, string propertyName, object value)
        {
            if (target == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            try
            {
                var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite)
                {
                    return false;
                }

                prop.SetValue(target, value);
                return true;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to set property \"{propertyName}\" on \"{target.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to set a field on an object by name.
        /// </summary>
        static bool TrySetField(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrEmpty(fieldName))
            {
                return false;
            }

            try
            {
                var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    return false;
                }

                field.SetValue(target, value);
                return true;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to set field \"{fieldName}\" on \"{target.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse and set an enum property.
        /// </summary>
        static bool TrySetEnumProperty(object target, string propertyName, string enumTypeFullName, string value)
        {
            if (target == null || string.IsNullOrEmpty(enumTypeFullName) || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            try
            {
                var enumType = ItemChangerAssembly?.GetType(enumTypeFullName);
                if (enumType == null)
                {
                    return false;
                }

                var parsed = Enum.Parse(enumType, value);
                return TrySetProperty(target, propertyName, parsed);
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to set enum property \"{propertyName}\" on \"{target.GetType().FullName}\":\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to clear the Items list on an ItemChanger placement.
        /// </summary>
        static bool TryClearPlacementItems(object placement)
        {
            if (placement == null)
            {
                return false;
            }

            try
            {
                var itemsProp = placement.GetType().GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);
                if (itemsProp == null)
                {
                    return false;
                }

                if (itemsProp.GetValue(placement) is IList list)
                {
                    list.Clear();
                    return true;
                }
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to clear Items on placement type \"{placement.GetType().FullName}\":\n{e}");
            }

            return false;
        }

        /// <summary>
        /// Returns the names of all placements currently in ItemChanger's settings (if available).
        /// </summary>
        public static bool TryGetPlacementNames(out List<string> placementNames)
        {
            placementNames = new List<string>();

            if (!IsAvailable || SettingsType == null || ItemChangerModType == null)
            {
                return false;
            }

            try
            {
                var setField = ItemChangerModType.GetField("SET", BindingFlags.NonPublic | BindingFlags.Static);
                var settings = setField?.GetValue(null);
                if (settings == null)
                {
                    return false;
                }

                var placementsField = SettingsType.GetField("Placements", BindingFlags.Public | BindingFlags.Instance);
                if (placementsField?.GetValue(settings) is IDictionary dict)
                {
                    foreach (DictionaryEntry kv in dict)
                    {
                        if (kv.Key is string name)
                        {
                            placementNames.Add(name);
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to fetch ItemChanger placements: {e}");
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove a placement by name. If settings are loaded, Unload() is called on the placement first.
        /// </summary>
        public static bool TryRemovePlacement(string placementName, bool unloadIfLoaded = true)
        {
            if (string.IsNullOrEmpty(placementName) || !IsAvailable || SettingsType == null || ItemChangerModType == null)
            {
                return false;
            }

            try
            {
                var setField = ItemChangerModType.GetField("SET", BindingFlags.NonPublic | BindingFlags.Static);
                var settings = setField?.GetValue(null);
                if (settings == null)
                {
                    return false;
                }

                var placementsField = SettingsType.GetField("Placements", BindingFlags.Public | BindingFlags.Instance);
                if (placementsField?.GetValue(settings) is IDictionary dict)
                {
                    if (!dict.Contains(placementName))
                    {
                        return false;
                    }

                    var placement = dict[placementName];
                    bool loaded = false;
                    var loadedField = SettingsType.GetField("loaded", BindingFlags.NonPublic | BindingFlags.Static);
                    if (loadedField != null)
                    {
                        loaded = (bool)(loadedField.GetValue(null) ?? false);
                    }

                    if (unloadIfLoaded && loaded && placement != null)
                    {
                        placement.GetType().GetMethod("Unload", BindingFlags.Public | BindingFlags.Instance)?.Invoke(placement, null);
                    }

                    dict.Remove(placementName);
                    return true;
                }
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to remove ItemChanger placement \"{placementName}\": {e}");
            }

            return false;
        }

        /// <summary>
        /// Type-safe handle for an ItemChanger item (AbstractItem clone).
        /// </summary>
        public readonly struct ItemChangerItem
        {
            public object InternalItem { get; }
            public bool IsValid => InternalItem != null;

            private ItemChangerItem(object item) => InternalItem = item;

            public static bool TryGet(string itemName, out ItemChangerItem item)
            {
                item = default;
                if (TryGetItem(itemName, out var obj) && obj != null)
                {
                    item = new ItemChangerItem(obj);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Type-safe handle for an ItemChanger location (AbstractLocation clone).
        /// </summary>
        public readonly struct ItemChangerLocation
        {
            public object InternalLocation { get; }
            public bool IsValid => InternalLocation != null;

            private ItemChangerLocation(object loc) => InternalLocation = loc;

            public static bool TryGet(string locationName, out ItemChangerLocation location)
            {
                location = default;
                if (TryGetLocation(locationName, out var obj) && obj != null)
                {
                    location = new ItemChangerLocation(obj);
                    return true;
                }
                return false;
            }

            public bool TryWrap(out ItemChangerPlacement placement)
            {
                placement = default;
                if (!IsValid)
                {
                    return false;
                }

                if (TryWrapPlacement(InternalLocation, out var p) && p != null)
                {
                    placement = new ItemChangerPlacement(p);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Type-safe handle for an ItemChanger placement (AbstractPlacement subclass).
        /// </summary>
        public readonly struct ItemChangerPlacement
        {
            public object InternalPlacement { get; }
            public bool IsValid => InternalPlacement != null;

            public string PlacementTypeName => InternalPlacement?.GetType().Name ?? string.Empty;
            public bool IsShopPlacement => PlacementTypeName.Contains("ShopPlacement");

            public ItemChangerPlacement(object placement)
            {
                InternalPlacement = placement;
            }

            public bool ClearItems() => TryClearPlacementItems(InternalPlacement);

            public bool AddItem(ItemChangerItem item) => TryAddItemToPlacement(InternalPlacement, item.InternalItem);

            public bool AddItemWithGeoCost(ItemChangerItem item, int geoCost)
            {
                if (!TryCreateGeoCost(geoCost, out var cost) || cost == null)
                {
                    return false;
                }

                return TryAddItemWithCost(InternalPlacement, item.InternalItem, cost)
                    || TryAddItemToPlacement(InternalPlacement, item.InternalItem);
            }

            public bool SetContainerType(string containerType)
            {
                return TrySetField(InternalPlacement, "containerType", containerType)
                    | TrySetProperty(InternalPlacement, "containerType", containerType);
            }

            public bool SetDefaultShopItems(string defaultShopItems)
            {
                return TrySetEnumProperty(InternalPlacement, "defaultShopItems", "ItemChanger.DefaultShopItems", defaultShopItems);
            }
        }
    }
}
