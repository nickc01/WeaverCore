using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WeaverCore.Playmaker.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an FSM state that can be used for restoration
    /// </summary>
    public class FsmSnapshot
		{        
			// Basic FSM information
			public string FsmName { get; private set; }
			public Dictionary<string, FsmStateSnapshot> States { get; private set; } = new Dictionary<string, FsmStateSnapshot>();
			public List<FsmTransitionSnapshot> GlobalTransitions { get; private set; } = new List<FsmTransitionSnapshot>();
			public List<FsmVariableSnapshot> Variables { get; private set; } = new List<FsmVariableSnapshot>();
			public string ActiveStateName { get; private set; }
			
			/// <summary>
			/// Creates a new snapshot from the given FSM
			/// </summary>
			public static FsmSnapshot Create(object fsm, Component fsmComponent)
			{            
				if (fsm == null || fsmComponent == null) return null;
				
				var snapshot = new FsmSnapshot
				{
					FsmName = PlayMakerUtilities.GetFsmName(fsmComponent),
					ActiveStateName = PlayMakerUtilities.GetActiveStateName(fsm)
				};
				
				// Capture state information
				var states = PlayMakerUtilities.GetStates(fsm);
				foreach (var state in states)
				{                
					var stateInfo = state.GetType().GetProperty("Name");                
					string stateName = (string)stateInfo.GetValue(state, null);
					
					var stateSnapshot = FsmStateSnapshot.Create(state);
					snapshot.States[stateName] = stateSnapshot;
				}
				
				// Capture global transitions
				var globalTransitions = PlayMakerUtilities.GetGlobalTransitions(fsm);
				foreach (var transition in globalTransitions)
				{                
					snapshot.GlobalTransitions.Add(FsmTransitionSnapshot.Create(transition));
				}
				
				// Capture variables
				var variables = PlayMakerUtilities.GetVariables(fsm);
				if (variables != null)
				{               
					// Get all variable collections from the FsmVariables object
					Type variablesType = variables.GetType();
					PropertyInfo[] properties = variablesType.GetProperties();
					
					foreach (var property in properties)
					{                    
						// Only look for array properties that might contain variables
						if (property.PropertyType.IsArray)
						{                        
							Array variableArray = property.GetValue(variables, null) as Array;
							if (variableArray != null)
							{                            
								foreach (var variable in variableArray)
								{                                
									var varSnapshot = FsmVariableSnapshot.Create(variable);
									if (varSnapshot != null)
									{
										snapshot.Variables.Add(varSnapshot);
									}
								}
							}
						}
					}
				}
				
				return snapshot;
			}
			
			/// <summary>
			/// Restores this snapshot to the given FSM
			/// </summary>
			public void Restore(object fsm, Component fsmComponent)
			{            
				if (fsm == null || fsmComponent == null) return;
				
				// First, restore variables
				RestoreVariables(fsm);
				
				// Restore global transitions
				RestoreGlobalTransitions(fsm);
				
				// For each state in the FSM, restore it if we have a snapshot
				var currentStates = PlayMakerUtilities.GetStates(fsm);
				foreach (var state in currentStates)
				{                
					var stateInfo = state.GetType().GetProperty("Name");                
					string stateName = (string)stateInfo.GetValue(state, null);
					
					if (States.TryGetValue(stateName, out var stateSnapshot))
					{                    
						stateSnapshot.Restore(state);
					}
				}
				
				// Set the active state last
				if (!string.IsNullOrEmpty(ActiveStateName))
				{                
					PlayMakerUtilities.SetActiveState(fsm, ActiveStateName);
				}
			}
			
			private void RestoreVariables(object fsm)
			{            
				if (fsm == null || Variables == null) return;
				
				foreach (var varSnapshot in Variables)
				{                
					varSnapshot.Restore(fsm);
				}
			}
			
			private void RestoreGlobalTransitions(object fsm)
			{            
				if (fsm == null) return;
				
				// Get current global transitions            
				var currentTransitions = PlayMakerUtilities.GetGlobalTransitions(fsm);
				int currentCount = currentTransitions.Length;
				
				// Remove all current transitions first (from end to start to avoid index shifting)
				for (int i = currentCount - 1; i >= 0; i--)
				{                    
					PlayMakerUtilities.RemoveGlobalTransition(fsm, i);
				}
				
				// Add all transitions from snapshot                
				foreach (var transSnapshot in GlobalTransitions)
				{                    
					var eventName = transSnapshot.EventName;
					var toState = transSnapshot.ToState;
					
					if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(toState))
					{                        
						PlayMakerUtilities.AddGlobalTransition(fsm, eventName, toState);
					}
				}
			}
		}
		
}
