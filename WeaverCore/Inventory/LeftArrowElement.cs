using UnityEngine;

namespace WeaverCore.Inventory
{
    public class LeftArrowElement : ArrowElement
    {
        [SerializeField]
        [Tooltip("The element to go to when on a right input")]
        InventoryElement OnRight;

        public override ArrowState ArrowType => ArrowState.Left;

        public override InventoryElement NavigateTo(MoveDirection move)
        {
            if (move == MoveDirection.Right)
            {
                return OnRight;
            }
            return null;
        }
    }
}