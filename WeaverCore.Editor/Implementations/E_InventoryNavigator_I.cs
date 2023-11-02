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
        //EditorCursor cursor;
        WeaverCore.Inventory.Cursor cursor;

        FadeGroup fadeGroup;

        bool visible = false;

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
            if (cursor.transform.parent != null)
            {
                return cursor.transform.parent.InverseTransformPoint(element.CursorPos);
            }
            else
            {
                return element.CursorPos;
            }

        }

        public override Vector3 GetCursorPosition()
        {
            //return cursor.CursorDestination;
            return cursor.CursorPosition;
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
            else
            {
                cursor.Begin(MainPanel, element);
                cursor.Show();
            }

            highlightedElement.OnHighlight();

            cursor.MoveTo(highlightedElement);
            //cursor.MoveToPosition(GetCursorPosForElement(highlightedElement), GetCursorBoundsForElement(highlightedElement), GetCursorOffsetForElement(highlightedElement));
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
                var cursorPrefab = panel.CustomCursor;

                if (cursorPrefab == null)
                {
                    cursorPrefab = WeaverCore.Inventory.Cursor.DefaultCursorPrefab;
                }
                //var cursorPrefab = EditorAssets.LoadEditorAsset<GameObject>("Editor Cursor");
                //cursor = GameObject.Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity).GetComponent<EditorCursor>();
                cursor = GameObject.Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity);
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
            InputManager.OnCancelEvent += InputManager_OnCancelEvent;
        }

        private IEnumerator Start()
        {
            yield return null;
            foreach (var camera in GameObject.FindObjectsOfType<Camera>())
            {
                if (camera.name == "tk2dCamera")
                {
                    camera.orthographic = true;
                }
            }
            /*var camera = Camera.main;

            if (camera != null)
            {
                camera.orthographic = true;
            }*/
        }

        public override void SetStartupElement(InventoryElement element)
        {
            startupElement = element;
        }

        IEnumerator FadeIn()
        {
            yield return null;
            var camera = GameObject.FindObjectsOfType<Camera>().FirstOrDefault(c => c.name == "tk2dCamera");
            if (camera != null)
            {
                camera.fieldOfView = 34.6f;
                camera.cullingMask = camera.cullingMask | LayerMask.GetMask("UI");
            }
            InvOpened();
            Internal_OnPaneOpenBegin();
            MainFadeGroup.FadeUp();
            yield return new WaitForSeconds(MainFadeGroup.fadeInTime);
            if (startupElement == null)
            {
                startupElement = MainPanel.RightArrow;
            }
            HighlightElement(startupElement);
            visible = true;
            Internal_OnPaneOpenEnd();
            //var renderables = GetComponentsInChildren<Renderer>();
            //yield break;
        }

        IEnumerator FadeOut()
        {
            InvClosed();
            Internal_OnPaneCloseBegin();
            MainFadeGroup.FadeDown();
            cursor.Hide();
            //cursor.gameObject.SetActive(false);
            yield return new WaitForSeconds(MainFadeGroup.fadeOutTime);
            cursor.End();
            GameObject.Destroy(cursor.gameObject);
            Internal_OnPaneCloseEnd();
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
            if (visible && highlightedElement != null && highlightedElement.Selectable)
            {
                highlightedElement.OnClick();
            }
        }

        private void InputManager_OnCancelEvent()
        {
            if (visible && cursor.CanMove())
            {
                visible = false;
                StartCoroutine(FadeOut());
            }
        }

        private void InputManager_OnDownEvent()
        {
            if (visible && cursor.CanMove())
            {
                HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Down));
            }
        }

        private void InputManager_OnUpEvent()
        {
            if (visible && cursor.CanMove())
            {
                HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Up));
            }
        }

        private void InputManager_OnRightEvent()
        {
            if (visible && cursor.CanMove())
            {
                HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Right));
            }
        }

        private void InputManager_OnLeftEvent()
        {
            if (visible && cursor.CanMove())
            {
                HighlightElement(FindNextElement(highlightedElement, InventoryElement.MoveDirection.Left));
            }
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

        public override void ShowCursor()
        {
            cursor.gameObject.SetActive(true);
        }

        public override void HideCursor()
        {
            cursor.gameObject.SetActive(false);
        }

        public override void MovePaneLeft()
        {
            WeaverLog.Log("MOVING PANE LEFT");
        }

        public override void MovePaneRight()
        {
            WeaverLog.Log("MOVING PANE RIGHT");
        }
    }
}
