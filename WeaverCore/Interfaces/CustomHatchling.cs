using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Interfaces
{
	public abstract class CustomHatchling : MonoBehaviour
	{
		public abstract void OnSpawn(CustomHatchlingSpawner spawner);
		public abstract void OnDeath(CustomHatchlingSpawner spawner);
	} 
}
