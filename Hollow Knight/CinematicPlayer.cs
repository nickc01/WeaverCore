using System.Collections;
using GlobalEnums;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshRenderer))]
public class CinematicPlayer : MonoBehaviour
{
	public enum MovieTrigger
	{
		ON_START = 0,
		MANUAL_TRIGGER = 1
	}

	public enum FadeInSpeed
	{
		NORMAL = 0,
		SLOW = 1,
		NONE = 2
	}

	public enum FadeOutSpeed
	{
		NORMAL = 0,
		SLOW = 1,
		NONE = 2
	}

	public enum VideoType
	{
		OpeningCutscene = 0,
		StagTravel = 1,
		InGameVideo = 2,
		OpeningPrologue = 3,
		EndingA = 4,
		EndingB = 5,
		EndingC = 6,
		EndingGG = 7
	}

	[SerializeField]
	private CinematicVideoReference videoClip;

	private CinematicVideoPlayer cinematicVideoPlayer;

	[SerializeField]
	private AudioSource additionalAudio;

	[SerializeField]
	private bool additionalAudioContinuesPastVideo;

	[SerializeField]
	private MeshRenderer selfBlanker;

	[Header("Cinematic Settings")]
	[Tooltip("Determines what will trigger the video playing.")]
	public MovieTrigger playTrigger;

	[Tooltip("The speed of the fade in, comes in different flavours.")]
	public FadeInSpeed fadeInSpeed;

	[Tooltip("The amount of time to wait before fading in the camera. Camera will stay black and the video will play.")]
	[Range(0f, 10f)]
	public float delayBeforeFadeIn;

	[Tooltip("Allows the player to skip the video.")]
	public SkipPromptMode skipMode;

	[Tooltip("Prevents the skip action from taking place until the lock is released. Useful for animators delaying skip feature.")]
	public bool startSkipLocked;

	[Tooltip("The speed of the fade in, comes in different flavours.")]
	public FadeOutSpeed fadeOutSpeed;

	[Tooltip("Video keeps looping until the player is explicitly told to stop.")]
	public bool loopVideo;

	[Space(6f)]
	[Tooltip("The name of the scene to load when the video ends. Leaving this blank will load the \"next scene\" as set in PlayerData.")]
	public VideoType videoType;

	public CinematicVideoFaderStyles faderStyle;

	private AudioSource audioSource;

	private MeshRenderer myRenderer;

	private GameManager gm;

	//private UIManager ui;

	private PlayerData pd;

	//private PlayMakerFSM cameraFSM;

	private bool videoTriggered;

	private bool loadingLevel;

	[SerializeField]
	[HideInInspector]
	private AudioMixerSnapshot masterOff;

