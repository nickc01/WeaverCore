﻿using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class Player_I : MonoBehaviour, IImplementation
    {
        public abstract void Initialize();
    }
}