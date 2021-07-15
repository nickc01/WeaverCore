using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
	public class BoundInputField : MonoBehaviour
	{
		public delegate bool OnValueChangedDelegate( BoundInputField source, string input );

		private bool initialized = false;
		private bool inputValid = true;
		private bool inputAltered = false;

		private InputField inputField;
		private TMPro.TMP_InputField inputFieldTMP;
		private Image inputFieldBackground;
		public InputField BackingField { get { return inputField; } }

		[System.NonSerialized]
		public string DefaultEmptyValue = string.Empty;

		[System.NonSerialized]
		public bool CacheTextOnValueChange = true;

		private string recentText = string.Empty;
		public string Text
		{
			get { return inputFieldTMP != null ? inputFieldTMP.text : inputField.text; }
			set
			{
				recentText = value;

				if( inputFieldTMP != null ? !inputFieldTMP.isFocused : !inputField.isFocused )
				{
					inputValid = true;

					if (inputFieldTMP != null)
					{
						inputFieldTMP.text = value;
					}
					else
					{
						inputField.text = value;
					}
					inputFieldBackground.color = Skin.InputFieldNormalBackgroundColor;
				}
			}
		}

		private int m_skinVersion = 0;
		private UISkin m_skin;
		public UISkin Skin
		{
			get { return m_skin; }
			set
			{
				if( m_skin != value || m_skinVersion != m_skin.Version )
				{
					Initialize();

					m_skin = value;
					m_skinVersion = m_skin.Version;

					if (inputFieldTMP != null)
					{
						inputFieldTMP.textComponent.SetSkinInputFieldText(m_skin);
					}
					else
					{
						inputField.textComponent.SetSkinInputFieldText(m_skin);
					}
					inputFieldBackground.color = m_skin.InputFieldNormalBackgroundColor;

					if (inputFieldTMP != null)
					{
						var placeholder = inputFieldTMP.placeholder as TMPro.TMP_Text;
						if (placeholder != null)
						{
							float placeholderAlpha = placeholder.color.a;
							placeholder.SetSkinInputFieldText(m_skin);

							Color placeholderColor = placeholder.color;
							placeholderColor.a = placeholderAlpha;
							placeholder.color = placeholderColor;
						}
					}
					else
					{
						Text placeholder = inputField.placeholder as Text;
						if (placeholder != null)
						{
							float placeholderAlpha = placeholder.color.a;
							placeholder.SetSkinInputFieldText(m_skin);

							Color placeholderColor = placeholder.color;
							placeholderColor.a = placeholderAlpha;
							placeholder.color = placeholderColor;
						}
					}
					
				}
			}
		}

		public OnValueChangedDelegate OnValueChanged;
		public OnValueChangedDelegate OnValueSubmitted;

		private void Awake()
		{
			Initialize();
		}

		public void Initialize()
		{
			if( initialized )
				return;

			inputField = GetComponent<InputField>();
			inputFieldTMP = GetComponent<TMPro.TMP_InputField>();
			inputFieldBackground = GetComponent<Image>();

			if (inputFieldTMP != null)
			{
				inputFieldTMP.onValueChanged.AddListener(InputFieldValueChanged);
				inputFieldTMP.onEndEdit.AddListener(InputFieldValueSubmitted);
			}
			else
			{
				inputField.onValueChanged.AddListener(InputFieldValueChanged);
				inputField.onEndEdit.AddListener(InputFieldValueSubmitted);
			}

			initialized = true;
		}

		private void InputFieldValueChanged( string str )
		{
			if( inputFieldTMP != null ? !inputFieldTMP.isFocused : !inputField.isFocused )
				return;

			inputAltered = true;

			if( str == null || str.Length == 0 )
				str = DefaultEmptyValue;

			if( OnValueChanged != null )
			{
				inputValid = OnValueChanged( this, str );
				if( inputValid && CacheTextOnValueChange )
					recentText = str;

				inputFieldBackground.color = inputValid ? Skin.InputFieldNormalBackgroundColor : Skin.InputFieldInvalidBackgroundColor;
			}
		}

		private void InputFieldValueSubmitted( string str )
		{
			inputFieldBackground.color = Skin.InputFieldNormalBackgroundColor;

			if( !inputAltered )
			{
				if (inputFieldTMP != null)
				{
					inputFieldTMP.text = recentText;
				}
				else
				{
					inputField.text = recentText;
				}
				return;
			}

			inputAltered = false;

			if( str == null || str.Length == 0 )
				str = DefaultEmptyValue;

			if( OnValueSubmitted != null )
			{
				if( OnValueSubmitted( this, str ) )
					recentText = str;
			}
			else if( inputValid )
				recentText = str;

			if (inputFieldTMP != null)
			{
				inputFieldTMP.text = recentText;
			}
			else
			{
				inputField.text = recentText;
			}
			inputValid = true;
		}
	}
}