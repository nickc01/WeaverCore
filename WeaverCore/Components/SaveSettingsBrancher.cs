using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Settings;

namespace WeaverCore.Components
{
    /// <summary>
    /// Triggers events based on the values of fields on a <see cref="SaveSpecificSettings"/> object.
    /// </summary>
    public class SaveSettingsBrancher : MonoBehaviour
    {
        [Serializable]
        class ValueContainer<T> : IValueContainer
        {
            public T Value;

            public object ValueRaw
            {
                get => Value;
                set => Value = value is T typedValue ? typedValue : default;
            }
        }

        interface IValueContainer
        {
            object ValueRaw { get; set; }
        }

        public class BranchEventListView : IList<BranchEvent>
        {
            readonly SaveSettingsBrancher source;

            public BranchEventListView(SaveSettingsBrancher source) => this.source = source;

            public BranchEvent this[int index]
            {
                get => new BranchEvent(source.fieldNames[index], source.fieldTypeNames[index], source.expectedValueJson[index], source.onExpectedValues[index]);
                set
                {
                    source.fieldNames[index] = value.FieldName;
                    source.fieldTypeNames[index] = value.FieldTypeName;
                    source.expectedValueJson[index] = value.ExpectedValueJson;
                    source.onExpectedValues[index] = value.OnExpectedValue ?? new UnityEvent();
                }
            }

            public int Count => source.fieldNames.Count;
            public bool IsReadOnly => false;

            public void Add(BranchEvent item)
            {
                source.fieldNames.Add(item.FieldName);
                source.fieldTypeNames.Add(item.FieldTypeName);
                source.expectedValueJson.Add(item.ExpectedValueJson);
                source.onExpectedValues.Add(item.OnExpectedValue ?? new UnityEvent());
            }

            public void Clear()
            {
                source.fieldNames.Clear();
                source.fieldTypeNames.Clear();
                source.expectedValueJson.Clear();
                source.onExpectedValues.Clear();
            }

            public bool Contains(BranchEvent item) => IndexOf(item) >= 0;

            public void CopyTo(BranchEvent[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[arrayIndex + i] = this[i];
                }
            }

            public IEnumerator<BranchEvent> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            public int IndexOf(BranchEvent item)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (source.fieldNames[i] == item.FieldName &&
                        source.fieldTypeNames[i] == item.FieldTypeName &&
                        source.expectedValueJson[i] == item.ExpectedValueJson &&
                        ReferenceEquals(source.onExpectedValues[i], item.OnExpectedValue))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public void Insert(int index, BranchEvent item)
            {
                source.fieldNames.Insert(index, item.FieldName);
                source.fieldTypeNames.Insert(index, item.FieldTypeName);
                source.expectedValueJson.Insert(index, item.ExpectedValueJson);
                source.onExpectedValues.Insert(index, item.OnExpectedValue ?? new UnityEvent());
            }

            public bool Remove(BranchEvent item)
            {
                int index = IndexOf(item);
                if (index < 0)
                {
                    return false;
                }

                RemoveAt(index);
                return true;
            }

            public void RemoveAt(int index)
            {
                source.fieldNames.RemoveAt(index);
                source.fieldTypeNames.RemoveAt(index);
                source.expectedValueJson.RemoveAt(index);
                source.onExpectedValues.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class BranchEvent
        {
            public string FieldName;
            public string FieldTypeName;
            public string ExpectedValueJson;
            public UnityEvent OnExpectedValue;

            public BranchEvent(string fieldName, string fieldTypeName, string expectedValueJson, UnityEvent onExpectedValue)
            {
                FieldName = fieldName;
                FieldTypeName = fieldTypeName;
                ExpectedValueJson = expectedValueJson;
                OnExpectedValue = onExpectedValue;
            }
        }

        static readonly Dictionary<Type, List<FieldInfo>> SupportedFieldCache = new Dictionary<Type, List<FieldInfo>>();

        [SerializeField]
        [Tooltip("The save settings asset whose registered runtime instance will be inspected.")]
        SaveSpecificSettings saveSettings;

        [SerializeField]
        [Tooltip("If enabled, Activate() is called automatically on Start.")]
        bool runOnStart = true;

        [SerializeField]
        [HideInInspector]
        List<string> fieldNames = new List<string>();

        [SerializeField]
        [HideInInspector]
        List<string> fieldTypeNames = new List<string>();

        [SerializeField]
        [HideInInspector]
        List<string> expectedValueJson = new List<string>();

        [SerializeField]
        [HideInInspector]
        List<UnityEvent> onExpectedValues = new List<UnityEvent>();

        BranchEventListView events;

        public SaveSpecificSettings SaveSettings
        {
            get => saveSettings;
            set => saveSettings = value;
        }

        public bool RunOnStart
        {
            get => runOnStart;
            set => runOnStart = value;
        }

        public IList<BranchEvent> Events => events ??= new BranchEventListView(this);

        void Start()
        {
            if (runOnStart)
            {
                Activate();
            }
        }

        public void Activate()
        {
            var settingsInstance = ResolveSettingsInstance();
            if (settingsInstance == null)
            {
                return;
            }

            var settingsType = settingsInstance.GetType();
            int branchCount = Math.Min(Math.Min(fieldNames.Count, fieldTypeNames.Count), Math.Min(expectedValueJson.Count, onExpectedValues.Count));

            for (int i = 0; i < branchCount; i++)
            {
                var fieldName = fieldNames[i];
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    continue;
                }

                var field = GetSupportedField(settingsType, fieldName);
                if (field == null)
                {
                    Debug.LogWarning($"Unable to find save settings field '{fieldName}' on '{settingsType.FullName}' for '{name}'", this);
                    continue;
                }

                if (FieldMatches(field, settingsInstance, expectedValueJson[i]))
                {
                    onExpectedValues[i]?.Invoke();
                }
            }
        }

