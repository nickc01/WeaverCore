using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	[ExecuteInEditMode]
	public class AreaTitle : MonoBehaviour
	{
		static ObjectPool AreaTitlePool;
		static GameObject Prefab;


		[SerializeField]
		RectTransform TextContainer;

		[SerializeField]
		TextMeshProUGUI _bottomText;
		public string BottomText
		{
			get
			{
				return _bottomText.text;
			}
			set
			{
				_bottomText.text = value;
			}
		}

		public TextMeshProUGUI BottomTextObject
		{
			get
			{
				return _bottomText;
			}
		}

		[SerializeField]
		TextMeshProUGUI _topText;
		public string TopText
		{
			get
			{
				return _topText.text;
			}
			set
			{
				_topText.text = value;
			}
		}

		public TextMeshProUGUI TopTextObject
		{
			get
			{
				return _topText;
			}
		}

		public Color FadedInColor = Color.white;
		public Color FadedOutColor = default(Color);

		public bool DoingFade
		{
			get
			{
				return fadeCoroutine != null;
			}
		}
		Coroutine fadeCoroutine;

		[SerializeField]
		AreaTitlePosition _position = AreaTitlePosition.BottomLeft;
		public AreaTitlePosition Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value != _position)
				{
					_position = value;
					UpdatePosition(value);
				}
			}
		}

		/*bool sceneHookAdded = false;

		private void Awake()
		{
			sceneHookAdded = true;
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChange;
		}

		private void OnEnable()
		{
			if (!sceneHookAdded)
			{
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChange;
				sceneHookAdded = true;
			}
		}

		private void OnDisable()
		{
			if (sceneHookAdded)
			{
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChange;
				sceneHookAdded = false;
			}
		}

		private void OnDestroy()
		{
			if (sceneHookAdded)
			{
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChange;
				sceneHookAdded = false;
			}
		}

		private void SceneChange(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
		{
			Destroy(gameObject);
		}*/

		void Update()
		{
			if (!Application.isPlaying)
			{
				UpdatePosition(_position);
			}
		}

		public void DoFade(float fadeInTime, float waitTime, float fadeOutTime, bool deleteWhenDone)
		{
			if (DoingFade)
			{
				StopCoroutine(fadeCoroutine);
				fadeCoroutine = null;
			}

			BottomTextObject.color = FadedOutColor;

			fadeCoroutine = StartCoroutine(FadeRoutine(fadeInTime, waitTime, fadeOutTime, deleteWhenDone));
		}

		IEnumerator FadeRoutine(float fadeInTime, float waitTime, float fadeOutTime, bool deleteWhenDone)
		{
			for (float i = 0; i < fadeInTime; i += Time.deltaTime)
			{
				BottomTextObject.color = Color.Lerp(FadedOutColor, FadedInColor, i / fadeInTime);
				TopTextObject.color = BottomTextObject.color;
				yield return null;
			}

			BottomTextObject.color = FadedInColor;
			TopTextObject.color = FadedInColor;

			for (float i = 0; i < waitTime; i += Time.deltaTime)
			{
				yield return null;
			}

			for (float i = 0; i < fadeOutTime; i += Time.deltaTime)
			{
				BottomTextObject.color = Color.Lerp(FadedInColor, FadedOutColor, i / fadeOutTime);
				TopTextObject.color = BottomTextObject.color;
				yield return null;
			}

			BottomTextObject.color = FadedOutColor;
			TopTextObject.color = FadedOutColor;

			fadeCoroutine = null;

			if (deleteWhenDone)
			{
				Delete();
			}
		}

		void UpdatePosition(AreaTitlePosition position)
		{
			switch (position)
			{
				case AreaTitlePosition.BottomLeft:
					TextContainer.anchorMin = new Vector2(0.0f, 0.0f);
					break;
				case AreaTitlePosition.BottomCenter:
					TextContainer.anchorMin = new Vector2(0.5f,0.0f);
					break;
				case AreaTitlePosition.BottomRight:
					TextContainer.anchorMin = new Vector2(1.0f, 0.0f);
					break;
				case AreaTitlePosition.MiddleLeft:
					TextContainer.anchorMin = new Vector2(0f, 0.5f);
					break;
				case AreaTitlePosition.MiddleCenter:
					TextContainer.anchorMin = new Vector2(0.5f, 0.5f);
					break;
				case AreaTitlePosition.MiddleRight:
					TextContainer.anchorMin = new Vector2(1f, 0.5f);
					break;
				case AreaTitlePosition.TopLeft:
					TextContainer.anchorMin = new Vector2(0f, 1f);
					break;
				case AreaTitlePosition.TopCenter:
					TextContainer.anchorMin = new Vector2(0.5f, 1f);
					break;
				case AreaTitlePosition.TopRight:
					TextContainer.anchorMin = new Vector2(1f, 1f);
					break;
				default:
					break;
			}
			TextContainer.anchorMax = TextContainer.anchorMin;
			TextContainer.pivot = TextContainer.anchorMin;
			TextContainer.anchoredPosition = Vector2.zero;
		}

		public void Delete()
		{
			AreaTitlePool.ReturnToPool(this);
		}

		public static AreaTitle SpawnNoFade()
		{
			return SpawnNoFade("Top Text", "Bottom Text");
		}

		public static AreaTitle SpawnNoFade(string topText, string bottomText)
		{
			if (AreaTitlePool == null)
			{
				Prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Area Title Small");
				AreaTitlePool = ObjectPool.Create(Prefab);
			}

			var title = AreaTitlePool.Instantiate<AreaTitle>(WeaverCanvas.SceneContent);
			title.TopText = topText;
			title.BottomText = bottomText;

			var oldRect = Prefab.GetComponent<RectTransform>();
			var newRect = title.GetComponent<RectTransform>();

			newRect.sizeDelta = oldRect.sizeDelta;
			newRect.anchoredPosition = oldRect.anchoredPosition;
			newRect.localScale = oldRect.localScale;

			//title.GetComponent<RectTransform>().size = Prefab.GetComponent<RectTransform>().rect;

			return title;
		}

		public static AreaTitle Spawn(string topText, string bottomText, float fadeInTime = 1.5f,float waitTime = 4.5f, float fadeOutTime = 1.5f, bool deleteWhenDone = true)
		{
			var title = SpawnNoFade(topText, bottomText);
			title.DoFade(fadeInTime, waitTime, fadeOutTime, deleteWhenDone);
			return title;
		}

		public static AreaTitle Spawn(float fadeInTime = 1.5f, float waitTime = 4.5f, float fadeOutTime = 1.5f, bool deleteWhenDone = true)
		{
			return Spawn("Top Text", "Bottom Text", fadeInTime, waitTime, fadeOutTime, deleteWhenDone);
		}
	}
}
