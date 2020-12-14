using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.TestStuff
{

	public class TMPTEST : MonoBehaviour
	{

		[SerializeField]
		TMP_FontAsset testFont;

		void Start()
		{
			StartCoroutine(test());
			/*TMP_FontAsset arial = Resources.Load<TMP_FontAsset>("fonts & materials/ARIAL SDF");

			if (arial != null)
			{
				var tmp = GetComponent<TextMeshProUGUI>();
				WeaverLog.Log("SETTING TO ARIAL = " + arial);
				tmp.font = arial;
			}*/

			/*foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
			{
				WeaverLog.Log("Found Font = " + font.name);
			}*/

		}

		IEnumerator test()
		{
			//WeaverLog.Log("TEST START");
			yield return new WaitForSeconds(1f);
			WeaverLog.Log("Render Height for " + gameObject.name + " = " + GetComponent<TextMeshProUGUI>().renderedHeight);
			/*foreach (var tmp in GameObject.FindObjectsOfType<TMP_Text>())
			{
				WeaverLog.Log("TMP = " + tmp.gameObject.name);
				tmp.font = testFont;
			}*/

			//TODO - USE RENDER WIDTH INSTEAD OF PREFERRED WITH AND SEE IF THAT WORKS

			//WeaverLog.Log("Text Count = " + GameObject.FindObjectsOfType<Text>().Length);

			//foreach (var txt in GameObject.FindObjectsOfType<Text>())
			//{
			//txt.text = "test";
			//}
		}
	}
}
