using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
	[Serializable]
	class JsonWrapper<T>
	{
		public T value;

		public JsonWrapper(T Value)
		{
			value = Value;
		}
	}

	[CreateAssetMenu( fileName = "Inspector Settings", menuName = "RuntimeInspector/Settings", order = 111 )]
	public class RuntimeInspectorSettings : ScriptableObject
	{
		bool initialized = false;

#pragma warning disable 0649
		[SerializeField]
		private InspectorField[] m_standardDrawers;
		public InspectorField[] StandardDrawers 
		{ 
			get 
			{
				Init();
				return m_standardDrawers; 
			} 
		}

		[SerializeField]
		private InspectorField[] m_referenceDrawers;
		public InspectorField[] ReferenceDrawers 
		{ 
			get 
			{
				Init();
				return m_referenceDrawers; 
			} 
		}

		[SerializeField]
		private VariableSet[] m_hiddenVariables;
		public VariableSet[] HiddenVariables 
		{ 
			get 
			{
				Init();
				return m_hiddenVariables; 
			} 
		}

		[SerializeField]
		private VariableSet[] m_exposedVariables;
		public VariableSet[] ExposedVariables 
		{ 
			get 
			{
				Init();
				return m_exposedVariables; 
			} 
		}

		void Init()
		{
			if (!initialized)
			{
				initialized = true;
#if GAME_BUILD
				WriteBackToObject("m_hiddenVariables");
				WriteBackToObject("m_exposedVariables");
#endif
			}
		}

#pragma warning restore 0649


		/*[SerializeField]
		//[HideInInspector]
		private LayoutElement[] m_standardDrawers_layoutElement;

		[SerializeField]
		//[HideInInspector]
		private TMPro.TextMeshProUGUI[] m_standardDrawers_variableNameText;

		[SerializeField]
		//[HideInInspector]
		private Image[] m_standardDrawers_variableNameMask;

		[SerializeField]
		//[HideInInspector]
		private MaskableGraphic[] m_standardDrawers_visibleArea;




		[SerializeField]
		[HideInInspector]
		private LayoutElement[] m_referenceDrawers_layoutElement;

		[SerializeField]
		[HideInInspector]
		private TMPro.TextMeshProUGUI[] m_referenceDrawers_variableNameText;

		[SerializeField]
		[HideInInspector]
		private Image[] m_referenceDrawers_variableNameMask;

		[SerializeField]
		[HideInInspector]
		private MaskableGraphic[] m_referenceDrawers_visibleArea;*/



		[SerializeField]
		[HideInInspector]
		private string[] m_hiddenVariables_m_type;

		[SerializeField]
		[HideInInspector]
		private string[] m_hiddenVariables_m_variables;

		[SerializeField]
		[HideInInspector]
		private int[] m_hiddenVariables_m_variables___startIndexes;


		[SerializeField]
		[HideInInspector]
		private string[] m_exposedVariables_m_type;

		[SerializeField]
		[HideInInspector]
		private string[] m_exposedVariables_m_variables;

		[SerializeField]
		[HideInInspector]
		private int[] m_exposedVariables_m_variables___startIndexes;

		/*private void Awake()
		{

			Debug.Log("TEST");
		}*/

#if UNITY_EDITOR

		private void OnValidate()
		{
			WriteToArrays("m_hiddenVariables");
			WriteToArrays("m_exposedVariables");

			//WriteBackToObject("m_hiddenVariables");
			//WriteBackToObject("m_exposedVariables");

			//Debug.Log("DRAWER TEST = " + JsonUtility.ToJson(new JsonWrapper<InspectorField[]>(m_standardDrawers)));
			//Debug.Log("TEST2");
		}



#endif

		void WriteToArrays(string fieldName)
		{
			var field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			//Debug.Log("Field Type = " + field.GetType());
			var fieldType = field.FieldType.GetElementType();
			var array = (Array)field.GetValue(this);
			var arrayCount = array.GetLength(0);

			foreach (var destField in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if (destField != field && destField.Name.Contains(fieldName + "_"))
				{
					var innerFieldName = destField.Name.Replace(fieldName + "_","");

					var innerFieldF = fieldType.GetField(innerFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (innerFieldF == null)
					{
						continue;
					}
					var innerFieldType = innerFieldF.FieldType;

					if (innerFieldType.IsArray)
					{

						var totalSize = GetTotalLengthOfInnerField(array, innerFieldF);
						InitArray(destField, totalSize);
						var destArray = (Array)destField.GetValue(this);

						var destFieldIndexesF = GetType().GetField(fieldName + "_" + innerFieldName + "___startIndexes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
						//Debug.Log("Index Field Name = " + fieldName + "_" + innerFieldName + "___startIndexes");
						//Debug.Log("Index Field Field = " + destFieldIndexesF);
						destFieldIndexesF.SetValue(this, Array.CreateInstance(typeof(int), array.GetLength(0)));
						var destFieldIndexes = (int[])destFieldIndexesF.GetValue(this);

						int currentIndex = 0;

						for (int i = 0; i < array.GetLength(0); i++)
						{
							var obj = array.GetValue(i);
							//Debug.Log("Obj = " + obj);
							//Debug.Log("Obj Type = " + fieldType.GetType());
							//Debug.Log("Inner Field Name = " + innerFieldName);
							var innerValue = (Array)innerFieldF.GetValue(obj);

							destFieldIndexes[i] = currentIndex;

							foreach (var innerValueValue in innerValue)
							{
								destArray.SetValue(innerValueValue, currentIndex);
								currentIndex++;
							}
						}

					}
					else
					{
						InitArray(destField, arrayCount);
						var destArray = (Array)destField.GetValue(this);

						for (int i = 0; i < array.GetLength(0); i++)
						{
							var obj = array.GetValue(i);
							//Debug.Log("Obj = " + obj);
							//Debug.Log("Obj Type = " + fieldType.GetType());
							//Debug.Log("Inner Field Name = " + innerFieldName);
							var innerValue = innerFieldF.GetValue(obj);
							//Debug.Log("INner Field Value = " + innerValue);
							//Debug.Log("Inner Field Type = " + innerValue?.GetType());
							//Debug.Log("Dest Array Type = " + destArray.GetType());
							destArray.SetValue(innerValue, i);
						}
					}

					
				}
			}

		}

		static int GetTotalLengthOfInnerField(Array array, FieldInfo field)
		{
			int size = 0;
			foreach (var obj in array)
			{
				var innerArray = (Array)field.GetValue(obj);
				size += innerArray.Length;
			}
			return size;
		}

		void InitArray(FieldInfo field, int length)
		{
			field.SetValue(this, Array.CreateInstance(field.FieldType.GetElementType(),length));
		}

		void WriteBackToObject(string fieldName)
		{
			var type = GetType();
			var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var fieldType = field.FieldType;
			var elementType = fieldType.GetElementType();

			var relatedFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f != field && f.Name.Contains(field.Name + "_"));

			var arraySize = GetArraySizeOfRelatedFields(this, relatedFields);

			field.SetValue(this, Array.CreateInstance(elementType, arraySize));
			var array = (Array)field.GetValue(this);

			for (int i = 0; i < arraySize; i++)
			{
				var instance = Activator.CreateInstance(elementType);
				foreach (var relatedField in relatedFields)
				{
					var innerFieldName = relatedField.Name.Replace(fieldName + "_", "");
					var relatedArray = (Array)relatedField.GetValue(this);

					var innerFieldF = elementType.GetField(innerFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (innerFieldF == null)
					{
						continue;
					}
					var innerFieldType = innerFieldF.FieldType;

					if (innerFieldType.IsArray)
					{
						var startingIndexes = (int[])type.GetField(relatedField.Name + "___startIndexes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(this);
						int amountOfIndexes = startingIndexes.GetLength(0);

						int start = startingIndexes[i];
						int length = 0;
						if (i + 1 == amountOfIndexes)
						{
							length = relatedArray.GetLength(0) - start;
						}
						else
						{
							length = startingIndexes[i + 1] - start;
						}

						innerFieldF.SetValue(instance, Array.CreateInstance(innerFieldType.GetElementType(), length));
						var innerFieldArray = (Array)innerFieldF.GetValue(instance);

						for (int j = start; j < start + length; j++)
						{
							innerFieldArray.SetValue(relatedArray.GetValue(j), j - start);
						}
					}
					else
					{
						innerFieldF.SetValue(instance,relatedArray.GetValue(i));
					}
				}
				array.SetValue(instance,i);
			}
		}

		int GetArraySizeOfRelatedFields(object target, IEnumerable<FieldInfo> relatedFields)
		{
			int minSize = int.MaxValue;
			foreach (var field in relatedFields)
			{
				if (field.FieldType.IsArray)
				{
					var array = (Array)field.GetValue(target);
					var length = array.GetLength(0);
					if (length < minSize)
					{
						minSize = length;
					}
				}
			}
			if (minSize == int.MaxValue)
			{
				return 0;
			}
			else
			{
				return minSize;
			}
		}
	}
}