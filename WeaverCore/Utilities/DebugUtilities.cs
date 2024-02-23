using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class DebugUtilities
    {
        public static void DrawCube(Vector3 center, Color color, float xSize, float ySize = -1f, float zSize = -1f, float duration = 0f)
        {
            if (ySize < 0)
            {
                ySize = xSize;
            }

            if (ySize < 0)
            {
                ySize = xSize;
            }

            var halfX = xSize / 2f;
            var halfY = ySize / 2f;
            var halfZ = zSize / 2f;

            Debug.DrawLine(center + new Vector3(-halfX, -halfY, -halfZ), center + new Vector3(halfX, -halfY, -halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, -halfY, -halfZ), center + new Vector3(halfX, halfY, -halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, halfY, -halfZ), center + new Vector3(-halfX, halfY, -halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(-halfX, halfY, -halfZ), center + new Vector3(-halfX, -halfY, -halfZ), color, duration);

            Debug.DrawLine(center + new Vector3(-halfX, -halfY, -halfZ), center + new Vector3(-halfX, -halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, -halfY, -halfZ), center + new Vector3(halfX, -halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, halfY, -halfZ), center + new Vector3(halfX, halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(-halfX, halfY, -halfZ), center + new Vector3(-halfX, halfY, halfZ), color, duration);

            Debug.DrawLine(center + new Vector3(-halfX, -halfY, halfZ), center + new Vector3(halfX, -halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, -halfY, halfZ), center + new Vector3(halfX, halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(halfX, halfY, halfZ), center + new Vector3(-halfX, halfY, halfZ), color, duration);
            Debug.DrawLine(center + new Vector3(-halfX, halfY, halfZ), center + new Vector3(-halfX, -halfY, halfZ), color, duration);
        }
    }
}
