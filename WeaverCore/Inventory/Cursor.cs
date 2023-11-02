using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore.Inventory
{
    public abstract class Cursor : MonoBehaviour
    {
        public static Cursor DefaultCursorPrefab => WeaverAssets.LoadWeaverAsset<GameObject>("Default Cursor").GetComponent<Cursor>();

        public InventoryPanel Panel { get; private set; }
        public InventoryElement HighlightedElement { get; private set; }

        protected abstract void OnBegin(InventoryElement beginElement);
        protected abstract void OnShow();
        protected abstract void OnHide();
        protected abstract void OnMoveTo(InventoryElement element);
        protected abstract void OnEnd();

        public virtual bool CanMove() => true;

        public void Show()
        {
            OnShow();
        }

        public void Hide()
        {
            OnHide();
        }

        public void Begin(InventoryPanel panel, InventoryElement beginElement)
        {
            Panel = panel;
            HighlightedElement = beginElement;
            OnBegin(beginElement);
        }

        public void End()
        {
            OnEnd();
            HighlightedElement = null;
            Panel = null;
        }

        public void MoveTo(InventoryElement element)
        {
            if (HighlightedElement != element)
            {
                HighlightedElement = element;
                OnMoveTo(element);
            }
        }

        public abstract Vector3 CursorPosition { get; }
    }
}
