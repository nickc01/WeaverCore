using GlobalEnums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x02000421 RID: 1057
[RequireComponent(typeof(BoxCollider2D))]
public class TransitionPoint : MonoBehaviour
{
	// Token: 0x14000031 RID: 49
	// (add) Token: 0x060017CE RID: 6094 RVA: 0x000702F4 File Offset: 0x0006E4F4
	// (remove) Token: 0x060017CF RID: 6095 RVA: 0x0007032C File Offset: 0x0006E52C
	public event TransitionPoint.BeforeTransitionEvent OnBeforeTransition;

	// Token: 0x17000313 RID: 787
	// (get) Token: 0x060017D0 RID: 6096 RVA: 0x00070361 File Offset: 0x0006E561
	public static List<TransitionPoint> TransitionPoints => TransitionPoint.transitionPoints;

	// Token: 0x060017D1 RID: 6097 RVA: 0x00070368 File Offset: 0x0006E568
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		TransitionPoint.transitionPoints = new List<TransitionPoint>();
	}

	// Token: 0x060017D2 RID: 6098 RVA: 0x00070374 File Offset: 0x0006E574
	protected void Awake()
	{
		TransitionPoint.transitionPoints.Add(this);
	}

	// Token: 0x060017D3 RID: 6099 RVA: 0x00070381 File Offset: 0x0006E581
	protected void OnDestroy()
	{
		TransitionPoint.transitionPoints.Remove(this);
	}

	// Token: 0x060017D4 RID: 6100 RVA: 0x00070390 File Offset: 0x0006E590
	private void Start()
	{
		gm = GameManager.instance;
		playerData = PlayerData.instance;
		if (!nonHazardGate && respawnMarker == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Transition Gate ",
				base.name,
				" in ",
				gm.sceneName,
				" does not have its respawn marker set in inspector."
			}));
		}
	}

	// Token: 0x060017D5 RID: 6101 RVA: 0x00070408 File Offset: 0x0006E608
	private void OnTriggerEnter2D(Collider2D movingObj)
	{
		if (!isADoor && movingObj.gameObject.layer == 9 && gm.gameState == GameState.PLAYING)
		{
			if (!string.IsNullOrEmpty(targetScene) && !string.IsNullOrEmpty(entryPoint))
			{
				/*if (this.customFadeFSM)
				{
					this.customFadeFSM.SendEvent("FADE");
				}*/
				if (atmosSnapshot != null)
				{
					atmosSnapshot.TransitionTo(1.5f);
				}
				if (enviroSnapshot != null)
				{
					enviroSnapshot.TransitionTo(1.5f);
				}
				if (actorSnapshot != null)
				{
					actorSnapshot.TransitionTo(1.5f);
				}
				if (musicSnapshot != null)
				{
					musicSnapshot.TransitionTo(1.5f);
				}
				activated = true;
				TransitionPoint.lastEntered = base.gameObject.name;
				if (OnBeforeTransition != null)
				{
					OnBeforeTransition();
				}
				gm.BeginSceneTransition(new GameManager.SceneLoadInfo
				{
					SceneName = targetScene,
					EntryGateName = entryPoint,
					HeroLeaveDirection = new GatePosition?(GetGatePosition()),
					EntryDelay = entryDelay,
					WaitForSceneTransitionCameraFade = true,
					PreventCameraFadeOut = false,
					Visualization = sceneLoadVisualization,
					AlwaysUnloadUnusedAssets = alwaysUnloadUnusedAssets,
					forceWaitFetch = forceWaitFetch
				});
				return;
			}
			Debug.LogError(gm.sceneName + " " + base.name + " no target scene has been set on this gate.");
		}
	}

	// Token: 0x060017D6 RID: 6102 RVA: 0x000705CA File Offset: 0x0006E7CA
	private void OnTriggerStay2D(Collider2D movingObj)
	{
		if (!activated)
		{
			OnTriggerEnter2D(movingObj);
		}
	}

	// Token: 0x060017D7 RID: 6103 RVA: 0x000705DC File Offset: 0x0006E7DC
	private void OnDrawGizmos()
	{
		if (base.transform != null)
		{
			Vector3 position = base.transform.position + new Vector3(0f, base.GetComponent<BoxCollider2D>().bounds.extents.y + 1.5f, 0f);
#if UNITY_EDITOR
			UnityEditor.Handles.color = myGreen;
			UnityEditor.Handles.Label(position, targetScene);
#endif
			//GizmoUtility.DrawText(GUI.skin, this.targetScene, position, new Color?(this.myGreen), 10, 0f);
		}
	}

	// Token: 0x060017D8 RID: 6104 RVA: 0x00070664 File Offset: 0x0006E864
	public GatePosition GetGatePosition()
	{
		string name = base.name;
		if (name.Contains("top"))
		{
			return GatePosition.top;
		}
		if (name.Contains("right"))
		{
			return GatePosition.right;
		}
		if (name.Contains("left"))
		{
			return GatePosition.left;
		}
		if (name.Contains("bot"))
		{
			return GatePosition.bottom;
		}
		if (name.Contains("door") || isADoor)
		{
			return GatePosition.door;
		}
		Debug.LogError("Gate name " + name + "does not conform to a valid gate position type. Make sure gate name has the form 'left1'");
		return GatePosition.unknown;
	}

	// Token: 0x060017D9 RID: 6105 RVA: 0x000706E1 File Offset: 0x0006E8E1
	public void SetTargetScene(string newScene)
	{
		targetScene = newScene;
	}

	// Token: 0x060017DB RID: 6107 RVA: 0x00070711 File Offset: 0x0006E911
	// Note: this type is marked as 'beforefieldinit'.
	static TransitionPoint()
	{
		TransitionPoint.lastEntered = "";
	}

	// Token: 0x04001C9D RID: 7325
	private GameManager gm;

	// Token: 0x04001C9E RID: 7326
	private PlayerData playerData;

	// Token: 0x04001C9F RID: 7327
	private bool activated;

	// Token: 0x04001CA0 RID: 7328
	[Header("Door Type Gate Settings")]
	[Space(5f)]
	public bool isADoor;

	// Token: 0x04001CA1 RID: 7329
	public bool dontWalkOutOfDoor;

	// Token: 0x04001CA2 RID: 7330
	[Header("Gate Entry")]
	[Tooltip("The wait time before entering from this gate (not the target gate).")]
	public float entryDelay;

	// Token: 0x04001CA3 RID: 7331
	public bool alwaysEnterRight;

	// Token: 0x04001CA4 RID: 7332
	public bool alwaysEnterLeft;

	// Token: 0x04001CA5 RID: 7333
	[Header("Force Hard Land (Top Gates Only)")]
	[Space(5f)]
	public bool hardLandOnExit;

	// Token: 0x04001CA6 RID: 7334
	[Header("Destination Scene")]
	[Space(5f)]
	public string targetScene;

	// Token: 0x04001CA7 RID: 7335
	public string entryPoint;

	// Token: 0x04001CA8 RID: 7336
	public Vector2 entryOffset;

	// Token: 0x04001CA9 RID: 7337
	[SerializeField]
	private bool alwaysUnloadUnusedAssets;

	// Token: 0x04001CAA RID: 7338
	//public PlayMakerFSM customFadeFSM;

	// Token: 0x04001CAB RID: 7339
	[Header("Hazard Respawn")]
	[Space(5f)]
	public bool nonHazardGate;

	// Token: 0x04001CAC RID: 7340
	public HazardRespawnMarker respawnMarker;

	// Token: 0x04001CAD RID: 7341
	[Header("Set Audio Snapshots")]
	[Space(5f)]
	public AudioMixerSnapshot atmosSnapshot;

	// Token: 0x04001CAE RID: 7342
	public AudioMixerSnapshot enviroSnapshot;

	// Token: 0x04001CAF RID: 7343
	public AudioMixerSnapshot actorSnapshot;

	// Token: 0x04001CB0 RID: 7344
	public AudioMixerSnapshot musicSnapshot;

	// Token: 0x04001CB1 RID: 7345
	private Color myGreen = new Color(0f, 0.8f, 0f, 0.5f);

	// Token: 0x04001CB2 RID: 7346
	[Header("Cosmetics")]
	public GameManager.SceneLoadVisualizations sceneLoadVisualization;

	// Token: 0x04001CB3 RID: 7347
	public bool customFade;

	// Token: 0x04001CB4 RID: 7348
	public bool forceWaitFetch;

	// Token: 0x04001CB5 RID: 7349
	private static List<TransitionPoint> transitionPoints;

	// Token: 0x04001CB6 RID: 7350
	public static string lastEntered;

	// Token: 0x02000422 RID: 1058
	// (Invoke) Token: 0x060017DD RID: 6109
	public delegate void BeforeTransitionEvent();
}
