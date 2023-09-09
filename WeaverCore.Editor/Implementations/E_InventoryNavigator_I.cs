using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Inventory;

namespace WeaverCore.Editor.Implementations
{
    public class E_InventoryNavigator_I : InventoryNavigator_I
    {
        EditorCursor cursor;

        FadeGroup fadeGroup;

        //InventoryPanel loadedPanel;

        public override bool CanCloseInventory { get; set; }

        //public override GameObject LeftArrow => loadedPanel.LeftArrow.gameObject;

        //public override GameObject RightArrow => loadedPanel.RightArrow.gameObject;

        InventoryElement startupElement = null;

        InventoryElement highlightedElement = null;


        public override InventoryElement HighlightedElement => highlightedElement;

        public override FadeGroup MainFadeGroup
        {
            get
            {
                if (fadeGroup == null)
                {
                    fadeGroup = gameObject.AddComponent<FadeGroup>();
                }
                return fadeGroup;
            }
        }

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
            return cursor.CursorDestination;
        }

        /*public override GameObject GetHighlightedObject()
        {
            return null;
        }*/

        public override void HighlightElement(InventoryElement element)
        {
            if (highlightedElement == element)
            {
                return;
            }

            var prevHighlightedElement = highlightedElement;
            highlightedElement = element;

            if (prevHighlightedElement != null)
            {
                prevHighlightedElement.OnUnHighlight();
            }

            highlightedElement.OnHighlight();

            cursor.MoveToPosition(GetCursorPosForElement(highlightedElement), GetCursorBoundsForElement(highlightedElement), GetCursorOffsetForElement(highlightedElement));
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
            if (cursor == null)
            {
                var cursorPrefab = EditorAssets.LoadEditorAsset<GameObject>("Editor Cursor");
                cursor = GameObject.Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity).GetComponent<EditorCursor>();
            }

            if (MainFadeGroup.spriteRenderers == null)
            {
                MainFadeGroup.spriteRenderers = new SpriteRenderer[0];
            }

            if (MainFadeGroup.texts == null)
            {
                MainFadeGroup.texts = new TextMeshPro[0];
            }

            List<SpriteRenderer> existingSprites = MainFadeGroup.spriteRenderers.ToList();
            existingSprites.AddRange(gameObject.GetComponentsInChildren<SpriteRenderer>());
            MainFadeGroup.spriteRenderers = existingSprites.ToArray();

            var existingTexts = MainFadeGroup.texts.ToList();
            existingTexts.AddRange(gameObject.GetComponentsInChildren<TextMeshPro>());
            MainFadeGroup.texts = existingTexts.ToArray();
            StartCoroutine(FadeIn());

            InputManager.OnLeftEvent += InputManager_OnLeftEvent;
            InputManager.OnRightEvent += InputManager_OnRightEvent;
            InputManager.OnUpEvent += InputManager_OnUpEvent;
            InputManager.OnDownEvent += InputManager_OnDownEvent;
            InputManager.OnSelectEvent += InputManager_OnSelectEvent;
        }

        public override void SetStartupElement(InventoryElement element)
        {
            startupElement = element;
        }

        IEnumerator FadeIn()
        {
            InvOpened();
            Internal_OnPaneOpenBegin();
            MainFadeGroup.FadeUp();
            yield return new WaitForSeconds(MainFadeGroup.fadeInTime);
            if (startupElement == null)
            {
                startupElement = MainPanel.RightArrow;
            }
            HighlightElement(startupElement);
            Internal_OnPaneOpenEnd();
            //var renderables = GetComponentsInChildren<Renderer>();
            //yield break;
        }

        static void InvOpened()
        {
            //WeaverLog.Log("INV OPENED");
            InventoryOpenEvent?.Invoke();
        }

        static void InvClosed()
        {
            //WeaverLog.Log("INV CLOSED");
            InventoryCloseEvent?.Invoke();
        }

        private void InputManager_OnSelectEvent()
        {
            if (highlightedElement != null && highlightedElement.Selectable)
            {
                highlightedElement.OnClick();
            }
        }

        private void InputManager_OnDownEvent()
        {
            HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Down));
        }

        private void InputManager_OnUpEvent()
        {
            HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Up));
        }

        private void InputManager_OnRightEvent()
        {
            HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Right));
        }

        private void InputManager_OnLeftEvent()
        {
            HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Left));
        }

        /*static void PaneOpenBegin(GameObject pane)
        {
            //WeaverLog.Log("PANE OPEN BEGIN = " + pane.gameObject.name);
            var nav = pane.GetComponentInChildren<E_InventoryNavigator_I>();
            if (nav != null)
            {
                nav.Internal_OnPaneOpenBegin();
            }
            PaneOpenBeginEvent?.Invoke(pane.gameObject.name);
        }

        static void PaneOpenEnd(GameObject pane)
        {
            //WeaverLog.Log("PANE OPEN END = " + pane.gameObject.name);
            var nav = pane.GetComponentInChildren<E_InventoryNavigator_I>();
            if (nav != null)
            {
                nav.Internal_OnPaneOpenEnd();
            }
            PaneOpenEndEvent?.Invoke(pane.gameObject.name);
        }

        static void PaneCloseBegin(GameObject pane)
        {
            //WeaverLog.Log("PANE CLOSE BEGIN = " + pane.gameObject.name);
            //WeaverLog.Log(new System.Diagnostics.StackTrace());
            var nav = pane.GetComponentInChildren<E_InventoryNavigator_I>();
            if (nav != null)
            {
                nav.Internal_OnPaneCloseBegin();
            }
            PaneCloseBeginEvent?.Invoke(pane.gameObject.name);
        }

        static void PaneCloseEnd(GameObject pane)
        {
            //WeaverLog.Log("PANE CLOSE END = " + pane.gameObject.name);
            //WeaverLog.Log(new System.Diagnostics.StackTrace());
            var nav = pane.GetComponentInChildren<E_InventoryNavigator_I>();
            if (nav != null)
            {
                nav.Internal_OnPaneCloseEnd();
            }
            PaneCloseEndEvent?.Invoke(pane.gameObject.name);
        }*/

        void Internal_OnPaneOpenBegin()
        {
            PaneOpenBeginEvent?.Invoke($"{MainPanel.GetType().FullName}_{MainPanel.PanelGUID}");
        }

        void Internal_OnPaneOpenEnd()
        {
            PaneOpenEndEvent?.Invoke($"{MainPanel.GetType().FullName}_{MainPanel.PanelGUID}");
        }

        void Internal_OnPaneCloseBegin()
        {
            PaneCloseBeginEvent?.Invoke($"{MainPanel.GetType().FullName}_{MainPanel.PanelGUID}");
        }

        void Internal_OnPaneCloseEnd()
        {
            highlightedElement = null;
            PaneCloseEndEvent?.Invoke($"{MainPanel.GetType().FullName}_{MainPanel.PanelGUID}");
        }

        /*public override void UpdateHighlightedObject(GameObject obj)
        {
            
        }*/
    }
}
