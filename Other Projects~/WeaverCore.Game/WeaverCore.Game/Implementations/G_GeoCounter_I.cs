using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_GeoCounter_I : WeaverCore.Implementations.GeoCounter
	{
		[OnInit]
		static void Init()
		{
			On.GeoCounter.Awake += GeoCounter_Awake;
		}

		private static void GeoCounter_Awake(On.GeoCounter.orig_Awake orig, GeoCounter self)
		{
			orig(self);
			if (self.gameObject.GetComponent<G_GeoCounter_I>() == null)
			{
				self.gameObject.AddComponent<G_GeoCounter_I>();
			}
		}

		protected class G_Static_I : Static_I
		{
			public override WeaverCore.Implementations.GeoCounter GetCounter()
			{
				return HeroController.instance.geoCounter.GetComponent<WeaverCore.Implementations.GeoCounter>();
			}
		}

		global::GeoCounter counter;

		Func<global::GeoCounter, int> GetDisplayGeo;
		Func<global::GeoCounter, int> GetAddRollerState;
		Func<global::GeoCounter, int> GetTakeRollerState;

		void Awake()
		{
			counter = GetComponent<global::GeoCounter>();
			GetDisplayGeo = ReflectionUtilities.CreateFieldGetter<global::GeoCounter, int>("counterCurrent");
			GetAddRollerState = ReflectionUtilities.CreateFieldGetter<global::GeoCounter, int>("addRollerState");
			GetTakeRollerState = ReflectionUtilities.CreateFieldGetter<global::GeoCounter, int>("takeRollerState");
		}

		public override int Geo
		{
			get
			{
				return GetDisplayGeo(counter);
			}
		}

		public override bool CurrentlyTakingGeo
		{
			get
			{
				return GetAddRollerState(counter) == 0 && GetTakeRollerState(counter) == 0;
			}
		}

		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public override void AddGeo(int geo)
		{
			counter.AddGeo(geo);
		}

		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public override void TakeGeo(int geo)
		{
			counter.TakeGeo(geo);
		}

		//NOTE: THIS IS VISUAL ONLY, THIS DOES NOT APPLY TO THE PLAYER, ONLY THE DISPLAY
		public override void ToZero()
		{
			counter.ToZero();
		}
	}
}
