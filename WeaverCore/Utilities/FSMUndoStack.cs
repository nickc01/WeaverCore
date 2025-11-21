using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Playmaker;
using WeaverCore.Utilities;



namespace WeaverCore.Utilities
{
    /// <summary>
    /// A specialized action stack for PlayMaker FSM operations that provides an easy way to modify FSMs
    /// with the ability to completely undo changes. Wraps PlayMakerUtilities functionality.
    /// </summary>
    public class FSMUndoStack : UndoStack
    {
        // The FSM component being operated on
        //private readonly MonoBehaviour fsmComponent;
        private readonly object fsm;

        /// <summary>
        /// Creates a new FSMActionStack for the specified FSM component.
        /// </summary>
        /// <param name="fsmComponent">The PlayMaker FSM component to modify.</param>
        public FSMUndoStack(MonoBehaviour fsmComponent) : base()
        {
            if (fsmComponent == null)
            {
                throw new ArgumentNullException(nameof(fsmComponent));
            }

            this.fsm = PlayMakerUtilities.GetFsm(fsmComponent);
        }

        /// <summary>
        /// Creates a new FSMActionStack for an FSM on the specified GameObject by name.
        /// </summary>
        /// <param name="gameObject">The GameObject containing the FSM.</param>
        /// <param name="fsmName">The name of the FSM to find.</param>
        public FSMUndoStack(GameObject gameObject, string fsmName) : base()
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            if (string.IsNullOrEmpty(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }

            // Find the FSM by name
            var component = PlayMakerUtilities.FindPlayMakerFSM(gameObject, fsmName);

            if (component == null)
            {
                throw new ArgumentException($"FSM with name '{fsmName}' not found on GameObject '{gameObject.name}'.");
            }

            fsm = PlayMakerUtilities.GetFsm(component);
        }

        /// <summary>
        /// Gets a state wrapper for the specified state name.
        /// </summary>
        /// <param name="stateName">The name of the state to get.</param>
        /// <returns>A wrapper for the state.</returns>
        public FsmStateWrapper GetState(string stateName)
        {
            return GetFsmWrapper().GetState(stateName);
        }

        public bool TryGetState(string stateName, out FsmStateWrapper state)
        {
            return GetFsmWrapper().TryGetState(stateName, out state);
        }

        /// <summary>
        /// Adds an action to the specified state and adds an undo operation to the stack.
        /// </summary>
        /// <param name="stateName">The name of the state to modify.</param>
        /// <param name="action">The action to add to the state.</param>
        public void AddAction(string stateName, FsmActionWrapper action)
        {
            AddActionAtIndex(stateName, action, -1);
        }

        class QuickWeaverAction1 : WeaverFSMAction
        {
            public readonly Action MainAction;
            public QuickWeaverAction1(Action action)
            {
                MainAction = action;
            }

            public override void OnEnter()
            {
                MainAction?.Invoke();
            }
        }

        class QuickWeaverAction2 : WeaverFSMAction
        {
            public readonly Action<FsmWrapper> MainAction;
            public QuickWeaverAction2(Action<FsmWrapper> action)
            {
                MainAction = action;
            }

            public override void OnEnter()
            {
                MainAction?.Invoke(FsmWrapper);
            }
        }

        public void AddAction(string stateName, Action action)
        {
            AddAction(stateName, new QuickWeaverAction1(action));
        }


        public void AddAction(string stateName, Action<FsmWrapper> action)
        {
            AddAction(stateName, new QuickWeaverAction2(action));
        }

        public FsmActionWrapper AddAction(string stateName, WeaverFSMAction action)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(action);
            AddActionAtIndex(stateName, fsmAction, -1);
            return fsmAction;
        }

        public FsmActionWrapper AddActionAtIndex(string stateName, Action action, int index)
        {
            return AddActionAtIndex(stateName, new QuickWeaverAction1(action), index);
        }

        public FsmActionWrapper AddActionAtIndex(string stateName, Action<FsmWrapper> action, int index)
        {
            return AddActionAtIndex(stateName, new QuickWeaverAction2(action), index);
        }

