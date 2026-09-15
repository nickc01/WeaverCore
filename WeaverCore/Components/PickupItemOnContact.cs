using UnityEngine;

namespace WeaverCore.Components
{
    public class PickupItemOnContact : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (TryGetComponent<DroppedItem>(out var item))
            {
                item.GiveItem();
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (TryGetComponent<DroppedItem>(out var item))
            {
                item.GiveItem();
            }
        }
    }
}
