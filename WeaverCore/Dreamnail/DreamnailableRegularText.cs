using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Dreamnail
{
    /// <summary>
    /// Displays dreamnail text when dreamnailing an object
    /// </summary>
    public class DreamnailableRegularText : DreamnailableObject
    {
        [SerializeField]
        private string convoTitle;

        [SerializeField]
        private string convoSheetName;

        EventManager eventManager;

        protected virtual void Awake()
        {
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

            DreamnailUtilities.DisplayRegularDreamnailMessage(convoTitle, convoSheetName);

            DreamnailUtilities.PlayDreamnailEffects(transform.position);

            return base.OnDreamnailHit(player);
        }

        protected virtual void OnConvoCancel()
        {
            DreamnailUtilities.CancelRegularDreamnailMessage();
        }

        protected override void Reset()
        {
            convoTitle = "GENERIC";
            base.Reset();
        }
    }
}
