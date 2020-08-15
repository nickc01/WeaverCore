using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using WeaverCore.WeaverAssets.Components;

namespace WeaverCore.WeaverAssets.Components
{
	public class GameObjectSelectButton : MonoBehaviour, IPointerClickHandler
	{
		GameObject sourceGM;
		TextMeshProUGUI textMesh;


		public void OnPointerClick(PointerEventData eventData)
		{
			DumpUI.Instance.SetGameObject(sourceGM);
		}

		public void SetSourceGM(GameObject obj, string name = null)
		{
			if (name == null)
			{
				name = obj.name;
			}
			sourceGM = obj;

			if (textMesh == null)
			{
				textMesh = GetComponentInChildren<TextMeshProUGUI>();
			}
			textMesh.text = name;
		}

		public GameObject GetSourceGM()
		{
			return sourceGM;
		}
	}
}
