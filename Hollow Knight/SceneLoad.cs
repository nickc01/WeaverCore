using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020004EE RID: 1262
public class SceneLoad
{
	// Token: 0x1700035B RID: 859
	// (get) Token: 0x06001BCC RID: 7116 RVA: 0x00084190 File Offset: 0x00082390
	public string TargetSceneName
	{
		get
		{
			return this.targetSceneName;
		}
	}

	// Token: 0x1700035C RID: 860
	// (get) Token: 0x06001BCD RID: 7117 RVA: 0x00084198 File Offset: 0x00082398
	// (set) Token: 0x06001BCE RID: 7118 RVA: 0x000841A0 File Offset: 0x000823A0
	public bool IsFetchAllowed { get; set; }

	// Token: 0x1700035D RID: 861
	// (get) Token: 0x06001BCF RID: 7119 RVA: 0x000841A9 File Offset: 0x000823A9
	// (set) Token: 0x06001BD0 RID: 7120 RVA: 0x000841B1 File Offset: 0x000823B1
	public bool IsActivationAllowed { get; set; }

	// Token: 0x1700035E RID: 862
	// (get) Token: 0x06001BD1 RID: 7121 RVA: 0x000841BA File Offset: 0x000823BA
	// (set) Token: 0x06001BD2 RID: 7122 RVA: 0x000841C2 File Offset: 0x000823C2
	public bool IsUnloadAssetsRequired { get; set; }

	// Token: 0x1700035F RID: 863
	// (get) Token: 0x06001BD3 RID: 7123 RVA: 0x000841CB File Offset: 0x000823CB
	// (set) Token: 0x06001BD4 RID: 7124 RVA: 0x000841D3 File Offset: 0x000823D3
	public bool IsGarbageCollectRequired { get; set; }

	// Token: 0x17000360 RID: 864
	// (get) Token: 0x06001BD5 RID: 7125 RVA: 0x000841DC File Offset: 0x000823DC
	// (set) Token: 0x06001BD6 RID: 7126 RVA: 0x000841E4 File Offset: 0x000823E4
	public bool IsFinished { get; private set; }

	// Token: 0x06001BD7 RID: 7127 RVA: 0x000841F0 File Offset: 0x000823F0
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

	// Token: 0x17000361 RID: 865
	// (get) Token: 0x06001BD8 RID: 7128 RVA: 0x00084242 File Offset: 0x00082442
	public float? BeginTime
	{
		get
		{
			return this.phaseInfos[0].BeginTime;
		}
	}

	// Token: 0x06001BD9 RID: 7129 RVA: 0x00084251 File Offset: 0x00082451
	private void RecordBeginTime(SceneLoad.Phases phase)
	{
		this.phaseInfos[(int)phase].BeginTime = new float?(Time.realtimeSinceStartup);
	}

	// Token: 0x06001BDA RID: 7130 RVA: 0x0008426A File Offset: 0x0008246A
	private void RecordEndTime(SceneLoad.Phases phase)
	{
		this.phaseInfos[(int)phase].EndTime = new float?(Time.realtimeSinceStartup);
	}

	// Token: 0x06001BDB RID: 7131 RVA: 0x00084284 File Offset: 0x00082484
	public float? GetDuration(SceneLoad.Phases phase)
	{
		SceneLoad.PhaseInfo phaseInfo = this.phaseInfos[(int)phase];
		if (phaseInfo.BeginTime != null && phaseInfo.EndTime != null)
		{
			return new float?(phaseInfo.EndTime.Value - phaseInfo.BeginTime.Value);
		}
		return null;
	}

	// Token: 0x06001BDC RID: 7132 RVA: 0x000842DA File Offset: 0x000824DA
	public void Begin()
	{
		this.runner.StartCoroutine(this.BeginRoutine());
	}

