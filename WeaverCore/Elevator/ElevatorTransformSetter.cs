using System;
using UnityEngine;

namespace WeaverCore.Elevator
{
    [ExecuteAlways]
    public class ElevatorTransformSetter : MonoBehaviour
    {
        public Transform TopTransform;
        public Transform BottomTransform;

        [NonSerialized]
        Elevator elevator;

        private void OnValidate()
        {
            Update();
        }

        private void Update()
        {
            if (elevator == null)
            {
                elevator = GetComponent<Elevator>();
            }
            if (elevator != null)
            {
                if (TopTransform != null)
                {
                    elevator.DefaultTopPosition = TopTransform.position;
                }

                if (BottomTransform != null)
                {
                    elevator.DefaultBottomPosition = BottomTransform.position;
                }
            }
        }
    }
}