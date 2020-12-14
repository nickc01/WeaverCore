using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

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
		public abstract void AddGeo(int geo);
		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public abstract void TakeGeo(int geo);
		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public abstract void ToZero();
		public abstract int Geo { get; }
		public abstract bool CurrentlyTakingGeo { get; }
	}
}
