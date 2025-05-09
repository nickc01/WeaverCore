namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmTransition objects
    /// </summary>
    public struct FsmTransitionWrapper
	{
		public object InternalTransition { get; }
		
		public FsmTransitionWrapper(object transition)
		{
			InternalTransition = transition;
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmTransitionWrapper fsm)
			{
				return fsm.InternalTransition == InternalTransition;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalTransition != null ? InternalTransition.GetHashCode() : 0;
		}

		public static bool operator==(FsmTransitionWrapper a, FsmTransitionWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmTransitionWrapper a, FsmTransitionWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
