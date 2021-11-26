using RuntimeInspectorNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Assets
{
	public class ToolsArea : MonoBehaviour
	{
		public interface ToolAction
		{
			public string ToolName { get; }
			public void OnToolExecute();
			/// <summary>
			/// Called when the selected gameObject in the RuntimeHierarchy is updated. If this function returns true, the tools button will be displayed
			/// </summary>
			/// <param name="selection"></param>
			/// <returns></returns>
			public bool OnGameObjectUpdated(GameObject selection);
		}

		[SerializeField]
		Button buttonPrefab;

		private void Start()
		{
			UpdateButtons();
			RuntimeHierarchy.Instance.OnSelectionChanged = (t) =>
			{
				UpdateButtons();
			};
		}

		private void UpdateButtons()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
			foreach (var tool in GetAllTools())
			{
				if (RuntimeHierarchy.Instance?.CurrentSelection != null && tool.OnGameObjectUpdated(RuntimeHierarchy.Instance?.CurrentSelection.gameObject))
				{
					var button = GameObject.Instantiate(buttonPrefab, transform);
					button.onClick.AddListener(tool.OnToolExecute);
					button.GetComponentInChildren<TMP_Text>().text = tool.ToolName;
				}
			}
		}

		static IEnumerable<ToolAction> GetAllTools()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in asm.GetTypes())
				{
					if (typeof(ToolAction).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition)
					{
						ToolAction tool = null;
						try
						{
							tool = (ToolAction)Activator.CreateInstance(type);
						}
						catch (Exception e)
						{
							WeaverLog.LogError("Error adding tool to Debug Tools Menu");
							WeaverLog.LogException(e);
						}
						if (tool != null)
						{
							yield return tool;
						}
					}
				}
			}
		}
	}
}
