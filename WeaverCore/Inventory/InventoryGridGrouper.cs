using UnityEngine;

namespace WeaverCore.Inventory
{
    [ExecuteAlways]
    public class InventoryGridGrouper : MonoBehaviour
    {
        [SerializeField]
        int maxRowSize = 4;

        [SerializeField]
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