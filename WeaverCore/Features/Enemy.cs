using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
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
	}
}
