using System;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Features;

namespace WeaverCore.Components
{
    /// <summary>
    /// Base class for dropped items that grant or update a WeaverCore inventory item.
    /// </summary>
    public abstract class DroppedInventoryItem : DroppedItem
    {
        public enum CollectionState
        {
            /// <summary>
            /// Sets the primary acquired state. For counted items, increments the count.
            /// </summary>
            Primary,

            /// <summary>
            /// Sets the secondary acquired state. Intended for OneTwo and OneTwoBoth items.
            /// </summary>
            Secondary,

            /// <summary>
            /// Sets both primary and secondary acquired states. Intended for OneTwoBoth items.
            /// </summary>
            Both,

            /// <summary>
            /// Clears primary and secondary acquired states. For counted items, sets count to zero.
            /// </summary>
            None
        }

        /// <summary>
        /// The inventory item to update when collected.
        /// </summary>
        public abstract InventoryItem InventoryItem { get; }

        /// <summary>
        /// Called when the player collects this dropped item.
        /// </summary>
        protected abstract void OnCollect(InventoryItem item);

        protected override void OnGiveItem()
        {
            var item = InventoryItem;
            if (item == null)
            {
                throw new Exception("No inventory item has been specified");
            }

            OnCollect(item);
        }

        /// <summary>
        /// Applies one of the common inventory item collection states.
        /// </summary>
        protected void ApplyCollectionState(InventoryItem item, CollectionState state, int countedAmount = 1, bool exclusivePrimarySecondaryStates = true)
        {
            if (item.Type == InventoryItem.ItemType.Counted)
            {
                ApplyCountedState(item, state, countedAmount);
                return;
            }

            switch (state)
            {
                case CollectionState.Primary:
                    ApplyPrimaryState(item, exclusivePrimarySecondaryStates);
                    break;
                case CollectionState.Secondary:
                    ApplySecondaryState(item, exclusivePrimarySecondaryStates);
                    break;
                case CollectionState.Both:
                    ApplyBothState(item);
                    break;
                case CollectionState.None:
                    ClearCollectionState(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        protected void ApplyPrimaryState(InventoryItem item, bool clearSecondaryState = true)
        {
            item.Acquired = true;
            if (clearSecondaryState)
            {
                item.SecondAcquired = false;
            }
        }

        protected void ApplySecondaryState(InventoryItem item, bool clearPrimaryState = true)
        {
            if (clearPrimaryState)
            {
                item.Acquired = false;
            }
            item.SecondAcquired = true;
        }

        protected void ApplyBothState(InventoryItem item)
        {
            item.Acquired = true;
            item.SecondAcquired = true;
        }

        protected void ClearCollectionState(InventoryItem item)
        {
            item.Acquired = false;
            item.SecondAcquired = false;
            if (item.Type == InventoryItem.ItemType.Counted)
            {
                item.Count = 0;
            }
        }

        protected void AddCount(InventoryItem item, int amount = 1)
        {
            item.Count += amount;
        }

        protected void SetCount(InventoryItem item, int amount)
        {
            item.Count = amount;
        }

        protected void ShowItemGetMessage(InventoryItem item, CollectionState state, Sprite spriteOverride = null, string textOverride = null)
        {
            ItemGetMessage.Spawn(GetMessageSprite(item, state, spriteOverride), GetMessageText(item, state, textOverride));
        }

        protected Sprite GetMessageSprite(InventoryItem item, CollectionState state, Sprite spriteOverride = null)
        {
            if (spriteOverride != null)
            {
                return spriteOverride;
            }

            switch (state)
            {
                case CollectionState.Secondary:
                    return item.AlternateSprite;
                case CollectionState.Both:
                    return item.BothSprite;
                default:
                    return item.Sprite;
            }
        }

        protected string GetMessageText(InventoryItem item, CollectionState state, string textOverride = null)
        {
            if (!string.IsNullOrEmpty(textOverride))
            {
                return textOverride;
            }

            switch (state)
            {
                case CollectionState.Secondary:
                    return item.AlternateName;
                case CollectionState.Both:
                    return item.BothName;
                default:
                    return item.Name;
            }
        }

        void ApplyCountedState(InventoryItem item, CollectionState state, int countedAmount)
        {
            switch (state)
            {
                case CollectionState.Primary:
                case CollectionState.Secondary:
                case CollectionState.Both:
                    AddCount(item, countedAmount);
                    break;
                case CollectionState.None:
                    SetCount(item, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
