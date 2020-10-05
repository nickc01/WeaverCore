using UnityEngine;

namespace WeaverCore.Interfaces
{
	public interface IPoolableObject
	{
		void OnPool();
		GameObject gameObject { get; }
	}
}
