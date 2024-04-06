using UnityEngine;

namespace WeaverCore.Elevator
{
    public class MoveWithElevator : MonoBehaviour
    {
        [field: SerializeField]
        [field: Tooltip("The elevator to move along with")]
        public Elevator Elevator { get; private set; }

        [Tooltip("The position this object should be at when the elevator is at its top position")]
        public Vector3 PositionWhenAtTop;

        [Tooltip("The position this object should be at when the elevator is at its bottom position")]
        public Vector3 PositionWhenAtBottom;

        private void LateUpdate()
        {
            var amountAlongLine = ProjectOntoLineBetweenPoints(Elevator.DefaultBottomPosition, Elevator.DefaultTopPosition, Elevator.transform.position);

            transform.position = Vector3.LerpUnclamped(PositionWhenAtBottom, PositionWhenAtTop, amountAlongLine);
        }

        float ProjectOntoLineBetweenPoints(Vector3 start, Vector3 end, Vector3 pointToProject)
        {
            if (pointToProject == end)
            {
                return 0;
            }
            else if (pointToProject == start)
            {
                return 1;
            }

            var amountAlongLine = Vector3.Dot((end - start).normalized, pointToProject - start);

            return amountAlongLine / (end - start).magnitude;
        }
    }
}