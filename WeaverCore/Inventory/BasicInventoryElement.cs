﻿using UnityEngine;
using UnityEngine.Events;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// A basic implementation of an inventory element
    /// </summary>
    public class BasicInventoryElement : InventoryElement
    {
        [SerializeField]
        [Tooltip("Can this Inventory Element be highlighted by the cursor?")]
        bool highlightable = true;

        [SerializeField]
        [Tooltip("When highlighted, is the player able to click on this element like a button?")]
        bool selectable = true;

        [SerializeField]
        [Tooltip("The element to navigate to on left input")]
        InventoryElement OnLeft;

        [SerializeField]
        [Tooltip("The element to navigate to on right input")]
        InventoryElement OnRight;

        [SerializeField]
        [Tooltip("The element to navigate to on up input")]
        InventoryElement OnUp;

        [SerializeField]
        [Tooltip("The element to navigate to on down input")]
        InventoryElement OnDown;

        [SerializeField]
        [Tooltip("Called when the element is clicked on")]
        UnityEvent onClick;

        /// <inheritdoc/>
        public override InventoryElement NavigateTo(MoveDirection move)
        {
            switch (move)
            {
                case MoveDirection.Up:
                    return OnUp;
                case MoveDirection.Down:
                    return OnDown;
                case MoveDirection.Left:
                    return OnLeft;
                case MoveDirection.Right:
                    return OnRight;
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public override bool Highlightable => highlightable;

        /// <inheritdoc/>
        public override bool Selectable => selectable;

        /// <inheritdoc/>
        public override void OnClick()
        {
            onClick?.Invoke();
        }
    }
}