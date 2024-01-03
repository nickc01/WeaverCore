using UnityEngine;
using WeaverCore.Assets.Components;

namespace WeaverCore.Components
{
    /// <summary>
    /// Represents a basic dropped item in the game.
    /// </summary>
    public class DroppedItemBasic : DroppedItem
    {
        /// <summary>
        /// Gets or sets the sprite of the item.
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("The sprite of the item.")]
        Sprite ItemSprite { get; set; }

        /// <summary>
        /// The name of the item.
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("The name of the item.")]
        string ItemName { get; set; }

        /// <summary>
        /// Called when the item is picked up by the player.
        /// </summary>
        protected override void OnGiveItem()
        {
            // Spawn the item get message with the specified sprite and name.
            ItemGetMessage.Spawn(ItemSprite, ItemName);
        }
    }
}