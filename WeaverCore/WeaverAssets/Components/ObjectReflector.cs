using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;


namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used to create floor reflections for added objects
    /// </summary>
    public class ObjectReflector : MonoBehaviour
    {
        class ReflectionInfo
        {
            public MeshRenderer ReflectedRenderer;
            public MeshFilter ReflectedFilter;
            public Func<Mesh> MeshGetter;
            public Func<Color> ColorGetter;
            public Action<MeshRenderer, Color> ColorSetter;
            public bool IsColorPerRenderData;
        }

        [SerializeField]
        Vector2 ExtraReflectionOffset = new Vector2();

        [SerializeField]
        Color colorModifier = new Color(1, 1, 1, 0.25f);

        [SerializeField]
        float reflectorWidth = 5f;

        [SerializeField]
        System.Collections.Generic.List<Renderer> reflectedRenderers = new System.Collections.Generic.List<Renderer>();

        [NonSerialized]
        HashSet<Renderer> _objectsToReflect = new HashSet<Renderer>();

        [NonSerialized]
        Dictionary<Renderer, ReflectionInfo> reflectionMappings = new Dictionary<Renderer, ReflectionInfo>();

        [NonSerialized]
        MaterialPropertyBlock materialCopier;

        [NonSerialized]
        System.Collections.Generic.List<KeyValuePair<Renderer, ReflectionInfo>> renderersToRemove = new System.Collections.Generic.List<KeyValuePair<Renderer, ReflectionInfo>>();

        [NonSerialized]
        Mesh _quadCache;

        public IEnumerable<Renderer> ReflectedObjects => _objectsToReflect;
        public Mesh QuadMesh => _quadCache ??= CreateQuad();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            var leftSide = transform.position + new Vector3(-reflectorWidth / 2f, 0, 0);
            var rightSide = transform.position + new Vector3(reflectorWidth / 2f, 0, 0);

            Gizmos.DrawLine(leftSide, rightSide);

            Gizmos.DrawLine(leftSide - new Vector3(0f, 0.5f), leftSide + new Vector3(0f, 0.5f));
            Gizmos.DrawLine(rightSide - new Vector3(0f, 0.5f), rightSide + new Vector3(0f, 0.5f));
        }

        private void Awake()
        {
            materialCopier = new MaterialPropertyBlock();
            foreach (var rend in reflectedRenderers)
            {
                _objectsToReflect.Add(rend);
            }

            AddObjectToReflect(Player.Player1.gameObject);
        }

        static System.Collections.Generic.List<Vector3> vertexCache = new System.Collections.Generic.List<Vector3>();
        static System.Collections.Generic.List<Vector3> normalCache = new System.Collections.Generic.List<Vector3>();
        static bool colorIDSet = false;
        static int colorID = 0;

        static Mesh CreateMeshFromSprite(Sprite sprite)
        {
            Mesh mesh = new Mesh();

            vertexCache.Clear();
            normalCache.Clear();

            vertexCache.AddRange(sprite.vertices.Select(v2 => (Vector3)v2));

            /*for (int i = 0; i < vertexCache.Count; i++)
            {
                var point = vertexCache[i];
                if (point.y > maxHeight)
                {
                    point.y = maxHeight;
                }
                vertexCache[i] = point;
            }*/

            mesh.SetVertices(vertexCache);

            mesh.SetTriangles(sprite.triangles, 0);

            for (int i = 0; i < vertexCache.Count; i++)
            {
                normalCache.Add(-Vector3.forward);
            }

            mesh.SetNormals(normalCache);

            mesh.SetUVs(0, sprite.uv);

            return mesh;
        }

        static Mesh CreateQuad()
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[4]
            {
            new Vector3(0, 0, 0),
            new Vector3(1f, 0, 0),
            new Vector3(0, 1f, 0),
            new Vector3(1f, 1f, 0)
            };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
            0, 2, 1,
            2, 3, 1
            };
            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
            };
            mesh.normals = normals;

            Vector2[] uv = new Vector2[4]
            {
              new Vector2(0, 0),
              new Vector2(1, 0),
              new Vector2(0, 1),
              new Vector2(1, 1)
            };
            mesh.uv = uv;

            return mesh;
        }

        private bool ShouldRenderObject(Renderer renderer)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                return false;
            }
            var bounds = renderer.bounds;

            return bounds.max.y >= transform.position.y && bounds.max.x >= transform.position.x - (reflectorWidth / 2f) && bounds.min.x <= transform.position.x + (reflectorWidth / 2f);
        }

        private void LateUpdate()
        {
            _objectsToReflect.RemoveWhere(rend => rend == null || rend.gameObject == null);


            foreach (var mainRenderer in _objectsToReflect)
            {
                if (!colorIDSet)
                {
                    colorIDSet = true;
                    colorID = Shader.PropertyToID("_Color");
                }

                if (!reflectionMappings.ContainsKey(mainRenderer))
                {
                    var reflectedObj = new GameObject($"{mainRenderer.gameObject.name} Reflection");
                    reflectedObj.transform.SetParent(transform);
                    var reflectedRenderer = reflectedObj.AddComponent<MeshRenderer>();
                    Func<Mesh> meshGetter = null;
                    Func<Color> colorGetter = null;
                    Action<MeshRenderer, Color> colorSetter = null;
                    MeshFilter reflectedFilter = reflectedObj.AddComponent<MeshFilter>();
                    if (mainRenderer.TryGetComponent<MeshFilter>(out var sourceFilter))
                    {
                        meshGetter = () => sourceFilter.sharedMesh;
                    }
                    else if (mainRenderer.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    {
                        Mesh meshCache = null;
                        Sprite spriteCache = null;

                        meshGetter = () =>
                        {
                            if (meshCache == null || spriteCache != spriteRenderer.sprite)
                            {
                                spriteCache = spriteRenderer.sprite;
                                if (spriteCache == null)
                                {
                                    meshCache = QuadMesh;
                                }
                                else
                                {
                                    meshCache = CreateMeshFromSprite(spriteCache);
                                }
                            }
                            return meshCache;
                        };

                        colorGetter = () =>
                        {
                            if (spriteRenderer.enabled)
                            {
                                return spriteRenderer.color;
                            }
                            else
                            {
                                return default;
                            }
                        };

                        var spriteSetterProp = new MaterialPropertyBlock();

                        colorSetter = (rend, color) =>
                        {
                            rend.GetPropertyBlock(spriteSetterProp);
                            spriteSetterProp.SetColor(colorID, color);
                            rend.SetPropertyBlock(spriteSetterProp);
                        };
                    }

                    int colorIndex = mainRenderer.sharedMaterial.shader.FindPropertyIndex("_Color");
                    int mainTexIndex = mainRenderer.sharedMaterial.shader.FindPropertyIndex("_MainTex");

                    bool isColorPerRenderData = colorIndex >= 0 && (mainRenderer.sharedMaterial.shader.GetPropertyFlags(colorIndex) & UnityEngine.Rendering.ShaderPropertyFlags.PerRendererData) == UnityEngine.Rendering.ShaderPropertyFlags.PerRendererData;

                    bool isTexPerRenderData = mainTexIndex >= 0 && (mainRenderer.sharedMaterial.shader.GetPropertyFlags(mainTexIndex) & UnityEngine.Rendering.ShaderPropertyFlags.PerRendererData) == UnityEngine.Rendering.ShaderPropertyFlags.PerRendererData;

                    MaterialPropertyBlock colorGetterBlock = null;

                    if (colorGetter == null && colorIndex >= 0)
                    {
                        colorGetter = () =>
                        {
                            if (mainRenderer.enabled)
                            {
                                if (isColorPerRenderData)
                                {
                                    if (colorGetterBlock == null)
                                    {
                                        colorGetterBlock = new MaterialPropertyBlock();
                                    }
                                    mainRenderer.GetPropertyBlock(colorGetterBlock);
                                    var finalColor = colorGetterBlock.GetColor(colorID);

                                    if (finalColor == default)
                                    {
                                        finalColor = mainRenderer.material.GetColor(colorID);
                                    }

                                    return finalColor;
                                }
                                else
                                {
                                    return mainRenderer.material.GetColor(colorID);
                                }
                            }
                            else
                            {
                                return default;
                            }
                        };
                    }

                    reflectedRenderer.material = new Material(mainRenderer.sharedMaterial);
                    var propCopier = new MaterialPropertyBlock();
                    mainRenderer.GetPropertyBlock(propCopier);
                    reflectedRenderer.SetPropertyBlock(propCopier);


                    /*if (isColorPerRenderData)
                    {
                        reflectedRenderer.sharedMaterial = mainRenderer.sharedMaterial;
                        
                    }
                    else
                    {
                        reflectedRenderer.material = new Material(mainRenderer.sharedMaterial);
                    }*/

                    reflectionMappings.Add(mainRenderer, new ReflectionInfo
                    {
                        MeshGetter = meshGetter,
                        ColorGetter = colorGetter,
                        ReflectedFilter = reflectedFilter,
                        ReflectedRenderer = reflectedRenderer,
                        IsColorPerRenderData = isColorPerRenderData,
                        ColorSetter = colorSetter
                    });
                }


                var info = reflectionMappings[mainRenderer];

                info.ReflectedRenderer.enabled = ShouldRenderObject(mainRenderer);

                info.ReflectedFilter.sharedMesh = info.MeshGetter();

                info.ReflectedRenderer.sharedMaterial.CopyPropertiesFromMaterial(mainRenderer.sharedMaterial);
                /*if (info.IsColorPerRenderData)
                {
                    info.ReflectedRenderer.sharedMaterial = mainRenderer.sharedMaterial;
                }
                else
                {
                    
                }*/

                if (info.ColorGetter != null)
                {
                    if (info.ColorSetter != null)
                    {
                        info.ColorSetter(info.ReflectedRenderer, info.ColorGetter() * colorModifier);
                    }
                    else
                    {
                        mainRenderer.GetPropertyBlock(materialCopier);
                        materialCopier.SetColor(colorID, info.ColorGetter() * colorModifier);
                        info.ReflectedRenderer.SetPropertyBlock(materialCopier);
                        info.ReflectedRenderer.sharedMaterial.SetColor(colorID, info.ColorGetter() * colorModifier);
                    }
                }

                /*if (info.IsColorPerRenderData)
                {
                    
                }
                else
                {
                    if (info.ColorGetter != null)
                    {
                        
                    }
                }*/

                var heightAboveLine = mainRenderer.transform.position.y - transform.position.y;

                info.ReflectedRenderer.transform.position = mainRenderer.transform.position.With(y: transform.position.y - heightAboveLine, z: transform.position.z) + (Vector3)ExtraReflectionOffset;
                info.ReflectedRenderer.transform.rotation = mainRenderer.transform.rotation;

                var mainScale = mainRenderer.transform.lossyScale;

                info.ReflectedRenderer.transform.localScale = mainRenderer.transform.lossyScale.With(y: -mainScale.y);
            }

            //Add all renderers in the reflectionMappings dictionary that are no longer in the _objectsToReflect list
            renderersToRemove.AddRange(reflectionMappings.Where(pair => !_objectsToReflect.Contains(pair.Key)));

            foreach (var pair in renderersToRemove)
            {
                GameObject.Destroy(pair.Value.ReflectedRenderer.gameObject);
                reflectionMappings.Remove(pair.Key);
            }

            renderersToRemove.Clear();
        }


        public bool AddObjectToReflect(Renderer obj) => _objectsToReflect.Add(obj);
        public bool AddObjectToReflect(GameObject obj) => _objectsToReflect.Add(obj.GetComponent<Renderer>());


        public bool RemoveObjectToReflect(Renderer obj) => _objectsToReflect.Remove(obj);
        public bool RemoveObjectToReflect(GameObject obj) => _objectsToReflect.Remove(obj.GetComponent<Renderer>());
    }
}