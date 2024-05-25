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

                if (elevator.Moving && elevator.MovingDestination.y <= elevator.transform.position.y)
                {
                    elevator.CallElevatorToOpposite(new Elevator.ElevatorInfo(elevator.GetDefaultInfo())
                    {
                        DoBob = false,
                        BeginDelay = -1f
                    });
                }
            }
        }
    }
}