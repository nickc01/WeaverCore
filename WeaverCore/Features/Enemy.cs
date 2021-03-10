using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
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

		public static IEnumerator Roar(GameObject source, float duration, bool lockPlayer = true)
		{
			return Roar(source, duration, null,lockPlayer);
		}

		public IEnumerator Roar(float duration, bool lockPlayer = true)
		{
			return Roar(gameObject, duration, null, lockPlayer);
		}

		public IEnumerator Roar(float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return Roar(gameObject, duration, roarSound, lockPlayer);
		}

		public static IEnumerator Roar(GameObject source, float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return staticImpl.Roar(source, duration, roarSound, lockPlayer);
		}

		public static IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, bool lockPlayer = true)
		{
			return Roar(source, spawnPosition, duration, null, lockPlayer);
		}

		public IEnumerator Roar(float duration, Vector3 spawnPosition, bool lockPlayer = true)
		{
			return Roar(gameObject, spawnPosition, duration, null, lockPlayer);
		}

		public IEnumerator Roar(float duration, Vector3 spawnPosition, AudioClip roarSound, bool lockPlayer = true)
		{
			return Roar(gameObject, spawnPosition, duration, roarSound, lockPlayer);
		}

		public static IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return staticImpl.Roar(source, spawnPosition, duration, roarSound, lockPlayer);
		}

		/*public static RoarEmitter Spawn(GameObject source)
		{
			return staticImpl.Spawn(source);
		}

		public static RoarEmitter Spawn(GameObject source, Vector3 spawnPosition)
		{
			return staticImpl.Spawn(source, spawnPosition);
		}*/

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
