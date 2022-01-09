using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad
{
	public string TargetSceneName
	{
		get
		{
			return this.targetSceneName;
		}
	}

	public bool IsFetchAllowed { get; set; }

	public bool IsActivationAllowed { get; set; }

	public bool IsUnloadAssetsRequired { get; set; }

	public bool IsGarbageCollectRequired { get; set; }

	public bool IsFinished { get; private set; }

	public SceneLoad(MonoBehaviour runner, string targetSceneName)
	{
		this.runner = runner;
		this.targetSceneName = targetSceneName;
		this.phaseInfos = new SceneLoad.PhaseInfo[8];
		for (int i = 0; i < 8; i++)
		{
			this.phaseInfos[i] = new SceneLoad.PhaseInfo
			{
				BeginTime = null
			};
		}
	}

	public float? BeginTime
	{
		get
		{
			return this.phaseInfos[0].BeginTime;
		}
	}

	private void RecordBeginTime(SceneLoad.Phases phase)
	{
		this.phaseInfos[(int)phase].BeginTime = new float?(Time.realtimeSinceStartup);
	}

	private void RecordEndTime(SceneLoad.Phases phase)
	{
		this.phaseInfos[(int)phase].EndTime = new float?(Time.realtimeSinceStartup);
	}

	public float? GetDuration(SceneLoad.Phases phase)
	{
		SceneLoad.PhaseInfo phaseInfo = this.phaseInfos[(int)phase];
		if (phaseInfo.BeginTime != null && phaseInfo.EndTime != null)
		{
			return new float?(phaseInfo.EndTime.Value - phaseInfo.BeginTime.Value);
		}
		return null;
	}

	public void Begin()
	{
		this.runner.StartCoroutine(this.BeginRoutine());
	}

	static string MakePathRelative(string relativeTo, string path)
	{
		if (relativeTo.Last() != '\\')
		{
			relativeTo += "\\";
		}

		Uri fullPath = new Uri(path, UriKind.Absolute);
		Uri relRoot = new Uri(relativeTo, UriKind.Absolute);

		return relRoot.MakeRelativeUri(fullPath).ToString().Replace("%20"," ");
	}

	static string ConvertToProjectPath(string path)
	{
		var relative = MakePathRelative(new DirectoryInfo($"Assets{Path.DirectorySeparatorChar}..").FullName, path);
		relative = relative.Replace('\\', '/');
		return relative;
	}

	private IEnumerator BeginRoutine()
	{
		SceneAdditiveLoadConditional.loadInSequence = true;
		//yield return this.runner.StartCoroutine(ScenePreloader.FinishPendingOperations());
		this.RecordBeginTime(SceneLoad.Phases.FetchBlocked);
		while (!this.IsFetchAllowed)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.FetchBlocked);
		this.RecordBeginTime(SceneLoad.Phases.Fetch);
#if UNITY_EDITOR
		yield return null;
		/*foreach (var scene in AssetDatabase.FindAssets("*.unity"))
		{
			Debug.Log("Scene = " + AssetDatabase.GUIDToAssetPath(scene));
		}*/
		string scenePathToLoad = null;
		foreach (var sceneFile in new DirectoryInfo("Assets").GetFiles("*.unity",SearchOption.AllDirectories))
		{
			var assetPath = ConvertToProjectPath(sceneFile.FullName);
			if (assetPath.Contains(targetSceneName))
			{
				scenePathToLoad = assetPath;
				break;
			}
		}
		AsyncOperation loadOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePathToLoad ?? targetSceneName, new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Additive
		});
#else
		AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.targetSceneName, LoadSceneMode.Additive);