        public FsmActionWrapper AddActionAtIndex(string stateName, WeaverFSMAction action, int index)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(action);
            AddActionAtIndex(stateName, fsmAction, index);
            return fsmAction;
        }

        public FsmActionWrapper AddAction(string stateName, IEnumerator action)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(new EnumWeaverFSMAction(action));
            AddActionAtIndex(stateName, fsmAction, -1);
            return fsmAction;
        }

        public FsmActionWrapper AddActionAtIndex(string stateName, IEnumerator action, int index)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(new EnumWeaverFSMAction(action));
            AddActionAtIndex(stateName, fsmAction, index);
            return fsmAction;
        }

        /// <summary>
        /// Adds an action to the specified state at a specific index and adds an undo operation to the stack.
        /// </summary>
        /// <param name="stateName">The name of the state to modify.</param>
        /// <param name="action">The action to add to the state.</param>
        /// <param name="index">The index at which to add the action.</param>
        public void AddActionAtIndex(string stateName, FsmActionWrapper action, int index)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            //var stateRef = state.InternalState;

            // Create wrapper if needed

            // Add action at index and track it for undo
            Add(() =>
            {
                if (index < 0)
                {
                    WeaverLog.Log($"Adding Action in state {stateName} : {action.GetType().Name}");
                    PlayMakerUtilities.AddAction(state, action);
                }
                else
                {
                    WeaverLog.Log($"Adding Action in state {stateName} : {action.GetType().Name} at index {index}");
                    PlayMakerUtilities.AddActionAtIndex(state, action, index);
                }
                return () =>
                {
                    PlayMakerUtilities.RemoveAction(state, index);
                };
            });
        }

        public void RemoveAllActions(string stateName)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            var actions = PlayMakerUtilities.GetActions(state);

            for (int i = actions.Length - 1; i >= 0 ; i--)
            {
                RemoveAction(stateName, i);
            }
        }

        public void RemoveAllTransitions(string stateName)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            var transitions = PlayMakerUtilities.GetTransitions(state);

            for (int i = transitions.Length - 1; i >= 0 ; i--)
            {
                RemoveTransition(stateName, i);
            }
        }

        /// <summary>
        /// Removes an action at the specified index from a state and adds an undo operation to the stack.
        /// </summary>
        /// <param name="stateName">The name of the state to modify.</param>
        /// <param name="actionIndex">The index of the action to remove.</param>
        public void RemoveAction(string stateName, int actionIndex)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            //var stateRef = state.InternalState;
            var actions = PlayMakerUtilities.GetActions(state);

            if (actionIndex < 0 || actionIndex >= actions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(actionIndex), "Action index out of range.");
            }

            // Store the action before removal for undo
            var actionToRemove = actions[actionIndex];

            // Remove action and track it for undo
            Add(() =>
            {
                WeaverLog.Log($"Removing Action in state {stateName} at index {actionIndex}");
                PlayMakerUtilities.RemoveAction(state, actionIndex);
                return () =>
                {
                    PlayMakerUtilities.AddActionAtIndex(state, actionToRemove, actionIndex);
                };
            });
        }

        public void RemoveTransition(string stateName, int index)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            var transitions = state.GetTransitions();

            if (index < 0 || index >= transitions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Transition index out of range.");
            }

            // Store the action before removal for undo
            var transitionToRemove = transitions[index];

            //var oldFsmEvent = transitionToRemove.ReflectGetProperty("FsmEvent").ReflectGetProperty<string>("Name");
            //var oldToState = transitionToRemove.ReflectGetProperty<string>("ToState");

            // Remove action and track it for undo
            Add(() =>
            {
                WeaverLog.Log($"Removing Transition in state {stateName} at index {index}");
                PlayMakerUtilities.RemoveTransition(state, index);
                return () =>
                {
                    PlayMakerUtilities.AddTransition(state, transitionToRemove.FsmEvent.Name, transitionToRemove.ToState, index);
                };
            });
        }

        public void AddTransition(string stateName, string eventName, string toState, int index)
        {
            var state = GetState(stateName);
            if (state == default)
            {
                throw new ArgumentException($"State '{stateName}' not found in FSM.");
            }

            var transitions = state.GetTransitions();

            if (index < 0 || index >= transitions.Length)
            {
                index = transitions.Length - 1;
                //throw new ArgumentOutOfRangeException(nameof(index), "Transition index out of range.");
            }

            // Remove action and track it for undo
            Add(() =>
            {
                WeaverLog.Log($"Adding Transition in state {stateName} at index {index}");
                PlayMakerUtilities.AddTransition(state, eventName, toState, index);
                return () =>
                {
                    PlayMakerUtilities.RemoveTransition(state, index);
                };
            });
        }

        /// <summary>
        /// Replaces an action at the specified index in a state and adds an undo operation to the stack.
        /// </summary>
        /// <param name="stateName">The name of the state to modify.</param>
        /// <param name="actionIndex">The index of the action to replace.</param>
        /// <param name="newAction">The new action to use as a replacement.</param>
        public void ReplaceAction(string stateName, int actionIndex, FsmActionWrapper newAction)
        {
            RemoveAction(stateName, actionIndex);
            AddActionAtIndex(stateName, newAction, actionIndex);
        }

        public FsmActionWrapper ReplaceAction(string stateName, int actionIndex, WeaverFSMAction newAction)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(newAction);
            RemoveAction(stateName, actionIndex);
            AddActionAtIndex(stateName, fsmAction, actionIndex);
            return fsmAction;
        }

        public FsmActionWrapper ReplaceAction(string stateName, int actionIndex, IEnumerator newAction)
        {
            var fsmAction = PlayMakerUtilities.CreateFSMActionWrapperFromWeaverAction(new EnumWeaverFSMAction(newAction));
            RemoveAction(stateName, actionIndex);
            AddActionAtIndex(stateName, fsmAction, actionIndex);
            return fsmAction;
        }

        /// <summary>
        /// Gets the value of an FSM bool variable.
        /// </summary>
        /// <param name="variableName">The name of the bool variable.</param>
        /// <returns>The value of the bool variable.</returns>
        public bool GetFsmBool(string variableName)
        {
            return PlayMakerUtilities.GetBoolVariable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM bool variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the bool variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmBool(string variableName, bool value)
        {
            // Store the original value for undo
            bool originalValue = GetFsmBool(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetBoolVariable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetBoolVariable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM float variable.
        /// </summary>
        /// <param name="variableName">The name of the float variable.</param>
        /// <returns>The value of the float variable.</returns>
        public float GetFsmFloat(string variableName)
        {
            return PlayMakerUtilities.GetFloatVariable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM float variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the float variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmFloat(string variableName, float value)
        {
            // Store the original value for undo
            float originalValue = GetFsmFloat(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetFloatVariable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetFloatVariable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM int variable.
        /// </summary>
        /// <param name="variableName">The name of the int variable.</param>
        /// <returns>The value of the int variable.</returns>
        public int GetFsmInt(string variableName)
        {
            return PlayMakerUtilities.GetIntVariable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM int variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the int variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmInt(string variableName, int value)
        {
            // Store the original value for undo
            int originalValue = GetFsmInt(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetIntVariable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetIntVariable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM string variable.
        /// </summary>
        /// <param name="variableName">The name of the string variable.</param>
        /// <returns>The value of the string variable.</returns>
        public string GetFsmString(string variableName)
        {
            return PlayMakerUtilities.GetStringVariable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM string variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the string variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmString(string variableName, string value)
        {
            // Store the original value for undo
            string originalValue = GetFsmString(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetStringVariable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetStringVariable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM Vector2 variable.
        /// </summary>
        /// <param name="variableName">The name of the Vector2 variable.</param>
        /// <returns>The value of the Vector2 variable.</returns>
        public Vector2 GetFsmVector2(string variableName)
        {
            return PlayMakerUtilities.GetVector2Variable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM Vector2 variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the Vector2 variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmVector2(string variableName, Vector2 value)
        {
            // Store the original value for undo
            Vector2 originalValue = GetFsmVector2(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetVector2Variable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetVector2Variable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM Vector3 variable.
        /// </summary>
        /// <param name="variableName">The name of the Vector3 variable.</param>
        /// <returns>The value of the Vector3 variable.</returns>
        public Vector3 GetFsmVector3(string variableName)
        {
            return PlayMakerUtilities.GetVector3Variable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM Vector3 variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the Vector3 variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmVector3(string variableName, Vector3 value)
        {
            // Store the original value for undo
            Vector3 originalValue = GetFsmVector3(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetVector3Variable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetVector3Variable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Gets the value of an FSM GameObject variable.
        /// </summary>
        /// <param name="variableName">The name of the GameObject variable.</param>
        /// <returns>The value of the GameObject variable.</returns>
        public GameObject GetFsmGameObject(string variableName)
        {
            return PlayMakerUtilities.GetGameObjectVariable(fsm, variableName);
        }

        /// <summary>
        /// Sets the value of an FSM GameObject variable and adds an undo operation to the stack.
        /// </summary>
        /// <param name="variableName">The name of the GameObject variable.</param>
        /// <param name="value">The new value for the variable.</param>
        public void SetFsmGameObject(string variableName, GameObject value)
        {
            // Store the original value for undo
            GameObject originalValue = GetFsmGameObject(variableName);

            // Set the new value and track it for undo
            Add(() =>
            {
                PlayMakerUtilities.SetGameObjectVariable(fsm, variableName, value);
                return () =>
                {
                    PlayMakerUtilities.SetGameObjectVariable(fsm, variableName, originalValue);
                };
            });
        }

        /// <summary>
        /// Sends an event to the FSM.
        /// </summary>
        /// <param name="eventName">The name of the event to send.</param>
        public void SendEvent(string eventName)
        {
            PlayMakerUtilities.SendEvent(fsm, eventName);
        }

        /// <summary>
        /// Gets a wrapper for the FSM.
        /// </summary>
        /// <returns>A wrapper for the FSM.</returns>
        public FsmWrapper GetFsmWrapper()
        {
            return new FsmWrapper(fsm);
        }

        /// <summary>
        /// Creates a WeaverCore FSM action from a wrapper object.
        /// </summary>
        /// <param name="action">The action to convert.</param>
        /// <returns>A FSM action object.</returns>
        public object CreateFSMActionFromWeaverAction(WeaverFSMAction action)
        {
            return PlayMakerUtilities.CreateFSMActionFromWeaverAction(action);
        }
    }
}