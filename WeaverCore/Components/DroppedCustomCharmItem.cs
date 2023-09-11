﻿using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class DroppedCustomCharmItem : DroppedItem
    {
        [SerializeField]
        [Tooltip("The charm to give to the player when collecting this item")]
        WeaverCharm charm;

        protected override void OnGiveItem()
        {
            if (charm == null)
            {
                throw new System.Exception("No charm has been specified");
            }

            CharmUtilities.GiveCharmToPlayer(charm);
        }
    }
}