	// Token: 0x06001BDD RID: 7133 RVA: 0x000842EE File Offset: 0x000824EE
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
		AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.targetSceneName, LoadSceneMode.Additive);
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
		loadOperation.allowSceneActivation = true;
		yield return loadOperation;
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

	// Token: 0x14000037 RID: 55
	// (add) Token: 0x06001BDE RID: 7134 RVA: 0x00084300 File Offset: 0x00082500
	// (remove) Token: 0x06001BDF RID: 7135 RVA: 0x00084338 File Offset: 0x00082538
	public event SceneLoad.FetchCompleteDelegate FetchComplete;

	// Token: 0x14000038 RID: 56
	// (add) Token: 0x06001BE0 RID: 7136 RVA: 0x00084370 File Offset: 0x00082570
	// (remove) Token: 0x06001BE1 RID: 7137 RVA: 0x000843A8 File Offset: 0x000825A8
	public event SceneLoad.WillActivateDelegate WillActivate;

	// Token: 0x14000039 RID: 57
	// (add) Token: 0x06001BE2 RID: 7138 RVA: 0x000843E0 File Offset: 0x000825E0
	// (remove) Token: 0x06001BE3 RID: 7139 RVA: 0x00084418 File Offset: 0x00082618
	public event SceneLoad.ActivationCompleteDelegate ActivationComplete;

	// Token: 0x1400003A RID: 58
	// (add) Token: 0x06001BE4 RID: 7140 RVA: 0x00084450 File Offset: 0x00082650
	// (remove) Token: 0x06001BE5 RID: 7141 RVA: 0x00084488 File Offset: 0x00082688
	public event SceneLoad.CompleteDelegate Complete;

	// Token: 0x1400003B RID: 59
	// (add) Token: 0x06001BE6 RID: 7142 RVA: 0x000844C0 File Offset: 0x000826C0
	// (remove) Token: 0x06001BE7 RID: 7143 RVA: 0x000844F8 File Offset: 0x000826F8
	public event SceneLoad.StartCalledDelegate StartCalled;

	// Token: 0x1400003C RID: 60
	// (add) Token: 0x06001BE8 RID: 7144 RVA: 0x00084530 File Offset: 0x00082730
	// (remove) Token: 0x06001BE9 RID: 7145 RVA: 0x00084568 File Offset: 0x00082768
	public event SceneLoad.BossLoadCompleteDelegate BossLoaded;

	// Token: 0x1400003D RID: 61
	// (add) Token: 0x06001BEA RID: 7146 RVA: 0x000845A0 File Offset: 0x000827A0
	// (remove) Token: 0x06001BEB RID: 7147 RVA: 0x000845D8 File Offset: 0x000827D8
	public event SceneLoad.FinishDelegate Finish;

	// Token: 0x040021C0 RID: 8640
	private readonly MonoBehaviour runner;

	// Token: 0x040021C1 RID: 8641
	private readonly string targetSceneName;

	// Token: 0x040021C2 RID: 8642
	public const int PhaseCount = 8;

	// Token: 0x040021C3 RID: 8643
	private readonly SceneLoad.PhaseInfo[] phaseInfos;

	// Token: 0x020004EF RID: 1263
	public enum Phases
	{
		// Token: 0x040021D1 RID: 8657
		FetchBlocked,
		// Token: 0x040021D2 RID: 8658
		Fetch,
		// Token: 0x040021D3 RID: 8659
		ActivationBlocked,
		// Token: 0x040021D4 RID: 8660
		Activation,
		// Token: 0x040021D5 RID: 8661
		UnloadUnusedAssets,
		// Token: 0x040021D6 RID: 8662
		GarbageCollect,
		// Token: 0x040021D7 RID: 8663
		StartCall,
		// Token: 0x040021D8 RID: 8664
		LoadBoss
	}

	// Token: 0x020004F0 RID: 1264
	private class PhaseInfo
	{
		// Token: 0x040021D9 RID: 8665
		public float? BeginTime;

		// Token: 0x040021DA RID: 8666
		public float? EndTime;
	}

	// Token: 0x020004F1 RID: 1265
	// (Invoke) Token: 0x06001BEE RID: 7150
	public delegate void FetchCompleteDelegate();

	// Token: 0x020004F2 RID: 1266
	// (Invoke) Token: 0x06001BF2 RID: 7154
	public delegate void WillActivateDelegate();

	// Token: 0x020004F3 RID: 1267
	// (Invoke) Token: 0x06001BF6 RID: 7158
	public delegate void ActivationCompleteDelegate();

	// Token: 0x020004F4 RID: 1268
	// (Invoke) Token: 0x06001BFA RID: 7162
	public delegate void CompleteDelegate();

	// Token: 0x020004F5 RID: 1269
	// (Invoke) Token: 0x06001BFE RID: 7166
	public delegate void StartCalledDelegate();

	// Token: 0x020004F6 RID: 1270
	// (Invoke) Token: 0x06001C02 RID: 7170
	public delegate void BossLoadCompleteDelegate();

	// Token: 0x020004F7 RID: 1271
	// (Invoke) Token: 0x06001C06 RID: 7174
	public delegate void FinishDelegate();
}
