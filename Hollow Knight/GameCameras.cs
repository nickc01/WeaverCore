using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class GameCameras : MonoBehaviour
{
	private static GameCameras _instance;
	public static GameCameras instance
	{
		get
		{
			if (GameCameras._instance == null)
			{
				GameCameras._instance = UnityEngine.Object.FindObjectOfType<GameCameras>();
				if (GameCameras._instance == null)
				{
					var instanceProp = WeaverTypeHelpers.GetWeaverProperty("WeaverCore.WeaverCamera", "Instance");
					var cam = (Component)instanceProp.GetValue(null);
					GameCameras._instance = cam.GetComponentInParent<GameCameras>();
					if (GameCameras._instance == null)
					{
						Debug.LogError("Couldn't find GameCameras, make sure one exists in the scene.");
						return null;
					}
				}
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(GameCameras._instance.gameObject);
				}
			}
			return GameCameras._instance;
		}
	}

	private void Awake()
	{
		if (GameCameras._instance == null)
		{
			GameCameras._instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
			return;
		}
		if (this != GameCameras._instance)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		cameraController.transform.position += transform.localPosition;
		transform.localPosition = default;
	}

	public void SceneInit()
	{
        //Debug.LogError("GAME CAMERAS START SCENE A");
        if (this == GameCameras._instance)
		{
			//Debug.LogError("BBB");
			this.StartScene();
		}
	}

	private void SetupGameRefs()
	{
		if (this.cameraController != null)
		{
			this.cameraController.GameInit();
		}
		else
		{
			Debug.LogError("CameraController not set in inspector.");
		}
		if (this.cameraTarget != null)
		{
			this.cameraTarget.GameInit();
		}
		else
		{
			Debug.LogError("CameraTarget not set in inspector.");
		}
		if (this.sceneColorManager != null)
		{
			this.sceneColorManager.GameInit();
		}
		else
		{
			Debug.LogError("SceneColorManager not set in inspector.");
		}
		this.init = true;
	}

	private void StartScene()
	{
		//Debug.LogError("GAME CAMERAS START SCENE");
		if (!this.init)
		{
			this.SetupGameRefs();
		}
		if (GameManager.instance.IsGameplayScene() || GameManager.instance.ShouldKeepHUDCameraActive())
		{
			if (hudCamera != null && !this.hudCamera.gameObject.activeSelf)
			{
				this.hudCamera.gameObject.SetActive(true);
			}
		}
		if (GameManager.instance.IsMenuScene())
		{
			this.cameraController.transform.SetPosition2D(14.6f, 8.5f);
		}
		else if (GameManager.instance.IsCinematicScene())
		{
			this.cameraController.transform.SetPosition2D(14.6f, 8.5f);
		}
		else if (GameManager.instance.IsNonGameplayScene())
		{
			if (GameManager.instance.IsBossDoorScene())
			{
				this.cameraController.transform.SetPosition2D(17.5f, 17.5f);
			}
			else
			{
				this.cameraController.transform.SetPosition2D(14.6f, 8.5f);
			}
		}
		//Debug.LogError("A");
		this.cameraController.SceneInit();
		this.cameraTarget.SceneInit();
		this.sceneColorManager.SceneInit();
	}

	public void DisableImageEffects()
	{

	}

	[Header("Controllers")]
	public CameraController cameraController;

	public CameraTarget cameraTarget;

	[Header("Cameras")]
	public Camera hudCamera;

	public Camera mainCamera;

	public Transform cameraParent;

	[Header("Camera Effects")]
	public ColorCorrectionCurves colorCorrectionCurves;

	public SceneColorManager sceneColorManager;

	private bool init;
}