using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMPro
{
	[AddComponentMenu("UI/TMP Dropdown", 35)]
	[RequireComponent(typeof(RectTransform))]
	public class TMP_Dropdown : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICancelHandler
	{
		protected internal class DropdownItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ICancelHandler
		{
			[SerializeField]
			private TMP_Text m_Text;

			[SerializeField]
			private Image m_Image;

			[SerializeField]
			private RectTransform m_RectTransform;

			[SerializeField]
			private Toggle m_Toggle;

			public TMP_Text text
			{
				get
				{
					return m_Text;
				}
				set
				{
					m_Text = value;
				}
			}

			public Image image
			{
				get
				{
					return m_Image;
				}
				set
				{
					m_Image = value;
				}
			}

			public RectTransform rectTransform
			{
				get
				{
					return m_RectTransform;
				}
				set
				{
					m_RectTransform = value;
				}
			}

			public Toggle toggle
			{
				get
				{
					return m_Toggle;
				}
				set
				{
					m_Toggle = value;
				}
			}

			public virtual void OnPointerEnter(PointerEventData eventData)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}

			public virtual void OnCancel(BaseEventData eventData)
			{
				TMP_Dropdown componentInParent = GetComponentInParent<TMP_Dropdown>();
				if ((bool)(UnityEngine.Object)(object)componentInParent)
				{
					componentInParent.Hide();
				}
			}
		}

		[Serializable]
		public class OptionData
		{
			[SerializeField]
			private string m_Text;

			[SerializeField]
			private Sprite m_Image;

			public string text
			{
				get
				{
					return m_Text;
				}
				set
				{
					m_Text = value;
				}
			}

			public Sprite image
			{
				get
				{
					return m_Image;
				}
				set
				{
					m_Image = value;
				}
			}

			public OptionData()
			{
			}

			public OptionData(string text)
			{
				this.text = text;
			}

			public OptionData(Sprite image)
			{
				this.image = image;
			}

			public OptionData(string text, Sprite image)
			{
				this.text = text;
				this.image = image;
			}
		}

		[Serializable]
		public class OptionDataList
		{
			[SerializeField]
			private List<OptionData> m_Options;

			public List<OptionData> options
			{
				get
				{
					return m_Options;
				}
				set
				{
					m_Options = value;
				}
			}

			public OptionDataList()
			{
				options = new List<OptionData>();
			}
		}

		[Serializable]
		public class DropdownEvent : UnityEvent<int>
		{
		}

		[SerializeField]
		private RectTransform m_Template;

		[SerializeField]
		private TMP_Text m_CaptionText;

		[SerializeField]
		private Image m_CaptionImage;

		[Space]
		[SerializeField]
		private TMP_Text m_ItemText;

		[SerializeField]
		private Image m_ItemImage;

		[Space]
		[SerializeField]
		private int m_Value;

		[Space]
		[SerializeField]
		private OptionDataList m_Options = new OptionDataList();

		[Space]
		[SerializeField]
		private DropdownEvent m_OnValueChanged = new DropdownEvent();

		private GameObject m_Dropdown;

		private GameObject m_Blocker;

		private List<DropdownItem> m_Items = new List<DropdownItem>();

		private TweenRunner<FloatTween> m_AlphaTweenRunner;

		private bool validTemplate = false;

		private static OptionData s_NoOptionData = new OptionData();

		public RectTransform template
		{
			get
			{
				return m_Template;
			}
			set
			{
				m_Template = value;
				RefreshShownValue();
			}
		}

		public TMP_Text captionText
		{
			get
			{
				return m_CaptionText;
			}
			set
			{
				m_CaptionText = value;
				RefreshShownValue();
			}
		}

		public Image captionImage
		{
			get
			{
				return m_CaptionImage;
			}
			set
			{
				m_CaptionImage = value;
				RefreshShownValue();
			}
		}

		public TMP_Text itemText
		{
			get
			{
				return m_ItemText;
			}
			set
			{
				m_ItemText = value;
				RefreshShownValue();
			}
		}

		public Image itemImage
		{
			get
			{
				return m_ItemImage;
			}
			set
			{
				m_ItemImage = value;
				RefreshShownValue();
			}
		}

		public List<OptionData> options
		{
			get
			{
				return m_Options.options;
			}
			set
			{
				m_Options.options = value;
				RefreshShownValue();
			}
		}

		public DropdownEvent onValueChanged
		{
			get
			{
				return m_OnValueChanged;
			}
			set
			{
				m_OnValueChanged = value;
			}
		}

		public int value
		{
			get
			{
				return m_Value;
			}
			set
			{
				if (!Application.isPlaying || (value != m_Value && options.Count != 0))
				{
					m_Value = Mathf.Clamp(value, 0, options.Count - 1);
					RefreshShownValue();
					m_OnValueChanged.Invoke(m_Value);
				}
			}
		}

		public bool IsExpanded
		{
			get
			{
				return m_Dropdown != null;
			}
		}

		protected TMP_Dropdown()
		{
		}

		protected override void Awake()
		{
			if (Application.isPlaying)
			{
				m_AlphaTweenRunner = new TweenRunner<FloatTween>();
				m_AlphaTweenRunner.Init((MonoBehaviour)(object)this);
				if ((bool)(UnityEngine.Object)(object)m_CaptionImage)
				{
					((Behaviour)(object)m_CaptionImage).enabled = (m_CaptionImage.sprite != null);
				}
				if ((bool)m_Template)
				{
					m_Template.gameObject.SetActive(false);
				}
			}
		}
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			if (((UIBehaviour)this).IsActive())
			{
				RefreshShownValue();
			}
		}
