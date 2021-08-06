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
					Debug.LogError("Couldn't find GameCameras, make sure one exists in the scene.");
				}
				else
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
	}

	public void SceneInit()
	{
		if (this == GameCameras._instance)
		{
			this.StartScene();
		}
	}

	private void SetupGameRefs()
	{
		//this.gm = GameManager.instance;
		//this.gs = this.gm.gameSettings;
		//this.canvasScaler = UIManager.instance.canvasScaler;
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
		/*if (this.sceneParticlesPrefab != null)
		{
			this.sceneParticles = UnityEngine.Object.Instantiate<SceneParticlesController>(this.sceneParticlesPrefab);
			this.sceneParticles.name = "SceneParticlesController";
			this.sceneParticles.transform.position = new Vector3(this.tk2dCam.transform.position.x, this.tk2dCam.transform.position.y, 0f);
			this.sceneParticles.transform.SetParent(this.tk2dCam.transform);
		}
		else
		{
			Debug.LogError("Scene Particles Prefab not set in inspector.");
		}*/
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

	// Token: 0x060004CC RID: 1228 RVA: 0x00018E1C File Offset: 0x0001701C
	private void StartScene()
	{
		if (!this.init)
		{
			this.SetupGameRefs();
		}
		if (GameManager.instance.IsGameplayScene() || GameManager.instance.ShouldKeepHUDCameraActive())
		{
			//this.MoveMenuToHUDCamera();
			if (hudCamera != null && !this.hudCamera.gameObject.activeSelf)
			{
				this.hudCamera.gameObject.SetActive(true);
			}
		}
		else
		{
			//this.DisableHUDCamIfAllowed();
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
			/*else if (InGameCutsceneInfo.IsInCutscene)
			{
				//this.cameraController.transform.SetPosition2D(InGameCutsceneInfo.CameraPosition);
			}*/
			else
			{
				this.cameraController.transform.SetPosition2D(14.6f, 8.5f);
			}
		}
		this.cameraController.SceneInit();
		this.cameraTarget.SceneInit();
		this.sceneColorManager.SceneInit();
		//this.sceneParticles.SceneInit();
	}

	public void DisableImageEffects()
	{
		//TODO TODO TODO
		/*this.mainCamera.GetComponent<FastNoise>().enabled = false;
		this.mainCamera.GetComponent<BloomOptimized>().enabled = false;
		this.mainCamera.GetComponent<ColorCorrectionCurves>().enabled = false;*/
	}

	[Header("Controllers")]
	public CameraController cameraController;

	// Token: 0x04000497 RID: 1175
	public CameraTarget cameraTarget;

	[Header("Cameras")]
	public Camera hudCamera;

	// Token: 0x04000495 RID: 1173
	public Camera mainCamera;

	public Transform cameraParent;

	[Header("Camera Effects")]
	public ColorCorrectionCurves colorCorrectionCurves;

	public SceneColorManager sceneColorManager;

	private bool init;
}