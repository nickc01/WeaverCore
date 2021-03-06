﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Settings
{
	public class Tab : MonoBehaviour
	{
		TextMeshProUGUI _textComponent;
		Button _button;

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

		public TextMeshProUGUI TextComponent
		{
			get
			{
				if (_textComponent == null)
				{
					_textComponent = GetComponentInChildren<TextMeshProUGUI>();
				}
				return _textComponent;
			}
		}

		public Panel Panel { get; set; }

		void Awake()
		{
			Button.onClick.AddListener(() => SettingsScreen.Instance.SelectTab(this));
		}
	}
}
