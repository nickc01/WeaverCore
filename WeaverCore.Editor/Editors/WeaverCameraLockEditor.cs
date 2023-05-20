using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeaverCore.Components;

[CustomEditor(typeof(WeaverCameraLock))]
public class WeaverCameraLockEditor : ExclusionEditor
{
    public override IEnumerable<string> PropertiesToExclude() => new List<string>
    {
        nameof(WeaverCameraLock.cameraXMin),
        nameof(WeaverCameraLock.cameraXMax),
        nameof(WeaverCameraLock.cameraYMin),
        nameof(WeaverCameraLock.cameraYMax),
    };
}
