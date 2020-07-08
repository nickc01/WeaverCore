using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace WeaverCore.Editor.Helpers
{
	public static class SerializedObjectExtensions
	{
		public static string GetString(this SerializedObject serializedObject, string propertyPath)
		{
			var property = serializedObject.FindProperty(propertyPath);
			return property.stringValue;
		}

		public static int GetInt(this SerializedObject serializedObject, string propertyPath)
		{
			var property = serializedObject.FindProperty(propertyPath);
			return property.intValue;
		}

		public static string SetString(this SerializedObject serializedObject, string propertyPath, string value)
		{
			var property = serializedObject.FindProperty(propertyPath);
			property.stringValue = value;
			return value;
		}

		public static int SetInt(this SerializedObject serializedObject, string propertyPath, int value)
		{
			var property = serializedObject.FindProperty(propertyPath);
			property.intValue = value;
			return value;
		}

		public static IEnumerable<SerializedProperty> Iterator(this SerializedObject serializedObject,params string[] excluded)
		{
			SerializedProperty iter = serializedObject.GetIterator();
			bool enterChildren = true;
			while (iter.NextVisible(enterChildren))
			{
				if (excluded == null || !excluded.Contains(iter.name))
				{
					yield return iter;
				}
				enterChildren = false;
			}
		}
	}
}
