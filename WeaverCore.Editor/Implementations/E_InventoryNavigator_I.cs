using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Inventory;

namespace WeaverCore.Editor.Implementations
{
    public class E_InventoryNavigator_I : InventoryNavigator_I
    {
        //InventoryPanel loadedPanel;

        public override bool CanCloseInventory { get; set; }

        //public override GameObject LeftArrow => loadedPanel.LeftArrow.gameObject;

        //public override GameObject RightArrow => loadedPanel.RightArrow.gameObject;

        public override InventoryElement HighlightedElement => null;

        public override Vector2 GetCursorBoundsForElement(InventoryElement element)
        {
            return element.CursorSize;
        }

        public override Vector2 GetCursorOffsetForElement(InventoryElement element)
        {
            return element.CursorOffset;
        }

        public override Vector3 GetCursorPosForElement(InventoryElement element)
        {
            return element.CursorPos;
        }

        public override Vector3 GetCursorPosition()
        {
            return Vector3.zero;
        }

        /*public override GameObject GetHighlightedObject()
        {
            return null;
        }*/

        public override void HighlightElement(InventoryElement element)
        {
            
        }

        /*public override void HighlightLeftArrow(InventoryElement arrowRepresentation)
        {
            
        }

        public override void HighlightRightArrow(InventoryElement arrowRepresentation)
        {
            
        }*/

        public override void InitPanel(InventoryPanel panel)
        {
            //loadedPanel = panel;
        }

        public override void SetStartupElement(InventoryElement element)
        {
            
        }

        /*public override void UpdateHighlightedObject(GameObject obj)
        {
            
        }*/
    }
}
