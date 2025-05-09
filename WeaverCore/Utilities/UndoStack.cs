using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// A stack-based undo system that can be used to implement undoable actions.
    /// Each action added to the stack can be undone by calling the Undo method.
    /// </summary>
    public class UndoStack
    {
        private interface IActionEntry
        {
            void Revert();
        }

        private class ActionEntryBasic : IActionEntry
        {
            public Action RevertAction { get; }
            
            public ActionEntryBasic(Action revertAction)
            {
                RevertAction = revertAction;
            }

            public void Revert()
            {
                RevertAction?.Invoke();
            }
        }

        /*private class ActionEntry<T> : IActionEntry
        {
            public Action<T> RevertAction { get; }

            public T State { get; }

            public ActionEntry(Action<T> revertAction, T state)
            {
                RevertAction = revertAction;
                State = state;
            }

            public void Revert()
            {
                RevertAction?.Invoke(State);
            }
        }*/

        // The stack of actions that can be undone
        private readonly Stack<IActionEntry> actionStack = new Stack<IActionEntry>();

        /// <summary>
        /// Gets the number of undoable actions on the stack.
        /// </summary>
        public int Count => actionStack.Count;

        /// <summary>
        /// Adds an action to the stack and executes it immediately.
        /// </summary>
        /// <typeparam name="T">The type of state data returned by the action.</typeparam>
        /// <param name="action">The action to execute, which returns state data for the revert operation.</param>
        /// <param name="revertAction">The action to execute when undoing, which takes the state data.</param>
        /// <returns>The result of the action.</returns>
        /*public void Add<T>(Func<T> action, Action<T> revertAction)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (revertAction == null)
            {
                throw new ArgumentNullException(nameof(revertAction));
            }

            // Execute the action and capture the state
            T state = action();

            // Add an entry to the stack for undoing later
            actionStack.Push(new ActionEntry<T>(revertAction, state));
        }*/

        /// <summary>
        /// Adds an action to the stack without a return value and executes it immediately.
        /// </summary>
        /// <typeparam name="T">The type of state data returned by the action.</typeparam>
        /// <param name="action">The action to execute, which returns state data for the revert operation.</param>
        /// <param name="revertAction">The action to execute when undoing, which takes the state data.</param>
        public void Add(Func<Action> actionWithRevert)
        {
            if (actionWithRevert == null)
            {
                throw new ArgumentNullException(nameof(actionWithRevert));
            }

            // Execute the action and capture the state
            var revert = actionWithRevert();

            // Add an entry to the stack for undoing later
            actionStack.Push(new ActionEntryBasic(revert));
        }

        /// <summary>
        /// Undoes the most recent action on the stack.
        /// </summary>
        /// <returns>True if an action was undone, false if the stack was empty.</returns>
        public bool Undo()
        {
            if (actionStack.Count == 0)
            {
                return false;
            }

            // Get the most recent action
            var actionEntry = actionStack.Pop();

            // Use reflection to call the Revert method on the action entry
            actionEntry.Revert();

            return true;
        }

        /// <summary>
        /// Undoes all actions on the stack.
        /// </summary>
        /// <returns>The number of actions undone.</returns>
        public int UndoAll()
        {
            int count = actionStack.Count;
            while (actionStack.Count > 0)
            {
                Undo();
            }
            return count;
        }

        /// <summary>
        /// Clears the action stack without undoing any actions.
        /// </summary>
        public void Clear()
        {
            actionStack.Clear();
        }
    }
}