using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Utilities
{
    public static class RendererUtilities
	{
		public interface IColorable
		{
			UnityEngine.Component BackingComponent {get;}
			Color color {get; set;}
		}

        public class SpriteColorable : IColorable
        {
			public readonly SpriteRenderer BackingRenderer;
            public SpriteColorable(SpriteRenderer renderer)
            {
				BackingRenderer = renderer;
            }

            public Component BackingComponent => BackingRenderer;

            public Color color { get => BackingRenderer.color; set => BackingRenderer.color = color; }
        }

		public class GraphicColorable : IColorable
        {
			public readonly Graphic BackingGraphic;
            public GraphicColorable(Graphic graphic)
            {
				BackingGraphic = graphic;
            }

            public Component BackingComponent => BackingGraphic;

            public Color color { get => BackingGraphic.color; set => BackingGraphic.color = color; }
        }

		public class RendererColorable : IColorable
        {
			public readonly Renderer BackingRenderer;
            public RendererColorable(Renderer renderer)
            {
				BackingRenderer = renderer;
            }

            public Component BackingComponent => BackingRenderer;

            public Color color 
			{ 
				get 
				{
					if (BackingRenderer.sharedMaterial.HasProperty("_Color"))
					{
						return BackingRenderer.sharedMaterial.GetColor("_Color"); 
					}
					else
					{
						return BackingRenderer.sharedMaterial.GetColor("_FaceColor"); 
					}

				}
				set
				{
					if (BackingRenderer.sharedMaterial.HasProperty("_Color"))
					{
						BackingRenderer.sharedMaterial.SetColor("_Color", color); 
					}
					else
					{
						BackingRenderer.sharedMaterial.SetColor("_FaceColor", color); 
					}
					//
				}
			}
        }

        public static IEnumerable<IColorable> GetObjectColorables(GameObject obj)
		{
			return GetObjectColorables(obj.GetComponents<Component>());
		}

		public static IEnumerable<IColorable> GetObjectColorableInChildren(GameObject obj)
		{
			return GetObjectColorables(obj.GetComponentsInChildren<Component>());
		}

		public static IEnumerable<IColorable> GetObjectColorables(IEnumerable<Component> components)
		{
			foreach (var c in components)
			{
				if (c is SpriteRenderer sr)
				{
					//WeaverLog.Log("FOUND SPRITE COLORABLE = " + sr);
					yield return new SpriteColorable(sr);
				}
				else if (c is Graphic g)
				{
					//WeaverLog.Log("FOUND GRAPHIC COLORABLE = " + g);
					yield return new GraphicColorable(g);
				}
				else if (c is Renderer r)
				{
					//WeaverLog.Log("FOUND RENDERERABLE COLORABLE = " + r);
					yield return new RendererColorable(r);
				}
			}
		}
	}
}
