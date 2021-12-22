using System;
using System.Collections;
using TMPro;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class WeaverArrowPrompt : MonoBehaviour
	{
		[SerializeField]
		float fadeInTime = 0.4f;
		[SerializeField]
		float fadeOutTime = 0.233f;
		[SerializeField]
		float fadedInAlpha = 1f;
		[SerializeField]
		float fadedOutAlpha = 0f;

		static WeaverArrowPrompt _defaultPrefab;
		public static WeaverArrowPrompt DefaultPrefab
		{
			get
			{
				if (_defaultPrefab == null)
				{
					_defaultPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Arrow Prompt").GetComponent<WeaverArrowPrompt>();
				}
				return _defaultPrefab;
			}
		}

		public bool OwnerSet { get; private set; }
		public GameObject Owner { get; private set; }
		public TextMeshPro Label { get; private set; }
		public SpriteRenderer Shadow { get; private set; }
		public bool IsVisible { get; private set; }
		WeaverAnimationPlayer anim;

		private void Awake()
		{
			anim = GetComponent<WeaverAnimationPlayer>();
			Label = GetComponentInChildren<TextMeshPro>();
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
		/// Hides the prompt. This will also destroy the prompt when it is fully hidden
		/// </summary>
		public void Hide()
		{
			anim.PlayAnimation("Down");
			IsVisible = false;
			Owner = null;
			StopAllCoroutines();
			StartCoroutine(FadeRoutine(fadedInAlpha, fadedOutAlpha, fadeOutTime));
			Destroy(gameObject, fadeOutTime);
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

		public void SetLabelText(string text)
		{
			Label.text = text;
		}

		public void SetLabelText(string convoName, string sheetName = "Prompts")
		{
			SetLabelText(WeaverLanguage.GetString(convoName, sheetName, "PLACEHOLDER"));
		}


		void DestroyOnLevelLoad()
		{
			GameObject.Destroy(gameObject);
		}

		private void OnEnable()
		{
			anim.PlayAnimation("Blank");
		}

		public static WeaverArrowPrompt Spawn(Vector3 spawnPosition)
		{
			return Spawn(DefaultPrefab, null, false,spawnPosition);
		}

		public static WeaverArrowPrompt Spawn(WeaverArrowPrompt prefab, Vector3 spawnPosition)
		{
			return Spawn(prefab, null, false, spawnPosition);
		}

		public static WeaverArrowPrompt Spawn(GameObject owner, Vector3 spawnPosition)
		{
			return Spawn(DefaultPrefab,owner,true, spawnPosition);
		}

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
