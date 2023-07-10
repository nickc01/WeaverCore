using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
    public class Laser : MonoBehaviour
    {
        [field: SerializeField]
        [field: Tooltip("How wide the laser should be at the start")]
        public float StartingWidth { get; set; } = 0.25f;

        [field: SerializeField]
        [field: Tooltip("The angle in degrees the laser should spread out")]
        [field: Range(0.5f, 85f)]
        public float Spread { get; set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("The maximum length of the laser beam")]
        public float MaximumLength { get; set; } = 20f;

        [field: SerializeField]
        [field: Tooltip("The collision mask the laser will use for collision")]
        public LayerMask CollisionMask { get; set; }

        [field: SerializeField]
        [field: Range(2, 200)]
        public int Quality { get; set; } = 10;

        [field: SerializeField]
        [field: Range(1, 6)]
        public int ColliderQuality { get; set; } = 1;

        [field: SerializeField]
        public float TextureStretch { get; set; }

        [field: SerializeField]
        public uint LengthSubdivisions { get; set; } = 10;

        public (Vector2 start, Vector2 end) TextureTopEdge
        {
            get
            {
                CheckInit();
                return (topEdgeStart, topEdgeEnd);
            }
        }

        public (Vector2 start, Vector2 end) TextureBottomEdge
        {
            get
            {
                CheckInit();
                return (bottomEdgeStart, bottomEdgeEnd);
            }
        }

        public (Vector2 start, Vector2 end) TopColliderEdge
        {
            get
            {
                CheckInit();
                return (polygonPoints[1], polygonPoints[2]);
            }
        }
        public (Vector2 start, Vector2 end) BottomColliderEdge
        {
            get
            {
                CheckInit();
                return (polygonPoints[0], polygonPoints[polygonPoints.Count - 1]);
            }
        }

        public List<Vector2> TextureContactPoints
        {
            get
            {
                CheckInit();
                return contactPoints;
            }
        }

        public List<Vector2> ColliderContactPoints
        {
            get
            {
                CheckInit();
                return colliderContactPoints;
            }
        }

        public PolygonCollider2D MainCollider
        {
            get
            {
                CheckInit();
                return mainCollider;
            }
        }

        public MeshRenderer MainRenderer
        {
            get
            {
                CheckInit();
                return mainRenderer;
            }
        }


        /*[SerializeField]
        Texture texture;*/

        [SerializeField]
        Sprite sprite;

        [SerializeField]
        Color color = Color.white;

        /*public Texture Texture
        {
            get => texture;
            set
            {
                texture = value;
                UpdateMaterialValues();
            }
        }*/

        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                UpdateMaterialValues();
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                UpdateMaterialValues();
            }
        }

        public Vector2 SpriteOffset { get; private set; }
        public Vector2 SpriteScale { get; private set; }

        Vector2 topEdgeStart;
        Vector2 topEdgeEnd;

        Vector2 bottomEdgeStart;
        Vector2 bottomEdgeEnd;

        [NonSerialized]
        List<Vector2> contactPoints = new List<Vector2>();

        [NonSerialized]
        List<Vector2> colliderContactPoints = new List<Vector2>();

        [NonSerialized]
        List<Vector2> polygonPoints;
        [NonSerialized]
        List<Vector3> verticies;
        [NonSerialized]
        List<int> indicies;
        [NonSerialized]
        List<Vector2> uvs;

        RaycastHit2D[] terrainHit = new RaycastHit2D[1];

        Mesh mesh;

        MeshFilter filter;
        MeshRenderer mainRenderer;
        PolygonCollider2D mainCollider;

        MaterialPropertyBlock block;

        Recoiler recoiler;

        int texMainID;
        int colorID;

        void CheckInit()
        {
            if (verticies == null)
            {
                Awake();
            }
        }

        private void Awake()
        {
            recoiler = GetComponentInParent<Recoiler>();
            //Debug.Log("MASK = " + CollisionMask.value);
            texMainID = Shader.PropertyToID("_MainTex");
            colorID = Shader.PropertyToID("_Color");
            block = new MaterialPropertyBlock();
            filter = GetComponent<MeshFilter>();
            mainRenderer = GetComponent<MeshRenderer>();
            mainCollider = GetComponent<PolygonCollider2D>();
            polygonPoints = new List<Vector2>();
            verticies = new List<Vector3>();
            indicies = new List<int>();
            uvs = new List<Vector2>();

            if (mainRenderer.sharedMaterial == null)
            {
                mainRenderer.sharedMaterial = WeaverAssets.LoadWeaverAsset<Material>("Default Sprite Material");
            }

            mesh = new Mesh();

            mesh.MarkDynamic();

            UpdateMaterialValues();

            UpdateMeshLists();
        }

        private void Reset()
        {
            CheckInit();
            var mask = new LayerMask();
            mask.value = 256;
            CollisionMask = mask;

            color = Color.white;
        }

        private void OnValidate()
        {
            CheckInit();
            UpdateMaterialValues();
        }


        void UpdateMaterialValues()
        {
            mainRenderer.GetPropertyBlock(block);

            /*var tex = texture;
            if (tex == null)
            {
                tex = Texture2D.whiteTexture;
            }*/
            var currentSprite = this.sprite;
            if (currentSprite == null)
            {
                var blankTexture = Texture2D.whiteTexture;
                currentSprite = Sprite.Create(blankTexture, new Rect(0f, 0f, blankTexture.width, blankTexture.height), new Vector2(0.5f, 0.5f), blankTexture.width);
            }

            block.SetTexture(texMainID, currentSprite.texture);
            block.SetColor(colorID, color);

            var textureSize = new Vector2(currentSprite.texture.width,currentSprite.texture.height);
            var spriteRect = currentSprite.rect;

            SpriteOffset = new Vector2(spriteRect.x / textureSize.x, spriteRect.y / textureSize.y);
            SpriteScale = new Vector2(spriteRect.width / textureSize.x, spriteRect.height / textureSize.y);

            //Debug.Log("FINAL SIZE = " + new Vector4(spriteRect.x / textureSize.x, spriteRect.y / textureSize.y, spriteRect.width / textureSize.x, spriteRect.height / textureSize.y));

            //block.SetVector("_MainTex_ST",new Vector4(spriteRect.width / textureSize.x,spriteRect.height / textureSize.y, spriteRect.x / textureSize.x, spriteRect.y / textureSize.y));
            //block.SetVector("_MainTex_ST",new Vector4(0.5f,0.5f,0.5f,0.5f));
            //block.SetVector("_MainTex_ST", new Vector4(1f,1f,0f,0f));
            //Debug.Log("ST = " + block.GetVector("_MainTex_ST"));

            mainRenderer.SetPropertyBlock(block);

            //var texRect = currentSprite.textureRect;

            //mainRenderer.sharedMaterial.mainTextureOffset = new Vector2(0.5f,0.5f);
            //mainRenderer.sharedMaterial.mainTextureScale = new Vector2(1f,1f);
        }

        Vector2 AdjustUVCoordinate(Vector2 uv)
        {
            return new Vector2(Mathf.Lerp(SpriteOffset.x,SpriteOffset.x + SpriteScale.x,uv.x),Mathf.Lerp(SpriteOffset.y,SpriteOffset.y + SpriteScale.y,uv.y));
        }

        void UpdateMeshLists()
        {
            //float z = transform.GetZPosition();
            if (Quality < 2)
            {
                Quality = 2;
            }
            else if (Quality > 200)
            {
                Quality = 200;
            }
            CheckInit();

            if (ColliderQuality < 1)
            {
                ColliderQuality = 1;
            }
            else if (ColliderQuality > 6)
            {
                ColliderQuality = 6;
            }
            verticies.Clear();
            indicies.Clear();
            uvs.Clear();
            polygonPoints.Clear();
            colliderContactPoints.Clear();

            Spread = Mathf.Clamp(Spread, 0.5f, 85f);
            var halfWidth = StartingWidth / 2f;

            //verticies.Add(new Vector3(0f, halfWidth));

            var startLocation = new Vector3(0f, halfWidth);

            var firingDirection = MathUtilities.CartesianToPolar(Vector2.right);

            //firingDirection.x = Mathf.RoundToInt(firingDirection.x);//Mathf.RoundToInt(firingDirection.x * 10f) * 0.1f;

            var firstAngle = firingDirection.x - Spread;
            var firstLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * firstAngle);
            var firstDirection = MathUtilities.PolarToCartesian(firstAngle, MaximumLength);

            //verticies[2] = verticies[1] + (Vector3)firstDirection;

            var secondAngle = firingDirection.x + Spread;
            var secondLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * secondAngle);
            var secondDirection = MathUtilities.PolarToCartesian(secondAngle, MaximumLength);

            //verticies[3] = verticies[0] + (Vector3)secondDirection;

            float sourcePointX = startLocation.x - (secondDirection.x * startLocation.y / secondDirection.y);
            Vector2 sourcePoint = new Vector2(sourcePointX, 0f);

            //Debug.DrawLine(transform.TransformPoint(startLocation), transform.TransformPoint(sourcePoint));

            //Debug.DrawRay(transform.TransformPoint(startLocation),transform.TransformDirection(firstDirection));
            //Debug.DrawRay(transform.TransformPoint(startLocation),transform.TransformDirection(secondDirection));

            //Debug.DrawLine(transform.TransformPoint(startLocation),transform.TransformPoint(-startLocation));

            //Debug.Log("Source Point = " + sourcePoint);

            //Vector2 laserOrigin = transform.TransformPoint(sourcePoint);

            polygonPoints.Add(new Vector2(0f, -halfWidth));
            polygonPoints.Add(new Vector2(0f, halfWidth));

            var extraStretch = halfWidth * TextureStretch;

            for (int i = 0; i <= Quality; i++)
            {
                var targetDirection = Vector2.Lerp(secondDirection, firstDirection, i / (float)Quality);

                var rayStartPosition = new Vector2(0f, Mathf.Lerp(startLocation.y, -startLocation.y, i / (float)Quality));

                verticies.Add(rayStartPosition * new Vector2(1f, TextureStretch));

                var vertexUVy = LerpUtilities.UnclampedInverseLerp(extraStretch, -extraStretch, verticies[verticies.Count - 1].y);

                uvs.Add(AdjustUVCoordinate(new Vector2(0f, vertexUVy)));
                Vector3 destVertex = default;

                if (Physics2D.RaycastNonAlloc(transform.TransformPoint(rayStartPosition), transform.TransformDirection(targetDirection / new Vector2(1f,TextureStretch)).normalized, terrainHit, MaximumLength, CollisionMask.value) > 0)
                {
                    destVertex = transform.InverseTransformPoint(terrainHit[0].point);
                }
                else
                {
                    destVertex = rayStartPosition + (targetDirection / new Vector2(1f, TextureStretch)).normalized * MaximumLength;
                }
                if (i % ColliderQuality == 0)
                {

                    polygonPoints.Add(destVertex);
                    colliderContactPoints.Add(destVertex);

                    destVertex.y *= TextureStretch;
                    if (TextureStretch != 1f)
                    {
                        if (Physics2D.RaycastNonAlloc(transform.TransformPoint(rayStartPosition), transform.TransformDirection(targetDirection).normalized, terrainHit, MaximumLength, CollisionMask.value) > 0)
                        {
                            destVertex = transform.InverseTransformPoint(terrainHit[0].point);
                        }
                        else
                        {
                            destVertex = rayStartPosition + (targetDirection).normalized * MaximumLength;
                        }
                    }
                }



                var previousVertex = verticies[verticies.Count - 1];

                destVertex.z = 0f;

                for (int s = 0; s < LengthSubdivisions; s++)
                {
                    verticies.Add(Vector3.Lerp(previousVertex, destVertex,s / (float)LengthSubdivisions));
                    uvs.Add(AdjustUVCoordinate(new Vector2(s / (float)LengthSubdivisions, vertexUVy)));
                }
                verticies.Add(destVertex);

                uvs.Add(AdjustUVCoordinate(new Vector2(1f, vertexUVy)));
            }

            topEdgeStart = verticies[0];
            topEdgeEnd = verticies[(int)LengthSubdivisions + 1];

            bottomEdgeStart = verticies[verticies.Count - 2 - (int)LengthSubdivisions];
            bottomEdgeEnd = verticies[verticies.Count - 1];

            contactPoints.Clear();

            for (int i = 0; i <= Quality - 1; i++)
            {
                int vIndex = i * 2 + ((int)LengthSubdivisions * i);

                for (int s = 0; s <= LengthSubdivisions; s++)
                {
                    int currentIndex = vIndex + s;
                    int nextIndex = vIndex + 2 + (int)LengthSubdivisions + s;

                    indicies.Add(currentIndex);
                    indicies.Add(currentIndex + 1);
                    indicies.Add(nextIndex + 1);

                    indicies.Add(currentIndex);
                    indicies.Add(nextIndex + 1);
                    indicies.Add(nextIndex);
                }

                contactPoints.Add(verticies[vIndex + (int)LengthSubdivisions + 1]);
            }

            mesh.Clear();

            mesh.SetVertices(verticies);
            mesh.SetTriangles(indicies, 0);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            filter.mesh = mesh;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateMaterialValues();
                FixedUpdate();
            }
#endif
        }

        private void FixedUpdate()
        {
            CheckInit();
            mainCollider.SetPath(0, polygonPoints);
        }

        private void LateUpdate()
        {
            UpdateMeshLists();
        }
    }

}