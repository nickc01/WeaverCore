using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Editor.Utilities;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_GeoCounter_I : GeoCounter
	{
		[SerializeField]
		TextMeshProUGUI textComponent;

		int currentGeo = 0;

		static E_GeoCounter_I editorCounter;

		protected class E_Static_I : Static_I
		{
			public override GeoCounter GetCounter()
			{
				if (editorCounter == null)
				{
					editorCounter = GameObject.FindObjectOfType<E_GeoCounter_I>();
					if (editorCounter == null)
					{
						var counterPrefab = EditorAssets.LoadEditorAsset<GameObject>("Geo Counter").GetComponent<E_GeoCounter_I>();


						editorCounter = GameObject.Instantiate(counterPrefab, WeaverCanvas.Content);
					}
				}
				return editorCounter;
			}
		}

		void Awake()
		{
			editorCounter = this;
			textComponent.text = "Geo : " + currentGeo;
		}

		public override int Geo
		{
			get
			{
				return currentGeo;
			}
		}

		public override bool CurrentlyTakingGeo
		{
			get
			{
				return false;
			}
		}

		public override string GeoText
		{
			get
			{
				return textComponent.text;
			}

			set
			{
				textComponent.text = value;
			}
		}

		public override void AddGeoToDisplay(int geo)
		{
			currentGeo += geo;
			textComponent.text = "Geo : " + currentGeo;
		}

		public override void TakeGeoFromDisplay(int geo)
		{
			currentGeo -= geo;
			if (currentGeo < 0)
			{
				currentGeo = 0;
			}
			textComponent.text = "Geo : " + currentGeo;
		}

		public override void SetDisplayToZero()
		{
			currentGeo = 0;
			textComponent.text = "Geo : " + currentGeo;
		}
	}
}
