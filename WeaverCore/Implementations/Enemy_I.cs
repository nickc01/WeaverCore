using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class Enemy_I : MonoBehaviour, IImplementation
    {
        public abstract class Statics : IImplementation
        {
            public abstract IEnumerator Roar(GameObject source, float duration, AudioClip roarSound, bool lockPlayer);
            public abstract IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, AudioClip roarSound, bool lockPlayer);
        }
    }
}
