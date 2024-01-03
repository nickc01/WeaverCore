using UnityEngine;

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
        int maxRowSize = 4;

        [SerializeField]
        [Tooltip("The amount of spacing between each inventory element")]
        Vector2 spacing = new Vector2(1, 1);

        private void LateUpdate()
        {
            int row = 0;
            int column = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localPosition = new Vector3(row * spacing.x,column * spacing.y);
                row++;
                if (row >= maxRowSize)
                {
                    row = 0;
                    column++;
                }
            }
        }
    }
}