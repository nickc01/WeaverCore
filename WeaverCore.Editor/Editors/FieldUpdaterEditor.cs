using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore;

[CustomEditor(typeof(FieldUpdater))]
public class FieldUpdaterEditor : Editor
{
    public enum UpdateState
    {
        None,
        Update,
        Remove
    };

    static List<Type> _allTypes;

    static List<Type> AllTypes => _allTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(Component).IsAssignableFrom(t) && !t.ContainsGenericParameters).ToList();

    Vector2 typeSelectScrollPos = default;
    float typeSelectMaxScrollSize = 200f;

    Vector2 fieldSelectScrollPos = default;
    float fieldSelectMaxScrollSize = 200f;

    string typeString = "";
    Type selectedType;
    List<MemberInfo> members;

    string memberString = "";
    MemberInfo selectedMember;
    Type selectedMemberType;
    object newMemberValue;

    private void OnEnable()
    {
        //EditorGUI.BeginChangeCheck();

        //WeaverLog.Log("CHECKING RESERVED OBJECTS");
        if (CheckReservedObjects() > 0)
        {
            WeaverLog.Log("APPLYING MODIFIED PROPERTIES");
            serializedObject.ApplyModifiedProperties();
        }

        /*if (EditorGUI.EndChangeCheck())
        {
            WeaverLog.Log("APPLYING MODIFIED PROPERTIES");
            
        }*/
    }

    int CheckReservedObjects()
    {
        List<string> validGUIDs = new List<string>();
        foreach (var field in GetAll())
        {
            if (field.FieldValueJson.StartsWith("GUID"))
            {
                var split = field.FieldValueJson.Split(':');

                if (GetIndexOfGUID(split[1]) >= 0)
                {
                    validGUIDs.Add(split[1]);
                }
                else
                {
                    field.FieldValueJson = "GUID:NULL";
                }
            }
        }
        //WeaverLog.Log("VALID GUIDS = " + validGUIDs.Count);
        return RemoveGUIDByPredicate(s => !validGUIDs.Contains(s));
    }

    private void OnDisable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        //EditorGUI.BeginChangeCheck();

        bool updated = false;
        bool removed = false;

        if (GetCount() > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));

            int index = 0;
            foreach (var field in GetAll())
            {
                switch (DisplayUpdatedField(field))
                {
                    case UpdateState.Update:
                        updated = true;
                        UpdateField(index, field);
                        break;
                    case UpdateState.Remove:
                        Remove(index);
                        removed = true;
                        updated = true;
                        index--;
                        break;
                }
                index++;
            }

            if (removed)
            {
                if (CheckReservedObjects() > 0)
                {
                    updated = true;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

        }

        if (DisplayFieldUpdateCreator(out var newField))
        {
            //WeaverLog.Log("UPDATED FIELD = " + JsonUtility.ToJson(newField));
            updated = true;
            Add(newField);
        }

        if (updated)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    IEnumerable<FieldUpdater.UpdatedField> GetAll()
    {
        var count = GetCount();
        for (int i = 0; i < count; i++)
        {
            yield return Get(i);
            count = GetCount();
        }
    }

    FieldUpdater.UpdatedField Get(int index)
    {
        var componentTypeName = serializedObject.FindProperty("componentTypeNames").GetArrayElementAtIndex(index).stringValue;
        var fieldName = serializedObject.FindProperty("fieldNames").GetArrayElementAtIndex(index).stringValue;
        var fieldType = serializedObject.FindProperty("fieldTypes").GetArrayElementAtIndex(index).stringValue;
        var fieldValueContainer = serializedObject.FindProperty("fieldValueContainers").GetArrayElementAtIndex(index).stringValue;

        return new FieldUpdater.UpdatedField(componentTypeName, fieldName, fieldType, fieldValueContainer, null);
    }

    int GetCount()
    {
        return serializedObject.FindProperty("componentTypeNames").arraySize;
    }

    void Add(FieldUpdater.UpdatedField newField)
    {
        var count = GetCount();
        serializedObject.FindProperty("componentTypeNames").InsertArrayElementAtIndex(count);
        serializedObject.FindProperty("componentTypeNames").GetArrayElementAtIndex(count).stringValue = newField.ComponentTypeName;

        serializedObject.FindProperty("fieldNames").InsertArrayElementAtIndex(count);
        serializedObject.FindProperty("fieldNames").GetArrayElementAtIndex(count).stringValue = newField.FieldName;

        serializedObject.FindProperty("fieldTypes").InsertArrayElementAtIndex(count);
        serializedObject.FindProperty("fieldTypes").GetArrayElementAtIndex(count).stringValue = newField.FieldType;

        serializedObject.FindProperty("fieldValueContainers").InsertArrayElementAtIndex(count);
        serializedObject.FindProperty("fieldValueContainers").GetArrayElementAtIndex(count).stringValue = newField.FieldValueJson;
    }

    void UpdateField(int index, FieldUpdater.UpdatedField newField)
    {
        serializedObject.FindProperty("componentTypeNames").GetArrayElementAtIndex(index).stringValue = newField.ComponentTypeName;
        serializedObject.FindProperty("fieldNames").GetArrayElementAtIndex(index).stringValue = newField.FieldName;
        serializedObject.FindProperty("fieldTypes").GetArrayElementAtIndex(index).stringValue = newField.FieldType;
        serializedObject.FindProperty("fieldValueContainers").GetArrayElementAtIndex(index).stringValue = newField.FieldValueJson;
    }

    bool Remove(int index)
    {
        if (index >= 0 && index < GetCount())
        {
            serializedObject.FindProperty("componentTypeNames").DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("fieldNames").DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("fieldTypes").DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("fieldValueContainers").DeleteArrayElementAtIndex(index);
            return true;
        }
        return false;
    }

    UpdateState DisplayUpdatedField(FieldUpdater.UpdatedField field)
    {
        //WeaverLog.Log("FIELD DATA = " + field.FieldValueJson);
        UpdateState updateState = UpdateState.None;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(field.ComponentTypeName.Split(':')[1], GUILayout.MaxWidth(250f));
        //var oldValue = field.NewFieldValue;

        object oldValue;

        if (typeof(UnityEngine.Object).IsAssignableFrom(field.MemberType))
        {
            var split = field.FieldValueJson.Split(':');
            /*if (split[1] == "NULL")
            {
                oldValue = null;
            }
            else
            {
                
            }*/
            oldValue = GetObjectByGUID(split[1]);
        }
        else
        {
            oldValue = FieldUpdater.UpdatedField.RawGetJsonValue(field.FieldValueJson, field.MemberType).ValueRaw;
        }

        //var oldValue = FieldUpdater.UpdatedField.RawGetValue(field.FieldValueJson, field.MemberType);
        //EditorGUILayout.LabelField(field.FieldName, GUILayout.MaxWidth(175f));
        var newValue = DisplayGenericField(field.FieldName, oldValue, field.MemberType);

        if (oldValue != newValue)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.MemberType))
            {
                RemoveUnityObj(oldValue as UnityEngine.Object);
                field.FieldValueJson = $"GUID:{GetOrAddUnityObj(newValue as UnityEngine.Object)}";
            }
            else
            {
                field.FieldValueJson = JsonUtility.ToJson(FieldUpdater.UpdatedField.RawCreateJsonContainer(newValue, field.MemberType));
            }
            //
            updateState = UpdateState.Update;
        }

        if (GUILayout.Button("X", GUILayout.MaxWidth(45f)))
        {
            updateState = UpdateState.Remove;
        }

        EditorGUILayout.EndHorizontal();

        return updateState;
    }

    bool DisplayFieldUpdateCreator(out FieldUpdater.UpdatedField field)
    {
        bool fieldCreated = false;
        field = default;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));

        EditorGUILayout.LabelField("Add Field");
        EditorGUILayout.Space();
        typeString = EditorGUILayout.TextField("Search for Component: ", typeString);

        typeSelectScrollPos = EditorGUILayout.BeginScrollView(typeSelectScrollPos, GUILayout.MaxHeight(typeSelectMaxScrollSize));

        int count = 0;
        const int max = 30;

        //foreach (var type in AllTypes)
        for (int i = 0; i < AllTypes.Count; i++)
        {
            var type = AllTypes[i];
            if (type.FullName.ToLower().Contains(typeString.ToLower()))
            {
                if (GUILayout.Button(type.FullName))
                {
                    typeString = type.FullName;
                    selectedType = type;
                    members = new List<MemberInfo>();
                    HashSet<string> addedFields = new HashSet<string>();

                    var currentType = selectedType;

                    while (currentType != null && currentType != typeof(Component))
                    {
                        WeaverLog.Log("CURRENT TYPE = " + currentType);
                        foreach (var newField in currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(f => IsDisplayableField(f.FieldType)))
                        {
                            WeaverLog.Log("FOUND FIELD = " + newField.Name);
                            if (!addedFields.Contains($"field_{newField.Name}"))
                            {
                                addedFields.Add($"field_{newField.Name}");
                                members.Add(newField);
                            }
                        }

                        foreach (var newProperty in currentType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(p => IsDisplayableField(p.PropertyType) && p.GetSetMethod() != null))
                        {
                            WeaverLog.Log("FOUND PROPERTY = " + newProperty.Name);
                            if (!addedFields.Contains($"property_{newProperty.Name}"))
                            {
                                addedFields.Add($"property_{newProperty.Name}");
                                members.Add(newProperty);
                            }
                        }

                        currentType = currentType.BaseType;
                    }

                    foreach (var member in members)
                    {
                        WeaverLog.Log($"MEMBER = {member.DeclaringType.FullName}:{member.Name}");
                    }

                    /*members.AddRange(selectedType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(f => IsDisplayableField(f.FieldType)));
                    members.AddRange(selectedType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(p => IsDisplayableField(p.PropertyType) && p.GetSetMethod() != null));*/

                    //members.RemoveAll(m => !IsDisplayableField(m.));
                    //members = selectedType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }
                count++;
                if (count >= max)
                {
                    break;
                }
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));
        EditorGUILayout.LabelField($"Selected Type: {selectedType?.FullName ?? ""}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();


        if (selectedType != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));
            memberString = EditorGUILayout.TextField("Search for Field To Modify: ", memberString);

            fieldSelectScrollPos = EditorGUILayout.BeginScrollView(fieldSelectScrollPos, GUILayout.MaxHeight(fieldSelectMaxScrollSize));

            count = 0;

            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];

                if (!member.Name.ToLower().Contains(memberString.ToLower()))
                {
                    continue;
                }

                string memberDisplayName;
                Type memberType;
                if (member is PropertyInfo propertyInfo)
                {
                    memberDisplayName = $"(Property) {propertyInfo.Name}";
                    memberType = propertyInfo.PropertyType;
                }
                else if (member is FieldInfo fieldInfo)
                {
                    memberDisplayName = $"(Field) {fieldInfo.Name}";
                    memberType = fieldInfo.FieldType;
                }
                else
                {
                    continue;
                }

                if (GUILayout.Button(memberDisplayName))
                {
                    selectedMember = member;
                    memberString = member.Name;
                    selectedMemberType = memberType;
                    newMemberValue = memberType.IsValueType ? Activator.CreateInstance(memberType) : null;
                }
                count++;
                if (count >= max)
                {
                    break;
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            if (selectedMemberType != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));
                newMemberValue = DisplayGenericField($"New Value for {selectedMember.Name}: ", newMemberValue, selectedMemberType);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0f));
            if (GUILayout.Button("Add Field"))
            {
                /*string memberInfo = "Field";

                if (selectedMember is PropertyInfo propertyInfo)
                {
                    memberInfo = "Property";
                }*/

                string valueContainer = "";

                if (typeof(UnityEngine.Object).IsAssignableFrom(selectedMemberType))
                {
                    valueContainer = $"GUID:{GetOrAddUnityObj(newMemberValue as UnityEngine.Object)}";
                }
                else
                {
                    valueContainer = JsonUtility.ToJson(FieldUpdater.UpdatedField.RawCreateJsonContainer(newMemberValue, selectedMemberType));
                }


                field = new FieldUpdater.UpdatedField(selectedType, selectedMember, valueContainer, null);
                fieldCreated = true;
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        return fieldCreated;
    }

    UnityEngine.Object GetObjectByGUID(string guid)
    {
        if (guid == "NULL")
        {
            return null;
        }
        else
        {
            var reservedObjectGUIDs = serializedObject.FindProperty("reservedObjectGUIDs");
            var reservedObjects = serializedObject.FindProperty("reservedObjects");

            for (int i = reservedObjectGUIDs.arraySize - 1; i >= 0; i--)
            {
                if (reservedObjectGUIDs.GetArrayElementAtIndex(i).stringValue == guid)
                {
                    return reservedObjects.GetArrayElementAtIndex(i).objectReferenceValue;
                }
            }

            return null;
        }
    }

    string GetOrAddUnityObj(UnityEngine.Object obj)
    {
        if (obj == null)
        {
            return "NULL";
        }
        var reservedObjects = serializedObject.FindProperty("reservedObjects");

        int index = -1;
        for (int i = reservedObjects.arraySize - 1; i >= 0; i--)
        {
            if (reservedObjects.GetArrayElementAtIndex(i).objectReferenceValue == obj)
            {
                index = i;
                break;
            }
        }

        //var index = reservedObjects.IndexOf(obj);
        if (index >= 0)
        {
            //return reservedObjectGUIDs[index];
            return serializedObject.FindProperty("reservedObjectGUIDs").GetArrayElementAtIndex(index).stringValue;
        }
        else
        {
            var guid = Guid.NewGuid().ToString();
            var reservedGUIDs = serializedObject.FindProperty("reservedObjectGUIDs");

            reservedGUIDs.InsertArrayElementAtIndex(reservedObjects.arraySize);
            reservedGUIDs.GetArrayElementAtIndex(reservedObjects.arraySize).stringValue = guid;

            reservedObjects.InsertArrayElementAtIndex(reservedObjects.arraySize);
            reservedObjects.GetArrayElementAtIndex(reservedObjects.arraySize - 1).objectReferenceValue = obj;
            //reservedObjects.Add(obj);
            //var guid = Guid.NewGuid().ToString();
            //reservedObjectGUIDs.Add(guid);
            return guid;
        }
    }

    int GetIndexOfGUID(string guid)
    {
        var reservedObjectGUIDs = serializedObject.FindProperty("reservedObjectGUIDs");

        for (int i = reservedObjectGUIDs.arraySize - 1; i >= 0; i--)
        {
            if (reservedObjectGUIDs.GetArrayElementAtIndex(i).stringValue == guid)
            {
                return i;
            }
        }
        return -1;
    }

    int RemoveGUIDByPredicate(Predicate<string> predicate)
    {
        int removedCount = 0;
        var reservedObjects = serializedObject.FindProperty("reservedObjects");
        var reservedObjectGUIDs = serializedObject.FindProperty("reservedObjectGUIDs");
        for (int i = reservedObjectGUIDs.arraySize - 1; i >= 0; i--)
        {
            if (predicate(reservedObjectGUIDs.GetArrayElementAtIndex(i).stringValue))
            {
                removedCount++;
                //WeaverLog.Log("REMOVING ID = " + reservedObjectGUIDs.GetArrayElementAtIndex(i).stringValue);
                reservedObjects.GetArrayElementAtIndex(i).objectReferenceValue = null;
                reservedObjects.DeleteArrayElementAtIndex(i);
                reservedObjectGUIDs.DeleteArrayElementAtIndex(i);
            }
        }

        return removedCount;
        /*var index = GetIndexOfGUID(guid);

        if (index >= 0)
        {
            serializedObject.FindProperty("reservedObjects").DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("reservedObjectGUIDs").DeleteArrayElementAtIndex(index);
            return true;
        }
        else
        {
            return false;
        }*/
    }

    bool RemoveObjectByGUID(string guid)
    {
        var index = GetIndexOfGUID(guid);

        if (index >= 0)
        {
            var reservedObjects = serializedObject.FindProperty("reservedObjects");
            reservedObjects.GetArrayElementAtIndex(index).objectReferenceValue = null;
            reservedObjects.DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("reservedObjectGUIDs").DeleteArrayElementAtIndex(index);
            return true;
        }
        else
        {
            return false;
        }
    }

    bool RemoveUnityObj(UnityEngine.Object obj)
    {
        var reservedObjects = serializedObject.FindProperty("reservedObjects");

        int index = -1;
        for (int i = reservedObjects.arraySize - 1; i >= 0; i--)
        {
            if (reservedObjects.GetArrayElementAtIndex(i).objectReferenceValue == obj)
            {
                index = i;
                break;
            }
        }
        //var index = reservedObjects.IndexOf(obj);

        if (index >= 0)
        {
            reservedObjects.GetArrayElementAtIndex(index).objectReferenceValue = null;
            reservedObjects.DeleteArrayElementAtIndex(index);
            serializedObject.FindProperty("reservedObjectGUIDs").DeleteArrayElementAtIndex(index);

            //reservedObjects.RemoveAt(index);
            //reservedObjectGUIDs.RemoveAt(index);
            return true;
        }
        else
        {
            return false;
        }
    }

    static List<Type> displayableTypes = new List<Type>
    {
        typeof(bool),
        typeof(Bounds),
        typeof(BoundsInt),
        typeof(Color),
        typeof(AnimationCurve),
        typeof(double),
        typeof(Enum),
        typeof(float),
        typeof(Gradient),
        typeof(int),
        typeof(long),
        typeof(UnityEngine.Object),
        typeof(Rect),
        typeof(RectInt),
        typeof(string),
        typeof(Vector2),
        typeof(Vector2Int),
        typeof(Vector3),
        typeof(Vector3Int),
        typeof(Vector4)
    };

    static bool IsDisplayableField(Type memberType)
    {
        return displayableTypes.Any(t => t == memberType || t.IsAssignableFrom(memberType));
        /*if (prevValue is Bounds prevBounds)
        {
            return true;
        }
        else if (prevValue is BoundsInt prevBoundsInt)
        {
            return true;
        }
        else if (prevValue is Color prevColor)
        {
            return true;
        }
        else if (typeof(AnimationCurve).IsAssignableFrom(memberType) )
        {
            return true;
        }
        else if (prevValue is double prevDouble)
        {
            return true;
        }
        else if (typeof(Enum).IsAssignableFrom(memberType))
        {
            return true;
        }
        else if (prevValue is float prevFloat)
        {
            return true;
        }
        else if (typeof(Gradient).IsAssignableFrom(memberType))
        {
            return true;
        }
        else if (prevValue is int prevInt)
        {
            return true;
        }
        else if (prevValue is long prevLong)
        {
            return true;
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(memberType))
        {
            return true;
        }
        else if (prevValue is Rect prevRect)
        {
            return true;
        }
        else if (prevValue is RectInt prevRectInt)
        {
            return true;
        }
        else if (typeof(string).IsAssignableFrom(memberType))
        {
            return true;
        }
        else if (prevValue is Vector2 prevVector2)
        {
            return true;
        }
        else if (prevValue is Vector2Int prevVector2Int)
        {
            return true;
        }
        else if (prevValue is Vector3 prevVector3)
        {
            return true;
        }
        else if (prevValue is Vector3Int prevVector3Int)
        {
            return true;
        }
        else if (prevValue is Vector4 prevVector4)
        {
            return true;
        }
        else
        {
            return false;
        }*/
    }

    static object DisplayGenericField(string label, object prevValue, Type memberType, params GUILayoutOption[] layoutOptions)
    {
        if (prevValue is bool prevBool)
        {
            return EditorGUILayout.Toggle(label, prevBool, layoutOptions);
        }
        if (prevValue is Bounds prevBounds)
        {
            return EditorGUILayout.BoundsField(label, prevBounds, layoutOptions);
        }
        else if (prevValue is BoundsInt prevBoundsInt)
        {
            return EditorGUILayout.BoundsIntField(label, prevBoundsInt, layoutOptions);
        }
        else if (prevValue is Color prevColor)
        {
            return EditorGUILayout.ColorField(label, prevColor, layoutOptions);
        }
        else if (typeof(AnimationCurve).IsAssignableFrom(memberType) /*prevValue is AnimationCurve prevCurve*/)
        {
            return EditorGUILayout.CurveField(label, prevValue as AnimationCurve, layoutOptions);
        }
        else if (prevValue is double prevDouble)
        {
            return EditorGUILayout.DoubleField(label, prevDouble, layoutOptions);
        }
        else if (typeof(Enum).IsAssignableFrom(memberType)/*prevValue is Enum prevEnum*/)
        {
            return EditorGUILayout.EnumPopup(label, prevValue as Enum, layoutOptions);
        }
        else if (prevValue is float prevFloat)
        {
            return EditorGUILayout.FloatField(label, prevFloat, layoutOptions);
        }
        else if (typeof(Gradient).IsAssignableFrom(memberType)/*prevValue is Gradient prevGradient*/)
        {
            return EditorGUILayout.GradientField(label, prevValue as Gradient, layoutOptions);
        }
        else if (prevValue is int prevInt)
        {
            return EditorGUILayout.IntField(label, prevInt, layoutOptions);
        }
        else if (prevValue is long prevLong)
        {
            return EditorGUILayout.LongField(label, prevLong, layoutOptions);
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(memberType)/*prevValue is UnityEngine.Object prevObject*/)
        {
            return EditorGUILayout.ObjectField(label, prevValue as UnityEngine.Object, memberType,false, layoutOptions);
        }
        else if (prevValue is Rect prevRect)
        {
            return EditorGUILayout.RectField(label, prevRect, layoutOptions);
        }
        else if (prevValue is RectInt prevRectInt)
        {
            return EditorGUILayout.RectIntField(label, prevRectInt, layoutOptions);
        }
        else if (typeof(string).IsAssignableFrom(memberType) /*prevValue is string prevString*/)
        {
            return EditorGUILayout.TextField(label, prevValue as string, layoutOptions);
        }
        else if (prevValue is Vector2 prevVector2)
        {
            return EditorGUILayout.Vector2Field(label, prevVector2, layoutOptions);
        }
        else if (prevValue is Vector2Int prevVector2Int)
        {
            return EditorGUILayout.Vector2IntField(label, prevVector2Int, layoutOptions);
        }
        else if (prevValue is Vector3 prevVector3)
        {
            return EditorGUILayout.Vector3Field(label, prevVector3, layoutOptions);
        }
        else if (prevValue is Vector3Int prevVector3Int)
        {
            return EditorGUILayout.Vector3IntField(label, prevVector3Int, layoutOptions);
        }
        else if (prevValue is Vector4 prevVector4)
        {
            return EditorGUILayout.Vector4Field(label, prevVector4, layoutOptions);
        }
        else
        {
            return prevValue;
        }
    }
}

