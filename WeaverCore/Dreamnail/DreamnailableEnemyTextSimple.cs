using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Dreamnail
{
    /// <summary>
    /// A simple version of <see cref="DreamnailableEnemyText"/>. Displays one of several possible enemy messages upon dreamnailing
    /// </summary>
    public class DreamnailableEnemyTextSimple : DreamnailableObject
    {
        [SerializeField]
        [Tooltip("A list of all the possible messages that could be displayed when dreamnailed")]
        System.Collections.Generic.List<string> possibleMessages = new System.Collections.Generic.List<string>();

        protected override int OnDreamnailHit(Player player)
        {
            if (!CanBeDreamnailed)
            {
                return 0;
            }

            DreamnailUtilities.DisplayEnemyDreamnailMessage(possibleMessages);

            return base.OnDreamnailHit(player);
        }
    }
}
