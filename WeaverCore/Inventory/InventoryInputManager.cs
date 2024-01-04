using System;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Implementations;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// Used to receive input events from the player within an <see cref="InventoryPanel"/>
    /// </summary>
    public sealed class InventoryInputManager : MonoBehaviour
    {
        /// <summary>
        /// Called when the left input was pressed
        /// </summary>
        public event Action OnLeftEvent;

        /// <summary>
        /// Called when the right input was pressed
        /// </summary>
        public event Action OnRightEvent;

        /// <summary>
        /// Called when the up input was pressed
        /// </summary>
        public event Action OnUpEvent;

        /// <summary>
        /// Called when the down input was pressed
        /// </summary>
        public event Action OnDownEvent;

        public event Action OnRsUpEvent;
        public event Action OnRsDownEvent;

        /// <summary>
        /// Called when the select input was pressed
        /// </summary>
        public event Action OnSelectEvent;

        /// <summary>
        /// Called when the cancel input was pressed
        /// </summary>
        public event Action OnCancelEvent;


        EventManager manager;

        private void Awake()
        {
#if UNITY_EDITOR
            EditorSetup();
#else
            GameSetup();
#endif
        }

        void GameSetup()
        {
            manager = transform.parent.GetComponent<EventManager>();
            if (manager == null)
            {
                manager = transform.parent.gameObject.AddComponent<EventManager>();
            }

            manager.AddReceiverForEvent("UI LEFT", OnLeft);
            manager.AddReceiverForEvent("UI RIGHT", OnRight);
            manager.AddReceiverForEvent("UI UP", OnUp);
            manager.AddReceiverForEvent("UI RS UP", OnRSUp);
            manager.AddReceiverForEvent("UI DOWN", OnDown);
            manager.AddReceiverForEvent("UI RS DOWN", OnRSDown);

            manager.AddReceiverForEvent("UI CONFIRM", OnSelect);
            manager.AddReceiverForEvent("UI CANCEL", OnCancel);
        }



        void EditorSetup()
        {

        }

#if UNITY_EDITOR
        private void Update()
        {
            if (PlayerInput.up.WasPressed)
            {
                OnUp();
            }

            if (PlayerInput.down.WasPressed)
            {
                OnDown();
            }

            if (PlayerInput.left.WasPressed)
            {
                OnLeft();
            }

            if (PlayerInput.right.WasPressed)
            {
                OnRight();
            }

            if (PlayerInput.jump.WasPressed)
            {
                OnSelect();
            }

            if (PlayerInput.focus.WasPressed)
            {
                OnCancel();
            }
        }
#endif

        void OnLeft()
        {
            //WeaverLog.Log("LEFT PRESSSED");
            OnLeftEvent?.Invoke();
        }

        void OnRight()
        {
            //WeaverLog.Log("RIGHT PRESSSED");
            OnRightEvent?.Invoke();
        }

        void OnUp()
        {
            //WeaverLog.Log("UP PRESSSED");
            OnUpEvent?.Invoke();
        }

        void OnDown()
        {
            //WeaverLog.Log("DOWN PRESSSED");
            OnDownEvent?.Invoke();
        }

        void OnRSUp()
        {
            //WeaverLog.Log("Right Stick UP PRESSSED");
            OnUpEvent?.Invoke();
        }

        void OnRSDown()
        {
            //WeaverLog.Log("Right Stick DOWN PRESSSED");
            OnDownEvent?.Invoke();
        }

        void OnSelect()
        {
            //WeaverLog.Log("SELECT PRESSSED");
            OnSelectEvent?.Invoke();
        }

        void OnCancel()
        {
            //WeaverLog.Log("CANCEL PRESSSED");
            OnCancelEvent?.Invoke();
        }
    }
}