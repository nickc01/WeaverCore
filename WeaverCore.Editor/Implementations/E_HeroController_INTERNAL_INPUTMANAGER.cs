using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Implementations;
using static WeaverCore.PlayerInput;

namespace WeaverCore.Editor.Implementations
{
	class E_HeroController_INTERNAL_INPUTMANAGER : HeroController.INTERNAL_INPUTMANAGER
	{
		Dictionary<string, PlayerInputButton> Buttons = new Dictionary<string, PlayerInputButton>();
		Dictionary<string, PlayerInputJoystick> Joysticks = new Dictionary<string, PlayerInputJoystick>();

		public E_HeroController_INTERNAL_INPUTMANAGER()
		{
			foreach (var prop in typeof(PlayerInput).GetProperties(BindingFlags.Public | BindingFlags.Static))
			{
				if (typeof(PlayerInputButton).IsAssignableFrom(prop.PropertyType))
				{
					Buttons.Add(prop.Name,(PlayerInputButton)prop.GetValue(null));
				}
				if (typeof(PlayerInputJoystick).IsAssignableFrom(prop.PropertyType))
				{
					Joysticks.Add(prop.Name, (PlayerInputJoystick)prop.GetValue(null));
				}
			}
		}

		public override Vector2 GetInputVector(string joystickName)
		{
			if (Joysticks.TryGetValue(joystickName,out var value))
			{
				return value.Vector;
			}
			return default;
		}

		public override bool IsInputPressed(string inputName)
		{
			if (Buttons.TryGetValue(inputName,out var button))
			{
				return button.IsPressed;
			}
			return false;
		}

		public override bool WasInputPressed(string inputName)
		{
			if (Buttons.TryGetValue(inputName, out var button))
			{
				return button.WasPressed;
			}
			return false;
		}

		public override bool WasInputReleased(string inputName)
		{
			if (Buttons.TryGetValue(inputName, out var button))
			{
				return button.WasReleased;
			}
			return false;
		}
	}
}
