using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Dreamnail
{
    /// <summary>
    /// Displays dreamnail text when dreamnailing an object (enemy variant)
    /// </summary>
    public class DreamnailableEnemyText : DreamnailableObject
    {
        [SerializeField]
        private int convoAmount;

        [SerializeField]
        private string convoTitle;

        protected override int OnDreamnailHit(Player player)
        {
            if (!CanBeDreamnailed)
            {
                return 0;
            }

            DreamnailUtilities.DisplayEnemyDreamnailMessage(convoAmount, convoTitle);

            return base.OnDreamnailHit(player);
        }

        private void Reset()
        {
            convoAmount = 8;
            convoTitle = "GENERIC";
        }
    }
}
