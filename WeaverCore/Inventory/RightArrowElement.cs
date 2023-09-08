using UnityEngine;

namespace WeaverCore.Inventory
{
    public class RightArrowElement : ArrowElement
    {
        [SerializeField]
        [Tooltip("The element to go to when on a left input")]
        InventoryElement OnLeft;

        public override ArrowState ArrowType => ArrowState.Right;

        public override InventoryElement NavigateTo(MoveDirection move)
        {
            if (move == MoveDirection.Left)
            {
                return OnLeft;
            }
            return null;
        }

        public override void OnHighlight()
        {
            //MainPanel.Navigator.UpdateHighlightedObject(MainPanel.RightArrow.gameObject);
        }
    }
}