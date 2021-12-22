using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Features;

[CustomPropertyDrawer(typeof(LanguageTable.Entry))]
public class LanguageTableEntriesPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keyProp = property.FindPropertyRelative(nameof(LanguageTable.Entry.key));
        var valueProp = property.FindPropertyRelative(nameof(LanguageTable.Entry.value));

        var left = new Rect(position.x,position.y,position.width / 2,position.height);
        var right = new Rect(position.x + (position.width / 2),position.y,position.width / 2,position.height);
        //EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(LanguageTable.Entry.key)));
        //EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(LanguageTable.Entry.value)));


        //EditorGUI.PropertyField(left,, property.FindPropertyRelative(nameof(LanguageTable.Entry.key)));
        //EditorGUI.PropertyField(right, property.FindPropertyRelative(nameof(LanguageTable.Entry.value)));

        var keyDimensions = GUI.skin.label.CalcSize(new GUIContent("Key "));

        var keyLabelRect = new Rect(left.x,left.y,keyDimensions.x,left.height);
        var keyPropRect = new Rect(keyLabelRect.x + keyLabelRect.width, keyLabelRect.y,left.width - keyLabelRect.width,keyLabelRect.height);

        EditorGUI.LabelField(keyLabelRect, new GUIContent("Key "));

        keyProp.stringValue = EditorGUI.TextField(keyPropRect, keyProp.stringValue);


        var valueDimensions = GUI.skin.label.CalcSize(new GUIContent("  Value "));

        var valueLabelRect = new Rect(right.x, right.y, valueDimensions.x, right.height);
        var valuePropRect = new Rect(valueLabelRect.x + valueLabelRect.width, valueLabelRect.y, right.width - valueLabelRect.width, valueLabelRect.height);

        EditorGUI.LabelField(valueLabelRect, new GUIContent("  Value "));

        valueProp.stringValue = EditorGUI.TextField(valuePropRect, valueProp.stringValue);

        //valueProp.stringValue = EditorGUI.TextField(valueRect, valueProp.stringValue);

        //EditorGUILayout.EndHorizontal();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, GUIContent.none);
    }
}


/*[CustomPropertyDrawer(typeof(LanguageTable.Entries))]
public class LanguageTableEntriesPropertyDrawer : PropertyDrawer
{
    class EntryListWrapper : IList<KeyValuePair<string,string>>, IList
    {
        SerializedProperty EntriesObj;
        SerializedProperty keysObj;
        SerializedProperty valuesObj;

        public EntryListWrapper(SerializedProperty obj)
        {
            EntriesObj = obj;
            keysObj = obj.FindPropertyRelative(nameof(LanguageTable.Entries.keys));
            valuesObj = obj.FindPropertyRelative(nameof(LanguageTable.Entries.values));
        }

        KeyValuePair<string, string> GetIndex(int index)
        {
            return new KeyValuePair<string, string>(keysObj.GetArrayElementAtIndex(index).stringValue,valuesObj.GetArrayElementAtIndex(index).stringValue);
        }

        void SetIndex(int index, KeyValuePair<string, string> value)
        {
            keysObj.GetArrayElementAtIndex(index).stringValue = value.Key;
            valuesObj.GetArrayElementAtIndex(index).stringValue = value.Value;
        }

        bool ParamCheck(object input, out KeyValuePair<string,string> output, bool throwException = false)
        {
            if (input is KeyValuePair<string, string> kv)
            {
                output = kv;
                return true;
            }
            else if (throwException)
            {
                throw new Exception($"The value needs to be of type {typeof(KeyValuePair<string, string>).FullName}");
            }
            return false;
        }

        bool AreEqual(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
        {
            return a.Key == b.Key && a.Value == b.Value;
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Count => keysObj.arraySize;

        public bool IsSynchronized => false;

        public object SyncRoot => new object();

        object IList.this[int index]
        {
            get => GetIndex(index);
            set
            {
                if (ParamCheck(value, out var kv, true))
                {
                    SetIndex(index, kv);
                }
            }
        }

        public KeyValuePair<string, string> this[int index]
        {
            get => GetIndex(index);
            set => SetIndex(index, value);
        }

        public int Add(object value)
        {
            if (ParamCheck(value,out var kv))
            {
                Add(kv);
                return keysObj.arraySize - 1;
            }
            return -1;
        }

        public void Add(KeyValuePair<string, string> item)
        {
            keysObj.arraySize++;
            valuesObj.arraySize++;

            var index = keysObj.arraySize - 1;

            keysObj.GetArrayElementAtIndex(index).stringValue = item.Key;
            valuesObj.GetArrayElementAtIndex(index).stringValue = item.Value;
        }

        public void Clear()
        {
            keysObj.arraySize = 0;
            valuesObj.arraySize = 0;
        }

        public bool Contains(object value)
        {
            if (ParamCheck(value,out var kv))
            {
                return Contains(kv);
            }
            return false;
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                var indexVal = GetIndex(i);
                if (AreEqual(indexVal,item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            for (int i = index; i < keysObj.arraySize; i++)
            {
                array.SetValue(GetIndex(i), i - index);
            }
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < keysObj.arraySize; i++)
            {
                array.SetValue(GetIndex(i), i - arrayIndex);
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                yield return GetIndex(i);
            }
        }

        public int IndexOf(object value)
        {
            if (ParamCheck(value,out var kv))
            {
                return IndexOf(kv);
            }
            else
            {
                return -1;
            }
        }

        public int IndexOf(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                if (AreEqual(GetIndex(i),item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            if (ParamCheck(value,out var kv))
            {
                Insert(index, kv);
            }
        }

        public void Insert(int index, KeyValuePair<string, string> item)
        {
            keysObj.InsertArrayElementAtIndex(index);
            valuesObj.InsertArrayElementAtIndex(index);

            keysObj.GetArrayElementAtIndex(index).stringValue = item.Key;
            valuesObj.GetArrayElementAtIndex(index).stringValue = item.Value;
        }

        public void Remove(object value)
        {
            if (ParamCheck(value,out var kv))
            {
                Remove(kv);
            }
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                if (AreEqual(GetIndex(i),item))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            for (int i = index + 1; i < keysObj.arraySize; i++)
            {
                SetIndex(i - 1, GetIndex(i));
            }
            keysObj.arraySize--;
            valuesObj.arraySize--;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                yield return GetIndex(i);
            }
        }
    }


    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {

        var rootElement = new VisualElement();

        var container = new VisualElement();
        //UnityEngine.Random.InitState(property.displayName.GetHashCode());
        //container.style.backgroundColor = UnityEngine.Random.ColorHSV();

        const int itemHeight = 16;

        var listWrapper = new EntryListWrapper(property);

        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () => new Label();

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = $"{listWrapper[i].Key} - {listWrapper[i].Value}";

        container.Add(new ListView(listWrapper,itemHeight,makeItem,bindItem));
        //container.Add(new PropertyField(property.FindPropertyRelative("");

        rootElement.Add(container);

        return rootElement;
    }
}*/
