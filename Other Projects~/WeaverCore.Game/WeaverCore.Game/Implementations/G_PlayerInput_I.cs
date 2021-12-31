using InControl;
using System;
using System.Reflection;
using UnityEngine;
using WeaverCore.Implementations;
using static WeaverCore.PlayerInput;

namespace WeaverCore.Game.Implementations
{
	public class G_PlayerInput_I : PlayerInput_I
	{
		class G_PlayerInputJoystick : PlayerInputJoystick
		{
			PlayerTwoAxisAction action;
			FieldInfo field;

			public G_PlayerInputJoystick(string actionName)
			{
				field = typeof(HeroActions).GetField(actionName, BindingFlags.Public | BindingFlags.Instance);
				if (field == null)
				{
					throw new Exception("The HeroAction " + actionName + " could not be found");
				}

			}

			void RefreshField()
			{
				if (InputHandler.Instance != null)
				{
					action = (PlayerTwoAxisAction)field.GetValue(InputHandler.Instance.inputActions);
				}
			}

			public override Vector2 Vector
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.Vector;
					}
					else
					{
						return default;
					}
				}
			}

			public override Vector2 PreviousVector
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.LastValue;
					}
					else
					{
						return default;
					}
				}
			}
		}

		class G_PlayerInputButton : PlayerInputButton
		{
			PlayerAction action;
			FieldInfo field;

			public G_PlayerInputButton(string actionName)
			{
				field = typeof(HeroActions).GetField(actionName, BindingFlags.Public | BindingFlags.Instance);
				if (field == null)
				{
					throw new Exception("The HeroAction " + actionName + " could not be found");
				}
				
			}

			void RefreshField()
			{
				if (InputHandler.Instance != null)
				{
					action = (PlayerAction)field.GetValue(InputHandler.Instance.inputActions);
				}
			}

			public override bool IsPressed
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.IsPressed;
					}
					else
					{
						return false;
					}
				}
			}

			public override bool WasPressed
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.WasPressed;
					}
					else
					{
						return false;
					}
				}
			}

			public override bool WasReleased
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.WasReleased;
					}
					else
					{
						return false;
					}
				}
			}

			public override bool PreviousState
			{
				get
				{
					if (action == null)
					{
						RefreshField();
					}
					if (action != null)
					{
						return action.LastValue < -action.StateThreshold || action.LastValue > action.StateThreshold;
					}
					else
					{
						return false;
					}
				}
			}
		}

		public override PlayerInputButton GetInputButton(string buttonName)
		{
			return new G_PlayerInputButton(buttonName);
		}

		public override PlayerInputJoystick GetJoystick(string joystickName)
		{
			return new G_PlayerInputJoystick(joystickName);
		}
	}
}