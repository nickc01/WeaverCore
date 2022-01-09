using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Utilities;
using static WeaverCore.PlayerInput;

namespace WeaverCore.Editor.Implementations
{
	class E_PlayerInput_I : PlayerInput_I
	{
		class InputUpdater : MonoBehaviour
		{
			private void LateUpdate()
			{
				foreach (var input in ButtonInputs)
				{
					if (!input.Value.StateUpdated)
					{
						input.Value.FrameUpdate();
					}
					input.Value.StateUpdated = false;
				}

				foreach (var input in JoystickInputs)
				{
					if (!input.Value.StateUpdated)
					{
						input.Value.FrameUpdate();
					}
					input.Value.StateUpdated = false;
				}
			}
		}

		static Dictionary<string, E_PlayerInputButton> ButtonInputs = new Dictionary<string, E_PlayerInputButton>();
		static Dictionary<string, E_PlayerInputJoystick> JoystickInputs = new Dictionary<string, E_PlayerInputJoystick>();

		[OnRuntimeInit]
		static void RuntimeInit()
		{
			AddButton("left", () => Input.GetAxisRaw("Horizontal") < -0.5f || Input.GetAxisRaw("Dpad Horizontal") < -0.5f);
			AddButton("right", () => Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Dpad Horizontal") > 0.5f);
			AddButton("up", () => Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Dpad Vertical") > 0.5f);
			AddButton("down", () => Input.GetAxisRaw("Vertical") < -0.5f || Input.GetAxisRaw("Dpad Vertical") < -0.5f);
			AddButton("jump", () => Input.GetButton("Jump"));
			AddButton("dash", () => Input.GetAxisRaw("Right Trigger") > 0.1f || Input.GetButton("Dash Keyboard"));
			AddButton("superDash", () => Input.GetAxisRaw("Left Trigger") > 0.1f || Input.GetButton("Super Dash Keyboard"));
			AddButton("rs_up", () => Input.GetAxisRaw("RS Vertical") > 0.5f);
			AddButton("rs_down", () => Input.GetAxisRaw("RS Vertical") < -0.5f);
			AddButton("rs_left", () => Input.GetAxisRaw("RS Horizontal") < -0.5f);
			AddButton("rs_right", () => Input.GetAxisRaw("RS Horizontal") > 0.5f);
			AddButton("dreamNail", () => Input.GetButton("Dream Nail"));
			AddButton("attack", () => Input.GetButton("Attack"));
			AddButton("focus", () => Input.GetButton("Focus"));
			AddButton("quickMap", () => Input.GetButton("Quick Map"));
			AddButton("quickCast", () => Input.GetButton("Quick Cast"));
			AddButton("openInventory", () => Input.GetButton("Inventory"));
			AddButton("pause", () => Input.GetButton("Pause"));

			AddJoystick("moveVector", () => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
			AddJoystick("rightStick", () => new Vector2(Input.GetAxisRaw("RS Horizontal"), Input.GetAxisRaw("RS Vertical")));

			UnboundCoroutine.Start(ButtonTest());
			var iUpdater = new GameObject("Input Updater");
			iUpdater.hideFlags = HideFlags.HideAndDontSave;
			GameObject.DontDestroyOnLoad(iUpdater);
			iUpdater.AddComponent<InputUpdater>();
		}

		static IEnumerator ButtonTest()
		{
			while (true)
			{
				yield return null;

				if (!Application.isPlaying)
				{
					yield break;
				}
			}
		}

		public static void AddButton(string buttonName, Func<bool> condition)
		{
			ButtonInputs.Add(buttonName, new E_PlayerInputButton(buttonName,condition));
		}

		public static void AddJoystick(string joystickName, Func<Vector2> inputGetter)
		{
			JoystickInputs.Add(joystickName, new E_PlayerInputJoystick(joystickName, inputGetter));
		}

		class E_PlayerInputJoystick : PlayerInputJoystick
		{
			public bool StateUpdated = false;

			Vector2 previousState;
			Vector2 currentState;
			Func<Vector2> comparer;
			string buttonName;

			public E_PlayerInputJoystick(string buttonName, Func<Vector2> comparer)
			{
				this.buttonName = buttonName;
				this.comparer = comparer;
			}

			public virtual void FrameUpdate()
			{
				StateUpdated = true;
				previousState = currentState;
				currentState = comparer();
			}

			public override Vector2 Vector
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return currentState;
				}
			}

			public override Vector2 PreviousVector
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return previousState;
				}
			}
		}

		class E_DummyPlayerInputJoystick : E_PlayerInputJoystick
		{
			public E_DummyPlayerInputJoystick(string buttonName, Func<Vector2> comparer) : base(buttonName, comparer) { }

			public override void FrameUpdate() { }
			public override Vector2 PreviousVector => default;
			public override Vector2 Vector => default;
		}



		class E_PlayerInputButton : PlayerInputButton
		{
			public bool StateUpdated = false;

			bool previousState;
			bool currentState;

			Func<bool> comparer;

			public E_PlayerInputButton(string buttonName, Func<bool> comparer)
			{
				this.comparer = comparer;
			}

			public virtual void FrameUpdate()
			{
				StateUpdated = true;
				previousState = currentState;
				currentState = comparer();
			}

			public override bool IsPressed
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return currentState;
				}
			}

			public override bool PreviousState
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return previousState;
				}
			}

			public override bool WasPressed
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return currentState ? !previousState : false;
				}
			}

			public override bool WasReleased
			{
				get
				{
					if (!StateUpdated)
					{
						FrameUpdate();
					}
					return !currentState ? previousState : false;
				}
			}
		}

		class E_DummyPlayerInputButton : E_PlayerInputButton
		{
			public E_DummyPlayerInputButton(string buttonName, Func<bool> comparer) : base(buttonName, comparer) { }

			public override bool IsPressed => false;
			public override bool WasPressed => false;
			public override bool WasReleased => false;

			public override void FrameUpdate() { }
		}



		public override PlayerInputButton GetInputButton(string buttonName)
		{
			if (ButtonInputs.TryGetValue(buttonName, out var button))
			{
				return button;
			}
			else
			{
				return new E_DummyPlayerInputButton(buttonName, () => false);
			}
		}

		public override PlayerInputJoystick GetJoystick(string joystickName)
		{
			if (JoystickInputs.TryGetValue(joystickName,out var joystick))
			{
				return joystick;
			}
			else
			{
				return new E_DummyPlayerInputJoystick(joystickName, () => default);
			}
		}
	}
}
