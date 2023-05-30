using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class DroppedCustomCharmItem : DroppedItem
    {
        [SerializeField]
        WeaverCharm charm;

        public override void GiveItem()
        {
            if (charm == null)
            {
                throw new System.Exception("No charm has been specified");
            }

            GiveCharm(CharmUtilities.GetCustomCharmID(charm));
        }
    }
}