	[SerializeField]
	[HideInInspector]
	private AudioMixerSnapshot masterResume;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		myRenderer = GetComponent<MeshRenderer>();
		if (videoType == VideoType.InGameVideo)
		{
			myRenderer.enabled = false;
		}
	}

	protected void OnDestroy()
	{
		if (cinematicVideoPlayer != null)
		{
			cinematicVideoPlayer.Dispose();
			cinematicVideoPlayer = null;
		}
	}

	private void Start()
	{
		gm = GameManager.instance;
		//ui = UIManager.instance;
		pd = PlayerData.instance;
		/*if (startSkipLocked)
		{
			gm.inputHandler.SetSkipMode(SkipPromptMode.NOT_SKIPPABLE);
		}
		else
		{
			gm.inputHandler.SetSkipMode(skipMode);
		}*/
		if (playTrigger == MovieTrigger.ON_START)
		{
			StartCoroutine(StartVideo());
		}
	}

	private void Update()
	{
		if (cinematicVideoPlayer != null)
		{
			cinematicVideoPlayer.Update();
		}
		if (Time.frameCount % 10 == 0)
		{
			Update10();
		}
	}

	private void Update10()
	{
		if ((cinematicVideoPlayer == null || (!cinematicVideoPlayer.IsLoading && !cinematicVideoPlayer.IsPlaying)) && !loadingLevel && videoTriggered)
		{
			if (videoType == VideoType.InGameVideo)
			{
				FinishInGameVideo();
			}
			else
			{
				FinishVideo();
			}
		}
	}

	public IEnumerator SkipVideo()
	{
		if (!videoTriggered)
		{
			yield break;
		}
		if (videoType == VideoType.InGameVideo)
		{
			if (fadeOutSpeed != FadeOutSpeed.NONE)
			{
				float duration = 0f;
				if (fadeOutSpeed == FadeOutSpeed.NORMAL)
				{
					duration = 0.5f;
				}
				else if (fadeOutSpeed == FadeOutSpeed.SLOW)
				{
					duration = 2.3f;
				}
				selfBlanker.enabled = true;
				float timer = 0f;
				while (videoTriggered && timer < duration)
				{
					float a = Mathf.Clamp01(timer / duration);
					selfBlanker.material.color = new Color(0f, 0f, 0f, a);
					yield return null;
					timer += Time.unscaledDeltaTime;
				}
			}
			else
			{
				yield return null;
			}
		}
		else if (fadeOutSpeed == FadeOutSpeed.NORMAL)
		{
            WeaverTypeHelpers.BroadcastEvent("JUST FADE", gameObject);
			//PlayMakerFSM.BroadcastEvent("JUST FADE");
			yield return new WaitForSeconds(0.5f);
		}
		else if (fadeOutSpeed == FadeOutSpeed.SLOW)
		{
            WeaverTypeHelpers.BroadcastEvent("START FADE", gameObject);
			//PlayMakerFSM.BroadcastEvent("START FADE");
			yield return new WaitForSeconds(2.3f);
		}
		else
		{
			yield return null;
		}
		if (cinematicVideoPlayer != null)
		{
			cinematicVideoPlayer.Stop();
		}
	}

	public void TriggerStartVideo()
	{
		StartCoroutine(StartVideo());
	}

	public void TriggerStopVideo()
	{
		if (videoType == VideoType.InGameVideo)
		{
			StartCoroutine(SkipVideo());
		}
	}

	public void UnlockSkip()
	{
		//gm.inputHandler.SetSkipMode(skipMode);
	}

	private IEnumerator StartVideo()
	{
        var camera = GameObject.FindObjectOfType<CameraController>();
		if (masterOff != null)
		{
			masterOff.TransitionTo(0f);
		}
		videoTriggered = true;
		if (videoType == VideoType.InGameVideo)
		{
			gm.gameState = GameState.CUTSCENE;
			if (cinematicVideoPlayer == null)
			{
				Debug.LogFormat("Creating new CinematicVideoPlayer for in game video");
				cinematicVideoPlayer = CinematicVideoPlayer.Create(new CinematicVideoPlayerConfig(videoClip, myRenderer, audioSource, faderStyle, GameManager.instance.GetImplicitCinematicVolume()));
			}
			Debug.LogFormat("Waiting for CinematicVideoPlayer in game video load...");
			while (cinematicVideoPlayer != null && cinematicVideoPlayer.IsLoading)
			{
				yield return null;
			}
			Debug.LogFormat("Starting cinematic video player in game video.");
			if (cinematicVideoPlayer != null)
			{
				cinematicVideoPlayer.IsLooping = loopVideo;
				cinematicVideoPlayer.Play();
				myRenderer.enabled = true;
			}
			if (additionalAudio != null)
			{
				additionalAudio.Play();
			}
			yield return new WaitForSeconds(delayBeforeFadeIn);

			if (fadeInSpeed == FadeInSpeed.SLOW)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN SLOWLY", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN SLOWLY");
			}
			else if (fadeInSpeed == FadeInSpeed.NORMAL)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN");
			}
		}
		else if (videoType == VideoType.StagTravel)
		{
			GameCameras.instance.DisableImageEffects();
			if (cinematicVideoPlayer == null)
			{
				cinematicVideoPlayer = CinematicVideoPlayer.Create(new CinematicVideoPlayerConfig(videoClip, myRenderer, audioSource, faderStyle, GameManager.instance.GetImplicitCinematicVolume()));
			}
			while (cinematicVideoPlayer != null && cinematicVideoPlayer.IsLoading)
			{
				yield return null;
			}
			if (cinematicVideoPlayer != null)
			{
				cinematicVideoPlayer.IsLooping = loopVideo;
				cinematicVideoPlayer.Play();
				myRenderer.enabled = true;
			}
			yield return new WaitForSeconds(delayBeforeFadeIn);
			if (fadeInSpeed == FadeInSpeed.SLOW)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN SLOWLY", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN SLOWLY");
			}
			else if (fadeInSpeed == FadeInSpeed.NORMAL)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN");
			}
			StartCoroutine(WaitForStagFadeOut());
            PlayerData.instance.SetBool("disablePause", true);
		}
		else
		{
			GameCameras.instance.DisableImageEffects();
			if (cinematicVideoPlayer == null)
			{
				cinematicVideoPlayer = CinematicVideoPlayer.Create(new CinematicVideoPlayerConfig(videoClip, myRenderer, audioSource, faderStyle, GameManager.instance.GetImplicitCinematicVolume()));
			}
			while (cinematicVideoPlayer != null && cinematicVideoPlayer.IsLoading)
			{
				yield return null;
			}
			if (cinematicVideoPlayer != null)
			{
				cinematicVideoPlayer.IsLooping = loopVideo;
				cinematicVideoPlayer.Play();
				myRenderer.enabled = true;
			}
			yield return new WaitForSeconds(delayBeforeFadeIn);
			if (fadeInSpeed == FadeInSpeed.SLOW)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN SLOWLY", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN SLOWLY");
			}
			else if (fadeInSpeed == FadeInSpeed.NORMAL)
			{
                WeaverTypeHelpers.SendEventToGameObject("FADE SCENE IN", camera.gameObject);
				//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN");
			}
		}
	}

	private void FinishVideo()
	{
        var camera = GameObject.FindObjectOfType<CameraController>();
		GameCameras.instance.EnableImageEffects(gm.IsGameplayScene(), isBloomForced: false);
		videoTriggered = false;
		if (videoType == VideoType.OpeningCutscene)
		{
			//GameCameras.instance.cameraFadeFSM.Fsm.Event("JUST FADE");
            WeaverTypeHelpers.SendEventToGameObject("JUST FADE", camera.gameObject);
			//ui.SetState(UIState.INACTIVE);
			loadingLevel = true;
			StartCoroutine(gm.LoadFirstScene());
		}
		else if (videoType == VideoType.OpeningPrologue)
		{
			//GameCameras.instance.cameraFadeFSM.Fsm.Event("JUST FADE");
            WeaverTypeHelpers.SendEventToGameObject("JUST FADE", camera.gameObject);
			//ui.SetState(UIState.INACTIVE);
			loadingLevel = true;
			gm.LoadOpeningCinematic();
		}
		else if (videoType == VideoType.EndingA || videoType == VideoType.EndingB || videoType == VideoType.EndingC)
		{
			//GameCameras.instance.cameraFadeFSM.Fsm.Event("JUST FADE");
            WeaverTypeHelpers.SendEventToGameObject("JUST FADE", camera.gameObject);
			//ui.SetState(UIState.INACTIVE);
			loadingLevel = true;
			gm.LoadScene("End_Credits");
		}
		else if (videoType == VideoType.StagTravel)
		{
			//ui.SetState(UIState.INACTIVE);
			loadingLevel = true;
			gm.ChangeToScene(pd.GetString("nextScene"), "door_stagExit", 0f);
		}
		else if (videoType == VideoType.EndingGG)
		{
			//GameCameras.instance.cameraFadeFSM.Fsm.Event("JUST FADE");
            WeaverTypeHelpers.SendEventToGameObject("JUST FADE", camera.gameObject);
			//ui.SetState(UIState.INACTIVE);
			loadingLevel = true;
			if (PlayerData.instance.GetBool("bossRushMode"))
			{
				gm.LoadScene("GG_End_Sequence");
			}
			else
			{
				gm.LoadScene("End_Credits");
			}
		}
	}

	private void FinishInGameVideo()
	{
		Debug.LogFormat("Finishing in-game video.");
        WeaverTypeHelpers.BroadcastEvent("CINEMATIC END", gameObject);
		//PlayMakerFSM.BroadcastEvent("CINEMATIC END");
		GameCameras.instance.EnableImageEffects(gm.IsGameplayScene(), isBloomForced: false);
		myRenderer.enabled = false;
		selfBlanker.enabled = false;
		if (masterResume != null)
		{
			masterResume.TransitionTo(0f);
		}
		if (!additionalAudioContinuesPastVideo && additionalAudio != null)
		{
			additionalAudio.Stop();
		}
		if (cinematicVideoPlayer != null)
		{
			cinematicVideoPlayer.Stop();
			cinematicVideoPlayer.Dispose();
			cinematicVideoPlayer = null;
		}
		videoTriggered = false;
		gm.gameState = GameState.PLAYING;
	}

	private IEnumerator WaitForStagFadeOut()
	{
        var camera = GameObject.FindObjectOfType<CameraController>();
		yield return new WaitForSeconds(2.6f);
        WeaverTypeHelpers.SendEventToGameObject("JUST FADE", camera.gameObject);
		//GameCameras.instance.cameraFadeFSM.Fsm.Event("JUST FADE");
	}
}
