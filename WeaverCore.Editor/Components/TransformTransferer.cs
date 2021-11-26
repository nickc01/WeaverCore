/*using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Components
{
	/// <summary>
	/// Will transform any positional, rotational, and scale changes to a Destination Object
	/// </summary>
	[ExecuteAlways]
	public class TransformTransferer : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The destination object that will be receiving transform changes from this object")]
		GameObject destinationObject;

		private void Awake()
		{
			EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
		}

		private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
		{
			Debug.Log("PLAY MODE STATE = " + obj);
			if (obj == PlayModeStateChange.ExitingEditMode)
			{
				UpdateTransform();
			}
		}

		private void Start()
		{
			//UpdateTransform();
		}

		private void UpdateTransform()
		{
			if (destinationObject != null)
			{
				destinationObject.transform.localPosition += transform.localPosition;
				transform.localPosition = default;

				destinationObject.transform.localRotation *= transform.localRotation;
				transform.localRotation = Quaternion.identity;

				destinationObject.transform.localScale += transform.localScale - Vector3.one;
				transform.localScale = Vector3.one;

				transform.hasChanged = false;
			}
		}
	}
}*/
