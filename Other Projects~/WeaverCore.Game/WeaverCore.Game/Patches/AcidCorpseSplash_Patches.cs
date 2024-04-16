using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Attributes;
using WeaverCore.Internal;

namespace WeaverCore.Game.Patches
{
    static class AcidCorpseSplash_Patches
	{
		[OnInit]
		static void Init()
		{
            On.AcidCorpseSplash.Start += AcidCorpseSplash_Start;
            On.AcidCorpseSplash.CorpseSplash += AcidCorpseSplash_CorpseSplash;
		}

        private static System.Collections.IEnumerator AcidCorpseSplash_CorpseSplash(On.AcidCorpseSplash.orig_CorpseSplash orig, AcidCorpseSplash self, UnityEngine.GameObject corpseObject)
        {
            var iter = orig(self, corpseObject);

            while (iter.MoveNext())
            {
                yield return iter.Current;
            }
            //yield return orig(self, corpseObject);

            if (corpseObject != null && corpseObject.GetComponent<Corpse>() != null && corpseObject.GetComponent<tk2dSprite>() == null && corpseObject.TryGetComponent<SpriteRenderer>(out var sRenderer))
            {
                Vector3 position = corpseObject.transform.position;
                if ((bool)self.corpseDetector)
                {
                    BoxCollider2D component2 = self.corpseDetector.GetComponent<BoxCollider2D>();
                    if ((bool)component2)
                    {
                        position.y = component2.bounds.max.y;
                    }
                }

                ParticleSystem acidBubble = null;
                if ((bool)self.bubCloudPrefab)
                {
                    acidBubble = Object.Instantiate(self.bubCloudPrefab, self.transform.position, self.bubCloudPrefab.transform.rotation);
                    if ((bool)acidBubble)
                    {
                        acidBubble.Play();
                    }
                }
                ParticleSystem acidSpore = null;
                if ((bool)self.sporeCloudPrefab)
                {
                    acidSpore = Object.Instantiate(self.sporeCloudPrefab, self.transform.position, self.sporeCloudPrefab.transform.rotation);
                    if ((bool)acidSpore)
                    {
                        acidSpore.Play();
                    }
                }

                float elapsed2 = 0f;
                for (float fadeTime = 1f; elapsed2 <= fadeTime; elapsed2 += Time.deltaTime)
                {
                    sRenderer.color = Color.Lerp(Color.white, Color.clear, elapsed2 / fadeTime);
                    yield return null;
                }

                if ((bool)acidBubble)
                {
                    acidBubble.Stop();
                }
                if ((bool)acidSpore)
                {
                    acidSpore.Stop();
                }
            }
        }

        private static void AcidCorpseSplash_Start(On.AcidCorpseSplash.orig_Start orig, AcidCorpseSplash self)
        {
            if (self.audioPlayerPefab == null)
            {
                self.audioPlayerPefab = Other_Preloads.AudioPlayerPrefab;
            }

            if (self.splashSound.Volume == 0)
            {
                self.splashSound.PitchMin = 1;
                self.splashSound.PitchMax = 1;
                self.splashSound.Volume = 1;
            }

            orig(self);
        }
    }
}
