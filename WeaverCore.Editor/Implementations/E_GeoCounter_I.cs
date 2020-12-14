using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Editor.Utilities;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_GeoCounter_I : GeoCounter
	{
		[SerializeField]
		Text GeoText;

		int currentGeo = 0;

		static E_GeoCounter_I editorCounter;

		protected class E_Static_I : Static_I
		{
			public override GeoCounter GetCounter()
			{
				if (editorCounter == null)
				{
					var counterPrefab = EditorAssets.LoadEditorAsset<GameObject>("Geo Counter").GetComponent<E_GeoCounter_I>();


					editorCounter = GameObject.Instantiate(counterPrefab, WeaverCanvas.Content);
				}
				return editorCounter;
			}
		}

		void Awake()
		{
			editorCounter = this;
			GeoText.text = "Geo : " + currentGeo;
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

		public override void AddGeo(int geo)
		{
			currentGeo += geo;
			GeoText.text = "Geo : " + currentGeo;
		}

		public override void TakeGeo(int geo)
		{
			currentGeo -= geo;
			if (currentGeo < 0)
			{
				currentGeo = 0;
			}
			GeoText.text = "Geo : " + currentGeo;
		}

		public override void ToZero()
		{
			currentGeo = 0;
			GeoText.text = "Geo : " + currentGeo;
		}
	}
}
