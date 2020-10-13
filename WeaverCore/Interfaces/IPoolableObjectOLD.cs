using UnityEngine;

namespace WeaverCore.Interfaces
{
	public interface IPoolableObjectOLD
	{
		void OnPool();
		GameObject gameObject { get; }
	}
}
