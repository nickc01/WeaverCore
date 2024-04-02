using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using WeaverCore;
using WeaverCore.Components;


[CustomEditor(typeof(DreamNailable))]
public class DreamNailableEditor : ExclusionEditor
{
	public override IEnumerable<string> PropertiesToExclude() => new System.Collections.Generic.List<string>
	{
		"dreamImpactPrefab",
		"convoAmount",
		"convoTitle"
	};
}
