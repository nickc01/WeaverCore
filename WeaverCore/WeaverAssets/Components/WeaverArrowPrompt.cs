using System;
using System.Collections;
using TMPro;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Used to display an arrow prompt with text above an object. Mainly used by <see cref="WeaverBench"/> and <see cref="WeaverNPC"/>
	/// </summary>
	public class WeaverArrowPrompt : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The time it will take to fade in the prompt")]
		float fadeInTime = 0.4f;
		[SerializeField]
		[Tooltip("The time it will take to fade out the prompt")]
		float fadeOutTime = 0.233f;
		[SerializeField]
		[Tooltip("The color alpha value when faded in")]
		float fadedInAlpha = 1f;
		[SerializeField]
		[Tooltip("The color alpha value when faded out")]
		float fadedOutAlpha = 0f;

		[Tooltip("Should the prompt automatically be destroyed when it fades out?")]
		public bool DestroyOnHide = true;

		static WeaverArrowPrompt _defaultPrefab;
		public static WeaverArrowPrompt DefaultPrefab
		{
			get
			{
				if (_defaultPrefab == null)
				{
					_defaultPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Arrow Prompt").GetComponent<WeaverArrowPrompt>();
				}
				return _defaultPrefab;
			}
		}

		/// <summary>
		/// Does this prompt have an owner configured?
		/// </summary>
		public bool OwnerSet { get; private set; }

		/// <summary>
		/// The owner of this arrow prompt. If the owner is destroyed, so is the prompt
		/// </summary>
		public GameObject Owner { get; private set; }

		/// <summary>
		/// The text object for displaying the label
		/// </summary>
		public TextMeshPro Label { get; private set; }

		/// <summary>
		/// The sprite for displaying a drop shadow
		/// </summary>
		public SpriteRenderer Shadow { get; private set; }

		/// <summary>
		/// The main sprite of the prompt
		/// </summary>
		public SpriteRenderer Renderer { get; private set; }

		/// <summary>
		/// Is the prompt currently visible?
		/// </summary>
		public bool IsVisible { get; private set; }
		WeaverAnimationPlayer anim;

		private void Awake()
		{
			anim = GetComponent<WeaverAnimationPlayer>();
			Label = GetComponentInChildren<TextMeshPro>();
			Renderer = GetComponent<SpriteRenderer>();
			Label.gameObject.SetActive(false);
			Shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();
			Shadow.gameObject.SetActive(false);
		}

		private void Start()
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.UnloadingLevel += DestroyOnLevelLoad;
			}
		}

		private void OnDestroy()
		{
			if (GameManager.instance != null && Application.isPlaying)
			{
				GameManager.instance.UnloadingLevel -= DestroyOnLevelLoad;
			}
		}

		private void Update()
		{
			if (IsVisible && OwnerSet && Owner == null)
			{
				Hide();
			}
		}

		/// <summary>
		/// Shows the prompt
		/// </summary>
		public void Show()
		{
			anim.PlayAnimation("Up");
			transform.SetZPosition(0f);
			IsVisible = true;
			StopAllCoroutines();
			StartCoroutine(FadeRoutine(fadedOutAlpha,fadedInAlpha,fadeInTime));
		}

		/// <summary>
		/// Instantly shows the prompt
		/// </summary>
		public void ShowInstant()
        {
			anim.StopCurrentAnimation();
			Renderer.sprite = anim.AnimationData.GetFrameFromClip("Up",anim.AnimationData.GetClipFrameCount("Up") - 1);
			transform.SetZPosition(0f);
			IsVisible = true;

			Color ShadowColor = Shadow.color;
			Color LabelColor = Label.color;

			if (fadedInAlpha > 0f)
			{
				Shadow.gameObject.SetActive(true);
				Label.gameObject.SetActive(true);
			}

			ShadowColor.a = fadedInAlpha;
			LabelColor.a = fadedInAlpha;

			Shadow.color = ShadowColor;
			Label.color = LabelColor;

			if (fadedInAlpha <= 0f)
			{
				Shadow.gameObject.SetActive(false);
				Label.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Instantly hides the prompt
		/// </summary>
		public void HideInstant()
        {
			anim.StopCurrentAnimation();
			Renderer.sprite = anim.AnimationData.GetFrameFromClip("Down", anim.AnimationData.GetClipFrameCount("Down") - 1);
			IsVisible = false;
			StopAllCoroutines();

			if (fadedOutAlpha > 0f)
			{
				Shadow.gameObject.SetActive(true);
				Label.gameObject.SetActive(true);
			}

			Color ShadowColor = Shadow.color;
			Color LabelColor = Label.color;

			ShadowColor.a = fadedOutAlpha;
			LabelColor.a = fadedOutAlpha;

			Shadow.color = ShadowColor;
			Label.color = LabelColor;

			if (fadedOutAlpha <= 0f)
			{
				Shadow.gameObject.SetActive(false);
				Label.gameObject.SetActive(false);
			}

			if (DestroyOnHide)
			{
				Owner = null;
				Destroy(gameObject, fadeOutTime);
			}
		}

		/// <summary>
		/// Hides the prompt. If <see cref="DestroyOnHide"/> is true, this will also destroy the prompt object when done
		/// </summary>
		public void Hide()
		{
			anim.PlayAnimation("Down");
			IsVisible = false;
			StopAllCoroutines();
			StartCoroutine(FadeRoutine(fadedInAlpha, fadedOutAlpha, fadeOutTime));
            if (DestroyOnHide)
            {
				Owner = null;
				Destroy(gameObject, fadeOutTime);
			}
		}

		IEnumerator FadeRoutine(float from, float to, float time)
		{
			Color ShadowColor = Shadow.color;
			Color LabelColor = Label.color;
			if (to > 0f)
			{
				Shadow.gameObject.SetActive(true);
				Label.gameObject.SetActive(true);
			}
			for (float i = 0; i < time; i += Time.deltaTime)
			{
				ShadowColor.a = Mathf.Lerp(from,to,i / time);
				LabelColor.a = Mathf.Lerp(from,to,i / time);

				Shadow.color = ShadowColor;
				Label.color = LabelColor;

				yield return null;
			}

			ShadowColor.a = to;
			LabelColor.a = to;

			Shadow.color = ShadowColor;
			Label.color = LabelColor;

			if (to <= 0f)
			{
				Shadow.gameObject.SetActive(false);
				Label.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Sets the prompt's text to a new value
		/// </summary>
		/// <param name="text">The new text to set</param>
		public void SetLabelText(string text)
		{
			Label.text = text;
		}

		/// <summary>
		/// Sets the prompt's text to a new value by finding the language key and sheetTitle
		/// </summary>
		/// <param name="langKey">The lang key to retrieve</param>
		/// <param name="sheetTitle">The sheet title to retrie</param>
		public void SetLabelTextLang(string langKey, string sheetTitle = "Prompts")
		{
			SetLabelText(WeaverLanguage.GetString(langKey, sheetTitle, $"ERROR FINDING LANG_KEY: {langKey}, SHEET_TITLE: {sheetTitle}"));
		}


		void DestroyOnLevelLoad()
		{
			GameObject.Destroy(gameObject);
		}

		private void OnEnable()
		{
			anim.PlayAnimation("Blank");
		}

		/// <summary>
		/// Spawns a new arrow prompt. Use the <see cref="Show"/> function to make the prompt visible
		/// </summary>
		/// <param name="spawnPosition">The position of the new prompt</param>
		/// <returns>Returns an instance to the arrow prompt</returns>
		public static WeaverArrowPrompt Spawn(Vector3 spawnPosition)
		{
			return Spawn(DefaultPrefab, null, false,spawnPosition);
		}

		/// <summary>
		/// Spawns a new arrow prompt. Use the <see cref="Show"/> function to make the prompt visible
		/// </summary>
		/// <param name="prefab">The arrow prompt prefab to spawn</param>
		/// <param name="spawnPosition">The position of the new prompt</param>
		/// <returns>Returns an instance to the arrow prompt</returns>
		public static WeaverArrowPrompt Spawn(WeaverArrowPrompt prefab, Vector3 spawnPosition)
		{
			return Spawn(prefab, null, false, spawnPosition);
		}

		/// <summary>
		/// Spawns a new arrow prompt. Use the <see cref="Show"/> function to make the prompt visible
		/// </summary>
		/// <param name="owner">The owner of the arrow prompt. If the owner is destroyed, so is the prompt</param>
		/// <param name="spawnPosition">The position of the new prompt</param>
		/// <returns>Returns an instance to the arrow prompt</returns>
		public static WeaverArrowPrompt Spawn(GameObject owner, Vector3 spawnPosition)
		{
			return Spawn(DefaultPrefab,owner,true, spawnPosition);
		}

		/// <summary>
		/// Spawns a new arrow prompt. Use the <see cref="Show"/> function to make the prompt visible
		/// </summary>
		/// <param name="prefab">The arrow prompt prefab to spawn</param>
		/// <param name="owner">The owner of the arrow prompt. If the owner is destroyed, so is the prompt</param>
		/// <param name="spawnPosition">The position of the new prompt</param>
		/// <returns>Returns an instance to the arrow prompt</returns>
		public static WeaverArrowPrompt Spawn(WeaverArrowPrompt prefab, GameObject owner, Vector3 spawnPosition)
		{
			return Spawn(prefab, owner, true, spawnPosition);
		}

		static WeaverArrowPrompt Spawn(WeaverArrowPrompt prefab, GameObject owner, bool useOwner, Vector3 spawnPosition)
		{
			if (useOwner && owner == null)
			{
				throw new NullReferenceException("The Owner Object being used for the prompt is null");
			}
			var instance = GameObject.Instantiate(prefab,spawnPosition,Quaternion.identity);
			instance.OwnerSet = useOwner;
			instance.Owner = owner;
			return instance;
		}
	}

}
