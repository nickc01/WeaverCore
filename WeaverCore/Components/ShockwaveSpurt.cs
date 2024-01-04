using System;
using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Represents a spurt segment of a shockwave
    /// </summary>
    public class ShockwaveSpurt : MonoBehaviour
    {
        /// <summary>
        /// Rotation vector applied to the ShockwaveSpurt object upon Awake.
        /// </summary>
        [Tooltip("The rotation of the shockwave applied on Awakee")]
        [SerializeField]
        Vector3 rotation = new Vector3(0, 180, 0);

        /// <summary>
        /// Delay before activating the shockwave after Awake.
        /// </summary>
        [Tooltip("Delay before activating the spurt collider for damaging the player")]
        [SerializeField]
        float activateDelay = 0.05f;

        /// <summary>
        /// Delay before deactivating the shockwave after activation.
        /// </summary>
        [Tooltip("Delay before deactivating the spurt collider")]
        [SerializeField]
        float deactivateDelay = 0.05f;

        [NonSerialized]
        Collider2D mainCollider = null;

        private void Awake()
        {
            if (mainCollider == null)
            {
                mainCollider = GetComponent<Collider2D>();
            }

            transform.rotation = Quaternion.Euler(rotation);

            mainCollider.enabled = false;
            StopAllCoroutines();
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            yield return new WaitForSeconds(activateDelay);

            mainCollider.enabled = true;

            yield return new WaitForSeconds(deactivateDelay);

            mainCollider.enabled = false;
        }
    }
}
