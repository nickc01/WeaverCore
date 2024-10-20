﻿using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Dreamnail
{
    /// <summary>
    /// A simple version of <see cref="DreamnailableRegularText"/>. Displays one of several possible messages upon dreamnailing
    /// </summary>
    public class DreamnailableRegularTextSimple : DreamnailableObject
    {
        [SerializeField]
        [Tooltip("A list of all the possible messages that could be displayed when dreamnailed")]
        System.Collections.Generic.List<string> possibleMessages = new System.Collections.Generic.List<string>();

        EventManager eventManager;

        SpriteFlasher flasher;

        protected virtual void Awake()
        {
            flasher = gameObject.GetComponentInParent<SpriteFlasher>();

            if (!gameObject.TryGetComponent(out eventManager))
            {
                eventManager = gameObject.AddComponent<EventManager>();
            }

            eventManager.AddReceiverForEvent("CONVO CANCEL", OnConvoCancel);
        }

        protected override int OnDreamnailHit(Player player)
        {
            if (!CanBeDreamnailed)
            {
                return 0;
            }

            DreamnailUtilities.DisplayRegularDreamnailMessage(possibleMessages);
            DreamnailUtilities.PlayDreamnailEffects(transform.position);

            flasher.flashDreamImpact();

            return base.OnDreamnailHit(player);
        }

        protected virtual void OnConvoCancel()
        {
            DreamnailUtilities.CancelRegularDreamnailMessage();
        }
    }
}
