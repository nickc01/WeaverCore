using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(MeshRenderer))]
    public class LaserOLD : MonoBehaviour
    {
        [field: SerializeField]
        [field: Tooltip("How wide the laser should be at the start")]
        public float StartingWidth { get; set; } = 0.25f;

        [field: SerializeField]
        [field: Tooltip("The angle in degrees the laser should spread out")]
        [field: Range(0f,85f)]
        public float Spread { get; set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("The maximum length of the laser beam")]
        public float MaximumLength { get; set; } = 20f;

        [field: SerializeField]
        [field: Tooltip("The collision mask the laser will use for collision")]
        public LayerMask CollisionMask { get; set; }

        [NonSerialized]
        PolygonCollider2D mainCollider;

        [NonSerialized]
        MeshRenderer mainRenderer;

        Vector2[] polygonPoints;
        Vector3[] verticies;
        int[] indicies;
        Vector2[] uvs;

        RaycastHit2D[] terrainHit = new RaycastHit2D[1];

        RaycastHit2D[] multiTerrainHit = new RaycastHit2D[2];

        static Collider2D[] foundColliders = new Collider2D[30];

        static List<(Vector2 point, Vector2 normal)> raycastPoints = new List<(Vector2 point, Vector2 normal)>();

        static RaycastSorter Sorter = new RaycastSorter();

        class RaycastSorter : IComparer<(Vector2,Vector2)>
        {
            public Vector2 comparerVector;
            public Vector2 relativePosition;
            //Matrix4x4 worldToLocal;

            Comparer<float> numberComparer = Comparer<float>.Default; 
            public int Compare((Vector2, Vector2) x, (Vector2, Vector2) y)
            {
                //return 0;
                //return numberComparer.Compare(Vector2.Dot(comparerVector,x - relativePosition),Vector2.Dot(comparerVector,y - relativePosition));

                return numberComparer.Compare(Slope(x.Item1), Slope(y.Item1));
            }

            float Slope(Vector2 vector)
            {
                return (vector.y - relativePosition.y) / (vector.x - relativePosition.x);
            }
        }

        private void Awake()
        {
            mainCollider = GetComponent<PolygonCollider2D>();
            mainRenderer = GetComponent<MeshRenderer>();

            var mesh = new Mesh();

            //Marked dynamic since the mesh is getting updated every frame
            mesh.MarkDynamic();

            polygonPoints = new Vector2[4];
            verticies = new Vector3[4];
            indicies = new int[6];
            uvs = new Vector2[4];

            indicies[0] = 0;
            indicies[1] = 3;
            indicies[2] = 1;

            indicies[3] = 1;
            indicies[4] = 3;
            indicies[5] = 2;

            UpdateMeshLists();
        }

        void UpdateMeshLists()
        {
            Spread = Mathf.Clamp(Spread, 0f, 85f);
            var halfWidth = StartingWidth / 2f;

            verticies[0] = new Vector3(0f,halfWidth);
            verticies[1] = new Vector3(0f,-halfWidth);

            var firingDirection = MathUtilities.CartesianToPolar(Vector2.right);

            //cos (firstAngle) = MaximumLength / h
            //h = MaximumLength / cos(firstAngle)



            var firstAngle = firingDirection.x - Spread;
            var firstLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * firstAngle);
            var firstDirection = MathUtilities.PolarToCartesian(firstAngle, firstLength);

            verticies[2] = verticies[1] + (Vector3)firstDirection;

            var secondAngle = firingDirection.x + Spread;
            var secondLength = MaximumLength / Mathf.Cos(Mathf.Deg2Rad * secondAngle);
            var secondDirection = MathUtilities.PolarToCartesian(secondAngle, secondLength);

            verticies[3] = verticies[0] + (Vector3)secondDirection;

            float sourcePointX = verticies[0].x - ((verticies[3].x - verticies[0].x) * verticies[0].y / (verticies[3].y - verticies[0].y));
            Vector2 sourcePoint = new Vector2(sourcePointX, 0f);

            Debug.Log("Source Point = " + sourcePoint);

            Vector2 laserOrigin = transform.TransformPoint(sourcePoint);

            for (int i = verticies.Length - 1; i >= 0; i--)
            {
                polygonPoints[i] = verticies[i];
            }

            mainCollider.SetPath(0, polygonPoints);

            var colliderCount = Physics2D.OverlapCollider(mainCollider, new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = CollisionMask,
                useTriggers = false
            }, foundColliders);

            Debug.Log("FOUND COLLIDERS = " + colliderCount);

            raycastPoints.Clear();

            if (Physics2D.RaycastNonAlloc(transform.TransformPoint(verticies[0]), transform.TransformDirection(secondDirection).normalized, terrainHit, secondLength, CollisionMask.value) > 0)
            {
                raycastPoints.Add((terrainHit[0].point,terrainHit[0].normal));
                //Debug.DrawRay(verticies[0], secondDirection.normalized, Color.cyan);
                Debug.DrawLine(transform.TransformPoint(verticies[0]), raycastPoints[raycastPoints.Count - 1].point, Color.blue);
            }
            else
            {
                raycastPoints.Add((transform.TransformPoint((Vector2)verticies[0] + secondDirection), (laserOrigin - (Vector2)verticies[3]).normalized));
                //Debug.DrawLine(transform.position, raycastPoints[raycastPoints.Count - 1], Color.red);
            }

            for (int i = 0; i < colliderCount; i++)
            {
                var collider = foundColliders[i];

                var matrix = collider.transform.localToWorldMatrix;
                var oldRotation = collider.transform.rotation;
                var extents = collider.bounds.extents;
                var pos = collider.transform.position;

                collider.transform.rotation = Quaternion.identity;

                void AddPoint(Vector2 point, Vector2 normal)
                {
                    if (IsWithinLaser(point))
                    {
                        raycastPoints.Add((point,normal));
                    }
                }

                if (collider is BoxCollider2D box)
                {
                    var offset = box.offset;

                    AddPoint(matrix.MultiplyPoint3x4(new Vector3(extents.x + offset.x,extents.y + offset.y)),new Vector2(extents.x,extents.y).normalized);
                    AddPoint(matrix.MultiplyPoint3x4(new Vector3(-extents.x + offset.x, extents.y + offset.y)), new Vector2(-extents.x, extents.y).normalized);
                    AddPoint(matrix.MultiplyPoint3x4(new Vector3(extents.x + offset.x, -extents.y + offset.y)), new Vector2(extents.x, -extents.y).normalized);
                    AddPoint(matrix.MultiplyPoint3x4(new Vector3(-extents.x + offset.x, -extents.y + offset.y)), new Vector2(-extents.x, -extents.y).normalized);

                    //AddPoint(new Vector3(extents.x, extents.y));
                    //AddPoint(new Vector3(-extents.x, extents.y));
                    //AddPoint(new Vector3(extents.x, -extents.y));
                    //AddPoint(new Vector3(-extents.x, -extents.y));

                    //Debug.Log($"EXTENTS of {collider.gameObject.name} = ${collider.bounds.extents}");
                }
                else if (collider is EdgeCollider2D edge)
                {
                    var points = edge.points;
                    for (int p = 0; p < edge.pointCount; p++)
                    {
                        AddPoint(matrix.MultiplyPoint3x4(points[p]),Vector2.zero);
                    }
                }
                else if (collider is PolygonCollider2D poly)
                {
                    var points = poly.points;
                    var pointCount = points.Length;
                    for (int p = 0; p < pointCount; p++)
                    {
                        AddPoint(matrix.MultiplyPoint3x4(points[p]), Vector2.zero);
                    }
                }

                collider.transform.rotation = oldRotation;
            }


            if (Physics2D.RaycastNonAlloc(transform.TransformPoint(verticies[1]), transform.TransformDirection(firstDirection).normalized, terrainHit, firstLength, CollisionMask.value) > 0)
            {
                raycastPoints.Add((terrainHit[0].point,terrainHit[0].normal));
                Debug.DrawLine(transform.TransformPoint(verticies[1]), raycastPoints[raycastPoints.Count - 1].point, Color.gray);
            }
            else
            {
                raycastPoints.Add((transform.TransformPoint((Vector2)verticies[1] + firstDirection), (laserOrigin - (Vector2)verticies[2]).normalized));
                //Debug.DrawLine(transform.position, raycastPoints[raycastPoints.Count - 1], Color.Lerp(Color.yellow, Color.red, 0.5f));
            }


            //var slope = (verticies[3].y - verticies[0].y) / (verticies[3].x - verticies[0].x);

            //var sourcePointSlope = (verticies[0].y - 0f) / (verticies[0].x - xUnknown);


            //(verticies[3].y - verticies[0].y) / (verticies[3].x - verticies[0].x) = (verticies[0].y - 0f) / (verticies[0].x - xUnknown);

            //(verticies[3].x - verticies[0].x) / (verticies[3].y - verticies[0].y) = (verticies[0].x - xUnknown) / (verticies[0].y - 0f);

            //(((verticies[3].x - verticies[0].x) * (verticies[0].y - 0f)) / (verticies[3].y - verticies[0].y)) - verticies[0].x =  -xUnknown;

            //Calculate the origin point for the laser
            //float sourcePointX = -(((verticies[3].x - verticies[0].x) * verticies[0].y / (verticies[3].y - verticies[0].y)) - verticies[0].x);

            //Sorter.comparerVector = transform.TransformDirection(verticies[3] - new Vector3(-secondLength,0f));
            Sorter.comparerVector = transform.TransformDirection(verticies[3] - (Vector3)sourcePoint);
            //Sorter.relativePosition = transform.TransformPoint(new Vector3(-secondLength, 0f));
            Sorter.relativePosition = transform.TransformPoint(sourcePoint);

            //Debug.DrawRay(Sorter.relativePosition, Sorter.comparerVector);
            //Debug.DrawRay(Sorter.relativePosition, transform.TransformDirection(verticies[2] - (Vector3)sourcePoint));

            raycastPoints.Sort(Sorter);

            /*for (int i = 0; i < raycastPoints.Count; i++)
            {
                Debug.DrawLine(transform.position, raycastPoints[i],Color.Lerp(Color.yellow,Color.red,0.5f));
                Debug.Log($"Angle of {i} = {MathUtilties.CartesianToPolar((Vector2)transform.InverseTransformPoint(raycastPoints[i]) - sourcePoint).x}");

                //Debug.Log($"DOT PRODUCT OF {i} = {Vector2.Dot(Sorter.comparerVector,(raycastPoints[i] - Sorter.relativePosition))}");
            }*/

            //Vector2 laserOrigin = transform.TransformPoint(sourcePoint);

            Vector2 previousPoint = laserOrigin;
            Vector2 previousHit = laserOrigin;

            //foreach (var rcpoint in raycastPoints)
            for (int i = 0; i < raycastPoints.Count; i++)
            {
                //Debug.DrawLine(laserOrigin, raycastPoints[i].point, Color.HSVToRGB((i * 20f) / 360f,1f,1f));
            }
            //{

            //}
            //Debug.DrawLine(laserOrigin,raycastPoints[1]);


            //bool lastPointBlocked = false;

            for (int i = 0; i < raycastPoints.Count; i++)
            {
                //Debug.DrawLine(laserOrigin, laserOrigin + (raycastPoints[i].point - laserOrigin + (raycastPoints[i].normal * 0.001f)));

                //Debug.DrawRay(raycastPoints[i].point, raycastPoints[i].normal, Color.blue);

                int collisionCount = Physics2D.RaycastNonAlloc(laserOrigin, (raycastPoints[i].point - laserOrigin + (raycastPoints[i].normal * 0.001f)).normalized, multiTerrainHit, MaximumLength, CollisionMask.value);
                //Debug.Log($"COLLISION COUNT FOR POINT {i} = {collisionCount}");
                //bool valid = false;

                if (collisionCount > 0)
                {
                    //for (int c = collisionCount - 1; c >= 0; c--)
                    for (int c = 0; c < collisionCount; c++)
                    {
                        var hit = multiTerrainHit[c].point;
                        //Debug.DrawLine(laserOrigin, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                        var distanceToHit = Vector2.Distance(laserOrigin, multiTerrainHit[c].point);
                        var distanceToPoint = Vector2.Distance(laserOrigin, raycastPoints[i].point);

                        var prevDistanceToHit = Vector2.Distance(laserOrigin, previousHit);

                        if (distanceToHit > Vector2.Distance(laserOrigin,transform.position))
                        {
                            if (distanceToHit > distanceToPoint + 0.1f)
                            {
                                if (prevDistanceToHit < distanceToPoint)
                                {
                                    Debug.Log($"Hit {i} = A");
                                    Debug.DrawLine(previousHit, raycastPoints[i].point, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    Debug.DrawLine(raycastPoints[i].point, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    previousHit = hit;
                                }
                                else
                                {
                                    Debug.Log($"Hit {i} = B");
                                    Debug.DrawLine(previousHit, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    Debug.DrawLine(hit, raycastPoints[i].point, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    previousHit = raycastPoints[i].point;
                                }
                            }
                            else
                            {
                                Debug.Log($"Hit {i} = C");
                                Debug.DrawLine(previousHit, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                //Debug.DrawLine(previousPoint, raycastPoints[i], Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                previousHit = hit;
                            }

                            if (distanceToHit >= distanceToPoint - 0.01f)
                            {
                                //var hit = multiTerrainHit[c].point;
                                
                                /*var hit = multiTerrainHit[c].point;
                                Debug.DrawLine(laserOrigin, previousPoint, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                Debug.DrawLine(previousPoint, raycastPoints[i], Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                Debug.DrawLine(raycastPoints[i], hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));


                                previousPoint = raycastPoints[i];
                                previousHit = hit;*/

                                //Debug.DrawLine(laserOrigin, previousHit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                //Debug.DrawLine(previousHit, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                //Debug.DrawLine(hit, raycastPoints[i], Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                /*var hit = multiTerrainHit[c].point;

                                var fromPrevToHitDist = Vector2.Distance(previousHit, hit);

                                var fromPreviousCount = Physics2D.RaycastNonAlloc(previousHit + ((hit - previousHit).normalized * -0.005f), (hit - previousHit).normalized, multiTerrainHit, fromPrevToHitDist, CollisionMask.value);

                                bool valid = true;

                                for (int j = 0; j < fromPreviousCount; j++)
                                {
                                    Debug.Log("Hit = " + multiTerrainHit[j].distance);
                                    if (multiTerrainHit[j].distance > 0.001f && Mathf.Abs(multiTerrainHit[j].distance - fromPrevToHitDist) > 0.01f)
                                    {
                                        valid = false;
                                        break;
                                    }
                                }

                                if (valid)
                                {
                                    Debug.DrawLine(previousHit, hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    Debug.DrawLine(hit, raycastPoints[i], Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    previousHit = raycastPoints[i];
                                }
                                else
                                {
                                    Debug.DrawLine(previousHit, raycastPoints[i], Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    Debug.DrawLine(raycastPoints[i], hit, Color.HSVToRGB((i * 20f) / 360f, 1f, 0.5f));
                                    previousHit = hit;
                                }
                                previousPoint = raycastPoints[i];*/
                            }
                            break;
                        }
                    }
                }


                /*if (collisionCount > 0)
                {
                    for (int c = collisionCount - 1; c >= 0; c--)
                    {
                        Debug.Log("RAYCAST POINT = " + raycastPoints[i]);
                        Debug.Log("NEW POINT = " + multiTerrainHit[c].point);
                        Debug.Log("DISTANCE = " + Vector2.Distance(multiTerrainHit[c].point, raycastPoints[i]));
                        if (Vector2.Distance(multiTerrainHit[c].point, raycastPoints[i]) <= 0.01f)
                        {
                            valid = true;
                        }
                    }
                }
                else
                {
                    valid = true;
                }*/

                /*if (valid)
                {
                    Debug.Log("POINT DRAWN");
                    Debug.DrawLine(previousPoint, raycastPoints[i], Color.cyan);
                    previousPoint = raycastPoints[i];
                }*/

                //Debug.DrawLine(raycastPoints[i],raycastPoints[i - 1],Color.cyan);
            }

            Debug.DrawLine(previousHit, laserOrigin, Color.Lerp(Color.white,Color.red,0.5f));

            //Debug.DrawLine(transform.position,);
        }

        bool IsWithinLaser(Vector2 point)
        {
            //return true;
            //var relPoint = point - (Vector2)transform.position;
            var relPoint = transform.InverseTransformPoint(point);

            if (relPoint.x >= verticies[0].x && relPoint.x <= verticies[2].x)
            {
                var xPercentage = Mathf.InverseLerp(verticies[0].x, verticies[2].x,relPoint.x);

                return relPoint.y <= Mathf.Lerp(verticies[0].y, verticies[3].y, xPercentage) && relPoint.y >= Mathf.Lerp(verticies[1].y, verticies[2].y, xPercentage);
            }


            return false;
        }

        private void LateUpdate()
        {
            if (mainCollider == null)
            {
                Awake();
            }
            UpdateMeshLists();
        }
    }
}
