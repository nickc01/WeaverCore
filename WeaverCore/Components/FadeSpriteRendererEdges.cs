using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
	public class FadeSpriteRendererEdges : MonoBehaviour
	{
		[NonSerialized]
		SpriteRenderer _mainRenderer;

		public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

		[NonSerialized]
		Sprite prevSprite;

		[NonSerialized]
		MaterialPropertyBlock block;

		[NonSerialized]
		int _SpriteCoordsID;

        private void Reset()
        {
			MainRenderer.material = WeaverAssets.LoadWeaverAsset<Material>("Sprite - Faded Edges");
        }

		static Vector4 GetCoords(Vector2[] uvs)
		{
            if (uvs == null || uvs.Length == 0)
            {
                return new Vector4(0, 0, 1, 1);
            }

            Vector4 coords = new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);

			for (int i = 0; i < uvs.Length; i++)
			{
				if (uvs[i].x < coords.x)
				{
                    coords.x = uvs[i].x;
				}

                if (uvs[i].y < coords.y)
                {
                    coords.y = uvs[i].y;
                }

                if (uvs[i].x > coords.z)
                {
                    coords.z = uvs[i].x;
                }

                if (uvs[i].y > coords.w)
                {
                    coords.w = uvs[i].y;
                }
            }

			return coords;
		}

        private void LateUpdate()
        {
			var newSprite = MainRenderer.sprite;

			if (newSprite != prevSprite)
			{
				prevSprite = newSprite;

				if (block == null)
				{
					block = new MaterialPropertyBlock();
					_SpriteCoordsID = Shader.PropertyToID("_SpriteCoords");
                }

				Vector4 spriteUVCoords;

				if (newSprite != null)
				{
					spriteUVCoords = GetCoords(newSprite.uv);

                }
				else
				{
					spriteUVCoords = new Vector4(0f, 0f, 1f, 1f);
				}

				MainRenderer.GetPropertyBlock(block);
				block.SetVector(_SpriteCoordsID, spriteUVCoords);
				MainRenderer.SetPropertyBlock(block);
			}
        }
    }
}
