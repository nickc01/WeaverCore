using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WeaverCore.Components;

namespace WeaverCore.Assets.Components
{

    /// <summary>
    /// Used to display the "Map Updated" text when the map gets updated. Called from <see cref="WeaverBench"/> when the player sits on it
    /// </summary>
    public class MapUpdateMessage : MonoBehaviour
    {
		GameObject Animation;
		GameObject Text;
		GameObject BackboardLarge;
		GameObject BackboardSmall;

		bool mapShop = false;
		string Anim_Down = "";
		float WaitTime;

		private void Start()
		{
			BackboardLarge = transform.Find("Backboard Large").gameObject;
			BackboardSmall = transform.Find("Backboard Small").gameObject;
			Animation = transform.Find("Animation").gameObject;
			Text = transform.Find("Text").gameObject;

			transform.position = new Vector3(-1.3f,-6.8f,0.0f);

			if (mapShop)
			{
				StartCoroutine(MapShop());
			}
			else
			{
				StartCoroutine(Display());
			}
		}


		//AKA : Map Shop Pos State
		IEnumerator MapShop()
		{
			transform.position = new Vector3(-10.1f, -6.8f, 0.0f);
			yield return Display();
		}

		//AKA : Set State
		IEnumerator Display()
		{
			string text_str = WeaverLanguage.GetString("MAP_UPDATED", "UI", "Map Updated");
			Animation.GetComponent<WeaverAnimationPlayer>().PlayAnimation("Map Writing");
			Text.GetComponent<TextMeshPro>().text = text_str;
			Anim_Down = "Entry Full Down";
			GetComponent<AudioSource>().Play();

			//NEW STATE : Unseen
			BackboardLarge.SetActive(true);
			WaitTime = 3f;

			//NEW STATE : Up
			foreach (var c in GetComponentsInChildren<MapUpdateDisplay>())
			{
				c.Up();
			}
			yield return new WaitForSeconds(WaitTime);

			//NEW STATE : Map Complete
			Animation.GetComponent<WeaverAnimationPlayer>().PlayAnimation("Map Complete");

			//NEW STATE : DOWN
			foreach (var c in GetComponentsInChildren<MapUpdateDisplay>())
			{
				c.Down();
			}
			yield return new WaitForSeconds(1f);

			Destroy(gameObject);

			yield break;
		}

	}
}
