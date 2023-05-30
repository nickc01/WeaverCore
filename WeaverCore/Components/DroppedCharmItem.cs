using UnityEngine;

namespace WeaverCore.Components
{

    public class DroppedCharmItem : DroppedItem
    {
        [SerializeField]
        int charmID = 0;


        public override void GiveItem()
        {
            GiveCharm(charmID);
        }
    }
}
