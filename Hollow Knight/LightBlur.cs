using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class LightBlur : PostEffectsBase
{
	private const string BlurShaderName = "Hollow Knight/Light Blur";

	private const int BlurMaterialPassCount = 2;

	private int passGroupCount;

	private const int BlurPassCountMax = 4;

	private const string BlurInfoPropertyName = "_BlurInfo";

	private int blurInfoId;

	private Shader blurShader;

	private Material blurMaterial;

	public int PassGroupCount
	{
		get
		{
			return passGroupCount;
		}
		set
		{
			passGroupCount = value;
		}
	}

	public int BlurPassCount => passGroupCount * 2;

	protected void Awake()
	{
		passGroupCount = 2;
	}

	protected void OnDestroy()
	{
		if (blurMaterial != null)
		{
			Object.Destroy(blurMaterial);
		}
		blurMaterial = null;
	}

	public override bool CheckResources()
	{
		bool flag = true;
		if (blurInfoId == 0)
		{
			blurInfoId = Shader.PropertyToID("_BlurInfo");
		}
		if (blurShader == null)
		{
			blurShader = Shader.Find("Hollow Knight/Light Blur");
			if (blurShader == null)
			{
				Debug.LogErrorFormat(this, "Failed to find shader {0}", "Hollow Knight/Light Blur");
				flag = false;
			}
		}
		if (blurMaterial == null)
		{
			blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);
		}
		return CheckSupport() && flag;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Debug.LogWarningFormat(this, "Light blur misconfigured or unsupported");
			base.enabled = false;
			Graphics.Blit(source, destination);
			return;
		}
		RenderTexture renderTexture = source;
		for (int i = 0; i < BlurPassCount; i++)
		{
			RenderTexture renderTexture2 = ((i != BlurPassCount - 1) ? RenderTexture.GetTemporary(source.width, source.height, 16, source.format) : destination);
			blurMaterial.SetVector(blurInfoId, new Vector4(1f / (float)source.width, 1f / (float)source.height, 0f, 0f));
			renderTexture.filterMode = FilterMode.Bilinear;
			Graphics.Blit(renderTexture, renderTexture2, blurMaterial, i % 2);
			if (renderTexture != source)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			renderTexture = renderTexture2;
		}
	}
}
