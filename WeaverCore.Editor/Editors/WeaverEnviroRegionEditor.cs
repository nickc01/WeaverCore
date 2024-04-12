using System.Collections.Generic;
using UnityEditor;
using WeaverCore;

[CustomEditor(typeof(WeaverEnviroRegion))]
[CanEditMultipleObjects]
public class WeaverEnviroRegionEditor : ExclusionEditor
{
    public override IEnumerable<string> PropertiesToExclude() => new System.Collections.Generic.List<string>
    {
        "environmentType"
    };
}
