using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    /// <summary>
    /// Represents an item that, when dropped, gives a charm to the player.
    /// </summary>
    public class DroppedCharmItem : DroppedItem
    {
        /// <summary>
        /// The ID of the charm to give to the player when this item is picked up
        /// </summary>
        [Tooltip("The ID of the charm to give to the player when this item is picked up")]
        [SerializeField]
        int charmID = 0;

        /// <summary>
        /// Called when the item is picked up by the player.
        /// </summary>
        protected override void OnGiveItem()
        {
            CharmUtilities.GiveCharmToPlayer(charmID);
        }
    }
}