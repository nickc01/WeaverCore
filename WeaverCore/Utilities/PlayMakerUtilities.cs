using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Implementations;
using WeaverCore.Playmaker;
//using WeaverCore.Playmaker.Snapshots;
using WeaverCore.Utilities;

namespace WeaverCore.Utilities
{
	public static class PlayMakerUtilities
	{
		private static PlayMaker_I impl = ImplFinder.GetImplementation<PlayMaker_I>();
		public static bool PlayMakerAvailable => Initialization.Environment == WeaverCore.Enums.RunningState.Game;

		#region Type Cache

		// Cache reflection types to improve performance
		public static readonly Type PlayMakerFSMType;
		public static readonly Type FsmType;
		public static readonly Type FsmStateType;
		public static readonly Type FsmTransitionType;
		public static readonly Type FsmStateActionType;
		public static readonly Type FsmEventType;
		public static readonly Type ActionDataType;
		public static readonly Type FsmVariablesType;

		// Cache commonly used methods and properties
		private static readonly MethodInfo GetFsmEventMethod;
		private static readonly PropertyInfo StatesProperty;
		private static readonly PropertyInfo EventsProperty;
		private static readonly PropertyInfo GlobalTransitionsProperty;
		private static readonly PropertyInfo ActiveStateProperty;
		private static readonly PropertyInfo ActiveStateNameProperty;
		private static readonly PropertyInfo FsmNameProperty;

		private static readonly string STATES_PROP_NAME = "States";
		private static readonly string EVENTS_PROP_NAME = "Events";
		private static readonly string GLOBAL_TRANSITIONS_PROP_NAME = "GlobalTransitions";
		private static readonly string ACTIVE_STATE_PROP_NAME = "ActiveState";
		private static readonly string ACTIVE_STATE_NAME_PROP_NAME = "ActiveStateName";
		private static readonly string FSM_NAME_PROP_NAME = "FsmName";
		private static readonly string PS_FSM_NAME_PROP_NAME = "Fsm";

		static PlayMakerUtilities()
		{
			if (!PlayMakerAvailable)
			{
				return;
			}
			
			// Initialize cached types through reflection
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType("PlayMakerFSM");
				if (type != null)
				{
					PlayMakerFSMType = type;
				}
			}

			FsmType = TypeUtilities.NameToType("HutongGames.PlayMaker.Fsm", PlayMakerFSMType.Assembly.GetName().Name);
			FsmStateType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmState", PlayMakerFSMType.Assembly.GetName().Name);
			FsmTransitionType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmTransition", PlayMakerFSMType.Assembly.GetName().Name);
			FsmStateActionType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmStateAction", PlayMakerFSMType.Assembly.GetName().Name);
			FsmEventType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmEvent", PlayMakerFSMType.Assembly.GetName().Name);
			ActionDataType = TypeUtilities.NameToType("HutongGames.PlayMaker.ActionData", PlayMakerFSMType.Assembly.GetName().Name);
			FsmVariablesType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmVariables", PlayMakerFSMType.Assembly.GetName().Name);

			// Cache commonly used methods and properties
			GetFsmEventMethod = FsmEventType?.GetMethod("GetFsmEvent", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
			
			// Property accessors for FSM
			StatesProperty = FsmType?.GetProperty(STATES_PROP_NAME);
			EventsProperty = FsmType?.GetProperty(EVENTS_PROP_NAME);
			GlobalTransitionsProperty = FsmType?.GetProperty(GLOBAL_TRANSITIONS_PROP_NAME);
			ActiveStateProperty = FsmType?.GetProperty(ACTIVE_STATE_PROP_NAME);
			ActiveStateNameProperty = FsmType?.GetProperty(ACTIVE_STATE_NAME_PROP_NAME);
			FsmNameProperty = PlayMakerFSMType?.GetProperty(FSM_NAME_PROP_NAME);
		}

		#endregion

		#region FSM Finding and Access

		public static bool IsPlayMakerFSM(object obj) =>
        obj != null && obj.GetType() == PlayMakerFSMType;

		public static bool IsFsm(object obj) =>
			obj != null && obj.GetType() == FsmType;

		public static bool IsFsmState(object obj) =>
			obj != null && obj.GetType() == FsmStateType;

		public static bool IsFsmTransition(object obj) =>
			obj != null && obj.GetType() == FsmTransitionType;

		public static bool IsFsmStateAction(object obj) =>
			obj != null && FsmStateActionType.IsAssignableFrom(obj.GetType());

		public static bool IsFsmEvent(object obj) =>
			obj != null && FsmEventType.IsAssignableFrom(obj.GetType());

		public static bool IsActionData(object obj) =>
			obj != null && obj.GetType() == ActionDataType;

		public static bool IsFsmVariables(object obj) =>
			obj != null && obj.GetType() == FsmVariablesType;

		public static MonoBehaviour FindPlayMakerFSM(GameObject gameObject, string fsmName)
		{
			if (gameObject == null || PlayMakerFSMType == null)
			{
				return null;
			}

			Component[] fsms = gameObject.GetComponents(PlayMakerFSMType);
			foreach (var fsm in fsms)
			{
				if (fsm.ReflectGetProperty<string>(FSM_NAME_PROP_NAME) == fsmName)
				{
					return (MonoBehaviour)fsm;
				}
			}

			return null;
		}

		public static PlayMakerFsmWrapper FindPlayMakerFSMWrapper(GameObject gameObject, string fsmName)
		{
			MonoBehaviour component = FindPlayMakerFSM(gameObject, fsmName);
			return component != null ? new PlayMakerFsmWrapper(component) : default;
		}

