using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Features;

//UNUSED
public class LanguageTableEditor : Editor
{
    /*class EntryListWrapper : IList<KeyValuePair<string, string>>, IList
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
            return new KeyValuePair<string, string>(keysObj.GetArrayElementAtIndex(index).stringValue, valuesObj.GetArrayElementAtIndex(index).stringValue);
        }

        void SetIndex(int index, KeyValuePair<string, string> value)
        {
            keysObj.GetArrayElementAtIndex(index).stringValue = value.Key;
            valuesObj.GetArrayElementAtIndex(index).stringValue = value.Value;
        }

        bool ParamCheck(object input, out KeyValuePair<string, string> output, bool throwException = false)
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
            if (ParamCheck(value, out var kv))
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
            if (ParamCheck(value, out var kv))
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
                if (AreEqual(indexVal, item))
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
            if (ParamCheck(value, out var kv))
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
                if (AreEqual(GetIndex(i), item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            if (ParamCheck(value, out var kv))
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
            if (ParamCheck(value, out var kv))
            {
                Remove(kv);
            }
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < keysObj.arraySize; i++)
            {
                if (AreEqual(GetIndex(i), item))
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
    }*/
    
    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();

        // Draw the legacy IMGUI base
        //var imgui = new IMGUIContainer(OnInspectorGUI);
        //container.Add(imgui);

        // Create property fields.
        // Add fields to the container.
        /*container.Add(
                 new PropertyField(serializedObject.FindProperty("EventToCatch")));*/

        bool enterChildren = true;

        SerializedProperty iterator = serializedObject.GetIterator();

        while (iterator.NextVisible(enterChildren))
        {
            Debug.Log("Iterator Name = " + iterator.name);
            if (iterator.name == "entries")
            {
                Debug.Log("DRAWING ENTRIES");
                DrawLanguageTableEntries(iterator,container);
            }
            else
            {
                var prop = new PropertyField(iterator);

                if (iterator.propertyPath == "m_Script")
                {
                    prop.SetEnabled(false);
                }
                container.Add(prop);
            }
            
            /*using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }*/

            enterChildren = false;
        }


        return container;

    }

    void DrawLanguageTableEntries(SerializedProperty property, VisualElement root)
    {
        var container = new VisualElement();

        container.style.flexDirection = FlexDirection.Row;

        //UnityEngine.Random.InitState(property.displayName.GetHashCode());
        //container.style.backgroundColor = UnityEngine.Random.ColorHSV();

        const int itemHeight = 16;

        //var listWrapper = new EntryListWrapper(property);

        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () => new Label("TEST");

        var testList = new List<int>()
        {
            123,
            456,
            789
        };

        Action<VisualElement, int> bindItemTEST = (e, i) => (e as Label).text = $"{testList[i]}";

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        //Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = $"{listWrapper[i].Key} - {listWrapper[i].Value}";


        Func<VisualElement> makeItemDEBUG = () =>
        {
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Row;
            box.style.flexGrow = 1f;
            box.style.flexShrink = 0f;
            box.style.flexBasis = 0f;
            box.Add(new Label());
            box.Add(new Button(() => { }) { text = "Button" });
            return box;
        };

        Action<VisualElement, int> bindItemDEBUG = (e, i) => (e.ElementAt(0) as Label).text = testList[i].ToString();

        var listView = new ListView(testList, itemHeight, makeItemDEBUG, bindItemDEBUG)
        {
            visible = true,
            viewDataKey = "Language_Table_Entries"
        };

        listView.selectionType = SelectionType.Multiple;

        listView.onItemChosen += obj => Debug.Log(obj);
        listView.onSelectionChanged += objects => Debug.Log(objects);

        listView.style.flexGrow = 1f;
        listView.style.flexShrink = 0f;
        listView.style.flexBasis = 0f;

        var col = new VisualElement();
        col.style.flexGrow = 1f;
        col.style.flexShrink = 0f;
        col.style.flexBasis = 0f;

        col.Add(new Label() { text = listView.viewDataKey });
        col.Add(listView);

        container.Add(col);

        root.Add(container);

        //container.Add(new ListView(listWrapper, itemHeight, makeItem, bindItem));
        //container.Add();
        //container.Add(new PropertyField(property.FindPropertyRelative("");

        /*{ // Create drawer using C#
            var popup = new PopupWindow();
            container.Add(popup);
            popup.text = property.displayName + " - Using C#";
            popup.Add(new PropertyField(property.FindPropertyRelative("amount")));
            popup.Add(new PropertyField(property.FindPropertyRelative("unit")));
            popup.Add(new PropertyField(property.FindPropertyRelative("name"), "CustomLabel: Name"));
        }*/

        /*{ // Create drawer using UXML
            var vsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Examples/Editor/Bindings/custom-drawer.uxml");
            var drawer = vsTree.CloneTree(property.propertyPath);
            drawer.Q<PopupWindow>().text = property.displayName + " - Using UXML";
            container.Add(drawer);
        }*/

        //return container;
    }

   // public override void OnInspectorGUI()
    //{
        /*EditorGUI.BeginChangeCheck();
        obj.UpdateIfRequiredOrScript();
        SerializedProperty iterator = obj.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            enterChildren = false;
        }

        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();*/

        //DrawDefaultInspector();
   //}
}
