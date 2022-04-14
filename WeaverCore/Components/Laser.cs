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

        //[field: SerializeField]
        //public float ColliderOffset { get; set; } = 0f;

        [SerializeField]
        Texture texture;

        [SerializeField]
        Color color = Color.white;

        public Texture Texture
        {
            get => texture;
            set
            {
                texture = value;
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

        int texMainID;
        int colorID;


        private void Awake()
        {
            Debug.Log("MASK = " + CollisionMask.value);
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
            mainCollider.isTrigger = true;

            if (mainRenderer.sharedMaterial == null)
            {
                mainRenderer.sharedMaterial = WeaverAssets.LoadWeaverAsset<Material>("Default Sprite Material");
            }

            mesh = new Mesh();

            mesh.MarkDynamic();
        }

        private void Reset()
        {
            var mask = new LayerMask();
            mask.value = 256;
            CollisionMask = mask;

            color = Color.white;
        }

        private void OnValidate()
        {
            if (verticies == null)
            {
                Awake();
            }
            UpdateMaterialValues();
        }

        void UpdateMaterialValues()
        {
            mainRenderer.GetPropertyBlock(block);

            var tex = texture;
            if (tex == null)
            {
                tex = Texture2D.whiteTexture;
            }

            block.SetTexture(texMainID, tex);
            block.SetColor(colorID, color);
            mainRenderer.SetPropertyBlock(block);
        }

        void UpdateMeshLists()
        {
            if (Quality < 2)
            {
                Quality = 2;
            }
            else if (Quality > 200)
            {
                Quality = 200;
            }
            if (verticies == null)
            {
                Awake();
            }

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

            Spread = Mathf.Clamp(Spread, 0.5f, 85f);
            var halfWidth = StartingWidth / 2f;

            //verticies.Add(new Vector3(0f, halfWidth));

            var startLocation = new Vector3(0f, halfWidth);

            var firingDirection = MathUtilties.CartesianToPolar(Vector2.right);

            //firingDirection.x = Mathf.RoundToInt(firingDirection.x);//Mathf.RoundToInt(firingDirection.x * 10f) * 0.1f;

            var firstAngle = firingDirection.x - Spread;
            var firstLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * firstAngle);
            var firstDirection = MathUtilties.PolarToCartesian(firstAngle, MaximumLength);

            //verticies[2] = verticies[1] + (Vector3)firstDirection;

            var secondAngle = firingDirection.x + Spread;
            var secondLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * secondAngle);
            var secondDirection = MathUtilties.PolarToCartesian(secondAngle, MaximumLength);

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

            var extraStretch = halfWidth + TextureStretch;

            for (int i = 0; i <= Quality; i++)
            {
                //var targetDirection = Mathf.Lerp(firstDirection,secondDirection, i / (float)Quality);
                var targetDirection = Vector2.Lerp(secondDirection, firstDirection, i / (float)Quality);

                //var centerX = Mathf.InverseLerp(sourcePoint.x, targetDirection.x, 0f);

                //var uvY = Mathf.InverseLerp(secondDirection,firstDirection,)

                verticies.Add(new Vector2(0f, Mathf.Lerp(startLocation.y, -startLocation.y, i / (float)Quality)));

                var vertexUVy = LerpUtilities.UnclampedInverseLerp(extraStretch, -extraStretch, verticies[verticies.Count - 1].y);

                uvs.Add(new Vector2(0f, vertexUVy) /*1f - (i / (float)Quality))*/);

                //Debug.DrawRay(laserOrigin, transform.TransformDirection(targetDirection), Color.red);

                if (Physics2D.RaycastNonAlloc(transform.TransformPoint(verticies[verticies.Count - 1]), transform.TransformDirection(targetDirection).normalized, terrainHit, MaximumLength, CollisionMask.value) > 0)
                {
                    //Debug.DrawLine(laserOrigin, terrainHit[0].point,Color.red);
                    verticies.Add(transform.InverseTransformPoint(terrainHit[0].point));
                }
                else
                {
                    //Debug.DrawRay(laserOrigin, transform.TransformDirection(targetDirection).normalized * MaximumLength, Color.red);
                    verticies.Add((Vector2)verticies[verticies.Count - 1] + (targetDirection).normalized * MaximumLength);
                }
                if (i % ColliderQuality == 0)
                {

                    var addedVertex = verticies[verticies.Count - 1];

                    /*var firedLength = (verticies[verticies.Count - 1] - verticies[verticies.Count - 2]).magnitude;

                    //var polygonDirection = 
                    //var polygonDirection = Vector2.Lerp(secondDirection * new Vector2(0f, ColliderOffset), firstDirection * new Vector2(1f, ColliderOffset), i / (float)Quality);

                    var adjustedFirstDirection = firstDirection.normalized * firedLength;
                    var adjustedSecondDirection = secondDirection.normalized * firedLength;


                    var polygonDirection = Vector2.Lerp(adjustedSecondDirection + new Vector2(0f, ColliderOffset), adjustedFirstDirection - new Vector2(0f, ColliderOffset), i / (float)Quality);


                    var xDifference = (verticies[verticies.Count - 2] + (Vector3)polygonDirection).x - addedVertex.x;


                    //var directionDifference = (polygonDirection.normalized * addedVertex.magnitude) - (targetDirection.normalized * addedVertex.magnitude);

                    polygonPoints.Add(verticies[verticies.Count - 2] + (Vector3)polygonDirection + new Vector3(ColliderOffset * (i / (float)Quality) - 0.5f, 0f));
                    */
                    polygonPoints.Add(addedVertex);

                    Debug.DrawLine(verticies[verticies.Count - 2], verticies[verticies.Count - 1], Color.magenta);
                    Debug.DrawLine(verticies[verticies.Count - 2], polygonPoints[polygonPoints.Count - 1], Color.blue);
                }
                //uvs.Add(verticies[verticies.Count - 1].With(y: 1f - (i / (float)Quality)));

                uvs.Add(new Vector2(1f, vertexUVy));

                //Debug.DrawRay(transform.TransformPoint(verticies[verticies.Count - 2]), transform.TransformDirection(targetDirection), Color.blue);

                //Debug.DrawLine(laserOrigin,transform.TransformPoint(verticies[verticies.Count - 1]),Color.red);
            }

            var firstVertexSource = verticies[0];
            var firstVertexDest = verticies[1];

            var lastVertexSource = verticies[verticies.Count - 2];
            var lastVertexDest = verticies[verticies.Count - 1];

            int lastVertexIndex = verticies.Count - 1;

            verticies.Add(firstVertexSource + new Vector3(0f, TextureStretch));
            verticies.Add(firstVertexDest + new Vector3(0f, TextureStretch));

            verticies.Add(lastVertexSource - new Vector3(0f, TextureStretch));
            verticies.Add(lastVertexDest - new Vector3(0f, TextureStretch));

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));

            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));

            int vertexCount = verticies.Count;


            for (int i = 0; i <= Quality - 1; i++)
            {
                int vIndex = i * 2;
                indicies.Add(vIndex);
                indicies.Add(vIndex + 1);
                indicies.Add(vIndex + 2);

                indicies.Add(vIndex + 2);
                indicies.Add(vIndex + 1);
                indicies.Add(vIndex + 3);
            }

            indicies.Add(0);
            indicies.Add(vertexCount - 4);
            indicies.Add(vertexCount - 3);

            indicies.Add(0);
            indicies.Add(vertexCount - 3);
            indicies.Add(1);

            indicies.Add(lastVertexIndex - 1);
            indicies.Add(vertexCount - 1);
            indicies.Add(vertexCount - 2);

            indicies.Add(lastVertexIndex - 1);
            indicies.Add(lastVertexIndex);
            indicies.Add(vertexCount - 1);

            //Debug.Log("VERTICIES = " + verticies.Count);

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
            //verticies.Add(new Vector3(0f, -halfWidth));
        }

        private void FixedUpdate()
        {
            if (verticies == null)
            {
                Awake();
            }
            mainCollider.SetPath(0, polygonPoints);
        }

        private void LateUpdate()
        {
            UpdateMeshLists();
            /*var firingDirection = MathUtilties.CartesianToPolar(Vector2.right);

            //cos (firstAngle) = MaximumLength / h
            //h = MaximumLength / cos(firstAngle)

            var halfWidth = StartingWidth / 2f;

            var secondPos = new Vector3(0f, halfWidth);
            var firstPos = new Vector3(0f, -halfWidth);



            var firstAngle = firingDirection.x - Spread;
            var firstLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * firstAngle);
            var firstDirection = MathUtilties.PolarToCartesian(firstAngle, firstLength);

            var secondAngle = firingDirection.x + Spread;
            var secondLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * secondAngle);
            var secondDirection = MathUtilties.PolarToCartesian(secondAngle, secondLength);

            if (Physics2D.RaycastNonAlloc(transform.TransformPoint(verticies[0]), transform.TransformDirection(secondDirection).normalized, terrainHit, secondLength, CollisionMask.value) > 0)
            {

            }*/
        }
    }

}