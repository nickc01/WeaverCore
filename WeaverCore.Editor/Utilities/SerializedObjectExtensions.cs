using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Contains several extension methods for working with SerializedObjects
	/// </summary>
	public static class SerializedObjectExtensions
	{
		/// <summary>
		/// Gets the value of a string property
		/// </summary>
		public static string GetString(this SerializedObject serializedObject, string propertyPath)
		{
			var property = serializedObject.FindProperty(propertyPath);
			return property.stringValue;
		}

		/// <summary>
		/// Gets the value of an int property
		/// </summary>
		public static int GetInt(this SerializedObject serializedObject, string propertyPath)
		{
			var property = serializedObject.FindProperty(propertyPath);
			return property.intValue;
		}

		/// <summary>
		/// Sets the value of a string property
		/// </summary>
		public static string SetString(this SerializedObject serializedObject, string propertyPath, string value)
		{
			var property = serializedObject.FindProperty(propertyPath);
			property.stringValue = value;
			return value;
		}

		/// <summary>
		/// Sets the value of an int property
		/// </summary>
		public static int SetInt(this SerializedObject serializedObject, string propertyPath, int value)
		{
			var property = serializedObject.FindProperty(propertyPath);
			property.intValue = value;
			return value;
		}

		/// <summary>
		/// Gets an interator for iterating over all the properties of a SerializeObject
		/// </summary>
		/// <param name="serializedObject">The object to iterate over</param>
		/// <param name="excluded">A list of any excluded properties to skip over</param>
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
