using UnityEngine;
using UnityEngine.Events;

namespace WeaverCore.Inventory
{
    public class RightArrowElement : ArrowElement
    {
        [SerializeField]
        [Tooltip("The element to go to when on a left input")]
        InventoryElement OnLeft;

        //public UnityEvent OnMovePaneR;

        public override ArrowState ArrowType => ArrowState.Right;

        public override InventoryElement NavigateTo(MoveDirection move)
        {
            if (move == MoveDirection.Left)
            {
                return OnLeft;
            }
            else if (move == MoveDirection.Right)
            {
                //OnMovePaneR.Invoke();
                //MainPanel.Navigator.MovePaneRight();
                if (MainPanel != null && MainPanel.Navigator != null)
                {
                    MainPanel.Navigator.MovePaneRight();
                }
            }
            return null;
        }

        public override void OnHighlight()
        {
            //MainPanel.Navigator.UpdateHighlightedObject(MainPanel.RightArrow.gameObject);
        }

        public override bool Selectable => true;

        public override void OnClick()
        {
            //OnMovePaneR.Invoke();
            MainPanel.Navigator.MovePaneRight();
        }
    }
}