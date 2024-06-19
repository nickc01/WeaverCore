using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Represents an item that, when dropped, gives a WeaverCharm to the player.
    /// </summary>
    public class DroppedCustomCharmItem : DroppedItem
    {
        [SerializeField]
        [Tooltip("The charm to give to the player when collecting this item")]
        protected WeaverCharm charm;

        protected override void OnGiveItem()
        {
            //WeaverLog.Log("CHARM = " + charm);
            if (charm == null)
            {
                throw new System.Exception("No charm has been specified");
            }

            CharmUtilities.GiveCharmToPlayer(charm);
        }
    }
}
