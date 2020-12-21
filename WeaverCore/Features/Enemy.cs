using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.DataTypes;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	[ShowFeature]
	public class Enemy : Feature
	{
		Enemy_I enemyImpl;
		static Enemy_I.Statics staticImpl = ImplFinder.GetImplementation<Enemy_I.Statics>();

		void Awake()
		{
			var enemyImplType = ImplFinder.GetImplementationType<Enemy_I>();
			enemyImpl = (Enemy_I)gameObject.AddComponent(enemyImplType);
		}

		public static IEnumerator Roar(GameObject source, float duration)
		{
			return Roar(source, duration, null);
		}

		public IEnumerator Roar(float duration)
		{
			return Roar(gameObject, duration, null);
		}

		public IEnumerator Roar(float duration, AudioClip roarSound)
		{
			return Roar(gameObject, duration, roarSound);
		}

		public static IEnumerator Roar(GameObject source, float duration, AudioClip roarSound)
		{
			return staticImpl.Roar(source, duration, roarSound);
		}

		/// <summary>
		/// Called when the enemy gets parried
		/// </summary>
		/// <param name="collider">The collider that recieved the parry</param>
		/// <param name="hit">The hit info of the parried attack</param>
		public virtual void OnParry(IHittable collider, HitInfo hit)
		{

		}
	}
}
