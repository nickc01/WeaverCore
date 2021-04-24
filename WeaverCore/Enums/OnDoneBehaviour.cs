using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Enums
{
	public enum OnDoneBehaviour
	{
		Nothing,
		Disable,
		Destroy,
		DestroyOrPool
	}

	public static class OnDoneBehaviour_Extensions
	{
		public static void DoneWithObject(this OnDoneBehaviour behaviour, GameObject gameObject)
		{
			switch (behaviour)
			{
				case OnDoneBehaviour.Nothing:
					break;
				case OnDoneBehaviour.Disable:
					gameObject.SetActive(false);
					break;
				case OnDoneBehaviour.Destroy:
					GameObject.Destroy(gameObject);
					break;
				case OnDoneBehaviour.DestroyOrPool:
					var poolable = gameObject.GetComponent<PoolableObject>();
					if (poolable != null)
					{
						poolable.ReturnToPool();
					}
					else
					{
						GameObject.Destroy(gameObject);
					}
					break;
				default:
					break;
			}
		}

		public static void DoneWithObject(this OnDoneBehaviour behaviour, Component component)
		{
			DoneWithObject(behaviour, component.gameObject);
		}
	}

}
