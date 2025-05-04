using UnityEngine;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for PlayMakerFSM components
    /// </summary>
    public struct PlayMakerFsmWrapper
	{
		public Component InternalComponent { get; }
		
		internal PlayMakerFsmWrapper(Component component)
		{
			InternalComponent = component;
		}
		
		public FsmWrapper GetFsm()
		{
			return new FsmWrapper(PlayMakerUtilities.GetFsm(InternalComponent));
		}
		
		public string GetName()
		{
			return PlayMakerUtilities.GetFsmName(InternalComponent);
		}
		
		public void SetName(string newName)
		{
			PlayMakerUtilities.SetFsmName(InternalComponent, newName);
		}
		
		public void SendEvent(string eventName)
		{
			PlayMakerUtilities.SendEvent(InternalComponent, eventName);
		}

		public override bool Equals(object obj)
		{
			if (obj is PlayMakerFsmWrapper fsm)
			{
				return fsm.InternalComponent == InternalComponent;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalComponent != null ? InternalComponent.GetHashCode() : 0;
		}

		public static bool operator==(PlayMakerFsmWrapper a, PlayMakerFsmWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(PlayMakerFsmWrapper a, PlayMakerFsmWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
