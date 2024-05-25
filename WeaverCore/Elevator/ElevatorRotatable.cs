using UnityEngine;

namespace WeaverCore.Elevator
{
    public class ElevatorRotatable : MonoBehaviour
    {
        [field: SerializeField]
        public float RotationIntensity { get; set; } = 500f;

        /// <summary>
        /// Used to rotate an object while the elevator is moving. Called every frame the elevator is moving
        /// </summary>
        /// <param name="elevator">The elevator instance</param>
        /// <param name="direction">The direction the elevator is traveling in</param>
        /// <param name="speed">The speed the elevator is traveling in</param>
        public virtual void Rotate(Elevator elevator, Vector3 direction, float speed)
        {
            if (direction.y >= 0f)
            {
                transform.localRotation *= Quaternion.Euler(0f, 0f, RotationIntensity * Time.deltaTime);
            }
            else
            {
                transform.localRotation *= Quaternion.Euler(0f, 0f, RotationIntensity * Time.deltaTime);
            }
        }
    }
}