/*using System.Collections.Generic;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an FSM state
    /// </summary>
    public class FsmStateSnapshot
		{        
			public string Name { get; private set; }
			public List<FsmActionSnapshot> Actions { get; private set; } = new List<FsmActionSnapshot>();
			public List<FsmTransitionSnapshot> Transitions { get; private set; } = new List<FsmTransitionSnapshot>();
			
			/// <summary>
			/// Creates a snapshot from the given state
			/// </summary>
			public static FsmStateSnapshot Create(object state)
			{            
				if (state == null) return null;
				
				var snapshot = new FsmStateSnapshot();
				
				// Get state name            
				var nameProperty = state.GetType().GetProperty("Name");            
				snapshot.Name = (string)nameProperty.GetValue(state, null);
				
				// Get actions            
				var actions = PlayMakerUtilities.GetActions(state);
				foreach (var action in actions)
				{                
					snapshot.Actions.Add(FsmActionSnapshot.Create(action));
				}
				
				// Get transitions            
				var transitions = PlayMakerUtilities.GetTransitions(state);
				foreach (var transition in transitions)
				{                
					snapshot.Transitions.Add(FsmTransitionSnapshot.Create(transition));
				}
				
				return snapshot;
			}
			
			/// <summary>
			/// Restores this snapshot to the given state
			/// </summary>
			public void Restore(object state)
			{            
				if (state == null) return;
				
				// Restore transitions            
				RestoreTransitions(state);
				
				// Restore actions            
				RestoreActions(state);
			}
			
			private void RestoreActions(object state)
			{            
				if (state == null) return;
				
				// Get current actions            
				var currentActions = PlayMakerUtilities.GetActions(state);
				int currentCount = currentActions.Length;
				
				// If count is different, we need to rebuild
				if (currentCount != Actions.Count)
				{                
					// Remove all current actions first (from end to start to avoid index shifting)
					for (int i = currentCount - 1; i >= 0; i--)
					{                    
						PlayMakerUtilities.RemoveAction(state, i);
					}
					
					// Add all actions from snapshot
					foreach (var actionSnapshot in Actions)
					{                    
						//var actionType = actionSnapshot.ActionType;
						if (actionSnapshot.Action != null)
						{                        
							var newAction = PlayMakerUtilities.AddAction(new FsmStateWrapper(state), new FsmActionWrapper(actionSnapshot.Action));
							actionSnapshot.RestoreProperties(newAction.InternalAction);
						}
					}
				}
				else
				{                
					// Same count, just update properties
					for (int i = 0; i < currentCount; i++)
					{                    
						var actionSnapshot = Actions[i];
						var currentAction = currentActions[i];
						
						// Check if types match before updating properties
						if (currentAction.GetType().FullName != actionSnapshot.Action.GetType().FullName)
						{       
							PlayMakerUtilities.RemoveAction(state, i);
							var newAction = PlayMakerUtilities.AddAction(new FsmStateWrapper(state), new FsmActionWrapper(actionSnapshot.Action));
							actionSnapshot.RestoreProperties(newAction.InternalAction);
						}
						else
						{
							actionSnapshot.RestoreProperties(currentAction);
						}
					}
				}
			}
			
			private void RestoreTransitions(object state)
			{            
				if (state == null) return;
				
				// Get current transitions            
				var currentTransitions = PlayMakerUtilities.GetTransitions(state);
				int currentCount = currentTransitions.Length;
				
				// Remove all current transitions first
				for (int i = currentCount - 1; i >= 0; i--)
				{                    
					PlayMakerUtilities.RemoveTransition(state, i);
				}
				
				// Add all transitions from snapshot                
				foreach (var transSnapshot in Transitions)
				{                    
					var eventName = transSnapshot.EventName;
					var toState = transSnapshot.ToState;
					
					if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(toState))
					{                        
						PlayMakerUtilities.AddTransition(state, eventName, toState);
					}
				}
			}
		}
		
}*/
