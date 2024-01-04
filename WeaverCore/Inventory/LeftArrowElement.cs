using UnityEngine;
using UnityEngine.Events;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// The component representing the left arrow in an Inventory Pane
    /// </summary>
    public class LeftArrowElement : ArrowElement
    {
        [SerializeField]
        [Tooltip("The element to go to when on a right input")]
        InventoryElement OnRight;

        //public UnityEvent OnMovePaneL;

        public override ArrowState ArrowType => ArrowState.Left;

        public override InventoryElement NavigateTo(MoveDirection move)
        {
            if (move == MoveDirection.Right)
            {
                return OnRight;
            }
            else if (move == MoveDirection.Left)
            {
                //OnMovePaneL.Invoke();
                if (MainPanel != null && MainPanel.Navigator != null)
                {
                    MainPanel.Navigator.MovePaneLeft();
                }
            }
            return null;
        }

        public override bool Selectable => true;

        public override void OnClick()
        {
            //OnMovePaneL.Invoke();
            MainPanel.Navigator.MovePaneLeft();
        }
    }
}