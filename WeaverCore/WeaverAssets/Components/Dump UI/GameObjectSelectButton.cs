using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WeaverCore.Assets.Components;

namespace WeaverCore.Assets.Components
{
	public class GameObjectSelectButton : MonoBehaviour, IPointerClickHandler
	{
		GameObject sourceGM;
		Text textMesh;


		public void OnPointerClick(PointerEventData eventData)
		{
			DumpUI.Instance.SetGameObject(sourceGM);
		}

		public void SetSourceGM(GameObject obj, string name = null)
		{
			//WeaverLog.Log("THIS IS THE NEW FILE");
			//WeaverLog.Log("Obj = " + obj);
			//WeaverLog.Log("Obj Null = " + (obj == null));
			//WeaverLog.Log("This Null = " + (this == null));
			//WeaverLog.Log("Source Null = " + (gameObject == null));
			if (name == null && obj != null)
			{
				//WeaverLog.Log("Setting Name = " + name);
				name = obj.name;
			}
			else if (name == null)
			{
				name = "";
			}
			//WeaverLog.Log("AAA");
			sourceGM = obj;
			//WeaverLog.Log("BBB");
			if (textMesh == null)
			{
				//WeaverLog.Log("CCC");
				textMesh = GetComponentInChildren<Text>();
			}
			/*foreach (var component in GetComponentsInChildren<Component>())
			{
				WeaverLog.Log("Component = " + component.GetType());
			}
			WeaverLog.Log("DDD");
			WeaverLog.Log("TextMeshPro = " + textMesh);*/
			textMesh.text = name;
			//WeaverLog.Log("EEE");
		}

		public GameObject GetSourceGM()
		{
			return sourceGM;
		}
	}
}
