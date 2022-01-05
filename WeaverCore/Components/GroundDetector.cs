using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to detect if an object is touching the ground
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        ContactPoint2D[] contacts = new ContactPoint2D[3];
        HashSet<GameObject> terrainCollisions = new HashSet<GameObject>();

        /// <summary>
        /// Is this object touching the ground?
        /// </summary>
        public bool TouchingGround => terrainCollisions.Count > 0;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            int contactCount = collision.GetContacts(contacts);

            for (int i = 0; i < contactCount; i++)
            {
                if (contacts[i].point.y < transform.position.y)
                {
                    terrainCollisions.Add(collision.gameObject);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            terrainCollisions.Remove(collision.gameObject);
        }
    }
}
