using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Implementations;
using WeaverCore.Playmaker;
using WeaverCore.Playmaker.Snapshots;
using WeaverCore.Utilities;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// A utility class that provides comprehensive access to PlayMakerFSM functionality through reflection.
	/// </summary>
	public static class PlayMakerUtilities
	{
		private static PlayMaker_I impl = ImplFinder.GetImplementation<PlayMaker_I>();
		public static bool IsAvailable => Initialization.Environment == WeaverCore.Enums.RunningState.Game;

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

		static PlayMakerUtilities()
		{
			if (IsAvailable)
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
			StatesProperty = FsmType?.GetProperty("States");
			EventsProperty = FsmType?.GetProperty("Events");
			GlobalTransitionsProperty = FsmType?.GetProperty("GlobalTransitions");
			ActiveStateProperty = FsmType?.GetProperty("ActiveState");
			ActiveStateNameProperty = FsmType?.GetProperty("ActiveStateName");
			FsmNameProperty = PlayMakerFSMType?.GetProperty("FsmName");
		}

		#endregion

		#region FSM Finding and Access

		/// <summary>
		/// Locates a PlayMakerFSM on a GameObject with the specified name.
		/// </summary>
		/// <param name="gameObject">The GameObject to search on.</param>
		/// <param name="fsmName">The name of the FSM to find.</param>
		/// <returns>The PlayMakerFSM component if found, null otherwise.</returns>
		public static Component FindPlayMakerFSM(GameObject gameObject, string fsmName)
		{
			if (gameObject == null)
			{
				return null;
			}

			Component[] fsms = gameObject.GetComponents(PlayMakerFSMType);
			foreach (Component fsm in fsms)
			{
				string currentName = (string)FsmNameProperty.GetValue(fsm, null);
				if (currentName == fsmName)
				{
					return fsm;
				}
			}

			return null;
		}

		/// <summary>
		/// Locates a PlayMakerFSM on a GameObject with the specified name, returning a type-safe wrapper.
		/// </summary>
		/// <param name="gameObject">The GameObject to search on.</param>
		/// <param name="fsmName">The name of the FSM to find.</param>
		/// <returns>A type-safe wrapper for the PlayMakerFSM component if found, null otherwise.</returns>
		public static PlayMakerFsmWrapper FindPlayMakerFSMWrapper(GameObject gameObject, string fsmName)
		{
			Component component = FindPlayMakerFSM(gameObject, fsmName);
			return component != null ? new PlayMakerFsmWrapper(component) : default;
		}

		/// <summary>
		/// Checks if a GameObject has a PlayMakerFSM with the specified name.
		/// </summary>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <param name="fsmName">The name of the FSM to look for.</param>
		/// <returns>True if the FSM exists, false otherwise.</returns>
		public static bool ContainsPlayMakerFSM(GameObject gameObject, string fsmName)
		{
			return FindPlayMakerFSM(gameObject, fsmName) != null;
		}

		/// <summary>
		/// Gets all PlayMakerFSMs on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to search on.</param>
		/// <returns>An array of PlayMakerFSM components.</returns>
		public static Component[] GetAllPlayMakerFSMs(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return new Component[0];
			}

			return gameObject.GetComponents(PlayMakerFSMType);
		}

		/// <summary>
		/// Gets all PlayMakerFSMs on a GameObject, returning type-safe wrappers.
		/// </summary>
		/// <param name="gameObject">The GameObject to search on.</param>
		/// <returns>An array of type-safe wrappers for PlayMakerFSM components.</returns>
		public static PlayMakerFsmWrapper[] GetAllPlayMakerFSMWrappers(GameObject gameObject)
		{
			Component[] components = GetAllPlayMakerFSMs(gameObject);
			return components.Select(c => new PlayMakerFsmWrapper(c)).ToArray();
		}

		/// <summary>
		/// Gets the underlying Fsm object from a PlayMakerFSM component.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component.</param>
		/// <returns>The underlying Fsm object.</returns>
		public static object GetFsm(Component playMakerFSM)
		{
			if (playMakerFSM == null)
			{
				return null;
			}

			PropertyInfo fsmProperty = PlayMakerFSMType.GetProperty("Fsm");
			return fsmProperty.GetValue(playMakerFSM, null);
		}

		/// <summary>
		/// Gets the underlying Fsm object from a PlayMakerFSM component, returning a type-safe wrapper.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component.</param>
		/// <returns>A type-safe wrapper for the underlying Fsm object.</returns>
		public static FsmWrapper GetFsmWrapper(Component playMakerFSM)
		{
			object fsm = GetFsm(playMakerFSM);
			return fsm != null ? new FsmWrapper(fsm) : default;
		}

		/// <summary>
		/// Gets the underlying Fsm object from a PlayMakerFSM wrapper.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper.</param>
		/// <returns>A type-safe wrapper for the underlying Fsm object.</returns>
		public static FsmWrapper GetFsmWrapper(PlayMakerFsmWrapper playMakerFSM)
		{
			object fsm = GetFsm(playMakerFSM.InternalComponent);
			return fsm != null ? new FsmWrapper(fsm) : default;
		}

		/// <summary>
		/// Gets the name of a PlayMakerFSM.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component.</param>
		/// <returns>The name of the FSM.</returns>
		public static string GetFsmName(Component playMakerFSM)
		{
			if (playMakerFSM == null)
			{
				return null;
			}

			return (string)FsmNameProperty.GetValue(playMakerFSM, null);
		}

		/// <summary>
		/// Gets the name of a PlayMakerFSM.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper.</param>
		/// <returns>The name of the FSM.</returns>
		public static string GetFsmName(PlayMakerFsmWrapper playMakerFSM)
		{
			return GetFsmName(playMakerFSM.InternalComponent);
		}

		/// <summary>
		/// Sets the name of a PlayMakerFSM.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component.</param>
		/// <param name="newName">The new name for the FSM.</param>
		public static void SetFsmName(Component playMakerFSM, string newName)
		{
			if (playMakerFSM == null)
			{
				return;
			}

			FsmNameProperty.SetValue(playMakerFSM, newName, null);
		}

		/// <summary>
		/// Sets the name of a PlayMakerFSM.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper.</param>
		/// <param name="newName">The new name for the FSM.</param>
		public static void SetFsmName(PlayMakerFsmWrapper playMakerFSM, string newName)
		{
			SetFsmName(playMakerFSM.InternalComponent, newName);
		}

		#endregion

		#region State Management

		/// <summary>
		/// Gets all states in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>An array of FsmState objects.</returns>
		public static object[] GetStates(object fsm)
		{
			if (fsm == null)
			{
				return new object[0];
			}

			return (object[])StatesProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets all states in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>An array of FsmState wrapper objects.</returns>
		public static FsmStateWrapper[] GetStates(FsmWrapper fsm)
		{
			object[] states = GetStates(fsm.InternalFsm);
			return states.Select(s => new FsmStateWrapper(s)).ToArray();
		}

		/// <summary>
		/// Gets a state by name from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="stateName">The name of the state to find.</param>
		/// <returns>The FsmState object if found, null otherwise.</returns>
		public static object GetState(object fsm, string stateName)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return null;
			}

			MethodInfo getStateMethod = FsmType.GetMethod("GetState", new[] { typeof(string) });
			return getStateMethod.Invoke(fsm, new object[] { stateName });
		}

		/// <summary>
		/// Gets a state by name from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="stateName">The name of the state to find.</param>
		/// <returns>The FsmState wrapper if found, default otherwise.</returns>
		public static FsmStateWrapper GetState(FsmWrapper fsm, string stateName)
		{
			object state = GetState(fsm.InternalFsm, stateName);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		/// <summary>
		/// Adds a new state to an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="stateName">The name for the new state.</param>
		/// <param name="position">The position for the new state in the FSM editor.</param>
		/// <returns>The newly created FsmState object.</returns>
		public static object AddState(object fsm, string stateName, Vector2 position)
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
			ConstructorInfo stateConstructor = FsmStateType.GetConstructor(new[] { FsmType });
			object newState = stateConstructor.Invoke(new[] { fsm });

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

		/// <summary>
		/// Adds a new state to an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="stateName">The name for the new state.</param>
		/// <param name="position">The position for the new state in the FSM editor.</param>
		/// <returns>The newly created FsmState wrapper.</returns>
		public static FsmStateWrapper AddState(FsmWrapper fsm, string stateName, Vector2 position)
		{
			object state = AddState(fsm.InternalFsm, stateName, position);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		/// <summary>
		/// Removes a state from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="stateName">The name of the state to remove.</param>
		/// <returns>True if the state was removed, false otherwise.</returns>
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

		/// <summary>
		/// Removes a state from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="stateName">The name of the state to remove.</param>
		/// <returns>True if the state was removed, false otherwise.</returns>
		public static bool RemoveState(FsmWrapper fsm, string stateName)
		{
			return RemoveState(fsm.InternalFsm, stateName);
		}

		/// <summary>
		/// Gets the active state of an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>The active FsmState object.</returns>
		public static object GetActiveState(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			return ActiveStateProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets the active state of an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>The active FsmState wrapper.</returns>
		public static FsmStateWrapper GetActiveState(FsmWrapper fsm)
		{
			object state = GetActiveState(fsm.InternalFsm);
			return state != null ? new FsmStateWrapper(state) : default;
		}

		/// <summary>
		/// Gets the name of the active state in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>The name of the active state.</returns>
		public static string GetActiveStateName(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			return (string)ActiveStateNameProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets the name of the active state in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>The name of the active state.</returns>
		public static string GetActiveStateName(FsmWrapper fsm)
		{
			return GetActiveStateName(fsm.InternalFsm);
		}

		/// <summary>
		/// Sets the active state of an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="stateName">The name of the state to set as active.</param>
		public static void SetActiveState(object fsm, string stateName)
		{
			if (fsm == null || string.IsNullOrEmpty(stateName))
			{
				return;
			}

			MethodInfo setStateMethod = FsmType.GetMethod("SetState", new[] { typeof(string) });
			setStateMethod.Invoke(fsm, new object[] { stateName });
		}

		/// <summary>
		/// Sets the active state of an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="stateName">The name of the state to set as active.</param>
		public static void SetActiveState(FsmWrapper fsm, string stateName)
		{
			SetActiveState(fsm.InternalFsm, stateName);
		}

		#endregion

		#region Action Management

		/// <summary>
		/// Gets all actions in a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <returns>An array of FsmStateAction objects.</returns>
		public static object[] GetActions(object state)
		{
			if (state == null)
			{
				return new object[0];
			}

			PropertyInfo actionsProperty = FsmStateType.GetProperty("Actions");
			return (object[])actionsProperty.GetValue(state, null);
		}
		
		/// <summary>
		/// Gets the index of an action in a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="action">The action to find.</param>
		/// <returns>The index of the action, or -1 if not found.</returns>
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

		/// <summary>
		/// Gets all actions in a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <returns>An array of FsmStateAction wrapper objects.</returns>
		public static FsmActionWrapper[] GetActions(FsmStateWrapper state)
		{
			object[] actions = GetActions(state.InternalState);
			return actions.Select(a => new FsmActionWrapper(a)).ToArray();
		}

		/// <summary>
		/// Adds an action to a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="action">The action to add.</param>
		/// <returns>The newly created action.</returns>
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

			// Create a new action
			//ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
			//object newAction = actionConstructor.Invoke(null);

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
		
		/// <summary>
		/// Adds an action to a state at a specific index.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="action">The action to add.</param>
		/// <param name="index">The index at which to add the action.</param>
		/// <returns>The newly created action.</returns>
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


		/// <summary>
		/// Adds an action to a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="action">The action to add.</param>
		/// <returns>The newly created action wrapper.</returns>
		public static FsmActionWrapper AddAction(FsmStateWrapper state, FsmActionWrapper action)
		{
			return new FsmActionWrapper(AddAction(state.InternalState, action.InternalAction));
		}
		
		/// <summary>
		/// Adds an action to a state at a specific index using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="action">The action to add.</param>
		/// <param name="index">The index at which to add the action.</param>
		/// <returns>The newly created action wrapper.</returns>
		public static FsmActionWrapper AddActionAtIndex(FsmStateWrapper state, FsmActionWrapper action, int index)
		{
			return new FsmActionWrapper(AddActionAtIndex(state.InternalState, action.InternalAction, index));
		}
		
		/// <summary>
		/// Gets the index of an action in a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="action">The action wrapper to find.</param>
		/// <returns>The index of the action, or -1 if not found.</returns>
		public static int GetActionIndex(FsmStateWrapper state, FsmActionWrapper action)
		{
			return GetActionIndex(state.InternalState, action.InternalAction);
		}

		/// <summary>
		/// Removes an action from a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="actionIndex">The index of the action to remove.</param>
		/// <returns>True if the action was removed, false otherwise.</returns>
		public static bool RemoveAction(object state, int actionIndex)
		{
			if (state == null || actionIndex < 0)
			{
				return false;
			}

			// Get the current actions
			object[] currentActions = GetActions(state);
			
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

		/// <summary>
		/// Removes an action from a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="actionIndex">The index of the action to remove.</param>
		/// <returns>True if the action was removed, false otherwise.</returns>
		public static bool RemoveAction(FsmStateWrapper state, int actionIndex)
		{
			return RemoveAction(state.InternalState, actionIndex);
		}

		/// <summary>
		/// Gets a property value from an action.
		/// </summary>
		/// <param name="action">The FsmStateAction object.</param>
		/// <param name="propertyName">The name of the property to get.</param>
		/// <returns>The value of the property.</returns>
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

		/// <summary>
		/// Gets a property value from an action using the wrapper type.
		/// </summary>
		/// <param name="action">The FsmStateAction wrapper.</param>
		/// <param name="propertyName">The name of the property to get.</param>
		/// <returns>The value of the property.</returns>
		public static object GetActionProperty(FsmActionWrapper action, string propertyName)
		{
			return GetActionProperty(action.InternalAction, propertyName);
		}

		/// <summary>
		/// Sets a property value on an action.
		/// </summary>
		/// <param name="action">The FsmStateAction object.</param>
		/// <param name="propertyName">The name of the property to set.</param>
		/// <param name="value">The value to set.</param>
		/// <returns>True if the property was set, false otherwise.</returns>
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

		/// <summary>
		/// Sets a property value on an action using the wrapper type.
		/// </summary>
		/// <param name="action">The FsmStateAction wrapper.</param>
		/// <param name="propertyName">The name of the property to set.</param>
		/// <param name="value">The value to set.</param>
		/// <returns>True if the property was set, false otherwise.</returns>
		public static bool SetActionProperty(FsmActionWrapper action, string propertyName, object value)
		{
			return SetActionProperty(action.InternalAction, propertyName, value);
		}

		#endregion

		#region Transition Management

		/// <summary>
		/// Gets all transitions from a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <returns>An array of FsmTransition objects.</returns>
		public static object[] GetTransitions(object state)
		{
			if (state == null)
			{
				return new object[0];
			}

			PropertyInfo transitionsProperty = FsmStateType.GetProperty("Transitions");
			return (object[])transitionsProperty.GetValue(state, null);
		}

		/// <summary>
		/// Gets all transitions from a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <returns>An array of FsmTransition wrapper objects.</returns>
		public static FsmTransitionWrapper[] GetTransitions(FsmStateWrapper state)
		{
			object[] transitions = GetTransitions(state.InternalState);
			return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
		}

		/// <summary>
		/// Gets all global transitions in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>An array of FsmTransition objects.</returns>
		public static object[] GetGlobalTransitions(object fsm)
		{
			if (fsm == null)
			{
				return new object[0];
			}

			return (object[])GlobalTransitionsProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets all global transitions in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>An array of FsmTransition wrapper objects.</returns>
		public static FsmTransitionWrapper[] GetGlobalTransitions(FsmWrapper fsm)
		{
			object[] transitions = GetGlobalTransitions(fsm.InternalFsm);
			return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
		}

		/// <summary>
		/// Adds a transition to a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="eventName">The name of the event that triggers the transition.</param>
		/// <param name="toState">The name of the state to transition to.</param>
		/// <returns>The newly created transition.</returns>
		public static object AddTransition(object state, string eventName, string toState)
		{
			if (state == null || string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(toState))
			{
				return null;
			}

			// Get the current transitions
			object[] currentTransitions = GetTransitions(state);
			
			// Create a new transitions array with one more element
			Array newTransitions = Array.CreateInstance(FsmTransitionType, currentTransitions.Length + 1);
			Array.Copy(currentTransitions, newTransitions, currentTransitions.Length);

			// Create a new transition
			ConstructorInfo transitionConstructor = FsmTransitionType.GetConstructor(Type.EmptyTypes);
			object newTransition = transitionConstructor.Invoke(null);

			// Set the transition properties
			PropertyInfo eventNameProperty = FsmTransitionType.GetProperty("EventName");
			eventNameProperty.SetValue(newTransition, eventName, null);

			PropertyInfo toStateProperty = FsmTransitionType.GetProperty("ToState");
			toStateProperty.SetValue(newTransition, toState, null);

			// Get the FsmEvent for this event name
			object fsmEvent = GetFsmEventMethod.Invoke(null, new object[] { eventName });
			
			PropertyInfo fsmEventProperty = FsmTransitionType.GetProperty("FsmEvent");
			fsmEventProperty.SetValue(newTransition, fsmEvent, null);

			// Get the target state object
			object fsm = GetStateProperty(state, "Fsm");
			object targetState = GetState(fsm, toState);
			
			PropertyInfo toFsmStateProperty = FsmTransitionType.GetProperty("ToFsmState");
			toFsmStateProperty.SetValue(newTransition, targetState, null);

			// Add the new transition to the array
			newTransitions.SetValue(newTransition, currentTransitions.Length);

			// Update the state's Transitions property
			PropertyInfo transitionsProperty = FsmStateType.GetProperty("Transitions");
			transitionsProperty.SetValue(state, newTransitions, null);

			return newTransition;
		}

		/// <summary>
		/// Adds a transition to a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="eventName">The name of the event that triggers the transition.</param>
		/// <param name="toState">The name of the state to transition to.</param>
		/// <returns>The newly created transition wrapper.</returns>
		public static FsmTransitionWrapper AddTransition(FsmStateWrapper state, string eventName, string toState)
		{
			object transition = AddTransition(state.InternalState, eventName, toState);
			return transition != null ? new FsmTransitionWrapper(transition) : default;
		}

		/// <summary>
		/// Adds a global transition to an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="eventName">The name of the event that triggers the transition.</param>
		/// <param name="toState">The name of the state to transition to.</param>
		/// <returns>The newly created transition.</returns>
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
			ConstructorInfo transitionConstructor = FsmTransitionType.GetConstructor(Type.EmptyTypes);
			object newTransition = transitionConstructor.Invoke(null);

			// Set the transition properties
			PropertyInfo eventNameProperty = FsmTransitionType.GetProperty("EventName");
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

			return newTransition;
		}

		/// <summary>
		/// Adds a global transition to an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="eventName">The name of the event that triggers the transition.</param>
		/// <param name="toState">The name of the state to transition to.</param>
		/// <returns>The newly created transition wrapper.</returns>
		public static FsmTransitionWrapper AddGlobalTransition(FsmWrapper fsm, string eventName, string toState)
		{
			object transition = AddGlobalTransition(fsm.InternalFsm, eventName, toState);
			return transition != null ? new FsmTransitionWrapper(transition) : default;
		}

		/// <summary>
		/// Removes a transition from a state.
		/// </summary>
		/// <param name="state">The FsmState object.</param>
		/// <param name="transitionIndex">The index of the transition to remove.</param>
		/// <returns>True if the transition was removed, false otherwise.</returns>
		public static bool RemoveTransition(object state, int transitionIndex)
		{
			if (state == null || transitionIndex < 0)
			{
				return false;
			}

			// Get the current transitions
			object[] currentTransitions = GetTransitions(state);
			
			if (transitionIndex >= currentTransitions.Length)
			{
				return false;
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

		/// <summary>
		/// Removes a transition from a state using the wrapper type.
		/// </summary>
		/// <param name="state">The FsmState wrapper.</param>
		/// <param name="transitionIndex">The index of the transition to remove.</param>
		/// <returns>True if the transition was removed, false otherwise.</returns>
		public static bool RemoveTransition(FsmStateWrapper state, int transitionIndex)
		{
			return RemoveTransition(state.InternalState, transitionIndex);
		}

		/// <summary>
		/// Removes a global transition from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="transitionIndex">The index of the transition to remove.</param>
		/// <returns>True if the transition was removed, false otherwise.</returns>
		public static bool RemoveGlobalTransition(object fsm, int transitionIndex)
		{
			if (fsm == null || transitionIndex < 0)
			{
				return false;
			}

			// Get the current global transitions
			object[] currentTransitions = GetGlobalTransitions(fsm);
			
			if (transitionIndex >= currentTransitions.Length)
			{
				return false;
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

		/// <summary>
		/// Removes a global transition from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="transitionIndex">The index of the transition to remove.</param>
		/// <returns>True if the transition was removed, false otherwise.</returns>
		public static bool RemoveGlobalTransition(FsmWrapper fsm, int transitionIndex)
		{
			return RemoveGlobalTransition(fsm.InternalFsm, transitionIndex);
		}

		/// <summary>
		/// Removes all transitions to a specific state from all states in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="stateName">The name of the state to remove transitions to.</param>
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

		/// <summary>
		/// Removes all transitions to a specific state from all states in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="stateName">The name of the state to remove transitions to.</param>
		private static void RemoveTransitionsToState(FsmWrapper fsm, string stateName)
		{
			RemoveTransitionsToState(fsm.InternalFsm, stateName);
		}

		#endregion

		#region Event Management

		/// <summary>
		/// Gets all events in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>An array of FsmEvent objects.</returns>
		public static object[] GetEvents(object fsm)
		{
			if (fsm == null)
			{
				return new object[0];
			}

			return (object[])EventsProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets all events in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>An array of FsmEvent wrapper objects.</returns>
		public static FsmEventWrapper[] GetEvents(FsmWrapper fsm)
		{
			object[] events = GetEvents(fsm.InternalFsm);
			return events.Select(e => new FsmEventWrapper(e)).ToArray();
		}

		/// <summary>
		/// Gets an event by name.
		/// </summary>
		/// <param name="eventName">The name of the event to get.</param>
		/// <returns>The FsmEvent object.</returns>
		public static object GetFsmEvent(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return null;
			}

			return GetFsmEventMethod.Invoke(null, new object[] { eventName });
		}

		/// <summary>
		/// Gets an event by name, returning a wrapper type.
		/// </summary>
		/// <param name="eventName">The name of the event to get.</param>
		/// <returns>A type-safe wrapper for the FsmEvent object.</returns>
		public static FsmEventWrapper GetFsmEventWrapper(string eventName)
		{
			object fsmEvent = GetFsmEvent(eventName);
			return fsmEvent != null ? new FsmEventWrapper(fsmEvent) : default;
		}

		/// <summary>
		/// Adds an event to an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="eventName">The name of the event to add.</param>
		/// <returns>The newly created event.</returns>
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

		/// <summary>
		/// Adds an event to an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="eventName">The name of the event to add.</param>
		/// <returns>The newly created event wrapper.</returns>
		public static FsmEventWrapper AddEvent(FsmWrapper fsm, string eventName)
		{
			object fsmEvent = AddEvent(fsm.InternalFsm, eventName);
			return fsmEvent != null ? new FsmEventWrapper(fsmEvent) : default;
		}

		/// <summary>
		/// Sends an event to an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="eventName">The name of the event to send.</param>
		public static void SendEvent(object fsm, string eventName)
		{
			if (fsm == null || string.IsNullOrEmpty(eventName))
			{
				return;
			}

			MethodInfo eventMethod = FsmType.GetMethod("Event", new[] { typeof(string) });
			eventMethod.Invoke(fsm, new object[] { eventName });
		}

		/// <summary>
		/// Sends an event to an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="eventName">The name of the event to send.</param>
		public static void SendEvent(FsmWrapper fsm, string eventName)
		{
			SendEvent(fsm.InternalFsm, eventName);
		}

		/// <summary>
		/// Sends an event to a PlayMakerFSM.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component.</param>
		/// <param name="eventName">The name of the event to send.</param>
		public static void SendEvent(Component playMakerFSM, string eventName)
		{
			if (playMakerFSM == null || string.IsNullOrEmpty(eventName))
			{
				return;
			}

			MethodInfo eventMethod = PlayMakerFSMType.GetMethod("SendEvent", new[] { typeof(string) });
			eventMethod.Invoke(playMakerFSM, new object[] { eventName });
		}

		/// <summary>
		/// Sends an event to a PlayMakerFSM using the wrapper type.
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper.</param>
		/// <param name="eventName">The name of the event to send.</param>
		public static void SendEvent(PlayMakerFsmWrapper playMakerFSM, string eventName)
		{
			SendEvent(playMakerFSM.InternalComponent, eventName);
		}

		#endregion

		#region Variable Management

		/// <summary>
		/// Gets the variables object from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <returns>The FsmVariables object.</returns>
		public static object GetVariables(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			PropertyInfo variablesProperty = FsmType.GetProperty("Variables");
			return variablesProperty.GetValue(fsm, null);
		}

		/// <summary>
		/// Gets the variables object from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <returns>The FsmVariables wrapper object.</returns>
		public static FsmVariablesWrapper GetVariables(FsmWrapper fsm)
		{
			object variables = GetVariables(fsm.InternalFsm);
			return variables != null ? new FsmVariablesWrapper(variables) : default;
		}


		/// <summary>
		/// Gets a array variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static object[] GetArrayVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return null;
			}

			MethodInfo getBoolMethod = FsmType.GetMethod("GetFsmArray", new[] { typeof(string) });
			object fsmArray = getBoolMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmArray.GetType().GetProperty("Values");
			return (object[])valueProperty.GetValue(fsmArray, null);
		}

		/// <summary>
		/// Gets a array variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static object[] GetArrayVariable(FsmWrapper fsm, string variableName)
		{
			return GetArrayVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a array variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetArrayVariable(object fsm, string variableName, object[] value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getArrayMethod = FsmType.GetMethod("GetFsmArray", new[] { typeof(string) });
			object fsmBool = getArrayMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmBool.GetType().GetProperty("Values");
			valueProperty.SetValue(fsmBool, value, null);
		}

		/// <summary>
		/// Sets a array variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetBoolVariable(FsmWrapper fsm, string variableName, object[] value)
		{
			SetArrayVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a boolean variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static bool GetBoolVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return false;
			}

			MethodInfo getBoolMethod = FsmType.GetMethod("GetFsmBool", new[] { typeof(string) });
			object fsmBool = getBoolMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmBool.GetType().GetProperty("Value");
			return (bool)valueProperty.GetValue(fsmBool, null);
		}

		/// <summary>
		/// Gets a boolean variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static bool GetBoolVariable(FsmWrapper fsm, string variableName)
		{
			return GetBoolVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a boolean variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetBoolVariable(object fsm, string variableName, bool value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getBoolMethod = FsmType.GetMethod("GetFsmBool", new[] { typeof(string) });
			object fsmBool = getBoolMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmBool.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmBool, value, null);
		}

		/// <summary>
		/// Sets a boolean variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetBoolVariable(FsmWrapper fsm, string variableName, bool value)
		{
			SetBoolVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a color variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Color GetColorVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			MethodInfo getColorMethod = FsmType.GetMethod("GetFsmColor", new[] { typeof(string) });
			object fsmColor = getColorMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			return (Color)valueProperty.GetValue(fsmColor, null);
		}

		/// <summary>
		/// Gets a color variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Color GetColorVariable(FsmWrapper fsm, string variableName)
		{
			return GetColorVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a color variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetColorVariable(object fsm, string variableName, Color value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getColorMethod = FsmType.GetMethod("GetFsmColor", new[] { typeof(string) });
			object fsmColor = getColorMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmColor, value, null);
		}

		/// <summary>
		/// Sets a color variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetColorVariable(FsmWrapper fsm, string variableName, Color value)
		{
			SetColorVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a enum variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Enum GetEnumVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			MethodInfo getEnumMethod = FsmType.GetMethod("GetFsmEnum", new[] { typeof(string) });
			object fsmColor = getEnumMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			return (Enum)valueProperty.GetValue(fsmColor, null);
		}

		/// <summary>
		/// Gets a enum variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Enum GetEnumVariable(FsmWrapper fsm, string variableName)
		{
			return GetEnumVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a enum variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetEnumVariable(object fsm, string variableName, Enum value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getEnumMethod = FsmType.GetMethod("GetFsmEnum", new[] { typeof(string) });
			object fsmColor = getEnumMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmColor, value, null);
		}

		/// <summary>
		/// Sets a enum variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetEnumVariable(FsmWrapper fsm, string variableName, Enum value)
		{
			SetEnumVariable(fsm.InternalFsm, variableName, value);
		}


		/// <summary>
		/// Gets a object variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static UnityEngine.Object GetObjectVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return default;
			}

			MethodInfo getObjectMethod = FsmType.GetMethod("GetFsmObject", new[] { typeof(string) });
			object fsmColor = getObjectMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			return (UnityEngine.Object)valueProperty.GetValue(fsmColor, null);
		}

		/// <summary>
		/// Gets a object variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static UnityEngine.Object GetObjectVariable(FsmWrapper fsm, string variableName)
		{
			return GetObjectVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a object variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetObjectVariable(object fsm, string variableName, UnityEngine.Object value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getObjectMethod = FsmType.GetMethod("GetFsmObject", new[] { typeof(string) });
			object fsmColor = getObjectMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmColor.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmColor, value, null);
		}

		/// <summary>
		/// Sets a object variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetObjectVariable(FsmWrapper fsm, string variableName, UnityEngine.Object value)
		{
			SetObjectVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets an integer variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static int GetIntVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return 0;
			}

			MethodInfo getIntMethod = FsmType.GetMethod("GetFsmInt", new[] { typeof(string) });
			object fsmInt = getIntMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmInt.GetType().GetProperty("Value");
			return (int)valueProperty.GetValue(fsmInt, null);
		}

		/// <summary>
		/// Gets an integer variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static int GetIntVariable(FsmWrapper fsm, string variableName)
		{
			return GetIntVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets an integer variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetIntVariable(object fsm, string variableName, int value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getIntMethod = FsmType.GetMethod("GetFsmInt", new[] { typeof(string) });
			object fsmInt = getIntMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmInt.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmInt, value, null);
		}

		/// <summary>
		/// Sets an integer variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetIntVariable(FsmWrapper fsm, string variableName, int value)
		{
			SetIntVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a float variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static float GetFloatVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return 0f;
			}

			MethodInfo getFloatMethod = FsmType.GetMethod("GetFsmFloat", new[] { typeof(string) });
			object fsmFloat = getFloatMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmFloat.GetType().GetProperty("Value");
			return (float)valueProperty.GetValue(fsmFloat, null);
		}

		/// <summary>
		/// Gets a float variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static float GetFloatVariable(FsmWrapper fsm, string variableName)
		{
			return GetFloatVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a float variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetFloatVariable(object fsm, string variableName, float value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getFloatMethod = FsmType.GetMethod("GetFsmFloat", new[] { typeof(string) });
			object fsmFloat = getFloatMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmFloat.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmFloat, value, null);
		}

		/// <summary>
		/// Sets a float variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetFloatVariable(FsmWrapper fsm, string variableName, float value)
		{
			SetFloatVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a string variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static string GetStringVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return null;
			}

			MethodInfo getStringMethod = FsmType.GetMethod("GetFsmString", new[] { typeof(string) });
			object fsmString = getStringMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmString.GetType().GetProperty("Value");
			return (string)valueProperty.GetValue(fsmString, null);
		}

		/// <summary>
		/// Gets a string variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static string GetStringVariable(FsmWrapper fsm, string variableName)
		{
			return GetStringVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a string variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetStringVariable(object fsm, string variableName, string value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getStringMethod = FsmType.GetMethod("GetFsmString", new[] { typeof(string) });
			object fsmString = getStringMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmString.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmString, value, null);
		}

		/// <summary>
		/// Sets a string variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetStringVariable(FsmWrapper fsm, string variableName, string value)
		{
			SetStringVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a Vector3 variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Vector3 GetVector3Variable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return Vector3.zero;
			}

			MethodInfo getVectorMethod = FsmType.GetMethod("GetFsmVector3", new[] { typeof(string) });
			object fsmVector = getVectorMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmVector.GetType().GetProperty("Value");
			return (Vector3)valueProperty.GetValue(fsmVector, null);
		}

		/// <summary>
		/// Gets a Vector3 variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static Vector3 GetVector3Variable(FsmWrapper fsm, string variableName)
		{
			return GetVector3Variable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a Vector3 variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetVector3Variable(object fsm, string variableName, Vector3 value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getVectorMethod = FsmType.GetMethod("GetFsmVector3", new[] { typeof(string) });
			object fsmVector = getVectorMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmVector.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmVector, value, null);
		}

		/// <summary>
		/// Sets a Vector3 variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetVector3Variable(FsmWrapper fsm, string variableName, Vector3 value)
		{
			SetVector3Variable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets a GameObject variable's value from an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static GameObject GetGameObjectVariable(object fsm, string variableName)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return null;
			}

			MethodInfo getGoMethod = FsmType.GetMethod("GetFsmGameObject", new[] { typeof(string) });
			object fsmGo = getGoMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmGo.GetType().GetProperty("Value");
			return (GameObject)valueProperty.GetValue(fsmGo, null);
		}

		/// <summary>
		/// Gets a GameObject variable's value from an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <returns>The value of the variable.</returns>
		public static GameObject GetGameObjectVariable(FsmWrapper fsm, string variableName)
		{
			return GetGameObjectVariable(fsm.InternalFsm, variableName);
		}

		/// <summary>
		/// Sets a GameObject variable's value in an FSM.
		/// </summary>
		/// <param name="fsm">The Fsm object.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetGameObjectVariable(object fsm, string variableName, GameObject value)
		{
			if (fsm == null || string.IsNullOrEmpty(variableName))
			{
				return;
			}

			MethodInfo getGoMethod = FsmType.GetMethod("GetFsmGameObject", new[] { typeof(string) });
			object fsmGo = getGoMethod.Invoke(fsm, new object[] { variableName });
			
			PropertyInfo valueProperty = fsmGo.GetType().GetProperty("Value");
			valueProperty.SetValue(fsmGo, value, null);
		}

		/// <summary>
		/// Sets a GameObject variable's value in an FSM using the wrapper type.
		/// </summary>
		/// <param name="fsm">The Fsm wrapper.</param>
		/// <param name="variableName">The name of the variable.</param>
		/// <param name="value">The value to set.</param>
		public static void SetGameObjectVariable(FsmWrapper fsm, string variableName, GameObject value)
		{
			SetGameObjectVariable(fsm.InternalFsm, variableName, value);
		}

		/// <summary>
		/// Gets the Object value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Object variable</param>
		/// <returns>Returns the Object value of the variable</returns>
		public static UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetObjectVariable(varName);
		}

		/// <summary>
		/// Sets the Object value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Object variable</param>
		/// <param name="value">The value to set in the Object variable</param>
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

		/// <summary>
		/// Gets the Material value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Material variable</param>
		/// <returns>Returns the Material value of the variable</returns>
		public static Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetMaterialVariable(varName);
		}

		/// <summary>
		/// Sets the Material value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Material variable</param>
		/// <param name="value">The value to set in the Material variable</param>
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

		/// <summary>
		/// Gets the Texture value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Texture variable</param>
		/// <returns>Returns the Texture value of the variable</returns>
		public static Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetTextureVariable(varName);
		}

		/// <summary>
		/// Sets the Texture value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Texture variable</param>
		/// <param name="value">The value to set in the Texture variable</param>
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

		/// <summary>
		/// Gets the Float value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Float variable</param>
		/// <returns>Returns the Float value of the variable</returns>
		public static float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetFloatVariable(varName);
		}

		/// <summary>
		/// Sets the Float value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Float variable</param>
		/// <param name="value">The value to set in the Float variable</param>
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

		/// <summary>
		/// Gets the Int value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Int variable</param>
		/// <returns>Returns the Int value of the variable</returns>
		public static int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetIntVariable(varName);
		}

		/// <summary>
		/// Sets the Int value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Int variable</param>
		/// <param name="value">The value to set in the Int variable</param>
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

		/// <summary>
		/// Gets the Bool value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Bool variable</param>
		/// <returns>Returns the Bool value of the variable</returns>
		public static bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetBoolVariable(varName);
		}

		/// <summary>
		/// Sets the Bool value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Bool variable</param>
		/// <param name="value">The value to set in the Bool variable</param>
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

		/// <summary>
		/// Gets the String value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the String variable</param>
		/// <returns>Returns the String value of the variable</returns>
		public static string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetStringVariable(varName);
		}

		/// <summary>
		/// Sets the String value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the String variable</param>
		/// <param name="value">The value to set in the String variable</param>
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

		/// <summary>
		/// Gets the Vector2 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Vector2 variable</param>
		/// <returns>Returns the Vector2 value of the variable</returns>
		public static Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetVector2Variable(varName);
		}

		/// <summary>
		/// Sets the Vector2 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Vector2 variable</param>
		/// <param name="value">The value to set in the Vector2 variable</param>
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

		/// <summary>
		/// Gets the Vector3 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Vector3 variable</param>
		/// <returns>Returns the Vector3 value of the variable</returns>
		public static Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetVector3Variable(varName);
		}

		/// <summary>
		/// Sets the Vector3 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Vector3 variable</param>
		/// <param name="value">The value to set in the Vector3 variable</param>
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

		/// <summary>
		/// Gets the Rect value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Rect variable</param>
		/// <returns>Returns the Rect value of the variable</returns>
		public static Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetRectVariable(varName);
		}

		/// <summary>
		/// Sets the Rect value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Rect variable</param>
		/// <param name="value">The value to set in the Rect variable</param>
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

		/// <summary>
		/// Gets the Quaternion value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Quaternion variable</param>
		/// <returns>Returns the Quaternion value of the variable</returns>
		public static Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetQuaternionVariable(varName);
		}

		/// <summary>
		/// Sets the Quaternion value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Quaternion variable</param>
		/// <param name="value">The value to set in the Quaternion variable</param>
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

		/// <summary>
		/// Gets the Color value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Color variable</param>
		/// <returns>Returns the Color value of the variable</returns>
		public static Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetColorVariable(varName);
		}

		/// <summary>
		/// Sets the Color value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Color variable</param
		/// <param name="value">The value to set in the Color variable</param>
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

		/// <summary>
		/// Gets the GameObject value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the GameObject variable</param>
		/// <returns>Returns the GameObject value of the variable</returns>
		public static GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetGameObjectVariable(varName);
		}

		/// <summary>
		/// Sets the GameObject value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the GameObject variable</param>
		/// <param name="value">The value to set in the GameObject variable</param>
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

		/// <summary>
		/// Gets the Array value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Array variable</param>
		/// <returns>Returns the Array value of the variable</returns>
		public static object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetArrayVariable(varName);
		}

		/// <summary>
		/// Sets the Array value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Array variable</param>
		/// <param name="value">The value to set in the Array variable</param>
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

		/// <summary>
		/// Gets the Enum value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Enum variable</param>
		/// <returns>Returns the Enum value of the variable</returns>
		public static Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			var psFSM = FindPlayMakerFSMWrapper(obj, fsmName);
			if (psFSM == default)
			{
				return default;
			}

			return psFSM.GetFsm().GetEnumVariable(varName);
		}

		/// <summary>
		/// Sets the Enum value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Enum variable</param>
		/// <param name="value">The value to set in the Enum variable</param>
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

		/// <summary>
		/// Creates a PlayMaker FSM action from a WeaverFSMAction
		/// </summary>
		/// <param name="action">The WeaverFSMAction to convert</param>
		/// <returns>Returns the created FSM action object or null if PlayMaker is not available</returns>
		public static object CreateFSMActionFromWeaverAction(WeaverFSMAction action)
		{
			return PlayMakerUtilities.impl.CreateFSMActionFromWeaverAction(action);
		}


		#endregion

		#region Utility Methods

		/// <summary>
		/// Gets the value of a property from an object using reflection.
		/// </summary>
		/// <param name="obj">The object to get the property from.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>The value of the property.</returns>
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

		/// <summary>
		/// Finds all PlayMakerFSMs in the scene that match a certain name.
		/// </summary>
		/// <param name="fsmName">The name of the FSM to find. If null or empty, all FSMs will be returned.</param>
		/// <returns>An array of PlayMakerFSM components.</returns>
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

		/// <summary>
		/// Finds all PlayMakerFSMs in the scene that match a certain name, returning wrapper types.
		/// </summary>
		/// <param name="fsmName">The name of the FSM to find. If null or empty, all FSMs will be returned.</param>
		/// <returns>An array of PlayMakerFSM wrapper components.</returns>
		public static PlayMakerFsmWrapper[] FindAllFSMWrappersInScene(string fsmName = null)
		{
			Component[] components = FindAllFSMsInScene(fsmName);
			return components.Select(c => new PlayMakerFsmWrapper(c)).ToArray();
		}

		/// <summary>
		/// Makes a deep copy of a PlayMakerFSM.
		/// </summary>
		/// <param name="originalFsm">The PlayMakerFSM to copy.</param>
		/// <param name="targetGameObject">The GameObject to add the copied FSM to.</param>
		/// <returns>The copied PlayMakerFSM component.</returns>
		public static Component CopyFSM(Component originalFsm, GameObject targetGameObject)
		{
			if (originalFsm == null || targetGameObject == null)
			{
				return null;
			}

			// Add a new PlayMakerFSM component to the target GameObject
			Component newFsm = targetGameObject.AddComponent(PlayMakerFSMType);
			
			// Get the original Fsm object
			object originalFsmObject = GetFsm(originalFsm);
			
			// Create a new Fsm with the same values as the original
			ConstructorInfo fsmCopyConstructor = FsmType.GetConstructor(new[] { FsmType, FsmVariablesType });
			object newFsmObject = fsmCopyConstructor.Invoke(new[] { originalFsmObject, null });
			
			// Set the owner of the new Fsm to the new PlayMakerFSM component
			PropertyInfo ownerProperty = FsmType.GetProperty("Owner");
			ownerProperty.SetValue(newFsmObject, newFsm, null);
			
			// Set the Fsm property of the new PlayMakerFSM component
			PropertyInfo fsmProperty = PlayMakerFSMType.GetProperty("Fsm");
			fsmProperty.SetValue(newFsm, newFsmObject, null);
			
			// Set the name of the new PlayMakerFSM component
			string originalName = GetFsmName(originalFsm);
			SetFsmName(newFsm, originalName);
			
			return newFsm;
		}

		/// <summary>
		/// Makes a deep copy of a PlayMakerFSM using wrapper types.
		/// </summary>
		/// <param name="originalFsm">The PlayMakerFSM wrapper to copy.</param>
		/// <param name="targetGameObject">The GameObject to add the copied FSM to.</param>
		/// <returns>The copied PlayMakerFSM wrapper component.</returns>
		public static PlayMakerFsmWrapper CopyFSM(PlayMakerFsmWrapper originalFsm, GameObject targetGameObject)
		{
			Component component = CopyFSM(originalFsm.InternalComponent, targetGameObject);
			return component != null ? new PlayMakerFsmWrapper(component) : default;
		}

		#endregion
		
		#region FSM Snapshots
		
		/// <summary>
		/// Creates a snapshot of the FSM's current state
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component</param>
		/// <returns>A snapshot that can be used to restore the FSM later</returns>
		public static FsmSnapshot CreateSnapshot(Component playMakerFSM)
		{        
			if (playMakerFSM == null) return null;
			
			object fsm = GetFsm(playMakerFSM);
			return FsmSnapshot.Create(fsm, playMakerFSM);
		}
		
		/// <summary>
		/// Creates a snapshot of the FSM's current state
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper</param>
		/// <returns>A snapshot that can be used to restore the FSM later</returns>
		public static FsmSnapshot CreateSnapshot(PlayMakerFsmWrapper playMakerFSM)
		{        
			return CreateSnapshot(playMakerFSM.InternalComponent);
		}
		
		/// <summary>
		/// Restores an FSM to a previously captured snapshot
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM component</param>
		/// <param name="snapshot">The snapshot to restore</param>
		public static void RestoreSnapshot(Component playMakerFSM, FsmSnapshot snapshot)
		{        
			if (playMakerFSM == null || snapshot == null) return;
			
			object fsm = GetFsm(playMakerFSM);
			snapshot.Restore(fsm, playMakerFSM);
		}
		
		/// <summary>
		/// Restores an FSM to a previously captured snapshot
		/// </summary>
		/// <param name="playMakerFSM">The PlayMakerFSM wrapper</param>
		/// <param name="snapshot">The snapshot to restore</param>
		public static void RestoreSnapshot(PlayMakerFsmWrapper playMakerFSM, FsmSnapshot snapshot)
		{        
			RestoreSnapshot(playMakerFSM.InternalComponent, snapshot);
		}
		#endregion
	}
}