#endif

		public void RefreshShownValue()
		{
			OptionData optionData = s_NoOptionData;
			if (options.Count > 0)
			{
				optionData = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
			}
			if ((bool)(UnityEngine.Object)(object)m_CaptionText)
			{
				if (optionData != null && optionData.text != null)
				{
					m_CaptionText.text = optionData.text;
				}
				else
				{
					m_CaptionText.text = "";
				}
			}
			if ((bool)(UnityEngine.Object)(object)m_CaptionImage)
			{
				if (optionData != null)
				{
					//m_CaptionImage.set_sprite(optionData.image);
					m_CaptionImage.sprite = optionData.image;
				}
				else
				{
					//m_CaptionImage.set_sprite((Sprite)null);
					m_CaptionImage.sprite = null;
				}
				//((Behaviour)(object)m_CaptionImage).enabled = (m_CaptionImage.sprite != null);
				m_CaptionImage.enabled = m_CaptionImage.sprite != null;
			}
		}

		public void AddOptions(List<OptionData> options)
		{
			this.options.AddRange(options);
			RefreshShownValue();
		}

		public void AddOptions(List<string> options)
		{
			for (int i = 0; i < options.Count; i++)
			{
				this.options.Add(new OptionData(options[i]));
			}
			RefreshShownValue();
		}

		public void AddOptions(List<Sprite> options)
		{
			for (int i = 0; i < options.Count; i++)
			{
				this.options.Add(new OptionData(options[i]));
			}
			RefreshShownValue();
		}

		public void ClearOptions()
		{
			options.Clear();
			RefreshShownValue();
		}

		private void SetupTemplate()
		{
			validTemplate = false;
			if (!m_Template)
			{
				Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", (UnityEngine.Object)(object)this);
				return;
			}
			GameObject gameObject = m_Template.gameObject;
			gameObject.SetActive(true);
			Toggle componentInChildren = m_Template.GetComponentInChildren<Toggle>();
			validTemplate = true;
			if (!(UnityEngine.Object)(object)componentInChildren || ((Component)(object)componentInChildren).transform == template)
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
			}
			else if (!(((Component)(object)componentInChildren).transform.parent is RectTransform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
			}
			else if ((UnityEngine.Object)(object)itemText != null && !itemText.transform.IsChildOf(((Component)(object)componentInChildren).transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
			}
			else if ((UnityEngine.Object)(object)itemImage != null && !((Component)(object)itemImage).transform.IsChildOf(((Component)(object)componentInChildren).transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
			}
			if (!validTemplate)
			{
				gameObject.SetActive(false);
				return;
			}
			DropdownItem dropdownItem = ((Component)(object)componentInChildren).gameObject.AddComponent<DropdownItem>();
			dropdownItem.text = m_ItemText;
			dropdownItem.image = m_ItemImage;
			dropdownItem.toggle = componentInChildren;
			dropdownItem.rectTransform = (RectTransform)((Component)(object)componentInChildren).transform;
			Canvas orAddComponent = GetOrAddComponent<Canvas>(gameObject);
			orAddComponent.overrideSorting = true;
			orAddComponent.sortingOrder = 30000;
			TMP_Dropdown.GetOrAddComponent<GraphicRaycaster>(gameObject);
			GetOrAddComponent<CanvasGroup>(gameObject);
			gameObject.SetActive(false);
			validTemplate = true;
		}

		private static T GetOrAddComponent<T>(GameObject go) where T : Component
		{
			T val = go.GetComponent<T>();
			if (!(UnityEngine.Object)val)
			{
				val = go.AddComponent<T>();
			}
			return val;
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			Show();
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			Show();
		}

		public virtual void OnCancel(BaseEventData eventData)
		{
			Hide();
		}

		public void Show()
		{
			bool flag = !this.IsActive() || !this.IsInteractable() || this.m_Dropdown != null;
			if (!flag)
			{
				bool flag2 = !this.validTemplate;
				if (flag2)
				{
					this.SetupTemplate();
					bool flag3 = !this.validTemplate;
					if (flag3)
					{
						return;
					}
				}
				List<Canvas> list = TMP_ListPool<Canvas>.Get();
				base.gameObject.GetComponentsInParent<Canvas>(false, list);
				bool flag4 = list.Count == 0;
				if (!flag4)
				{
					Canvas canvas = list[0];
					TMP_ListPool<Canvas>.Release(list);
					this.m_Template.gameObject.SetActive(true);
					this.m_Dropdown = this.CreateDropdownList(this.m_Template.gameObject);
					this.m_Dropdown.name = "Dropdown List";
					this.m_Dropdown.SetActive(true);
					RectTransform rectTransform = this.m_Dropdown.transform as RectTransform;
					rectTransform.SetParent(this.m_Template.transform.parent, false);
					TMP_Dropdown.DropdownItem componentInChildren = this.m_Dropdown.GetComponentInChildren<TMP_Dropdown.DropdownItem>();
					GameObject gameObject = componentInChildren.rectTransform.parent.gameObject;
					RectTransform rectTransform2 = gameObject.transform as RectTransform;
					componentInChildren.rectTransform.gameObject.SetActive(true);
					Rect rect = rectTransform2.rect;
					Rect rect2 = componentInChildren.rectTransform.rect;
					Vector2 vector = rect2.min - rect.min + new Vector2(componentInChildren.rectTransform.localPosition.x, componentInChildren.rectTransform.localPosition.y);
					Vector2 vector2 = rect2.max - rect.max + new Vector2(componentInChildren.rectTransform.localPosition.x, componentInChildren.rectTransform.localPosition.y);
					Vector2 size = rect2.size;
					this.m_Items.Clear();
					Toggle toggle = null;
					for (int i = 0; i < this.options.Count; i++)
					{
						TMP_Dropdown.OptionData data = this.options[i];
						TMP_Dropdown.DropdownItem item = this.AddItem(data, this.value == i, componentInChildren, this.m_Items);
						bool flag5 = item == null;
						if (!flag5)
						{
							item.toggle.isOn = (this.value == i);
							item.toggle.onValueChanged.AddListener(delegate (bool x)
							{
								this.OnSelectItem(item.toggle);
							});
							bool isOn = item.toggle.isOn;
							if (isOn)
							{
								item.toggle.Select();
							}
							bool flag6 = toggle != null;
							if (flag6)
							{
								Navigation navigation = toggle.navigation;
								Navigation navigation2 = item.toggle.navigation;
								navigation.mode = Navigation.Mode.Explicit;
								navigation2.mode = Navigation.Mode.Explicit;
								navigation.selectOnDown = item.toggle;
								navigation.selectOnRight = item.toggle;
								navigation2.selectOnLeft = toggle;
								navigation2.selectOnUp = toggle;
								toggle.navigation = navigation;
								item.toggle.navigation = navigation2;
							}
							toggle = item.toggle;
						}
					}
					Vector2 sizeDelta = rectTransform2.sizeDelta;
					sizeDelta.y = size.y * (float)this.m_Items.Count + vector.y - vector2.y;
					rectTransform2.sizeDelta = sizeDelta;
					float num = rectTransform.rect.height - rectTransform2.rect.height;
					bool flag7 = num > 0f;
					if (flag7)
					{
						rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - num);
					}
					Vector3[] array = new Vector3[4];
					rectTransform.GetWorldCorners(array);
					RectTransform rectTransform3 = canvas.transform as RectTransform;
					Rect rect3 = rectTransform3.rect;
					for (int j = 0; j < 2; j++)
					{
						bool flag8 = false;
						for (int k = 0; k < 4; k++)
						{
							Vector3 vector3 = rectTransform3.InverseTransformPoint(array[k]);
							bool flag9 = vector3[j] < rect3.min[j] || vector3[j] > rect3.max[j];
							if (flag9)
							{
								flag8 = true;
								break;
							}
						}
						bool flag10 = flag8;
						if (flag10)
						{
							RectTransformUtility.FlipLayoutOnAxis(rectTransform, j, false, false);
						}
					}
					for (int l = 0; l < this.m_Items.Count; l++)
					{
						RectTransform rectTransform4 = this.m_Items[l].rectTransform;
						rectTransform4.anchorMin = new Vector2(rectTransform4.anchorMin.x, 0f);
						rectTransform4.anchorMax = new Vector2(rectTransform4.anchorMax.x, 0f);
						rectTransform4.anchoredPosition = new Vector2(rectTransform4.anchoredPosition.x, vector.y + size.y * (float)(this.m_Items.Count - 1 - l) + size.y * rectTransform4.pivot.y);
						rectTransform4.sizeDelta = new Vector2(rectTransform4.sizeDelta.x, size.y);
					}
					this.AlphaFadeList(0.15f, 0f, 1f);
					this.m_Template.gameObject.SetActive(false);
					componentInChildren.gameObject.SetActive(false);
					this.m_Blocker = this.CreateBlocker(canvas);
				}
			}
		}

		protected virtual GameObject CreateBlocker(Canvas rootCanvas)
		{
			GameObject gameObject = new GameObject("Blocker");
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			rectTransform.SetParent(rootCanvas.transform, false);
			rectTransform.anchorMin = Vector3.zero;
			rectTransform.anchorMax = Vector3.one;
			rectTransform.sizeDelta = Vector2.zero;
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.overrideSorting = true;
			Canvas component = m_Dropdown.GetComponent<Canvas>();
			canvas.sortingLayerID = component.sortingLayerID;
			canvas.sortingOrder = component.sortingOrder - 1;
			gameObject.AddComponent<GraphicRaycaster>();
			Image val = gameObject.AddComponent<Image>();
			//((Graphic)val).set_color(Color.clear);
			val.color = Color.clear;
			Button val2 = gameObject.AddComponent<Button>();
			//((UnityEvent)(object)val2.onClick).AddListener((UnityAction)Hide);
			val2.onClick.AddListener(Hide);
			return gameObject;
		}

		protected virtual void DestroyBlocker(GameObject blocker)
		{
			UnityEngine.Object.Destroy(blocker);
		}

		protected virtual GameObject CreateDropdownList(GameObject template)
		{
			return UnityEngine.Object.Instantiate(template);
		}

		protected virtual void DestroyDropdownList(GameObject dropdownList)
		{
			UnityEngine.Object.Destroy(dropdownList);
		}

		protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
		{
			return UnityEngine.Object.Instantiate(itemTemplate);
		}

		protected virtual void DestroyItem(DropdownItem item)
		{
		}

		private DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
		{
			DropdownItem dropdownItem = CreateItem(itemTemplate);
			dropdownItem.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);
			dropdownItem.gameObject.SetActive(true);
			dropdownItem.gameObject.name = "Item " + items.Count + ((data.text != null) ? (": " + data.text) : "");
			if ((UnityEngine.Object)(object)dropdownItem.toggle != null)
			{
				//dropdownItem.toggle.isOn = false;
				dropdownItem.toggle.isOn = false;
			}
			if ((bool)(UnityEngine.Object)(object)dropdownItem.text)
			{
				dropdownItem.text.text = data.text;
			}
			if ((bool)(UnityEngine.Object)(object)dropdownItem.image)
			{
				//dropdownItem.image.set_sprite(data.image);
				dropdownItem.image.sprite = data.image;
				//((Behaviour)(object)dropdownItem.image).enabled = (dropdownItem.image.sprite != null);
				dropdownItem.image.enabled = dropdownItem.image.sprite != null;
			}
			items.Add(dropdownItem);
			return dropdownItem;
		}

		private void AlphaFadeList(float duration, float alpha)
		{
			CanvasGroup component = m_Dropdown.GetComponent<CanvasGroup>();
			AlphaFadeList(duration, component.alpha, alpha);
		}

		private void AlphaFadeList(float duration, float start, float end)
		{
			if (!end.Equals(start))
			{
				FloatTween floatTween = default(FloatTween);
				floatTween.duration = duration;
				floatTween.startValue = start;
				floatTween.targetValue = end;
				FloatTween info = floatTween;
				info.AddOnChangedCallback(SetAlpha);
				info.ignoreTimeScale = true;
				m_AlphaTweenRunner.StartTween(info);
			}
		}

		private void SetAlpha(float alpha)
		{
			if ((bool)m_Dropdown)
			{
				CanvasGroup component = m_Dropdown.GetComponent<CanvasGroup>();
				component.alpha = alpha;
			}
		}

		public void Hide()
		{
			if (m_Dropdown != null)
			{
				AlphaFadeList(0.15f, 0f);
				if (((UIBehaviour)this).IsActive())
				{
					base.StartCoroutine(DelayedDestroyDropdownList(0.15f));
				}
			}
			if (m_Blocker != null)
			{
				DestroyBlocker(m_Blocker);
			}
			m_Blocker = null;
			((Selectable)this).Select();
		}

		private IEnumerator DelayedDestroyDropdownList(float delay)
		{
			yield return new WaitForSecondsRealtime(delay);
			for (int i = 0; i < m_Items.Count; i++)
			{
				if (m_Items[i] != null)
				{
					DestroyItem(m_Items[i]);
				}
				m_Items.Clear();
			}
			if (m_Dropdown != null)
			{
				DestroyDropdownList(m_Dropdown);
			}
			m_Dropdown = null;
		}

		private void OnSelectItem(Toggle toggle)
		{
			/*if (!toggle.isOn)
			{
				toggle.isOn = true;
			}*/
			if (!toggle.isOn)
			{
				toggle.isOn = true;
			}
			int num = -1;
			Transform transform = ((Component)(object)toggle).transform;
			Transform parent = transform.parent;
			for (int i = 0; i < parent.childCount; i++)
			{
				if (parent.GetChild(i) == transform)
				{
					num = i - 1;
					break;
				}
			}
			if (num >= 0)
			{
				value = num;
				Hide();
			}
		}
	}
}
