/*namespace WeaverCore.Playmaker.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an FSM transition
    /// </summary>
    public class FsmTransitionSnapshot
		{        
			public string EventName { get; private set; }
			public string ToState { get; private set; }
			
			/// <summary>
			/// Creates a snapshot from the given transition
			/// </summary>
			public static FsmTransitionSnapshot Create(object transition)
			{            
				if (transition == null) return null;
				
				var snapshot = new FsmTransitionSnapshot();
				
				// Get transition properties            
				var eventNameProperty = transition.GetType().GetProperty("EventName");            
				snapshot.EventName = (string)eventNameProperty.GetValue(transition, null);
				
				var toStateProperty = transition.GetType().GetProperty("ToState");            
				snapshot.ToState = (string)toStateProperty.GetValue(transition, null);
				
				return snapshot;
			}
		}
		
}*/
