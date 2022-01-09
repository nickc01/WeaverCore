using RuntimeInspectorNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Assets
{
	/// <summary>
	/// Used in the <see cref="WeaverCoreDebugTools"/> menu to show the available tools to do on an object
	/// </summary>
	public class ToolsArea : MonoBehaviour
	{
		/// <summary>
		/// Defines a certain action that can be done for an action in the <see cref="WeaverCoreDebugTools"/> menu
		/// </summary>
		public interface ToolAction
		{
			/// <summary>
			/// The name of the tool
			/// </summary>
			public string ToolName { get; }

			/// <summary>
			/// Called when the tool is executed
			/// </summary>
			public void OnToolExecute();

			/// <summary>
			/// Called when the selected gameObject in the RuntimeHierarchy is updated. If this function returns true, the tools button will be displayed
			/// </summary>
			/// <param name="selection">The newly selected onbject in the RuntimeHierachy</param>
			/// <returns>Returns whether this tool is able to work on the object</returns>
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

		/// <summary>
		/// Finds all available tools that can be added to the tools menu
		/// </summary>
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
