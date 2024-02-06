using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    ///  Used for updating many fields at once on GameObjects
    /// </summary>
    [CreateAssetMenu(fileName = "FieldUpdater", menuName = "WeaverCore/Field Updater")]
    public class FieldUpdater : ScriptableObject
    {
        /// <summary>
        /// List view for accessing and modifying updated fields.
        /// </summary>
        public class UpdatedFieldListView : IList<UpdatedField>
        {
            /// <summary>
            /// Source <see cref="FieldUpdater"/> for the list view.
            /// </summary>
            public readonly FieldUpdater SourceUpdater;

            /// <summary>
            /// Creates an instance of <see cref="UpdatedFieldListView"/>.
            /// </summary>
            /// <param name="sourceUpdater">Source <see cref="FieldUpdater"/> for the list view.</param>
            public UpdatedFieldListView(FieldUpdater sourceUpdater)
            {
                SourceUpdater = sourceUpdater;
            }

            /// <inheritdoc/>
            public UpdatedField this[int index]
            {
                get
                {
                    return new UpdatedField(SourceUpdater.componentTypeNames[index],
                        SourceUpdater.fieldNames[index],
                        SourceUpdater.fieldTypes[index],
                        SourceUpdater.fieldValueContainers[index],
                        SourceUpdater);
                }
                set
                {
                    SourceUpdater.componentTypeNames[index] = value.ComponentTypeName;
                    SourceUpdater.fieldNames[index] = value.FieldName;
                    SourceUpdater.fieldTypes[index] = value.FieldType;
                    SourceUpdater.fieldValueContainers[index] = value.FieldValueJson;
                }
            }

            /// <inheritdoc/>
            public int Count => SourceUpdater.componentTypeNames.Count;

            /// <inheritdoc/>
            public bool IsReadOnly => false;

            /// <inheritdoc/>
            public void Add(UpdatedField item)
            {
                SourceUpdater.componentTypeNames.Add(item.ComponentTypeName);
                SourceUpdater.fieldNames.Add(item.FieldName);
                SourceUpdater.fieldTypes.Add(item.FieldType);
                SourceUpdater.fieldValueContainers.Add(item.FieldValueJson);
            }

            /// <inheritdoc/>
            public void Clear()
            {
                SourceUpdater.componentTypeNames.Clear();
                SourceUpdater.fieldNames.Clear();
                SourceUpdater.fieldTypes.Clear();
                SourceUpdater.fieldValueContainers.Clear();
            }

            /// <inheritdoc/>
            public bool Contains(UpdatedField item)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (SourceUpdater.componentTypeNames[i] == item.ComponentTypeName &&
                        SourceUpdater.fieldNames[i] == item.FieldName &&
                        SourceUpdater.fieldTypes[i] == item.FieldType &&
                        SourceUpdater.fieldValueContainers[i] == item.FieldValueJson)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <inheritdoc/>
            public void CopyTo(UpdatedField[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i + arrayIndex] = this[i];
                }
            }

            /// <inheritdoc/>
            public IEnumerator<UpdatedField> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            /// <inheritdoc/>
            public int IndexOf(UpdatedField item)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (SourceUpdater.componentTypeNames[i] == item.ComponentTypeName &&
                        SourceUpdater.fieldNames[i] == item.FieldName &&
                        SourceUpdater.fieldTypes[i] == item.FieldType &&
                        SourceUpdater.fieldValueContainers[i] == item.FieldValueJson)
                    {
                        return i;
                    }
                }

                return -1;
            }

            /// <inheritdoc/>
            public void Insert(int index, UpdatedField item)
            {
                SourceUpdater.componentTypeNames.Insert(index, item.ComponentTypeName);
                SourceUpdater.fieldNames.Insert(index, item.FieldName);
                SourceUpdater.fieldTypes.Insert(index, item.FieldType);
                SourceUpdater.fieldValueContainers.Insert(index, item.FieldValueJson);
            }

            /// <inheritdoc/>
            public bool Remove(UpdatedField item)
            {
                var index = IndexOf(item);

                if (index >= 0)
                {
                    RemoveAt(index);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <inheritdoc/>
            public void RemoveAt(int index)
            {
                SourceUpdater.componentTypeNames.RemoveAt(index);
                SourceUpdater.fieldNames.RemoveAt(index);
                SourceUpdater.fieldTypes.RemoveAt(index);
                SourceUpdater.fieldValueContainers.RemoveAt(index);
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }
        }

        string GetOrAddUnityObj(UnityEngine.Object obj)
        {
            var index = reservedObjects.IndexOf(obj);
            if (index >= 0)
            {
                return reservedObjectGUIDs[index];
            }
            else
            {
                reservedObjects.Add(obj);
                var guid = Guid.NewGuid().ToString();
                reservedObjectGUIDs.Add(guid);
                return guid;
            }
        }

        bool RemoveUnityObj(UnityEngine.Object obj)
        {
            var index = reservedObjects.IndexOf(obj);

            if (index >= 0)
            {
                reservedObjects.RemoveAt(index);
                reservedObjectGUIDs.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        [Serializable]
		public class UpdatedField : IEquatable<UpdatedField>
		{
            public interface IContainer
            {
                object ValueRaw { get; set; }
            }

            [Serializable]
            public struct Container<T> : IContainer
            {
                public T Value;

                public Container(T value)
                {
                    Value = value;
                }

                public object ValueRaw { get => Value; set => Value = (T)value; }
            }

            public string ComponentTypeName;
			public string FieldName;
			public string FieldType;
			public string FieldValueJson;

            public object GetSourceValue()
            {
                return sourceUpdater.GetFieldValue(this);
            }

            [NonSerialized]
            FieldUpdater sourceUpdater;

            public UpdatedField(string componentTypeName, string fieldName, string fieldType, string fieldValueContainer, FieldUpdater fieldUpdater)
            {
                ComponentTypeName = componentTypeName;
                FieldName = fieldName;
                FieldType = fieldType;
                FieldValueJson = fieldValueContainer;
                sourceUpdater = fieldUpdater;
            }


            public UpdatedField(Type componentType, MemberInfo member, string valueJson, FieldUpdater fieldUpdater)
            {
                Type memberType;

                string memberInfo;
                if (member is FieldInfo field)
                {
                    memberInfo = "Field";
                    memberType = field.FieldType;
                }
                else if (member is PropertyInfo property)
                {
                    memberInfo = "Property";
                    memberType = property.PropertyType;
                }
                else
                {
                    throw new Exception("member parameter must be a FieldInfo or PropertyInfo");
                }

                ComponentTypeName = $"{componentType.Assembly.GetName().Name}:{componentType.FullName}";
                FieldName = $"{memberInfo}:{member.Name}";
                FieldType = $"{memberType.Assembly.GetName().Name}:{memberType.FullName}";

                FieldValueJson = valueJson;

                sourceUpdater = fieldUpdater;
            }

            public Type ComponentType
            {
                get
                {
                    var nameSplit = ComponentTypeName.Split(':');
                    return TypeUtilities.NameToType(nameSplit[1], nameSplit[0]);
                }
            }

            public Type MemberType
            {
                get
                {
                    var nameSplit = FieldType.Split(':');
                    return TypeUtilities.NameToType(nameSplit[1], nameSplit[0]);
                }
            }

            public MemberInfo Member
            {
                get
                {
                    var nameSplit = FieldName.Split(':');
                    var currentType = ComponentType;
                    while (currentType != null && currentType != typeof(Component))
                    {
                        MemberInfo foundMember;
                        if (nameSplit[0] == "Field")
                        {
                            foundMember = currentType.GetField(nameSplit[1], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        }
                        else
                        {
                            foundMember = currentType.GetProperty(nameSplit[1], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        }

                        if (foundMember != null)
                        {
                            return foundMember;
                        }
                        else
                        {
                            currentType = currentType.BaseType;
                        }
                    }

                    return null;
                }
            }

            public bool Equals(UpdatedField other)
            {
                if (this == null && other == null)
                {
                    return true;
                }
                if (other == null)
                {
                    return false;
                }
                return ComponentTypeName == other.ComponentTypeName &&
                    FieldName == other.FieldName &&
                    FieldType == other.FieldType &&
                    FieldValueJson == other.FieldValueJson;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as UpdatedField);
            }

            public override int GetHashCode()
            {
                int hashCode = 1108266081;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ComponentTypeName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FieldName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FieldType);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FieldValueJson);
                return hashCode;
            }

            public static bool operator ==(UpdatedField a, UpdatedField b)
            {
                if (a == null && b == null)
                {
                    return true;
                }
                if (b == null)
                {
                    return false;
                }
                if (a == null)
                {
                    return false;
                }
                return a.ComponentTypeName == b.ComponentTypeName &&
                    a.FieldName == b.FieldName &&
                    a.FieldType == b.FieldType &&
                    a.FieldValueJson == b.FieldValueJson;
            }

            public static bool operator !=(UpdatedField a, UpdatedField b)
            {
                if (a == null && b == null)
                {
                    return false;
                }
                if (b == null)
                {
                    return true;
                }
                if (a == null)
                {
                    return true;
                }
                return a.ComponentTypeName != b.ComponentTypeName &&
                    a.FieldName != b.FieldName &&
                    a.FieldType != b.FieldType &&
                    a.FieldValueJson != b.FieldValueJson;
            }

            public static IContainer RawCreateJsonContainer(object obj, Type objType)
            {
                var containerType = typeof(Container<>).MakeGenericType(objType ?? obj.GetType());

                return (IContainer)Activator.CreateInstance(containerType, obj);
            }

            public static IContainer RawGetJsonValue(string containerJson, Type objType)
            {
                var containerType = typeof(Container<>).MakeGenericType(objType);

                return (IContainer)JsonUtility.FromJson(containerJson, containerType);
            }
        }

        UnityEngine.Object GetObjectByGUID(string guid)
        {
            if (guid == "NULL")
            {
                return null;
            }
            else
            {
                var guidIndex = reservedObjectGUIDs.IndexOf(guid);

                if (guidIndex >= 0)
                {
                    return reservedObjects[guidIndex];
                }
                else
                {
                    return null;
                }

            }
        }

        public object GetFieldValue(UpdatedField field)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.MemberType))
            {
                var split = field.FieldValueJson.Split(':');
                return GetObjectByGUID(split[1]);
            }
            else
            {
                if (typeof(Enum).IsAssignableFrom(field.MemberType))
                {
                    return Enum.Parse(field.MemberType, field.FieldValueJson);
                }
                else
                {
                    return FieldUpdater.UpdatedField.RawGetJsonValue(field.FieldValueJson, field.MemberType).ValueRaw;
                }
            }
        }

        /// <summary>
        /// Applies the Field Updater to a <see cref="GameObject"/>.
        /// </summary>
        /// <param name="obj">The object to have its component fields changed.</param>
        /// <param name="throwOnError">If set to true, then this function will throw an exception if a field could not be set, or a component is missing.</param>
        public void ApplyToObject(GameObject obj, bool throwOnError = true)
        {
            foreach (var field in Fields)
            {
                try
                {
                    if (obj.TryGetComponent(field.ComponentType, out var component))
                    {
                        var member = field.Member;

                        try
                        {
                            if (member != null)
                            {
                                WeaverLog.Log($"Setting Field {member.Name} of {field.ComponentType.Name}");
                                if (member is FieldInfo fieldInfo)
                                {
                                    fieldInfo.SetValue(component, field.GetSourceValue());
                                }
                                else if (member is PropertyInfo propertyInfo)
                                {
                                    propertyInfo.SetValue(component, field.GetSourceValue());
                                }
                            }
                            else if (throwOnError)
                            {
                                throw new Exception($"Could not find {field.FieldName.Split(':')[0].ToLower()} \"{field.FieldName.Split(':')[1]}\" on component {field.ComponentType.FullName}");
                            }
                        }
                        catch (Exception e)
                        {
                            WeaverLog.Log($"Failed Setting Field {member.Name} of {field.ComponentType.Name}");
                            WeaverLog.LogException(e);
                            throw;
                        }
                    }
                    else if (throwOnError)
                    {
                        throw new Exception($"Could not find component {field.ComponentType.FullName} on object {obj.name} in order to modify the {field.FieldName.Split(':')[0].ToLower()} \"{field.FieldName.Split(':')[1]}\"");
                    }
                }
                catch (Exception e)
                {
                    WeaverLog.LogException(e);
                    throw;
                }
            }
        }

        [SerializeField]
        List<string> componentTypeNames = new List<string>();

        [SerializeField]
        List<string> fieldNames = new List<string>();

        [SerializeField]
        List<string> fieldTypes = new List<string>();

        [SerializeField]
        List<string> fieldValueContainers = new List<string>();

        UpdatedFieldListView _listView;
        public UpdatedFieldListView Fields => _listView ??= new UpdatedFieldListView(this);

        [SerializeField]
        List<string> reservedObjectGUIDs = new List<string>();

        [SerializeField]
        List<UnityEngine.Object> reservedObjects = new List<UnityEngine.Object>();
    }
}
