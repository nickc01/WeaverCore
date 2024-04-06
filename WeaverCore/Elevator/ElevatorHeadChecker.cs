using System;
using UnityEngine;

namespace WeaverCore.Elevator
{
    /// <summary>
    /// This goes on an object with a collider. The collider is meant to go below the elevator, so the elevator stops and goes the other direction when the elevator starts to crush the player beneath it.
    /// </summary>
    public class ElevatorHeadChecker : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.gameObject.layer == LayerMask.NameToLayer("HeroBox"))
            {
                var elevator = gameObject.GetComponentInParent<Elevator>();

                if (elevator == null)
                {
                    throw new Exception("The head checker couldn't find an elevator in any parent object!");
                }

                elevator.CallElevatorToPosition(elevator.GetOppositeDestination(), 0f, false);
            }
        }
    }
}