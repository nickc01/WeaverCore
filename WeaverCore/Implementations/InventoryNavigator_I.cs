using System;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Inventory;

namespace WeaverCore.Implementations
{
    public abstract class InventoryNavigator_I : MonoBehaviour, IImplementation
    {
        /// <summary>
        /// Called when the inventory is opened
        /// </summary>
        public static UnityEvent InventoryOpenEvent = new UnityEvent();

        /// <summary>
        /// Called when the inventory is closed
        /// </summary>
        public static UnityEvent InventoryCloseEvent = new UnityEvent();

        /// <summary>
        /// Called when a Pane begins to open. This is before it has faded in
        /// </summary>
        public static UnityEvent<string> PaneOpenBeginEvent = new UnityEvent<string>();

        /// <summary>
        /// Called when a Pane is fully opened. This is after it has faded in
        /// </summary>
        public static UnityEvent<string> PaneOpenEndEvent = new UnityEvent<string>();

        /// <summary>
        /// Called when a Pane begins to close. This is before it has faded out
        /// </summary>
        public static UnityEvent<string> PaneCloseBeginEvent = new UnityEvent<string>();

        /// <summary>
        /// Caleld when a Pane is fully closed. This is after it has faded out
        /// </summary>
        public static UnityEvent<string> PaneCloseEndEvent = new UnityEvent<string>();

        public UnityEvent OnInventoryOpen;
        public UnityEvent OnInventoryClose;
        public UnityEvent OnPaneOpenBegin;
        public UnityEvent OnPaneOpenEnd;
        public UnityEvent OnPaneCloseBegin;
        public UnityEvent OnPaneCloseEnd;

        [NonSerialized]
        InventoryInputManager _inputManager;
        public InventoryInputManager InputManager => _inputManager ??= GetComponent<InventoryInputManager>();


        [NonSerialized]
        InventoryPanel _panel;
        public InventoryPanel MainPanel => _panel ??= GetComponent<InventoryPanel>();

        /// <summary>
        /// The main fade group that is used to fade in and out the inventory panel
        /// </summary>
        public abstract FadeGroup MainFadeGroup { get; }

        public abstract Vector3 GetCursorPosition();

        public abstract void InitPanel(InventoryPanel panel);

        /// <summary>
        /// Can the inventory be closed? Note: This is ignored if the player takes damage or gets stun locked by a boss
        /// </summary>
        public abstract bool CanCloseInventory { get; set; }

        public abstract void HighlightElement(InventoryElement element);

        public abstract InventoryElement HighlightedElement { get; }

        public abstract void SetStartupElement(InventoryElement element);

        public abstract Vector3 GetCursorPosForElement(InventoryElement element);
        public abstract Vector2 GetCursorBoundsForElement(InventoryElement element);
        public abstract Vector2 GetCursorOffsetForElement(InventoryElement element);

        public bool EnableCursorMovement { get; set; } = true;

        public InventoryElement FindNextElement(InventoryElement element, InventoryElement.MoveDirection direction)
        {
            if (!EnableCursorMovement)
            {
                return element;
            }
            var currentElement = element;
            for (int i = 0; i < 20; i++)
            {
                var newElement = currentElement.NavigateTo(direction);

                if (newElement == null)
                {
                    return element;
                }

                if (newElement.Highlightable)
                {
                    return newElement;
                }
                else
                {
                    currentElement = newElement;
                }
            }
            return element;
        }

        public abstract void ShowCursor();
        public abstract void HideCursor();

        public abstract void MovePaneLeft();
        public abstract void MovePaneRight();
    }
}
