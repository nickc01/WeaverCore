using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
    public abstract class GeoCounter : MonoBehaviour, IImplementation
	{
		protected abstract class Static_I : IImplementation
		{
			public abstract GeoCounter GetCounter();
		}

		static Static_I statics;

		public static GeoCounter Instance
		{
			get
			{
				if (statics == null)
				{
					statics = ImplFinder.GetImplementation<Static_I>();
				}
				return statics.GetCounter();
			}
		}

		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public abstract void AddGeoToDisplay(int geo);
		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public abstract void TakeGeoFromDisplay(int geo);
		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public abstract void SetDisplayToZero();
		public abstract int Geo { get; }
		public abstract bool CurrentlyTakingGeo { get; }
		public abstract string GeoText { get; set; }
	}
}
