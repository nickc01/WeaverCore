using System.Collections.Generic;
using UnityEditor;
using WeaverCore.Components;

[CustomEditor(typeof(WeaverAreaTitleController))]
public class WeaverAreaTitleControllerEditor : ExclusionEditor
{
    public override IEnumerable<string> PropertiesToExclude()
    {
        yield return "doorTrigger";
        yield return "displayRight";
        yield return "onlyOnRevisit";
        yield return "areaTitle";
    }
}
