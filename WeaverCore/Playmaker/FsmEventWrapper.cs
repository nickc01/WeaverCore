using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmEvent objects
    /// </summary>
    public struct FsmEventWrapper
	{
		public object InternalEvent { get; }
		
		public FsmEventWrapper(object fsmEvent)
		{
			InternalEvent = fsmEvent;
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmEventWrapper fsm)
			{
				return fsm.InternalEvent == InternalEvent;
			}

			return false;
		}

		public string Name => InternalEvent.ReflectGetProperty<string>("Name");

		public bool IsSystemEvent => InternalEvent.ReflectGetProperty<bool>("IsSystemEvent");

		public bool IsGlobal => InternalEvent.ReflectGetProperty<bool>("IsGlobal");
		
		public override int GetHashCode()
		{
			return InternalEvent != null ? InternalEvent.GetHashCode() : 0;
		}

		public static bool operator==(FsmEventWrapper a, FsmEventWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmEventWrapper a, FsmEventWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
