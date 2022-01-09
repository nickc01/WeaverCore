using UnityEngine;

[RequireComponent(typeof(GameCameras))]
public class LightBlurredBackground : MonoBehaviour
{
	[SerializeField]
	private float distantFarClipPlane;

	[SerializeField]
	private int renderTextureWidth;

	[SerializeField]
	private int renderTextureHeight;

	[SerializeField]
	private Material blitMaterial;

	[SerializeField]
	private float clipEpsilon;

	[SerializeField]
	private LayerMask blurPlaneLayer;

	private GameCameras gameCameras;

	private Camera sceneCamera;

	private Camera backgroundCamera;

	private RenderTexture renderTexture;

	private Material blurMaterialInstance;

	private Material blitMaterialInstance;

	private LightBlur lightBlur;

	private int passGroupCount;

	public int RenderTextureWidth
	{
		get
		{
			return renderTextureWidth;
		}
		set
		{
			renderTextureWidth = value;
		}
	}

	public int RenderTextureHeight
	{
		get
		{
			return renderTextureHeight;
		}
		set
		{
			renderTextureHeight = value;
		}
	}

	public int PassGroupCount
	{
		get
		{
			return passGroupCount;
		}
		set
		{
			passGroupCount = value;
			if (lightBlur != null)
			{
				lightBlur.PassGroupCount = passGroupCount;
			}
		}
	}

	protected void Awake()
	{
		gameCameras = GameCameras.instance;
		sceneCamera = gameCameras.mainCamera;
		passGroupCount = 2;
	}

	protected void OnEnable()
	{
		distantFarClipPlane = sceneCamera.farClipPlane;
		GameObject gameObject = new GameObject("BlurCamera");
		gameObject.transform.SetParent(sceneCamera.transform);
		backgroundCamera = gameObject.AddComponent<Camera>();
		backgroundCamera.CopyFrom(sceneCamera);
		backgroundCamera.rect = new Rect(0, 0, 1, 1); 
		backgroundCamera.farClipPlane = distantFarClipPlane;
		backgroundCamera.cullingMask &= ~blurPlaneLayer.value;
		backgroundCamera.depth -= 5f;
		lightBlur = gameObject.AddComponent<LightBlur>();
		lightBlur.PassGroupCount = passGroupCount;
		UpdateCameraClipPlanes();
		RenderTexture renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Default);
		backgroundCamera.targetTexture = renderTexture;
		blitMaterialInstance = new Material(blitMaterial);
		blitMaterialInstance.mainTexture = renderTexture;
		OnBlurPlanesChanged();
		BlurPlane.BlurPlanesChanged += OnBlurPlanesChanged;
	}

	protected void OnDisable()
	{
		BlurPlane.BlurPlanesChanged -= OnBlurPlanesChanged;
		for (int i = 0; i < BlurPlane.BlurPlaneCount; i++)
		{
			BlurPlane blurPlane = BlurPlane.GetBlurPlane(i);
			blurPlane.SetPlaneMaterial(null);
			blurPlane.SetPlaneVisibility(isVisible: true);
		}
		Object.Destroy(blitMaterialInstance);
		blitMaterialInstance = null;
		lightBlur = null;
		backgroundCamera.targetTexture = null;
		Object.Destroy(renderTexture);
		renderTexture = null;
		sceneCamera.farClipPlane = distantFarClipPlane;
		Object.Destroy(backgroundCamera.gameObject);
		backgroundCamera = null;
	}

	private void OnBlurPlanesChanged()
	{
		for (int i = 0; i < BlurPlane.BlurPlaneCount; i++)
		{
			BlurPlane blurPlane = BlurPlane.GetBlurPlane(i);
			blurPlane.SetPlaneVisibility(isVisible: true);
			blurPlane.SetPlaneMaterial(blitMaterialInstance);
		}
		UpdateCameraClipPlanes();
	}

	protected void LateUpdate()
	{
		UpdateCameraClipPlanes();
	}

	private void UpdateCameraClipPlanes()
	{
		BlurPlane closestBlurPlane = BlurPlane.ClosestBlurPlane;
		if (closestBlurPlane != null)
		{
			sceneCamera.farClipPlane = closestBlurPlane.PlaneZ - sceneCamera.transform.GetPositionZ() + clipEpsilon;
			backgroundCamera.nearClipPlane = closestBlurPlane.PlaneZ - backgroundCamera.transform.GetPositionZ() + clipEpsilon;
		}
		else
		{
			sceneCamera.farClipPlane = distantFarClipPlane;
			backgroundCamera.nearClipPlane = distantFarClipPlane;
		}
	}
}