#endif
		//
		loadOperation.allowSceneActivation = false;
		while (loadOperation.progress < 0.9f)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.Fetch);
		if (this.FetchComplete != null)
		{
			try
			{
				this.FetchComplete();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception in responders to SceneLoad.FetchComplete. Attempting to continue load regardless.");
				Debug.LogException(exception);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.ActivationBlocked);
		while (!this.IsActivationAllowed)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.ActivationBlocked);
		this.RecordBeginTime(SceneLoad.Phases.Activation);
		if (this.WillActivate != null)
		{
			try
			{
				this.WillActivate();
			}
			catch (Exception exception2)
			{
				Debug.LogError("Exception in responders to SceneLoad.WillActivate. Attempting to continue load regardless.");
				Debug.LogException(exception2);
			}
		}
		Debug.Log("Progress = " + loadOperation.progress);
		loadOperation.allowSceneActivation = true;
		yield return loadOperation;
		Debug.Log("Progress = " + loadOperation.progress);
		this.RecordEndTime(SceneLoad.Phases.Activation);
		if (this.ActivationComplete != null)
		{
			try
			{
				this.ActivationComplete();
			}
			catch (Exception exception3)
			{
				Debug.LogError("Exception in responders to SceneLoad.ActivationComplete. Attempting to continue load regardless.");
				Debug.LogException(exception3);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.UnloadUnusedAssets);
		if (this.IsUnloadAssetsRequired)
		{
			AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
			yield return asyncOperation;
		}
		this.RecordEndTime(SceneLoad.Phases.UnloadUnusedAssets);
		this.RecordBeginTime(SceneLoad.Phases.GarbageCollect);
		/*if (this.IsGarbageCollectRequired)
		{
			GCManager.Collect();
		}*/
		this.RecordEndTime(SceneLoad.Phases.GarbageCollect);
		if (this.Complete != null)
		{
			try
			{
				this.Complete();
			}
			catch (Exception exception4)
			{
				Debug.LogError("Exception in responders to SceneLoad.Complete. Attempting to continue load regardless.");
				Debug.LogException(exception4);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.StartCall);
		yield return null;
		this.RecordEndTime(SceneLoad.Phases.StartCall);
		if (this.StartCalled != null)
		{
			try
			{
				this.StartCalled();
			}
			catch (Exception exception5)
			{
				Debug.LogError("Exception in responders to SceneLoad.StartCalled. Attempting to continue load regardless.");
				Debug.LogException(exception5);
			}
		}
		if (SceneAdditiveLoadConditional.ShouldLoadBoss)
		{
			this.RecordBeginTime(SceneLoad.Phases.LoadBoss);
			yield return this.runner.StartCoroutine(SceneAdditiveLoadConditional.LoadAll());
			this.RecordEndTime(SceneLoad.Phases.LoadBoss);
			try
			{
				if (this.BossLoaded != null)
				{
					this.BossLoaded();
				}
				if (GameManager.instance)
				{
					GameManager.instance.LoadedBoss();
				}
			}
			catch (Exception exception6)
			{
				Debug.LogError("Exception in responders to SceneLoad.BossLoaded. Attempting to continue load regardless.");
				Debug.LogException(exception6);
			}
		}
		try
		{
			//ScenePreloader.Cleanup();
		}
		catch (Exception exception7)
		{
			Debug.LogError("Exception in responders to ScenePreloader.Cleanup. Attempting to continue load regardless.");
			Debug.LogException(exception7);
		}
		this.IsFinished = true;
		if (this.Finish != null)
		{
			try
			{
				this.Finish();
				yield break;
			}
			catch (Exception exception8)
			{
				Debug.LogError("Exception in responders to SceneLoad.Finish. Attempting to continue load regardless.");
				Debug.LogException(exception8);
				yield break;
			}
		}
		yield break;
	}

	public event SceneLoad.FetchCompleteDelegate FetchComplete;

	public event SceneLoad.WillActivateDelegate WillActivate;

	public event SceneLoad.ActivationCompleteDelegate ActivationComplete;

	public event SceneLoad.CompleteDelegate Complete;

	public event SceneLoad.StartCalledDelegate StartCalled;

	public event SceneLoad.BossLoadCompleteDelegate BossLoaded;

	public event SceneLoad.FinishDelegate Finish;

	private readonly MonoBehaviour runner;

	private readonly string targetSceneName;

	public const int PhaseCount = 8;

	private readonly SceneLoad.PhaseInfo[] phaseInfos;

	public enum Phases
	{
		FetchBlocked,
		Fetch,
		ActivationBlocked,
		Activation,
		UnloadUnusedAssets,
		GarbageCollect,
		StartCall,
		LoadBoss
	}

	private class PhaseInfo
	{
		public float? BeginTime;

		public float? EndTime;
	}

	public delegate void FetchCompleteDelegate();

	public delegate void WillActivateDelegate();

	public delegate void ActivationCompleteDelegate();

	public delegate void CompleteDelegate();

	public delegate void StartCalledDelegate();

	public delegate void BossLoadCompleteDelegate();

	public delegate void FinishDelegate();
}
