using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAdditiveLoadConditional : MonoBehaviour
{

	public static bool ShouldLoadBoss
	{
		get
		{
			return SceneAdditiveLoadConditional.additiveSceneLoads != null && SceneAdditiveLoadConditional.additiveSceneLoads.Count > 0;
		}
	}

	public static IEnumerator LoadAll()
	{
		yield break;
	}

	static SceneAdditiveLoadConditional()
	{
		SceneAdditiveLoadConditional.additiveSceneLoads = new List<SceneAdditiveLoadConditional>();
		SceneAdditiveLoadConditional.loadInSequence = false;
	}

	private static List<SceneAdditiveLoadConditional> additiveSceneLoads;

	public static bool loadInSequence;
}
