using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// The box that is used to damage the player if the player doesn't have acid armor on
    /// </summary>
    public class WeaverAcidBox : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The surface water objects of the acid. These will get disabled if the player doesn't have isma's tear. These must be disabled in that case, or else glitchy behaviour occurs")]
        List<WeaverSurfaceWater> surfaceWater;

        [SerializeField]
        [Tooltip("The colliders that will damage the player upon contact. Any boxes added to this list will get disabled when isma's tear is equipped")]
        List<Collider2D> acidDamageBoxes;

        private void Awake()
        {
            StartCoroutine(ArmorCheck());
        }

        IEnumerator ArmorCheck()
        {
            yield return null;
            foreach (var water in surfaceWater)
            {
                if (water != null)
                {
                    water.enabled = false;
                    water.GetComponent<Collider2D>().enabled = false;
                }
            }

            foreach (var box in acidDamageBoxes)
            {
                if (box != null)
                {
                    box.enabled = true;
                }
            }
            while (!PlayerData.instance.GetBool("hasAcidArmour"))
            {
                yield return new WaitForSeconds(0.1f);
            }

            foreach (var damager in GetComponentsInChildren<PlayerDamager>())
            {
                damager.damageDealt = 0;
            }

            foreach (var water in surfaceWater)
            {
                if (water != null)
                {
                    water.enabled = true;
                    water.GetComponent<Collider2D>().enabled = true;
                }
            }

            foreach (var box in acidDamageBoxes)
            {
                if (box != null)
                {
                    box.enabled = false;
                }
            }
        }
    }
}