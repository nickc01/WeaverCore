using UnityEngine;

namespace WeaverCore.Enums
{
    /// <summary>
    /// Used on a variety of different scripts to determine what they do when they finish
    /// </summary>
    public enum OnDoneBehaviour
	{
		/// <summary>
		/// The script does nothing when finished
		/// </summary>
		Nothing,
		/// <summary>
		/// The script's object will be disabled when finished
		/// </summary>
		Disable,
		/// <summary>
		/// The script's object will be destroyed when finished
		/// </summary>
		Destroy,
		/// <summary>
		/// The script's object will be sent back into a pool (or destroyed of it's not part of a pool)
		/// </summary>
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
