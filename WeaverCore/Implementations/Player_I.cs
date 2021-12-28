using System;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class Player_I : MonoBehaviour, IImplementation
	{
		public abstract void Initialize();

		public abstract void SoulGain();

		public abstract void RefreshSoulUI();

		public abstract void EnterParryState();

		public abstract void RecoverFromParry();

		public abstract void Recoil(CardinalDirection direction);

		public abstract bool HasDreamNail { get; }

		public abstract bool HasCharmEquipped(int charmNumber);

		public abstract int EssenceCollected { get; set; }

		public abstract int EssenceSpent { get; set; }

		public abstract void EnterRoarLock();
		public abstract void ExitRoarLock();
	}
}
