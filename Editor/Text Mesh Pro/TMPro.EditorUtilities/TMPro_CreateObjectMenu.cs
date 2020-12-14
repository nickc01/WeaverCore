using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMPro.EditorUtilities
{
	public static class TMPro_CreateObjectMenu
	{
		private const string kUILayerName = "UI";

		private const string kStandardSpritePath = "UI/Skin/UISprite.psd";

		private const string kBackgroundSpritePath = "UI/Skin/Background.psd";

		private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";

		private const string kKnobPath = "UI/Skin/Knob.psd";

		private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";

		private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";

		private const string kMaskPath = "UI/Skin/UIMask.psd";

		private static TMP_DefaultControls.Resources s_StandardResources;

		[MenuItem("GameObject/3D Object/TextMeshPro - Text", false, 30)]
		private static void CreateTextMeshProObjectPerform(MenuCommand command)
		{
			GameObject gameObject = new GameObject("TextMeshPro");
			TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
			textMeshPro.text = "Sample text";
			textMeshPro.alignment = TextAlignmentOptions.TopLeft;
			Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
			GameObject gameObject2 = command.context as GameObject;
			if (gameObject2 != null)
			{
				GameObjectUtility.SetParentAndAlign(gameObject, gameObject2);
				Undo.SetTransformParent(gameObject.transform, gameObject2.transform, "Parent " + gameObject.name);
			}
			Selection.activeGameObject = gameObject;
		}

		[MenuItem("GameObject/UI/TextMeshPro - Text", false, 2001)]
		private static void CreateTextMeshProGuiObjectPerform(MenuCommand command)
		{
			Canvas canvas = Object.FindObjectOfType<Canvas>();
			if (canvas == null)
			{
				GameObject gameObject = new GameObject("Canvas");
				canvas = gameObject.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.gameObject.AddComponent<GraphicRaycaster>();
				Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
			}
			GameObject gameObject2 = new GameObject("TextMeshPro Text");
			RectTransform rectTransform = gameObject2.AddComponent<RectTransform>();
			Undo.RegisterCreatedObjectUndo(gameObject2, "Create " + gameObject2.name);
			GameObject gameObject3 = command.context as GameObject;
			if (gameObject3 == null)
			{
				GameObjectUtility.SetParentAndAlign(gameObject2, canvas.gameObject);
				TextMeshProUGUI textMeshProUGUI = gameObject2.AddComponent<WeaverCore.Assets.TMPro.TextMeshProUGUI>();
				textMeshProUGUI.text = "New Text";
				textMeshProUGUI.alignment = TextAlignmentOptions.TopLeft;
			}
			else if ((Object)(object)gameObject3.GetComponent<Button>() != null)
			{
				rectTransform.sizeDelta = Vector2.zero;
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				GameObjectUtility.SetParentAndAlign(gameObject2, gameObject3);
				TextMeshProUGUI textMeshProUGUI2 = gameObject2.AddComponent<WeaverCore.Assets.TMPro.TextMeshProUGUI>();
				textMeshProUGUI2.text = "Button";
				textMeshProUGUI2.fontSize = 24f;
				textMeshProUGUI2.alignment = TextAlignmentOptions.Center;
			}
			else
			{
				GameObjectUtility.SetParentAndAlign(gameObject2, gameObject3);
				TextMeshProUGUI textMeshProUGUI3 = gameObject2.AddComponent<WeaverCore.Assets.TMPro.TextMeshProUGUI>();
				textMeshProUGUI3.text = "New Text";
				textMeshProUGUI3.alignment = TextAlignmentOptions.TopLeft;
			}
			if (!(Object)(object)Object.FindObjectOfType<EventSystem>())
			{
				GameObject gameObject4 = new GameObject("EventSystem", typeof(EventSystem));
				gameObject4.AddComponent<StandaloneInputModule>();
				Undo.RegisterCreatedObjectUndo(gameObject4, "Create " + gameObject4.name);
			}
			Selection.activeGameObject = gameObject2;
		}

		[MenuItem("GameObject/UI/TextMeshPro - Input Field", false, 2037)]
		private static void AddTextMeshProInputField(MenuCommand menuCommand)
		{
			GameObject element = TMP_DefaultControls.CreateInputField(GetStandardResources());
			PlaceUIElementRoot(element, menuCommand);
		}

		[MenuItem("GameObject/UI/TextMeshPro - Dropdown", false, 2036)]
		public static void AddDropdown(MenuCommand menuCommand)
		{
			GameObject element = TMP_DefaultControls.CreateDropdown(GetStandardResources());
			PlaceUIElementRoot(element, menuCommand);
		}

		private static TMP_DefaultControls.Resources GetStandardResources()
		{
			if (s_StandardResources.standard == null)
			{
				s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
				s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
				s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
				s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
				s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
				s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
				s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
			}
			return s_StandardResources;
		}

		private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null && SceneView.sceneViews.Count > 0)
			{
				sceneView = (SceneView.sceneViews[0] as SceneView);
			}
			if (!(sceneView == null) && !(sceneView.camera == null))
			{
				Camera camera = sceneView.camera;
				Vector3 zero = Vector3.zero;
				Vector2 localPoint;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPoint))
				{
					localPoint.x += canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
					localPoint.y += canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
					localPoint.x = Mathf.Clamp(localPoint.x, 0f, canvasRTransform.sizeDelta.x);
					localPoint.y = Mathf.Clamp(localPoint.y, 0f, canvasRTransform.sizeDelta.y);
					zero.x = localPoint.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
					zero.y = localPoint.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
					Vector3 vector = default(Vector3);
					vector.x = canvasRTransform.sizeDelta.x * (0f - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
					vector.y = canvasRTransform.sizeDelta.y * (0f - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
					Vector3 vector2 = default(Vector3);
					vector2.x = canvasRTransform.sizeDelta.x * (1f - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
					vector2.y = canvasRTransform.sizeDelta.y * (1f - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
					zero.x = Mathf.Clamp(zero.x, vector.x, vector2.x);
					zero.y = Mathf.Clamp(zero.y, vector.y, vector2.y);
				}
				itemTransform.anchoredPosition = zero;
				itemTransform.localRotation = Quaternion.identity;
				itemTransform.localScale = Vector3.one;
			}
		}

		private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
		{
			GameObject gameObject = menuCommand.context as GameObject;
			if (gameObject == null || gameObject.GetComponentInParent<Canvas>() == null)
			{
				gameObject = GetOrCreateCanvasGameObject();
			}
			string text = element.name = GameObjectUtility.GetUniqueNameForSibling(gameObject.transform, element.name);
			Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
			Undo.SetTransformParent(element.transform, gameObject.transform, "Parent " + element.name);
			GameObjectUtility.SetParentAndAlign(element, gameObject);
			if (gameObject != menuCommand.context)
			{
				SetPositionVisibleinSceneView(gameObject.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());
			}
			Selection.activeGameObject = element;
		}

		public static GameObject CreateNewUI()
		{
			GameObject gameObject = new GameObject("Canvas");
			gameObject.layer = LayerMask.NameToLayer("UI");
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			gameObject.AddComponent<CanvasScaler>();
			gameObject.AddComponent<GraphicRaycaster>();
			Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
			CreateEventSystem(false);
			return gameObject;
		}

		private static void CreateEventSystem(bool select)
		{
			CreateEventSystem(select, null);
		}

		private static void CreateEventSystem(bool select, GameObject parent)
		{
			EventSystem val = Object.FindObjectOfType<EventSystem>();
			if ((Object)(object)val == null)
			{
				GameObject gameObject = new GameObject("EventSystem");
				GameObjectUtility.SetParentAndAlign(gameObject, parent);
				val = gameObject.AddComponent<EventSystem>();
				gameObject.AddComponent<StandaloneInputModule>();
				Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
			}
			if (select && (Object)(object)val != null)
			{
				Selection.activeGameObject = ((Component)(object)val).gameObject;
			}
		}

		public static GameObject GetOrCreateCanvasGameObject()
		{
			GameObject activeGameObject = Selection.activeGameObject;
			Canvas canvas = (activeGameObject != null) ? activeGameObject.GetComponentInParent<Canvas>() : null;
			if (canvas != null && canvas.gameObject.activeInHierarchy)
			{
				return canvas.gameObject;
			}
			canvas = (Object.FindObjectOfType(typeof(Canvas)) as Canvas);
			if (canvas != null && canvas.gameObject.activeInHierarchy)
			{
				return canvas.gameObject;
			}
			return CreateNewUI();
		}
	}
}
