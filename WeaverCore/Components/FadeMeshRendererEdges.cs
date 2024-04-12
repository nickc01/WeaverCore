using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class FadeMeshRendererEdges : MonoBehaviour
    {
        [NonSerialized]
        MeshRenderer _mainRenderer;

        public MeshRenderer MainRenderer => _mainRenderer ??= GetComponent<MeshRenderer>();

        [NonSerialized]
        MeshFilter _filter;

        public MeshFilter Filter => _filter ??= GetComponent<MeshFilter>();

        [SerializeField]
        [Range(1,10)]
        int sharpness = 1;

        //[NonSerialized]
        //Sprite prevSprite;

        [NonSerialized]
        MaterialPropertyBlock block;

        [NonSerialized]
        int _SpriteCoordsID;

        [NonSerialized]
        int _SharpnessID;

        private void Awake()
        {
            UpdateRenderer();
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

        public void UpdateRenderer()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
                _SpriteCoordsID = Shader.PropertyToID("_SpriteCoords");
                _SharpnessID = Shader.PropertyToID("_Sharpness");
            }

            Vector4 meshUVCoords;

            meshUVCoords = GetCoords(Filter.sharedMesh.uv);

            MainRenderer.GetPropertyBlock(block);
            block.SetVector(_SpriteCoordsID, meshUVCoords);
            block.SetInt(_SharpnessID, sharpness);
            MainRenderer.SetPropertyBlock(block);
        }
    }
}
