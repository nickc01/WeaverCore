using WeaverCore.Interfaces;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Implementations
{
	public abstract class Shaker_I : MonoBehaviour, IImplementation
	{
		public abstract void Shake(Vector3 amount, float duration, int priority = 100);
		public abstract void Shake(ShakeType type);
		public abstract void SetRumble(Vector3 amount);
		public abstract void SetRumble(RumbleType type);
		public abstract void StopRumbling();
	}
}
