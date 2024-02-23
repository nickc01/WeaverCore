using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{

    public class ShadeGateSlashEffect : MonoBehaviour
    {
        [SerializeField]
        AudioClip repelSound;

        [SerializeField]
        GameObject blackWavePrefab;

        [SerializeField]
        GameObject slashImpactPrefab;

        ParticleSystem ptBounce;

        private void Awake()
        {
            ptBounce = GetComponentInChildren<ParticleSystem>();
            StartCoroutine(MainRoutine());
        }

        bool hit = false;

        IEnumerator MainRoutine()
        {
            var e = ptBounce.emission;
            while (true)
            {
                hit = false;
                e.enabled = false;
                yield return new WaitUntil(() => hit);
                CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);
                WeaverAudio.PlayAtPoint(repelSound,Player.Player1.transform.position);
                var heroY = Player.Player1.transform.position.y;

                var wave = Pooling.Instantiate(blackWavePrefab, transform.position, Quaternion.identity);
                wave.transform.SetLocalScaleXY(1f, 1f);
                ptBounce.transform.SetPositionY(heroY);
                wave.transform.SetYPosition(heroY);
                wave.transform.SetPositionZ(-1f);

                var slash = Pooling.Instantiate(slashImpactPrefab, transform.position, Quaternion.identity);
                slash.transform.SetPositionX(transform.position.x);
                slash.transform.SetPositionY(heroY);

                if (Player.Player1.transform.position.x >= transform.position.x)
                {
                    slash.transform.SetXLocalScale(-1f);
                    HeroController.instance.RecoilRightLong();
                    ptBounce.transform.SetZRotation(315f);
                }
                else
                {
                    slash.transform.SetXLocalScale(1f);
                    HeroController.instance.RecoilLeftLong();
                    ptBounce.transform.SetZRotation(135f);
                }

                e.enabled = true;
                var strike = Pooling.Instantiate(EffectAssets.NailStrikePrefab, Player.Player1.transform.position, Player.Player1.transform.localRotation);
                strike.transform.SetXLocalPosition(transform.position.x);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Nail Attack"))
            {
                hit = true;
            }
        }
    }
}