		public static bool ContainsPlayMakerFSM(GameObject gameObject, string fsmName, out Component psFSM)
		{
			return (psFSM = FindPlayMakerFSM(gameObject, fsmName)) != null;
		}

		public static bool ContainsPlayMakerFSM(GameObject gameObject, string fsmName, out PlayMakerFsmWrapper psFSM)
		{
			return (psFSM = FindPlayMakerFSMWrapper(gameObject, fsmName)) != default;
		}

		public static Component[] GetAllPlayMakerFSMs(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return CacheUtilities.GetTempArray<Component>(0);
			}

			return gameObject.GetComponents(PlayMakerFSMType);
		}

		public static PlayMakerFsmWrapper[] GetAllPlayMakerFSMsWrapper(GameObject gameObject)
		{
			Component[] components = GetAllPlayMakerFSMs(gameObject);
			return components.Select(c => new PlayMakerFsmWrapper(c)).ToArray();
		}

		public static object GetFsm(Component playMakerFSM)
		{
			if (!IsPlayMakerFSM(playMakerFSM))
			{
				return null;
			}

			return playMakerFSM.ReflectGetProperty(PS_FSM_NAME_PROP_NAME);
		}

		public static FsmWrapper GetFsmWrapper(Component playMakerFSM)
		{
			object fsm = GetFsm(playMakerFSM);
			return fsm != null ? new FsmWrapper(fsm) : default;
		}

		public static FsmWrapper GetFsmWrapper(PlayMakerFsmWrapper playMakerFSM)
		{
			object fsm = GetFsm(playMakerFSM.InternalComponent);
			return fsm != null ? new FsmWrapper(fsm) : default;
		}

		public static string GetFsmName(Component playMakerFSM)
		{
			if (!IsPlayMakerFSM(playMakerFSM))
			{
				return null;
			}

			return playMakerFSM.ReflectGetProperty<string>(FSM_NAME_PROP_NAME);
		}

		public static string GetFsmName(PlayMakerFsmWrapper playMakerFSM)
		{
			return GetFsmName(playMakerFSM.InternalComponent);
		}

		public static bool SetFsmName(Component playMakerFSM, string newName)
		{
			if (!IsPlayMakerFSM(playMakerFSM))
			{
				return false;
			}

			playMakerFSM.ReflectSetProperty(FSM_NAME_PROP_NAME, newName);
			return true;
		}

		public static void SetFsmName(PlayMakerFsmWrapper playMakerFSM, string newName)
		{
			SetFsmName(playMakerFSM.InternalComponent, newName);
		}

		#endregion

		#region State Management

		public static object[] GetStates(object fsm)
		{
			if (!IsFsm(fsm))
			{
				return CacheUtilities.GetTempArray<object>(0);
			}

			return fsm.ReflectGetProperty<object[]>(STATES_PROP_NAME);
		}

		public static FsmStateWrapper[] GetStates(FsmWrapper fsm)
		{
			object[] states = GetStates(fsm.InternalFsm);
			return states.Select(s => new FsmStateWrapper(s)).ToArray();
		}

		public static object GetState(object fsm, string stateName)
		{
			if (!IsFsm(fsm) || string.IsNullOrEmpty(stateName))
			{
				return null;
			}

			return fsm.ReflectCallMethod("GetState", CacheUtilities.GetTempSingleArray<object>(stateName));
		}

		public static FsmStateWrapper GetState(FsmWrapper fsm, string stateName)
		{
			object state = GetState(fsm.InternalFsm, stateName);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		public static object AddState(object fsm, string stateName, Vector2 position = default)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return null;
			}

			// Get the current states
			object[] currentStates = GetStates(fsm);
			
			// Create a new state array with one more element
			Array newStates = Array.CreateInstance(FsmStateType, currentStates.Length + 1);
			Array.Copy(currentStates, newStates, currentStates.Length);

			// Create a new state
			//ConstructorInfo stateConstructor = FsmStateType.GetConstructor(new[] { FsmType });
			//object newState = stateConstructor.Invoke(new[] { fsm });
			object newState = Activator.CreateInstance(FsmStateType, new object[] { fsm });

			// Set the state properties
			PropertyInfo nameProperty = FsmStateType.GetProperty("Name");
			nameProperty.SetValue(newState, stateName, null);

			PropertyInfo positionProperty = FsmStateType.GetProperty("Position");
			positionProperty.SetValue(newState, new Rect(position.x, position.y, 100f, 16f), null);

			// Add the new state to the array
			newStates.SetValue(newState, currentStates.Length);

			// Update the FSM's States property
			StatesProperty.SetValue(fsm, newStates, null);

