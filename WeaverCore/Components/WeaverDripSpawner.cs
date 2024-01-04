using System.Collections;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used for spawning the drop effects on the player when the player jumps out of water
    /// </summary>
    public class WeaverDripSpawner : MonoBehaviour
    {
        [SerializeField]
        GameObject spatterParticlePrefab;

        [SerializeField]
        float dripDuration = 0.4f;

        [SerializeField]
        float dripFrequency = 0.025f;

        [SerializeField]
        OnDoneBehaviour onDone;

        [SerializeField]
        bool enableFirstBurst = true;

        [SerializeField]
        bool enableSecondBurst = true;

        [SerializeField]
        bool enableThirdBurst = true;

        [Space]
        [Header("First Burst")]
        [SerializeField]
        Vector2Int firstBurstSpawnAmountRange = new Vector2Int(12, 15);

        [SerializeField]
        Vector2 firstBurstSpeedRange = new Vector2(5, 12);

        [SerializeField]
        Vector2 firstBurstAngleRange = new Vector2(80, 110);

        [SerializeField]
        Vector2 firstBurstOriginVariation = new Vector2(1,0.5f);

        [Space]
        [Header("Second Burst")]
        [SerializeField]
        Vector2Int secondBurstSpawnAmountRange = new Vector2Int(1, 1);

        [SerializeField]
        Vector2 secondBurstSpeedRange = new Vector2(0, 1);

        [SerializeField]
        Vector2 secondBurstAngleRange = new Vector2(90, 90);

        [SerializeField]
        Vector2 secondBurstOriginVariation = new Vector2(0.25f, 0.5f);

        [SerializeField]
        Vector2 secondBurstScaleRange = new Vector2(1, 1);

        [Space]
        [Header("Third Burst")]
        [SerializeField]
        Vector2Int thirdBurstSpawnAmountRange = new Vector2Int(1, 1);

        [SerializeField]
        Vector2 thirdBurstSpeedRange = new Vector2(0, 1);

        [SerializeField]
        Vector2 thirdBurstAngleRange = new Vector2(90, 90);

        [SerializeField]
        Vector2 thirdBurstOriginVariation = new Vector2(0.5f, 0.5f);

        [SerializeField]
        Vector2 thirdBurstScaleRange = new Vector2(1, 1);



        private void Awake()
        {
            transform.parent = Player.Player1.transform;
            transform.localPosition = new Vector3(0f,-0.5f,0.01f);
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            yield return new WaitForSeconds(0.04f);
            if (enableFirstBurst)
            {
                FlingUtilities.SpawnRandomObjectsPooled(spatterParticlePrefab, transform.position, firstBurstSpawnAmountRange, firstBurstSpeedRange, firstBurstAngleRange, firstBurstOriginVariation);
            }

            if (enableSecondBurst)
            {
                StartCoroutine(FlingUtilities.SpawnRandomObjectsPooledOverTime(spatterParticlePrefab, dripDuration, dripFrequency, () => transform.position, secondBurstSpawnAmountRange, secondBurstSpeedRange, secondBurstAngleRange, secondBurstOriginVariation, secondBurstScaleRange));
            }

            if (enableThirdBurst)
            {
                StartCoroutine(FlingUtilities.SpawnRandomObjectsPooledOverTime(spatterParticlePrefab, dripDuration, dripFrequency, () => transform.position, thirdBurstSpawnAmountRange, thirdBurstSpeedRange, thirdBurstAngleRange, thirdBurstOriginVariation, thirdBurstScaleRange));
            }

            for (float t = 0; t < dripDuration; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                yield return null;
            }

            StopAllCoroutines();
            onDone.DoneWithObject(this);
        }
    }
}