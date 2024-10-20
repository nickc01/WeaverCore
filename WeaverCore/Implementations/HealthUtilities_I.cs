﻿using UnityEngine;
using WeaverCore.Interfaces;
using static WeaverCore.Utilities.HealthUtilities;

namespace WeaverCore.Implementations
{
    public abstract class HealthUtilities_I : IImplementation
	{
		public abstract bool HasHealthComponent(GameObject obj);
		public abstract MonoBehaviour GetHealthComponent(GameObject obj);
        public abstract HealthComponentType GetHealthComponentType(MonoBehaviour healthComponent);
        public abstract bool TrySetHealth(MonoBehaviour healthComponent, int newHealth);
        public abstract bool TryGetHealth(MonoBehaviour healthComponent, out int result);


        public HealthComponentType GetHealthComponentType(GameObject obj) => GetHealthComponentType(GetHealthComponent(obj));
		public bool TrySetHealth(GameObject obj, int newHealth) => TrySetHealth(GetHealthComponent(obj), newHealth);
		public bool TryGetHealth(GameObject obj, out int result) => TryGetHealth(GetHealthComponent(obj), out result);
    }
}
