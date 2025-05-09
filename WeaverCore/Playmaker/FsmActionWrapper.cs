using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmStateAction objects
    /// </summary>
    public struct FsmActionWrapper
	{
		public object InternalAction { get; }
		
		public FsmActionWrapper(object action)
		{
			InternalAction = action;
		}
		
		public object GetProperty(string propertyName)
		{
			return PlayMakerUtilities.GetActionProperty(InternalAction, propertyName);
		}
		
		public bool SetProperty(string propertyName, object value)
		{
			return PlayMakerUtilities.SetActionProperty(InternalAction, propertyName, value);
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmActionWrapper fsm)
			{
				return fsm.InternalAction == InternalAction;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalAction != null ? InternalAction.GetHashCode() : 0;
		}

		public static bool operator==(FsmActionWrapper a, FsmActionWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmActionWrapper a, FsmActionWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
