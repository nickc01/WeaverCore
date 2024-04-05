using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Tools
{
    public static class CustomMenuIconImpl
	{
		[OnRuntimeInit]
		static void OnRuntimeInit()
		{
            On.UIManager.Start += UIManager_Start;
		}

        private static void UIManager_Start(On.UIManager.orig_Start orig, UIManager self)
        {
            orig(self);

            UnboundCoroutine.Start(WaitAFrame(self));
        }

        static IEnumerator WaitAFrame(UIManager self)
        {
            yield return null;

            //var material = WeaverAssets.LoadWeaverAsset<Material>("Lit Sprite Material");

            var hiddenDreamsLogo = self.transform.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(s => s.gameObject.name == "Hidden_Dreams_Logo");

            var isDefaultSprite = !WeaverMod.LoadedMods.Any(m => m.GetType().Name == "FiveKnights");

            int index = -1;
            foreach (var icon in Registry.GetAllFeatures<CustomMenuIcon>())
            {
                index++;
                var currentBounds = GetBoundsOfLogo(hiddenDreamsLogo);

                var newLogo = new GameObject(icon.name);
                newLogo.transform.localScale = hiddenDreamsLogo.transform.localScale;
                var newRenderer = newLogo.AddComponent<SpriteRenderer>();
                //newRenderer.sharedMaterial = material;
                newRenderer.sortingLayerName = "Over";
                newRenderer.sprite = icon.icon;

                //var newBounds = newRenderer.bounds;


                var currentMinLocal = hiddenDreamsLogo.transform.InverseTransformPoint(currentBounds.min);
                var currentMaxLocal = hiddenDreamsLogo.transform.InverseTransformPoint(currentBounds.max);
                //var centerMinDiff = newBounds.center - newBounds.min;

                //newLogo.transform.position = currentBounds.max.With(y: currentBounds.min.y) + centerMinDiff;
                newLogo.transform.SetParent(hiddenDreamsLogo.transform, true);
                newLogo.transform.SetZLocalPosition(0);
                newLogo.transform.localScale = Vector3.one;

                //Subtract 1.5
                var newBounds = newRenderer.bounds;

                var newMinLocal = newLogo.transform.InverseTransformPoint(newBounds.min);

                newLogo.transform.localPosition = currentMaxLocal.With(y: currentMinLocal.y) - newMinLocal;
                newLogo.transform.SetZLocalPosition(0f);

                if (index == 0 && isDefaultSprite)
                {
                    newLogo.transform.SetXLocalPosition(newLogo.transform.GetXLocalPosition() - .15f);
                }

                //WeaverLog.Log("FOUND ICON = " + icon);
            }
        }

        static Bounds GetBoundsOfLogo(SpriteRenderer mainLogo)
        {
            var mainBounds = mainLogo.bounds;

            var children = mainLogo.GetComponentsInChildren<SpriteRenderer>();

            foreach (var child in children)
            {
                var childBounds = child.bounds;

                if (childBounds.min.x < mainBounds.min.x)
                {
                    mainBounds.min = mainBounds.min.With(x: childBounds.min.x);
                }

                if (childBounds.max.x > mainBounds.max.x)
                {
                    mainBounds.max = mainBounds.max.With(x: childBounds.max.x);
                }

                if (childBounds.min.y < mainBounds.min.y)
                {
                    mainBounds.min = mainBounds.min.With(y: childBounds.min.y);
                }

                if (childBounds.max.y > mainBounds.max.y)
                {
                    mainBounds.max = mainBounds.max.With(y: childBounds.max.y);
                }

                if (childBounds.min.z < mainBounds.min.z)
                {
                    mainBounds.min = mainBounds.min.With(z: childBounds.min.z);
                }

                if (childBounds.max.z > mainBounds.max.z)
                {
                    mainBounds.max = mainBounds.max.With(z: childBounds.max.z);
                }
            }

            return mainBounds;
        }
    }
}
