using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Configuration;

namespace WeaverCore.Assets.Components
{
	public class Tab : MonoBehaviour
	{
		Text _tabText;
		Button _button;

		[HideInInspector]
		public ModSettings settings;

		public Button Button
		{
			get
			{
				if (_button == null)
				{
					_button = GetComponent<Button>();
				}
				return _button;
			}
		}

		public Text TabText
		{
			get
			{
				if (_tabText == null)
				{
					_tabText = GetComponentInChildren<Text>();
				}
				return _tabText;
			}
		}
		
		[HideInInspector]
		public int TabIndex = 0;

		void Start()
		{
			Button.onClick.AddListener(() => WeaverConfigScreen.Instance.OnTabClicked(this));
		}
	}
}
