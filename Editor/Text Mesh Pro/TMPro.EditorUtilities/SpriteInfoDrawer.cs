using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(TMP_Sprite))]
	public class SpriteInfoDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty property2 = property.FindPropertyRelative("id");
			SerializedProperty serializedProperty = property.FindPropertyRelative("name");
			SerializedProperty serializedProperty2 = property.FindPropertyRelative("hashCode");
			SerializedProperty serializedProperty3 = property.FindPropertyRelative("unicode");
			SerializedProperty serializedProperty4 = property.FindPropertyRelative("x");
			SerializedProperty serializedProperty5 = property.FindPropertyRelative("y");
			SerializedProperty serializedProperty6 = property.FindPropertyRelative("width");
			SerializedProperty serializedProperty7 = property.FindPropertyRelative("height");
			SerializedProperty property3 = property.FindPropertyRelative("xOffset");
			SerializedProperty property4 = property.FindPropertyRelative("yOffset");
			SerializedProperty property5 = property.FindPropertyRelative("xAdvance");
			SerializedProperty property6 = property.FindPropertyRelative("scale");
			SerializedProperty serializedProperty8 = property.FindPropertyRelative("sprite");
			Texture spriteSheet = (property.serializedObject.targetObject as TMP_SpriteAsset).spriteSheet;
			if (spriteSheet == null)
			{
				Debug.LogWarning("Please assign a valid Sprite Atlas texture to the [" + property.serializedObject.targetObject.name + "] Sprite Asset.", property.serializedObject.targetObject);
				return;
			}
			Vector2 vector = new Vector2(65f, 65f);
			if (serializedProperty6.floatValue >= serializedProperty7.floatValue)
			{
				vector.y = serializedProperty7.floatValue * vector.x / serializedProperty6.floatValue;
				position.y += (vector.x - vector.y) / 2f;
			}
			else
			{
				vector.x = serializedProperty6.floatValue * vector.y / serializedProperty7.floatValue;
				position.x += (vector.y - vector.x) / 2f;
			}
			GUI.DrawTextureWithTexCoords(texCoords: new Rect(serializedProperty4.floatValue / (float)spriteSheet.width, serializedProperty5.floatValue / (float)spriteSheet.height, serializedProperty6.floatValue / (float)spriteSheet.width, serializedProperty7.floatValue / (float)spriteSheet.height), position: new Rect(position.x + 5f, position.y, vector.x, vector.y), image: spriteSheet, alphaBlend: true);
			Rect rect = new Rect(position.x, position.y, position.width, 49f);
			rect.x += 70f;
			bool enabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUIUtility.labelWidth = 30f;
			EditorGUI.PropertyField(new Rect(rect.x + 5f, rect.y, 65f, 18f), property2, new GUIContent("ID:"));
			GUI.enabled = enabled;
			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 55f;
			GUI.SetNextControlName("Unicode Input");
			string s = EditorGUI.TextField(new Rect(rect.x + 75f, rect.y, 105f, 18f), "Unicode:", serializedProperty3.intValue.ToString("X"));
			if (GUI.GetNameOfFocusedControl() == "Unicode Input")
			{
				char character = Event.current.character;
				if ((character < '0' || character > '9') && (character < 'a' || character > 'f') && (character < 'A' || character > 'F'))
				{
					Event.current.character = '\0';
				}
				if (EditorGUI.EndChangeCheck())
				{
					serializedProperty3.intValue = TMP_TextUtilities.StringToInt(s);
					property.serializedObject.ApplyModifiedProperties();
					TMP_SpriteAsset tMP_SpriteAsset = property.serializedObject.targetObject as TMP_SpriteAsset;
					tMP_SpriteAsset.UpdateLookupTables();
				}
			}
			EditorGUIUtility.labelWidth = 45f;
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(new Rect(rect.x + 185f, rect.y, rect.width - 260f, 18f), serializedProperty, new GUIContent("Name: " + serializedProperty.stringValue));
			if (EditorGUI.EndChangeCheck())
			{
				Sprite sprite = serializedProperty8.objectReferenceValue as Sprite;
				if (sprite != null)
				{
					sprite.name = serializedProperty.stringValue;
				}
				serializedProperty2.intValue = TMP_TextUtilities.GetSimpleHashCode(serializedProperty.stringValue);
				property.serializedObject.ApplyModifiedProperties();
			}
			EditorGUIUtility.labelWidth = 30f;
			EditorGUIUtility.fieldWidth = 10f;
			float num = (rect.width - 75f) / 4f;
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 0f, rect.y + 22f, num - 5f, 18f), serializedProperty4, new GUIContent("X:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 1f, rect.y + 22f, num - 5f, 18f), serializedProperty5, new GUIContent("Y:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 2f, rect.y + 22f, num - 5f, 18f), serializedProperty6, new GUIContent("W:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 3f, rect.y + 22f, num - 5f, 18f), serializedProperty7, new GUIContent("H:"));
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 0f, rect.y + 44f, num - 5f, 18f), property3, new GUIContent("OX:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 1f, rect.y + 44f, num - 5f, 18f), property4, new GUIContent("OY:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 2f, rect.y + 44f, num - 5f, 18f), property5, new GUIContent("Adv."));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 3f, rect.y + 44f, num - 5f, 18f), property6, new GUIContent("SF."));
			if (!EditorGUI.EndChangeCheck())
			{
			}
		}
	}
}
