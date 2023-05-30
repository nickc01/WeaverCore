using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;

[ExecuteAlways]
public class WeaverCameraLock : CameraLockArea
{
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        var camBounds = new Bounds();
        // - 14.6f
        // - 8.3f
        camBounds.min = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        camBounds.max = transform.TransformPoint(new Vector3(0.5f, 0.5f));
        Gizmos.DrawCube(camBounds.center, camBounds.size);
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            RefreshCamBounds();
        }
    }
#endif

    public void RefreshCamBounds()
    {
        var camBounds = new Bounds
        {
            min = transform.TransformPoint(new Vector3(-0.5f, -0.5f)),
            max = transform.TransformPoint(new Vector3(0.5f, 0.5f))
        };
        cameraXMin = camBounds.min.x + 14.6f;
        cameraXMax = camBounds.max.x - 14.6f;
        cameraYMin = camBounds.min.y + 8.3f;
        cameraYMax = camBounds.max.y - 8.3f;
    }

    static void Awake_Postfix(CameraLockArea __instance)
    {
        if (__instance is WeaverCameraLock camLock)
        {
            camLock.RefreshCamBounds();
            //camLock.DoDreamNailTrigger();
            //Debug.LogError("CAM LOCK INIT");
        }
    }

    private void OnEnable()
    {
        RefreshCamBounds();
    }


    [OnHarmonyPatch]
    static void Init(HarmonyPatcher patcher)
    {
        var postFixMethod = typeof(WeaverCameraLock).GetMethod(nameof(Awake_Postfix), BindingFlags.Static | BindingFlags.NonPublic);
        patcher.Patch(typeof(CameraLockArea).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance), null, postFixMethod);
    }
}
