using System;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// A dropped item that gives an ItemChanger item when collected.
    /// </summary>
    public class ItemChangerDroppedItem : DroppedItem
    {
        [SerializeField]
        [Tooltip("The ItemChanger item name to give when this dropped item is collected.")]
        string itemName = "";

        [SerializeField]
        [Tooltip("The ItemChanger container context used when giving the item.")]
        string containerType = "Unknown";

        [SerializeField]
        [Tooltip("The ItemChanger message type used when giving the item.")]
        string messageType = "Any";

        [SerializeField]
        [Tooltip("The ItemChanger fling behavior used when giving the item.")]
        string flingType = "DirectDeposit";

        public string ItemName
        {
            get => itemName;
            set => itemName = value;
        }

        public string ContainerType
        {
            get => containerType;
            set => containerType = value;
        }

        public string MessageType
        {
            get => messageType;
            set => messageType = value;
        }

        public string FlingType
        {
            get => flingType;
            set => flingType = value;
        }

        protected override bool CanSpawn()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return base.CanSpawn();
            }
#endif
            if (!ItemChangerUtilities.IsAvailable)
            {
                return false;
            }

            return base.CanSpawn();
        }

        protected override void OnGiveItem()
        {
#if UNITY_EDITOR
            if (Application.isEditor && !ItemChangerUtilities.IsAvailable)
            {
                ItemGetMessage.Spawn(null, string.IsNullOrWhiteSpace(itemName) ? "ItemChanger Item" : itemName);
                return;
            }
#endif
            if (!ItemChangerUtilities.IsAvailable)
            {
                throw new InvalidOperationException("ItemChanger is not loaded, so this dropped item cannot give its configured item.");
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                throw new InvalidOperationException("No ItemChanger item name has been specified.");
            }

            if (!ItemChangerUtilities.TryGiveItem(itemName, transform, containerType, messageType, flingType))
            {
                throw new InvalidOperationException($"Failed to give ItemChanger item \"{itemName}\".");
            }
        }
    }
}
