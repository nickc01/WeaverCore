using UnityEngine;
using WeaverCore.Features;

namespace WeaverCore.Components
{
    /// <summary>
    /// A basic dropped inventory item.
    /// </summary>
    public class DroppedInventoryItemBasic : DroppedInventoryItem
    {
        [SerializeField]
        [Tooltip("The inventory item to update when collecting this dropped item.")]
        protected InventoryItem inventoryItem;

        [SerializeField]
        [Tooltip("The state to apply to the inventory item when collected.")]
        protected CollectionState stateToApply = CollectionState.Primary;

        [SerializeField]
        [Tooltip("How much to add when collecting a counted item.")]
        protected int countedAmount = 1;

        [SerializeField]
        [Tooltip("If true, Primary and Secondary collection states clear the other state.")]
        protected bool exclusivePrimarySecondaryStates = true;

        [SerializeField]
        [Tooltip("If true, shows an item get message after updating the inventory item.")]
        protected bool displayItemGetMessage = true;

        [SerializeField]
        [Tooltip("Optional message sprite override. Leave empty to use the inventory item's sprite for the applied state.")]
        protected Sprite messageSpriteOverride;

        [SerializeField]
        [Tooltip("Optional message text override. Leave empty to use the inventory item's name for the applied state.")]
        protected string messageTextOverride;

        public override InventoryItem InventoryItem => inventoryItem;

        protected override void OnCollect(InventoryItem item)
        {
            ApplyCollectionState(item, stateToApply, countedAmount, exclusivePrimarySecondaryStates);

            if (displayItemGetMessage)
            {
                ShowItemGetMessage(item, stateToApply, messageSpriteOverride, messageTextOverride);
            }
        }
    }
}
