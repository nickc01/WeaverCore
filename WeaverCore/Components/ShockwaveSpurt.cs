using System;
using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    public class ShockwaveSpurt : MonoBehaviour
    {
        [SerializeField]
        Vector3 rotation = new Vector3(0,180,0);

        [SerializeField]
        float activateDelay = 0.05f;

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