using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class DroppedCharmItem : DroppedItem
    {
        [SerializeField]
        int charmID = 0;

        protected override void OnGiveItem()
        {
            CharmUtilities.GiveCharmToPlayer(charmID);
        }
    }
}
