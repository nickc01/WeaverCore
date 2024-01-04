using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// Represents the cursor in the inventory pane. The player uses the arrow keys to move the cursor around to select an <see cref="InventoryElement"/>
    /// </summary>
    public abstract class Cursor : MonoBehaviour
    {
        /// <summary>
        /// The default cursor prefab to use
        /// </summary>
        public static Cursor DefaultCursorPrefab => WeaverAssets.LoadWeaverAsset<GameObject>("Default Cursor").GetComponent<Cursor>();

        /// <summary>
        /// The pane this Cursor is a part of
        /// </summary>
        public InventoryPanel Panel { get; private set; }

        /// <summary>
        /// The element the cursor is currently highlighting
        /// </summary>
        public InventoryElement HighlightedElement { get; private set; }

        /// <summary>
        /// Called when the inventory pane is opened
        /// </summary>
        /// <param name="beginElement">The element the cursor should start on</param>
        protected abstract void OnBegin(InventoryElement beginElement);
        /// <summary>
        /// Called when the cursor is to become visible
        /// </summary>
        protected abstract void OnShow();

        /// <summary>
        /// Called when the cursor is to become invisible
        /// </summary>
        protected abstract void OnHide();

        /// <summary>
        /// Called when the cursor is to move to a new <see cref="InventoryElement"/>
        /// </summary>
        /// <param name="element">The new inventory element to move to</param>
        protected abstract void OnMoveTo(InventoryElement element);

        /// <summary>
        /// Called when the inventory pane is closed
        /// </summary>
        protected abstract void OnEnd();

        /// <summary>
        /// Can the cursor be moved to a new element?
        /// </summary>
        public virtual bool CanMove() => true;

        /// <summary>
        /// Makes the cursor visible
        /// </summary>
        public void Show()
        {
            OnShow();
        }

        /// <summary>
        /// Makes the cursor invisible
        /// </summary>
        public void Hide()
        {
            OnHide();
        }

        /// <summary>
        /// Starts up the cursor
        /// </summary>
        /// <param name="panel">The pane this cursor is a part of</param>
        /// <param name="beginElement">The element the cursor should start on</param>
        public void Begin(InventoryPanel panel, InventoryElement beginElement)
        {
            Panel = panel;
            HighlightedElement = beginElement;
            OnBegin(beginElement);
        }

        /// <summary>
        /// Ends the cursor
        /// </summary>
        public void End()
        {
            OnEnd();
            HighlightedElement = null;
            Panel = null;
        }

        /// <summary>
        /// Moves the cursor to a new element
        /// </summary>
        /// <param name="element">The new element the cursor should highlight</param>
        public void MoveTo(InventoryElement element)
        {
            if (HighlightedElement != element)
            {
                HighlightedElement = element;
                OnMoveTo(element);
            }
        }

        /// <summary>
        /// The current position of the cursor
        /// </summary>
        public abstract Vector3 CursorPosition { get; }
    }
}
