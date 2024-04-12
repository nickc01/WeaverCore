using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class MeshUtilities
    {
        static System.Collections.Generic.List<Vector3> vertexCache = new System.Collections.Generic.List<Vector3>();
        static System.Collections.Generic.List<Vector3> normalCache = new System.Collections.Generic.List<Vector3>();

        public static Mesh CreateMeshFromSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return CreateQuad();
            }
            Mesh mesh = new Mesh();

            vertexCache.Clear();
            normalCache.Clear();

            vertexCache.AddRange(sprite.vertices.Select(v2 => (Vector3)v2));
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

        public static Mesh CreateQuad()
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
    }
}
