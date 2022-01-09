using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;


namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used by <see cref="WeaverBench"/> to see if the player is within the bench's vicinity
    /// </summary>
    public class DetectHero : MonoBehaviour
    {
        GameObject Parent;
        GameObject Lit;
        GameObject Light;
        GameObject ParticleB;
        GameObject ParticleF;

        bool heroInRange = false;
        /// <summary>
        /// Is the hero within range of the bench?
        /// </summary>
        public bool HeroInRange => heroInRange;

		private void Reset()
		{
			if (GetComponent<Collider2D>() == null)
			{
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
			}
		}

		private void Awake()
		{
            Parent = transform.parent.gameObject;
            Lit = Parent.transform.Find("Lit")?.gameObject;
            Light = Parent.transform.Find("Light")?.gameObject;
            ParticleB = Parent.transform.Find("Particle B")?.gameObject;
            ParticleF = Parent.transform.Find("Particle F")?.gameObject;

            gameObject.layer = LayerMask.NameToLayer("Hero Detector");
		}

		private void OnValidate()
		{
            UnboundCoroutine.Start(WaitAFrame(gameObject));
            static IEnumerator WaitAFrame(GameObject obj)
			{
                yield return null;
				if (obj != null)
				{
                    obj.layer = LayerMask.NameToLayer("Hero Detector");
                }
            }
		}

		private void Start()
		{
            EventManager.SendEventToGameObject("EXIT RANGE", Parent, gameObject);
			if (Light != null)
			{
                var lightSprite = Light.GetComponent<SpriteRenderer>();
				if (lightSprite != null)
				{
                    var lightColor = lightSprite.color;
                    var newLightColor = Color.Lerp(lightColor, new Color(1f, 1f, 1f, 0.36862746f), 0.5f);
                    lightSprite.color = newLightColor;
                }
			}


            if (Lit != null)
            {
                var litSprite = Lit.GetComponent<SpriteRenderer>();
                if (litSprite != null)
                {
                    var litColor = litSprite.color;
                    var newLitColor = Color.Lerp(litColor, new Color(1f, 1f, 1f, 0f), 0.5f);
                    litSprite.color = newLitColor;
                }
            }

			if (ParticleB != null)
			{
                var particles = ParticleB.GetComponent<ParticleSystem>();
				if (particles != null)
				{
                    var e = particles.emission;
                    e.rateOverTime = 0f;
				}
			}

            if (ParticleF != null)
            {
                var particles = ParticleF.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var e = particles.emission;
                    e.rateOverTime = 0f;
                }
            }

            heroInRange = false;

        }

		private void OnTriggerEnter2D(Collider2D collision)
		{
            EventManager.SendEventToGameObject("EXIT RANGE", Parent, gameObject);

            if (Light != null)
            {
                var lightSprite = Light.GetComponent<SpriteRenderer>();
                if (lightSprite != null)
                {
                    var lightColor = lightSprite.color;
                    var newLightColor = Color.Lerp(lightColor, new Color(1f, 1f, 1f, 1f), 0.5f);
                    lightSprite.color = newLightColor;
                }
            }


            if (Lit != null)
            {
                var litSprite = Lit.GetComponent<SpriteRenderer>();
                if (litSprite != null)
                {
                    var litColor = litSprite.color;
                    var newLitColor = Color.Lerp(litColor, new Color(1f, 1f, 1f, 1f), 0.5f);
                    litSprite.color = newLitColor;
                }
            }

            if (ParticleB != null)
            {
                var particles = ParticleB.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var e = particles.emission;
                    e.rateOverTime = 10f;
                }
            }

            if (ParticleF != null)
            {
                var particles = ParticleF.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var e = particles.emission;
                    e.rateOverTime = 5f;
                }
            }

            heroInRange = true;
        }

		private void OnTriggerExit2D(Collider2D collision)
		{
            Start();
        }
	}
}
