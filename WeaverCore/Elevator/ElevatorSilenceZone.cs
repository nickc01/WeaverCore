using UnityEngine;

namespace WeaverCore.Elevator
{
    public class ElevatorSilenceZone : MonoBehaviour
    {
        private void Reset()
        {
            if (gameObject.layer == 0)
            {
                gameObject.layer = 14;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<Elevator>(out var elevator))
            {
                elevator.SilentMode = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<Elevator>(out var elevator))
            {
                elevator.SilentMode = false;
            }
        }
    }
}