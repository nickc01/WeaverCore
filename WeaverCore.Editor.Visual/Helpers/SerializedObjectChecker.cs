using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor.Visual.Helpers
{
	public abstract class SerializedObjectChecker
	{
		public SerializedObject serializedObject { get; private set; }

		public SerializedObjectChecker(SerializedObject obj)
		{
			serializedObject = obj;
		}

		public SerializedObjectChecker(string assetPath)
		{
			serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(assetPath)[0]);
		}

		public string GetString(string propertyPath)
		{
			return serializedObject.GetString(propertyPath);
		}

		public int GetInt(string propertyPath)
		{
			return serializedObject.GetInt(propertyPath);
		}

		public void SetString(string propertyPath,string value)
		{
			serializedObject.SetString(propertyPath,value);
		}

		public void SetInt(string propertyPath, int value)
		{
			serializedObject.SetInt(propertyPath,value);
		}

		public abstract void Check();
	}
}
