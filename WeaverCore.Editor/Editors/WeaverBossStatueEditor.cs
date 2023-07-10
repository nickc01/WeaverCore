using System.Collections.Generic;
using UnityEditor;
using WeaverCore.Components;

[CustomEditor(typeof(WeaverBossStatue))]
public class WeaverBossStatueEditor : ExclusionEditor
{
    public override IEnumerable<string> PropertiesToExclude()
    {
        yield return nameof(BossStatue.bossDetails);
        yield return nameof(BossStatue.dreamBossDetails);
    }
}
