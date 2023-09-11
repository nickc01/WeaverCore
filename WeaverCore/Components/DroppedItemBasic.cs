using UnityEngine;
using WeaverCore.Assets.Components;

namespace WeaverCore.Components
{
    public class DroppedItemBasic : DroppedItem
    {
        [field: SerializeField]
        Sprite ItemSprite { get; set; }

        [field: SerializeField]
        string ItemName { get; set; }

        protected override void OnGiveItem()
        {
            ItemGetMessage.Spawn(ItemSprite, ItemName);
        }
    }
}
