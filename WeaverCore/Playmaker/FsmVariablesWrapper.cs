namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmVariables objects
    /// </summary>
    public struct FsmVariablesWrapper
	{
		public object InternalVariables { get; }
		
		internal FsmVariablesWrapper(object variables)
		{
			InternalVariables = variables;
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmVariablesWrapper fsm)
			{
				return fsm.InternalVariables == InternalVariables;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalVariables != null ? InternalVariables.GetHashCode() : 0;
		}

		public static bool operator==(FsmVariablesWrapper a, FsmVariablesWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmVariablesWrapper a, FsmVariablesWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