        SaveSpecificSettings ResolveSettingsInstance()
        {
            if (saveSettings == null)
            {
                Debug.LogWarning($"'{nameof(SaveSettingsBrancher)}' on '{name}' does not have a save settings object assigned", this);
                return null;
            }

            return SaveSpecificSettings.GetSaveSettings(saveSettings.GetType()) ?? saveSettings;
        }

        bool FieldMatches(FieldInfo field, SaveSpecificSettings settingsInstance, string expectedJson)
        {
            if (!TryDeserializeValue(expectedJson, field.FieldType, out var expectedValue))
            {
                Debug.LogWarning($"Unable to parse expected value for field '{field.Name}' on '{GetType().Name}' attached to '{name}'", this);
                return false;
            }

            var actualValue = field.GetValue(settingsInstance);
            return string.Equals(SerializeValue(actualValue, field.FieldType), SerializeValue(expectedValue, field.FieldType), StringComparison.Ordinal);
        }

        public static FieldInfo GetSupportedField(Type settingsType, string fieldName)
        {
            if (settingsType == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            var fields = GetSupportedFields(settingsType);
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Name == fieldName)
                {
                    return fields[i];
                }
            }

            return null;
        }

        public static IReadOnlyList<FieldInfo> GetSupportedFields(Type settingsType)
        {
            if (settingsType == null)
            {
                return Array.Empty<FieldInfo>();
            }

            if (!SupportedFieldCache.TryGetValue(settingsType, out var fields))
            {
                fields = CollectSupportedFields(settingsType);
                SupportedFieldCache[settingsType] = fields;
            }

            return fields;
        }

        static List<FieldInfo> CollectSupportedFields(Type settingsType)
        {
            var results = new List<FieldInfo>();
            var addedNames = new HashSet<string>();
            var currentType = settingsType;

            while (currentType != null && currentType != typeof(ScriptableObject) && currentType != typeof(UnityEngine.Object) && currentType != typeof(object))
            {
                foreach (var field in currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!addedNames.Contains(field.Name) && IsSupportedSerializableField(field))
                    {
                        addedNames.Add(field.Name);
                        results.Add(field);
                    }
                }

                currentType = currentType.BaseType;
            }

            return results;
        }

        public static bool IsSupportedSerializableField(FieldInfo field)
        {
            if (field == null || field.IsStatic || field.IsLiteral || field.IsInitOnly)
            {
                return false;
            }

            if (field.IsDefined(typeof(NonSerializedAttribute), true))
            {
                return false;
            }

            if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true))
            {
                return false;
            }

            return IsSupportedFieldType(field.FieldType);
        }

        public static bool IsSupportedFieldType(Type fieldType)
        {
            if (fieldType == null)
            {
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType) || typeof(Delegate).IsAssignableFrom(fieldType) || fieldType.IsPointer || fieldType == typeof(decimal))
            {
                return false;
            }

            if (fieldType == typeof(string) || fieldType.IsPrimitive || fieldType.IsEnum)
            {
                return true;
            }

            if (fieldType.IsArray)
            {
                return fieldType.GetArrayRank() == 1 && IsSupportedFieldType(fieldType.GetElementType());
            }

            if (IsListType(fieldType, out var elementType))
            {
                return IsSupportedFieldType(elementType);
            }

            if (fieldType.IsGenericType)
            {
                return false;
            }

            if (fieldType.IsValueType)
            {
                return fieldType.IsSerializable || fieldType.IsDefined(typeof(SerializableAttribute), false);
            }

            return fieldType.IsClass && fieldType.IsDefined(typeof(SerializableAttribute), false);
        }

        static bool IsListType(Type fieldType, out Type elementType)
        {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = fieldType.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        public static object GetDefaultValue(Type fieldType)
        {
            if (fieldType == null)
            {
                return null;
            }

            return fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
        }

        public static string SerializeValue(object value, Type fieldType)
        {
            if (fieldType == null)
            {
                return string.Empty;
            }

            var container = CreateValueContainer(value, fieldType);
            return JsonUtility.ToJson(container);
        }

        public static bool TryDeserializeValue(string json, Type fieldType, out object value)
        {
            value = GetDefaultValue(fieldType);

            if (fieldType == null || string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var containerType = typeof(ValueContainer<>).MakeGenericType(fieldType);
                if (!(JsonUtility.FromJson(json, containerType) is IValueContainer container))
                {
                    return false;
                }

                value = container.ValueRaw;
                return true;
            }
            catch
            {
                return false;
            }
        }

        static IValueContainer CreateValueContainer(object value, Type fieldType)
        {
            var containerType = typeof(ValueContainer<>).MakeGenericType(fieldType);
            var container = (IValueContainer)Activator.CreateInstance(containerType);
            container.ValueRaw = value;
            return container;
        }

        public static string GetFriendlyTypeName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (type.IsArray)
            {
                return $"{GetFriendlyTypeName(type.GetElementType())}[]";
            }

            if (IsListType(type, out var elementType))
            {
                return $"List<{GetFriendlyTypeName(elementType)}>";
            }

            return type.Name;
        }
    }
}