			return newState;
		}

		public static FsmStateWrapper AddState(FsmWrapper fsm, string stateName, Vector2 position = default)
		{
			object state = AddState(fsm.InternalFsm, stateName, position);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		public static object GetStateFsm(object state)
		{
			return state.ReflectGetProperty("Fsm");
		}

		public static FsmWrapper GetStateFsm(FsmStateWrapper state)
		{
			object fsm = GetStateFsm(state.InternalState);
			return fsm != null ? new FsmWrapper(fsm) : default;
		}

		public static bool RemoveState(object fsm, string stateName)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return false;
			}

			// Get the current states
			object[] currentStates = GetStates(fsm);
			
			// Find the state to remove
			object stateToRemove = GetState(fsm, stateName);
			if (stateToRemove == null)
			{
				return false;
			}

			// Create a new state array without the state to remove
			List<object> stateList = new List<object>(currentStates);
			stateList.Remove(stateToRemove);
			
			// Convert back to an array of the correct type
			Array newStates = Array.CreateInstance(FsmStateType, stateList.Count);
			stateList.CopyTo((object[])newStates);

			// Update the FSM's States property
			StatesProperty.SetValue(fsm, newStates, null);

			// Also need to remove any transitions to this state
			RemoveTransitionsToState(fsm, stateName);

			return true;
		}

		public static bool RemoveState(FsmWrapper fsm, string stateName)
		{
			return RemoveState(fsm.InternalFsm, stateName);
		}

		public static object GetActiveState(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			return ActiveStateProperty.GetValue(fsm, null);
		}

		public static FsmStateWrapper GetActiveState(FsmWrapper fsm)
		{
			object state = GetActiveState(fsm.InternalFsm);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		public static string GetActiveStateName(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			return (string)ActiveStateNameProperty.GetValue(fsm, null);
		}

		public static string GetActiveStateName(FsmWrapper fsm)
		{
			return GetActiveStateName(fsm.InternalFsm);
		}

		public static void SetActiveState(object fsm, string stateName)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return;
			}

			MethodInfo setStateMethod = FsmType.GetMethod("SetState", new[] { typeof(string) });
			setStateMethod.Invoke(fsm, new object[] { stateName });
		}

		public static void SetActiveState(FsmWrapper fsm, string stateName)
		{
			SetActiveState(fsm.InternalFsm, stateName);
		}

		#endregion

		#region Action Management

		public static object[] GetActions(object state)
		{
			if (state == null)
			{
				return new object[0];
			}

			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			return (object[])actionsProperty.GetValue(state, null);
		}

		public static object GetActionData(object stateObject)
        {
            if (stateObject == null)
            {
                throw new ArgumentNullException(nameof(stateObject));
            }

            try
            {
                return stateObject.ReflectGetProperty("ActionData");
            }
            catch (Exception)
            {

                throw new Exception("The component is not an FSMEvent");
            }
        }

		public static object GetActionData(FsmStateWrapper stateObject)
        {
			return stateObject.GetActionData();
        }
		
		public static int GetActionIndex(object state, object action)
		{
			if (state == null || action == null)
			{
				return -1;
			}

			object[] actions = GetActions(state);
			for (int i = 0; i < actions.Length; i++)
			{
				if (ReferenceEquals(actions[i], action))
				{
					return i;
				}
			}
			
			return -1;
		}

		public static FsmActionWrapper[] GetActions(FsmStateWrapper state)
		{
			object[] actions = GetActions(state.InternalState);
			return actions.Select(a => new FsmActionWrapper(a)).ToArray();
		}

		public static object AddAction(object state, object action)
		{
			if (state == null || action == null)
			{
				return null;
			}

			// Get the current actions
			object[] currentActions = GetActions(state);
			
			// Create a new actions array with one more element
			Array newActions = Array.CreateInstance(FsmStateActionType, currentActions.Length + 1);
			Array.Copy(currentActions, newActions, currentActions.Length);

			// Initialize the action
			MethodInfo initMethod = FsmStateActionType.GetMethod("Init", new[] { FsmStateType });
			initMethod.Invoke(action, new[] { state });

			// Add the new action to the array
			newActions.SetValue(action, currentActions.Length);

			// Update the state's Actions property
			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			actionsProperty.SetValue(state, newActions, null);

			// Save the actions to the state's action data
			MethodInfo saveActionsMethod = FsmStateType.GetMethod("SaveActions");
			saveActionsMethod.Invoke(state, null);

			return action;
		}
		
		public static object AddActionAtIndex(object state, object action, int index)
		{
			if (state == null || action == null)
			{
				return null;
			}

			object[] currentActions = GetActions(state);

			// Validate index
			if (index < 0 || index > currentActions.Length)
			{
				return AddAction(state, action);
			}

			Array newActions = Array.CreateInstance(FsmStateActionType, currentActions.Length + 1);

			Array.Copy(currentActions, 0, newActions, 0, index);

			MethodInfo initMethod = FsmStateActionType.GetMethod("Init");
			initMethod.Invoke(action, new[] { state });

			newActions.SetValue(action, index);

			Array.Copy(currentActions, index, newActions, index + 1, currentActions.Length - index);

			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			actionsProperty.SetValue(state, newActions, null);

			MethodInfo saveActionsMethod = FsmStateType.GetMethod("SaveActions");
			saveActionsMethod.Invoke(state, null);
			return action;
		}


		public static FsmActionWrapper AddAction(FsmStateWrapper state, FsmActionWrapper action)
		{
			return new FsmActionWrapper(AddAction(state.InternalState, action.InternalAction));
		}
		
		public static FsmActionWrapper AddActionAtIndex(FsmStateWrapper state, FsmActionWrapper action, int index)
		{
			return new FsmActionWrapper(AddActionAtIndex(state.InternalState, action.InternalAction, index));
		}
		
		public static int GetActionIndex(FsmStateWrapper state, FsmActionWrapper action)
		{
			return GetActionIndex(state.InternalState, action.InternalAction);
		}

		public static void RemoveAllActions(object state)
		{
			// Create a new actions array without the action to remove
			Array newActions = Array.CreateInstance(FsmStateActionType, 0);

			// Update the state's Actions property
			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			actionsProperty.SetValue(state, newActions, null);

			// Save the actions to the state's action data
			MethodInfo saveActionsMethod = FsmStateType.GetMethod("SaveActions");
			saveActionsMethod.Invoke(state, null);
		}

		public static bool RemoveAction(object state, int actionIndex)
		{
			if (state == null)
			{
				return false;
			}

			// Get the current actions
			object[] currentActions = GetActions(state);

			if (actionIndex < 0)
			{
				actionIndex = currentActions.Length - 1;
			}

			if (actionIndex >= currentActions.Length)
			{
				return false;
			}

			// Create a new actions array without the action to remove
			Array newActions = Array.CreateInstance(FsmStateActionType, currentActions.Length - 1);

			// Copy all actions except the one to remove
			for (int i = 0, j = 0; i < currentActions.Length; i++)
			{
				if (i != actionIndex)
				{
					newActions.SetValue(currentActions[i], j++);
				}
			}

			// Update the state's Actions property
			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			actionsProperty.SetValue(state, newActions, null);

			// Save the actions to the state's action data
			MethodInfo saveActionsMethod = FsmStateType.GetMethod("SaveActions");
			saveActionsMethod.Invoke(state, null);

			return true;
		}

		public static bool RemoveAction(FsmStateWrapper state, int actionIndex)
		{
			return RemoveAction(state.InternalState, actionIndex);
		}

		public static object GetActionProperty(object action, string propertyName)
		{
			if (action == null || string.IsNullOrEmpty(propertyName))
			{
				return null;
			}

			PropertyInfo property = action.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (property == null)
			{
				FieldInfo field = action.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
				if (field == null)
				{
					return null;
				}
				return field.GetValue(action);
			}
			
			return property.GetValue(action, null);
		}

		public static object GetActionProperty(FsmActionWrapper action, string propertyName)
		{
			return GetActionProperty(action.InternalAction, propertyName);
		}

		public static bool SetActionProperty(object action, string propertyName, object value)
		{
			if (action == null || string.IsNullOrEmpty(propertyName))
			{
				return false;
			}

			PropertyInfo property = action.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (property == null)
			{
				FieldInfo field = action.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
				if (field == null)
				{
					return false;
				}
				field.SetValue(action, value);
				return true;
			}
			
			property.SetValue(action, value, null);
			return true;
		}

		public static bool SetActionProperty(FsmActionWrapper action, string propertyName, object value)
		{
			return SetActionProperty(action.InternalAction, propertyName, value);
		}

		/// <summary>
		/// Gets whether the specified FSM action is enabled
		/// </summary>
		/// <param name="action">The FSM action to check</param>
		/// <returns>True if the action is enabled, false otherwise</returns>
		public static bool GetActionEnabled(object action)
		{
			if (action == null)
			{
				return false;
			}

			// Try to get the "Enabled" property from the action
			PropertyInfo enabledProperty = action.GetType().GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
			if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
			{
				return (bool)enabledProperty.GetValue(action, null);
			}

			// If no Enabled property found, assume the action is enabled by default
			return true;
		}

		/// <summary>
		/// Sets whether the specified FSM action is enabled
		/// </summary>
		/// <param name="action">The FSM action to modify</param>
		/// <param name="enabled">Whether the action should be enabled</param>
		/// <returns>True if the property was successfully set, false otherwise</returns>
		public static bool SetActionEnabled(object action, bool enabled)
		{
			if (action == null)
			{
				return false;
			}

			// Try to set the "Enabled" property on the action
			PropertyInfo enabledProperty = action.GetType().GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
			if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool) && enabledProperty.CanWrite)
			{
				enabledProperty.SetValue(action, enabled, null);
				return true;
			}

			// If no writable Enabled property found, return false
			return false;
		}

		/// <summary>
		/// Gets whether the specified FSM action wrapper is enabled
		/// </summary>
		/// <param name="action">The FSM action wrapper to check</param>
		/// <returns>True if the action is enabled, false otherwise</returns>
		public static bool GetActionEnabled(FsmActionWrapper action)
		{
			return GetActionEnabled(action.InternalAction);
		}

		/// <summary>
		/// Sets whether the specified FSM action wrapper is enabled
		/// </summary>
		/// <param name="action">The FSM action wrapper to modify</param>
		/// <param name="enabled">Whether the action should be enabled</param>
		/// <returns>True if the property was successfully set, false otherwise</returns>
		public static bool SetActionEnabled(FsmActionWrapper action, bool enabled)
		{
			return SetActionEnabled(action.InternalAction, enabled);
		}

		#endregion

		#region Transition Management

		public static object[] GetTransitions(object state)
		{
			if (state == null)
			{
				return new object[0];
			}

			PropertyInfo transitionsProperty = FsmStateType.GetProperty("Transitions");
			return (object[])transitionsProperty.GetValue(state, null);
		}

		public static FsmTransitionWrapper[] GetTransitions(FsmStateWrapper state)
		{
			object[] transitions = GetTransitions(state.InternalState);
			return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
		}

		public static object[] GetGlobalTransitions(object fsm)
		{
			if (fsm == null)
			{
				return new object[0];
			}

			return (object[])GlobalTransitionsProperty.GetValue(fsm, null);
		}

		public static FsmTransitionWrapper[] GetGlobalTransitions(FsmWrapper fsm)
		{
			object[] transitions = GetGlobalTransitions(fsm.InternalFsm);
			return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
		}

		public static object AddTransition(object state, string eventName, string toState, int index = -1)
		{
			if (state == null || string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(toState))
				return null;

			var current = GetTransitions(state) ?? Array.Empty<object>();
			var newArray = Array.CreateInstance(FsmTransitionType, current.Length + 1);

			if (index < 0 || index > current.Length)
				index = current.Length;

			Array.Copy(current, 0, newArray, 0, index);
			Array.Copy(current, index, newArray, index + 1, current.Length - index);

			object t = Activator.CreateInstance(FsmTransitionType);
			var fsm = new FsmStateWrapper(state).GetFsm();

			t.ReflectSetProperty("FsmEvent", GetFsmEvent(eventName));
			t.ReflectSetProperty("ToState", toState);
			t.ReflectSetProperty("ToFsmState", GetState(fsm.InternalFsm, toState));

			newArray.SetValue(t, index);
			state.ReflectSetProperty("Transitions", newArray);

			return t;
		}


		public static FsmTransitionWrapper AddTransition(FsmStateWrapper state, string eventName, string toState, int index = -1)
		{
			object transition = AddTransition(state.InternalState, eventName, toState, index);
			return transition != null ? new FsmTransitionWrapper(transition) : default;
		}

		public static object AddGlobalTransition(object fsm, string eventName, string toState)
		{
			if (fsm == null || string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(toState))
			{
				return null;
			}

			// Get the current global transitions
			object[] currentTransitions = GetGlobalTransitions(fsm);

			// Create a new transitions array with one more element
			Array newTransitions = Array.CreateInstance(FsmTransitionType, currentTransitions.Length + 1);
			Array.Copy(currentTransitions, newTransitions, currentTransitions.Length);

			// Create a new transition
			object newTransition = Activator.CreateInstance(FsmTransitionType);

			//var stateWrapper = new FsmStateWrapper(state);

			//var fsm = stateWrapper.GetFsm();

			newTransition.ReflectSetProperty("FsmEvent", GetFsmEvent(eventName));
			newTransition.ReflectSetProperty("ToState", toState);
			newTransition.ReflectSetProperty("ToFsmState", GetState(fsm, toState));

			return newTransition;

			// Set the transition properties
			/*PropertyInfo eventNameProperty = FsmTransitionType.GetProperty("EventName");
			eventNameProperty.SetValue(newTransition, eventName, null);

			PropertyInfo toStateProperty = FsmTransitionType.GetProperty("ToState");
			toStateProperty.SetValue(newTransition, toState, null);

			// Get the FsmEvent for this event name
			object fsmEvent = GetFsmEventMethod.Invoke(null, new object[] { eventName });
			
			PropertyInfo fsmEventProperty = FsmTransitionType.GetProperty("FsmEvent");
			fsmEventProperty.SetValue(newTransition, fsmEvent, null);

			// Get the target state object
			object targetState = GetState(fsm, toState);
			
			PropertyInfo toFsmStateProperty = FsmTransitionType.GetProperty("ToFsmState");
			toFsmStateProperty.SetValue(newTransition, targetState, null);

			// Add the new transition to the array
			newTransitions.SetValue(newTransition, currentTransitions.Length);

			// Update the FSM's GlobalTransitions property
			GlobalTransitionsProperty.SetValue(fsm, newTransitions, null);

			return newTransition;*/
		}

		public static FsmTransitionWrapper AddGlobalTransition(FsmWrapper fsm, string eventName, string toState)
		{
			object transition = AddGlobalTransition(fsm.InternalFsm, eventName, toState);
			return transition != null ? new FsmTransitionWrapper(transition) : default;
		}

		public static bool RemoveTransition(object state, int transitionIndex)
		{
			if (state == null || transitionIndex < 0)
			{
				return false;
			}

			// Get the current transitions
			object[] currentTransitions = GetTransitions(state);

			if (transitionIndex >= currentTransitions.Length || transitionIndex < 0)
			{
				transitionIndex = currentTransitions.Length - 1;
				//return false;
			}

			// Create a new transitions array without the transition to remove
			Array newTransitions = Array.CreateInstance(FsmTransitionType, currentTransitions.Length - 1);
			
			// Copy all transitions except the one to remove
			for (int i = 0, j = 0; i < currentTransitions.Length; i++)
			{
				if (i != transitionIndex)
				{
					newTransitions.SetValue(currentTransitions[i], j++);
				}
			}

			// Update the state's Transitions property
			PropertyInfo transitionsProperty = FsmStateType.GetProperty("Transitions");
			transitionsProperty.SetValue(state, newTransitions, null);

			return true;
		}

		public static bool RemoveTransition(FsmStateWrapper state, int transitionIndex)
		{
			return RemoveTransition(state.InternalState, transitionIndex);
		}

		public static bool RemoveGlobalTransition(object fsm, int transitionIndex)
		{
			if (fsm == null || transitionIndex < 0)
			{
				return false;
			}

			// Get the current global transitions
			object[] currentTransitions = GetGlobalTransitions(fsm);

			/*if (transitionIndex >= currentTransitions.Length)
			{
				return false;
			}*/
			
			if (transitionIndex < 0 || transitionIndex >= currentTransitions.Length)
			{
				transitionIndex = currentTransitions.Length - 1;
			}

			// Create a new transitions array without the transition to remove
			Array newTransitions = Array.CreateInstance(FsmTransitionType, currentTransitions.Length - 1);
			
			// Copy all transitions except the one to remove
			for (int i = 0, j = 0; i < currentTransitions.Length; i++)
			{
				if (i != transitionIndex)
				{
					newTransitions.SetValue(currentTransitions[i], j++);
				}
			}

			// Update the FSM's GlobalTransitions property
			GlobalTransitionsProperty.SetValue(fsm, newTransitions, null);

			return true;
		}

		public static bool RemoveGlobalTransition(FsmWrapper fsm, int transitionIndex)
		{
			return RemoveGlobalTransition(fsm.InternalFsm, transitionIndex);
		}

		private static void RemoveTransitionsToState(object fsm, string stateName)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return;
			}

			// Remove from global transitions
			object[] globalTransitions = GetGlobalTransitions(fsm);
			List<object> newGlobalTransitions = new List<object>();
			
			foreach (object transition in globalTransitions)
			{
				PropertyInfo toStateProperty = FsmTransitionType.GetProperty("ToState");
				string toState = (string)toStateProperty.GetValue(transition, null);
				
				if (toState != stateName)
				{
					newGlobalTransitions.Add(transition);
				}
			}
			
			// Convert back to an array of the correct type
			Array newGlobalTransArray = Array.CreateInstance(FsmTransitionType, newGlobalTransitions.Count);
			newGlobalTransitions.CopyTo((object[])newGlobalTransArray);
			
			// Update the FSM's GlobalTransitions property
			GlobalTransitionsProperty.SetValue(fsm, newGlobalTransArray, null);
			
			// Remove from each state's transitions
			object[] states = GetStates(fsm);
			foreach (object state in states)
			{
				object[] stateTransitions = GetTransitions(state);
				List<object> newStateTransitions = new List<object>();
				
				foreach (object transition in stateTransitions)
				{
					PropertyInfo toStateProperty = FsmTransitionType.GetProperty("ToState");
					string toState = (string)toStateProperty.GetValue(transition, null);
					
					if (toState != stateName)
					{
						newStateTransitions.Add(transition);
					}
				}
				
				// Convert back to an array of the correct type
				Array newStateTransArray = Array.CreateInstance(FsmTransitionType, newStateTransitions.Count);
				newStateTransitions.CopyTo((object[])newStateTransArray);
				
				// Update the state's Transitions property
				PropertyInfo transitionsProperty = FsmStateType.GetProperty("Transitions");
				transitionsProperty.SetValue(state, newStateTransArray, null);
			}
		}

		private static void RemoveTransitionsToState(FsmWrapper fsm, string stateName)
		{
			RemoveTransitionsToState(fsm.InternalFsm, stateName);
		}

		#endregion

		#region Event Management

		public static object[] GetEvents(object fsm)
		{
			if (fsm == null)
			{
				return new object[0];
			}

			return (object[])EventsProperty.GetValue(fsm, null);
		}

		public static FsmEventWrapper[] GetEvents(FsmWrapper fsm)
		{
			object[] events = GetEvents(fsm.InternalFsm);
			return events.Select(e => new FsmEventWrapper(e)).ToArray();
		}

		public static object GetFsmEvent(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return null;
			}

			return GetFsmEventMethod.Invoke(null, new object[] { eventName });
		}

		public static FsmEventWrapper GetFsmEventWrapper(string eventName)
		{
			object fsmEvent = GetFsmEvent(eventName);
			return fsmEvent != null ? new FsmEventWrapper(fsmEvent) : default;
		}

		public static object AddEvent(object fsm, string eventName)
		{
			if (fsm == null || string.IsNullOrEmpty(eventName))
			{
				return null;
			}

			// Get the current events
			object[] currentEvents = GetEvents(fsm);
			
			// Check if the event already exists
			foreach (object evt in currentEvents)
			{
				PropertyInfo nameProperty = FsmEventType.GetProperty("Name");
				string name = (string)nameProperty.GetValue(evt, null);
				
				if (name == eventName)
				{
					return evt;
				}
			}
			
			// Get the FsmEvent for this event name (will create if it doesn't exist)
			object newEvent = GetFsmEvent(eventName);
			
			// Create a new events array with one more element
			Array newEvents = Array.CreateInstance(FsmEventType, currentEvents.Length + 1);
			Array.Copy(currentEvents, newEvents, currentEvents.Length);
			
			// Add the new event to the array
			newEvents.SetValue(newEvent, currentEvents.Length);
			
			// Update the FSM's Events property
			EventsProperty.SetValue(fsm, newEvents, null);
			
			return newEvent;
		}

		public static FsmEventWrapper AddEvent(FsmWrapper fsm, string eventName)
		{
			object fsmEvent = AddEvent(fsm.InternalFsm, eventName);
			return fsmEvent != null ? new FsmEventWrapper(fsmEvent) : default;
		}

		public static void SendEvent(object fsm, string eventName)
		{
			if (fsm == null || string.IsNullOrEmpty(eventName))
			{
				return;
			}

			MethodInfo eventMethod = FsmType.GetMethod("Event", new[] { typeof(string) });
			eventMethod.Invoke(fsm, new object[] { eventName });
		}

		public static void SendEvent(FsmWrapper fsm, string eventName)
		{
			SendEvent(fsm.InternalFsm, eventName);
		}

		public static void SendEvent(Component playMakerFSM, string eventName)
		{
			if (playMakerFSM == null || string.IsNullOrEmpty(eventName))
			{
				return;
			}

			MethodInfo eventMethod = PlayMakerFSMType.GetMethod("SendEvent", new[] { typeof(string) });
			eventMethod.Invoke(playMakerFSM, new object[] { eventName });
		}

		public static void SendEvent(PlayMakerFsmWrapper playMakerFSM, string eventName)
		{
			SendEvent(playMakerFSM.InternalComponent, eventName);
		}

		#endregion

		#region Variable Management

		public static object GetVariables(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			PropertyInfo variablesProperty = FsmType.GetProperty("Variables");
			return variablesProperty.GetValue(fsm, null);
		}

		public static FsmVariablesWrapper GetVariables(FsmWrapper fsm)
		{
			object variables = GetVariables(fsm.InternalFsm);
			return variables != null ? new FsmVariablesWrapper(variables) : default;
		}


		public static object[] GetArrayVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmArray", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (object[])fsmGo.ReflectGetProperty("Values");
			}

			return default;
		}

		public static object[] GetArrayVariable(FsmWrapper fsm, string variableName)
		{
			return GetArrayVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetArrayVariable(object fsm, string variableName, object[] value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmArray", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Values", value) ?? false;
		}

		public static void SetBoolVariable(FsmWrapper fsm, string variableName, object[] value)
		{
			SetArrayVariable(fsm.InternalFsm, variableName, value);
		}

		public static bool GetBoolVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmBool", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (bool)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static bool GetBoolVariable(FsmWrapper fsm, string variableName)
		{
			return GetBoolVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetBoolVariable(object fsm, string variableName, bool value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmBool", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetBoolVariable(FsmWrapper fsm, string variableName, bool value)
		{
			SetBoolVariable(fsm.InternalFsm, variableName, value);
		}

		public static Color GetColorVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmColor", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Color)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Color GetColorVariable(FsmWrapper fsm, string variableName)
		{
			return GetColorVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetColorVariable(object fsm, string variableName, Color value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmColor", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetColorVariable(FsmWrapper fsm, string variableName, Color value)
		{
			SetColorVariable(fsm.InternalFsm, variableName, value);
		}

		public static Enum GetEnumVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmEnum", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Enum)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Enum GetEnumVariable(FsmWrapper fsm, string variableName)
		{
			return GetEnumVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetEnumVariable(object fsm, string variableName, Enum value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmEnum", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetEnumVariable(FsmWrapper fsm, string variableName, Enum value)
		{
			SetEnumVariable(fsm.InternalFsm, variableName, value);
		}


		public static UnityEngine.Object GetObjectVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmObject", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (UnityEngine.Object)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static UnityEngine.Object GetObjectVariable(FsmWrapper fsm, string variableName)
		{
			return GetObjectVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetObjectVariable(object fsm, string variableName, UnityEngine.Object value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmObject", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetObjectVariable(FsmWrapper fsm, string variableName, UnityEngine.Object value)
		{
			SetObjectVariable(fsm.InternalFsm, variableName, value);
		}

		public static int GetIntVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmInt", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (int)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static int GetIntVariable(FsmWrapper fsm, string variableName)
		{
			return GetIntVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetIntVariable(object fsm, string variableName, int value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmInt", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetIntVariable(FsmWrapper fsm, string variableName, int value)
		{
			SetIntVariable(fsm.InternalFsm, variableName, value);
		}

		public static float GetFloatVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmFloat", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (float)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static float GetFloatVariable(FsmWrapper fsm, string variableName)
		{
			return GetFloatVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetFloatVariable(object fsm, string variableName, float value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmFloat", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetFloatVariable(FsmWrapper fsm, string variableName, float value)
		{
			SetFloatVariable(fsm.InternalFsm, variableName, value);
		}

		public static string GetStringVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmString", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (string)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static string GetStringVariable(FsmWrapper fsm, string variableName)
		{
			return GetStringVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetStringVariable(object fsm, string variableName, string value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmString", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetStringVariable(FsmWrapper fsm, string variableName, string value)
		{
			SetStringVariable(fsm.InternalFsm, variableName, value);
		}

		public static Vector3 GetVector3Variable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmVector3", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Vector3)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Vector3 GetVector3Variable(FsmWrapper fsm, string variableName)
		{
			return GetVector3Variable(fsm.InternalFsm, variableName);
		}

		public static bool SetVector3Variable(object fsm, string variableName, Vector3 value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmVector3", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetVector3Variable(FsmWrapper fsm, string variableName, Vector3 value)
		{
			SetVector3Variable(fsm.InternalFsm, variableName, value);
		}

		public static GameObject GetGameObjectVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmGameObject", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (GameObject)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static GameObject GetGameObjectVariable(FsmWrapper fsm, string variableName)
		{
			return GetGameObjectVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetGameObjectVariable(object fsm, string variableName, GameObject value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmGameObject", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetGameObjectVariable(FsmWrapper fsm, string variableName, GameObject value)
		{
			SetGameObjectVariable(fsm.InternalFsm, variableName, value);
		}


		public static Quaternion GetQuaternionVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmQuaternion", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Quaternion)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Quaternion GetQuaternionVariable(FsmWrapper fsm, string variableName)
		{
			return GetQuaternionVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetQuaternionVariable(object fsm, string variableName, Quaternion value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmQuaternion", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetQuaternionVariable(FsmWrapper fsm, string variableName, Quaternion value)
		{
			SetQuaternionVariable(fsm.InternalFsm, variableName, value);
		}

		public static Rect GetRectVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmRect", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Rect)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Rect GetRectVariable(FsmWrapper fsm, string variableName)
		{
			return GetRectVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetRectVariable(object fsm, string variableName, Rect value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmRect", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetRectVariable(FsmWrapper fsm, string variableName, Rect value)
		{
			SetRectVariable(fsm.InternalFsm, variableName, value);
		}

		public static Vector2 GetVector2Variable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmVector2", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Vector2)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Vector2 GetVector2Variable(FsmWrapper fsm, string variableName)
		{
			return GetVector2Variable(fsm.InternalFsm, variableName);
		}

		public static bool SetVector2Variable(object fsm, string variableName, Vector2 value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmVector2", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetVector2Variable(FsmWrapper fsm, string variableName, Vector2 value)
		{
			SetVector2Variable(fsm.InternalFsm, variableName, value);
		}

		public static Material GetMaterialVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmMaterial", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Material)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Material GetMaterialVariable(FsmWrapper fsm, string variableName)
		{
			return GetMaterialVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetMaterialVariable(object fsm, string variableName, Material value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmMaterial", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetMaterialVariable(FsmWrapper fsm, string variableName, Material value)
		{
			SetMaterialVariable(fsm.InternalFsm, variableName, value);
		}

		public static Texture GetTextureVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			var fsmGo = fsm.ReflectCallMethod("GetFsmTexture", CacheUtilities.GetTempSingleArray(variableName));

			if (fsmGo != null)
			{
				return (Texture)fsmGo.ReflectGetProperty("Value");
			}

			return default;
		}

		public static Texture GetTextureVariable(FsmWrapper fsm, string variableName)
		{
			return GetTextureVariable(fsm.InternalFsm, variableName);
		}

		public static bool SetTextureVariable(object fsm, string variableName, Texture value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			return fsm.ReflectCallMethod("GetFsmTexture", CacheUtilities.GetTempSingleArray(variableName))?.ReflectSetProperty("Value", value) ?? false;
		}

		public static void SetTextureVariable(FsmWrapper fsm, string variableName, Texture value)
		{
			SetTextureVariable(fsm.InternalFsm, variableName, value);
		}



		public static UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetObjectVariable(varName);
		}

		public static bool SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetObjectVariable(varName, value);
			return true;
		}

		public static Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetMaterialVariable(varName);
		}

		public static bool SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetMaterialVariable(varName, value);
			return true;
		}

		public static Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetTextureVariable(varName);
		}

		public static bool SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetTextureVariable(varName, value);
			return true;
		}

		public static float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetFloatVariable(varName);
		}

		public static bool SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetFloatVariable(varName, value);
			return true;
		}

		public static int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetIntVariable(varName);
		}

		public static bool SetFsmInt(GameObject obj, string fsmName, string varName, int value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetIntVariable(varName, value);
			return true;
		}

		public static bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetBoolVariable(varName);
		}

		public static bool SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetBoolVariable(varName, value);
			return true;
		}

		public static string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetStringVariable(varName);
		}

		public static bool SetFsmString(GameObject obj, string fsmName, string varName, string value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetStringVariable(varName, value);
			return true;
		}

		public static Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetVector2Variable(varName);
		}

		public static bool SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetVector2Variable(varName, value);
			return true;
		}

		public static Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetVector3Variable(varName);
		}

		public static bool SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetVector3Variable(varName, value);
			return true;
		}

		public static Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetRectVariable(varName);
		}

		public static bool SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetRectVariable(varName, value);
			return true;
		}

		public static Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetQuaternionVariable(varName);
		}

		public static bool SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetQuaternionVariable(varName, value);
			return true;
		}

		public static Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetColorVariable(varName);
		}

		public static bool SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetColorVariable(varName, value);
			return true;
		}

		public static GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetGameObjectVariable(varName);
		}

		public static bool SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetGameObjectVariable(varName, value);
			return true;
		}

		public static object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetArrayVariable(varName);
		}

		public static bool SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetArrayVariable(varName, value);
			return true;
		}

		public static Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetEnumVariable(varName);
		}

		public static bool SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return false;
			}

			psFSM.GetFsm().SetEnumVariable(varName, value);
			return true;
		}

		public static object CreateFSMActionFromWeaverAction(WeaverFSMAction action)
		{
			return PlayMakerUtilities.impl.CreateFSMActionFromWeaverAction(action);
		}

		public static FsmActionWrapper CreateFSMActionWrapperFromWeaverAction(WeaverFSMAction action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.impl.CreateFSMActionFromWeaverAction(action));
		}


		#endregion

		#region Utility Methods

		private static object GetStateProperty(object obj, string propertyName)
		{
			if (obj == null || string.IsNullOrEmpty(propertyName))
			{
				return null;
			}

			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			if (property == null)
			{
				return null;
			}

			return property.GetValue(obj, null);
		}

		public static Component[] FindAllFSMsInScene(string fsmName = null)
		{
			Component[] allFsms = UnityEngine.Object.FindObjectsOfType(PlayMakerFSMType).OfType<Component>().ToArray();
			
			if (string.IsNullOrEmpty(fsmName))
			{
				return allFsms;
			}
			
			List<Component> matchingFsms = new List<Component>();
			foreach (Component fsm in allFsms)
			{
				string currentName = (string)FsmNameProperty.GetValue(fsm, null);
				if (currentName == fsmName)
				{
					matchingFsms.Add(fsm);
				}
			}
			
			return matchingFsms.ToArray();
		}

		public static PlayMakerFsmWrapper[] FindAllFSMWrappersInScene(string fsmName = null)
		{
			Component[] components = FindAllFSMsInScene(fsmName);
			return components.Select(c => new PlayMakerFsmWrapper(c)).ToArray();
		}

		#endregion
		
		#region FSM Snapshots
		
		/*public static FsmSnapshot CreateSnapshot(Component playMakerFSM)
		{        
			if (playMakerFSM == null) return null;
			
			object fsm = GetFsm(playMakerFSM);
			return FsmSnapshot.Create(fsm, playMakerFSM);
		}
		
		public static FsmSnapshot CreateSnapshot(PlayMakerFsmWrapper playMakerFSM)
		{        
			return CreateSnapshot(playMakerFSM.InternalComponent);
		}
		
		public static void RestoreSnapshot(Component playMakerFSM, FsmSnapshot snapshot)
		{        
			if (playMakerFSM == null || snapshot == null) return;
			
			object fsm = GetFsm(playMakerFSM);
			snapshot.Restore(fsm, playMakerFSM);
		}
		
		public static void RestoreSnapshot(PlayMakerFsmWrapper playMakerFSM, FsmSnapshot snapshot)
		{        
			RestoreSnapshot(playMakerFSM.InternalComponent, snapshot);
		}*/
		#endregion
	}
}
