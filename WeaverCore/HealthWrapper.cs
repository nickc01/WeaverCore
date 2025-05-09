using System;
using System.Collections;
using UnityEngine;

namespace WeaverCore
{
    public abstract class HealthWrapper
	{
		public abstract MonoBehaviour HealthComponent { get; }

		public abstract int Health { get; set; }
		public abstract int SmallGeo { get; set; }
		public abstract int MediumGeo { get; set; }
		public abstract int LargeGeo { get; set; }
		public abstract bool Invincible { get; set; }
		public abstract bool IsDead { get; }
		public abstract bool Hit(HitInfo hit);
		public abstract void Die(HitInfo hit);

		public abstract event Action<HitInfo> OnDeath;

		public Coroutine StartCoroutine(string methodName) => HealthComponent.StartCoroutine(methodName);
		public Coroutine StartCoroutine(string methodName, object value) => HealthComponent.StartCoroutine(methodName, value);
		public Coroutine StartCoroutine(IEnumerator routine) => HealthComponent.StartCoroutine(routine);
		public void StopCoroutine(IEnumerator routine) => HealthComponent.StopCoroutine(routine);
		public void StopCoroutine(Coroutine routine) => HealthComponent.StopCoroutine(routine);
		public bool enabled { get => HealthComponent.enabled; set => HealthComponent.enabled = value; }
		public bool isActiveAndEnabled => HealthComponent.isActiveAndEnabled;
		public Transform transform => HealthComponent.transform;
		public GameObject gameObject => HealthComponent.gameObject;
		public string tag { get => gameObject.tag; set => gameObject.tag = value; }
	}
}