using System.Collections.Generic;
using GlobalEnums;
using UnityEngine;
using UnityEngine.Audio;

public class TransitionPoint : MonoBehaviour
{
	public delegate void BeforeTransitionEvent();

	private bool activated;

	[Header("Door Type Gate Settings")]
	[Space(5f)]
	[Tooltip("If set to true, the player will not automatically use the TransitionPoint when colliding with it")]
	public bool isADoor;

	//Not sure what this does
	[HideInInspector]
	public bool dontWalkOutOfDoor;

	[Header("Gate Entry")]
	[Tooltip("The wait time before entering from this gate (not the target gate).")]
	public float entryDelay;

	[Tooltip("Should the player always face right when entering the scene from this transition point?")]
	public bool alwaysEnterRight;

	[Tooltip("Should the player always face left when entering the scene from this transition point?")]
	public bool alwaysEnterLeft;

	[Header("Force Hard Land (Top Gates Only)")]
	[Space(5f)]
	[Tooltip("Should the player land with a loud thud when entering the scene? Only works if the gate type is set to \"top\"")]
	public bool hardLandOnExit;

	[Header("Destination Scene")]
	[Space(5f)]
	[Tooltip("The destination scene the player will be transported to when touching this transition point")]
	public string targetScene;

	[Tooltip("The name of the TransitionPoint the player will be placed at when traveling to the destination scene")]
	public string entryPoint;

	[Tooltip("A positional offset applied to the player when they travel to the destination scene")]
	public Vector2 entryOffset;

	private bool alwaysUnloadUnusedAssets;


	[Header("Hazard Respawn")]
	[Space(5f)]
	[Tooltip("When the player travels to the destination scene, should the TransitionPoint NOT also be used as a hazard respawn point?")]
	public bool nonHazardGate;

	[Tooltip("The respawn marker that is used when \"nonHazardGate\" is set to true")]
	public HazardRespawnMarker respawnMarker;

	protected AudioMixerSnapshot atmosSnapshot;

	protected AudioMixerSnapshot enviroSnapshot;

	protected AudioMixerSnapshot actorSnapshot;

	protected AudioMixerSnapshot musicSnapshot;

	private Color myGreen = new Color(0f, 0.8f, 0f, 0.5f);

	[Header("Cosmetics")]
	public GameManager.SceneLoadVisualizations sceneLoadVisualization;

	[Tooltip("If set to true, the default fade-in-from-black transition does not play, allowing for custom transitions. This should be checked if the player is dream warping into this scene")]
	public bool customFade;

	bool forceWaitFetch;

	private static List<TransitionPoint> transitionPoints;

	public static string lastEntered = "";

	public static List<TransitionPoint> TransitionPoints => transitionPoints;

	public event BeforeTransitionEvent OnBeforeTransition;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		transitionPoints = new List<TransitionPoint>();
	}

	protected void Awake()
	{
		transitionPoints.Add(this);
	}

	protected void OnDestroy()
	{
		transitionPoints.Remove(this);
	}

	private void Start()
	{
		if (!nonHazardGate && respawnMarker == null)
		{
			Debug.LogError("Transition Gate " + base.name + " in " + GameManager.instance.sceneName + " does not have its respawn marker set in inspector.");
		}
	}

	private void OnTriggerEnter2D(Collider2D movingObj)
	{
		if (isADoor || movingObj.gameObject.layer != 9 || GameManager.instance.gameState != GameState.PLAYING)
		{
			return;
		}
		if (!string.IsNullOrEmpty(targetScene) && !string.IsNullOrEmpty(entryPoint))
		{
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
			lastEntered = base.gameObject.name;
			if (this.OnBeforeTransition != null)
			{
				this.OnBeforeTransition();
			}
			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = targetScene,
				EntryGateName = entryPoint,
				HeroLeaveDirection = GetGatePosition(),
				EntryDelay = entryDelay,
				WaitForSceneTransitionCameraFade = true,
				Visualization = sceneLoadVisualization,
				AlwaysUnloadUnusedAssets = alwaysUnloadUnusedAssets,
				forceWaitFetch = forceWaitFetch
			});
		}
		else
		{
			Debug.LogError(GameManager.instance.sceneName + " " + base.name + " no target scene has been set on this gate.");
		}
	}

	private void OnTriggerStay2D(Collider2D movingObj)
	{
		if (!activated)
		{
			OnTriggerEnter2D(movingObj);
		}
	}

	private void OnDrawGizmos()
	{
		if (base.transform != null)
		{
			var collider = GetComponent<BoxCollider2D>();
			if (collider != null)
			{
				Vector3 position = base.transform.position + new Vector3(0f, collider.bounds.extents.y + 1.5f, 0f);
#if UNITY_EDITOR
				UnityEditor.Handles.color = myGreen;
				UnityEditor.Handles.Label(position, targetScene);
#endif
				var scale = transform.localScale;
				Gizmos.color = myGreen;
				Gizmos.DrawCube(transform.position + new Vector3(collider.offset.x * scale.x, collider.offset.y * scale.y), new Vector3(collider.size.x * scale.x, collider.size.y * scale.y));
			}
		}
	}

	public GatePosition GetGatePosition()
	{
		string text = base.name;
		if (text.Contains("top"))
		{
			return GatePosition.top;
		}
		if (text.Contains("right"))
		{
			return GatePosition.right;
		}
		if (text.Contains("left"))
		{
			return GatePosition.left;
		}
		if (text.Contains("bot"))
		{
			return GatePosition.bottom;
		}
		if (text.Contains("door") || isADoor)
		{
			return GatePosition.door;
		}
		Debug.LogError("Gate name " + text + "does not conform to a valid gate position type. Make sure gate name has the form 'left1'");
		return GatePosition.unknown;
	}

	public void SetTargetScene(string newScene)
	{
		targetScene = newScene;
	}
}