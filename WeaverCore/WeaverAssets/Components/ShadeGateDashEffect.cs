using System.Collections;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
    public class ShadeGateDashEffect : MonoBehaviour
    {
        ParticleSystem ptBounce;
        ParticleSystem ptBounce2;
        GameObject blackFlash;

        [SerializeField]
        AudioClip passThroughSound;

        [SerializeField]
        GameObject blackWavePrefab;

        bool collided = false;

        private void Awake()
        {
            ptBounce = transform.Find("Pt Bounce").GetComponent<ParticleSystem>();
            ptBounce2 = transform.Find("Pt Bounce 2").GetComponent<ParticleSystem>();
            blackFlash = transform.Find("Black Flash").gameObject;

            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            var e1 = ptBounce.emission;
            var e2 = ptBounce2.emission;
            while (true)
            {
                collided = false;
                e1.enabled = false;
                yield return new WaitUntil(() => collided);

                if (HeroController.instance.GetState("shadowDashing"))
                {
                    blackFlash.SetActive(true);
                    if (passThroughSound != null)
                    {
                        WeaverAudio.PlayAtPoint(passThroughSound, Player.Player1.transform.position);
                    }

                    CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);

                    var heroY = Player.Player1.transform.position.y - 0.5f;

                    var wave = Pooling.Instantiate(blackWavePrefab, Player.Player1.transform.position, Quaternion.identity);

                    //wave.transform.SetPositionX(transform.position.x);
                    wave.transform.position = new Vector3(transform.position.x, heroY, -0.1f);
                    ptBounce.transform.SetPositionY(heroY);
                    ptBounce2.transform.SetPositionY(heroY);
                    ptBounce.Play();
                    ptBounce2.Play();
                    e1.enabled = true;
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            collided = true;
        }
    }
}
