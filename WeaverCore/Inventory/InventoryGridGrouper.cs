using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;
using static UnityEngine.UI.ScrollRect;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// Used to group many <see cref="InventoryElement"/> objects into a grid. When attached to a parent object, all child objects will be arranged into a grid
    /// </summary>
    [ExecuteAlways]
    public class InventoryGridGrouper : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("How many inventory elements can be placed in a row before starting a new column")]
        int maxLengthSize = 4;

        [SerializeField]
        bool vertical = false;

        [SerializeField]
        [Tooltip("The amount of spacing between each inventory element")]
        Vector2 spacing = new Vector2(1, 1);

        [field: SerializeField]
        AnimationCurve MovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public int MaxLengthSize { get => maxLengthSize; set => maxLengthSize = value; }

        public bool Vertical { get => vertical; set => vertical = value; }

        public Vector2 Spacing { get => spacing; set => spacing = value; }

        public Vector3 DefaultPosition { get; private set; }

        public Vector3 Destination
        {
            get
            {
                if (movementRoutine != null)
                {
                    return movementDestination;
                }
                else
                {
                    return transform.localPosition;
                }
            }
        }

        Vector3 movementDestination;
        Coroutine movementRoutine;

        private void Awake()
        {
            DefaultPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            int row = 0;
            int column = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (vertical)
                {
                    transform.GetChild(i).localPosition = new Vector3(column * spacing.x, row * spacing.y);
                }
                else
                {
                    transform.GetChild(i).localPosition = new Vector3(row * spacing.x, column * spacing.y);
                }
                row++;
                if (row >= maxLengthSize)
                {
                    row = 0;
                    column++;
                }
            }
        }

        IEnumerator MovementRoutine(Vector3 start, Vector3 end, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(start, end, MovementCurve.Evaluate(t / time));
                yield return null;
            }
            transform.localPosition = end;
        }

        public void MoveToInstant(Vector3 destination)
        {
            if (movementRoutine != null)
            {
                StopCoroutine(movementRoutine);
                movementRoutine = null;
            }
            transform.localPosition = destination;
        }

        public void MoveToInstant(Vector2 destination) => MoveToInstant(((Vector3)destination).With(z: transform.localPosition.z));

        public void MoveTo(Vector3 destination, float time)
        {
            if (movementRoutine != null)
            {
                StopCoroutine(movementRoutine);
                movementRoutine = null;
            }

            movementDestination = destination;
            WeaverLog.Log("START POS = " + transform.localPosition);
            WeaverLog.Log("END POS = " + destination);
            movementRoutine = StartCoroutine(MovementRoutine(transform.localPosition, destination, time));
        }

        public void MoveTo(Vector2 destination, float time) => MoveTo(((Vector3)destination).With(z: transform.localPosition.z), time);
    }
}