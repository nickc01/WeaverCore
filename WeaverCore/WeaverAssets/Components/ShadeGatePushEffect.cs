using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class ShadeGatePushEffect : MonoBehaviour
    {
        [SerializeField]
        bool isRight = true;

        [SerializeField]
        AudioClip repelSound;

        [SerializeField]
        GameObject BlackWaveEffect;

        ParticleSystem ptBounce;

        bool collided = false;

        private void Awake()
        {
            ptBounce = GetComponentInChildren<ParticleSystem>();
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            var e = ptBounce.emission;
            while (true)
            {
                collided = false;
                e.enabled = false;
                yield return new WaitUntil(() => collided);
                if (HeroController.instance.GetState("shadowDashing"))
                {
                    continue;
                }
                else
                {
                    CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);
                    WeaverAudio.PlayAtPoint(repelSound, Player.Player1.transform.position);
                    var heroY = Player.Player1.transform.position.y;

                    var wave = Pooling.Instantiate(BlackWaveEffect, transform.position, Quaternion.identity);

                    wave.transform.SetLocalScaleXY(1f, 1f);
                    wave.transform.position = new Vector3(transform.parent.position.x - 0.56f, heroY, -0.1f);
                    ptBounce.transform.SetPositionY(heroY);
                    if (isRight)
                    {
                        HeroController.instance.RecoilRightLong();
                    }
                    else
                    {
                        HeroController.instance.RecoilLeftLong();
                    }

                    e.enabled = true;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            collided = true;
        }
    }
}
