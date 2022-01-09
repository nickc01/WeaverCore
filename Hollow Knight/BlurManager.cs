using UnityEngine;

[RequireComponent(typeof(LightBlurredBackground))]
public class BlurManager : MonoBehaviour
{
	private LightBlurredBackground lightBlurredBackground;

	[SerializeField]
	private int baseWidth;

	[SerializeField]
	private int baseHeight;

	[SerializeField]
	private int largeConsoleWidth;

	[SerializeField]
	private int largeConsoleHeight;

	protected void Awake()
	{
		lightBlurredBackground = GetComponent<LightBlurredBackground>();
		int renderTextureWidth = baseWidth;
		int renderTextureHeight = baseHeight;
		if (Application.isConsolePlatform)
		{
			renderTextureWidth = largeConsoleWidth;
			renderTextureHeight = largeConsoleHeight;
		}
		lightBlurredBackground.RenderTextureWidth = renderTextureWidth;
		lightBlurredBackground.RenderTextureHeight = renderTextureHeight;
	}

	protected void Update()
	{
		GameManager unsafeInstance = GameManager.UnsafeInstance;
		if (!(unsafeInstance != null))
		{
			return;
		}
		lightBlurredBackground.enabled = true;
	}
}
