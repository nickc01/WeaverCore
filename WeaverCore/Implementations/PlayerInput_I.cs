using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Interfaces;
using static WeaverCore.PlayerInput;

namespace WeaverCore.Implementations
{
	/// <summary>
	/// Used for accessing input from the player's controller
	/// </summary>
    public abstract class PlayerInput_I : IImplementation
	{
		static PlayerInput_I impl = ImplFinder.GetImplementation<PlayerInput_I>();

		public abstract PlayerInputButton GetInputButton(string buttonName);
		public abstract PlayerInputJoystick GetJoystick(string joystickName);

		public static PlayerInputButton left => impl.GetInputButton(nameof(left));
		public static PlayerInputButton right => impl.GetInputButton(nameof(right));
		public static PlayerInputButton up => impl.GetInputButton(nameof(up));
		public static PlayerInputButton down => impl.GetInputButton(nameof(down));
		public static PlayerInputButton menuSubmit => impl.GetInputButton(nameof(menuSubmit));
		public static PlayerInputButton menuCancel => impl.GetInputButton(nameof(menuCancel));
		public static PlayerInputJoystick moveVector => impl.GetJoystick(nameof(moveVector));
		public static PlayerInputButton rs_up => impl.GetInputButton(nameof(rs_up));

		public static PlayerInputButton rs_down => impl.GetInputButton(nameof(rs_down));

		public static PlayerInputButton rs_left => impl.GetInputButton(nameof(rs_left));

		public static PlayerInputButton rs_right => impl.GetInputButton(nameof(rs_right));

		public static PlayerInputJoystick rightStick => impl.GetJoystick(nameof(rightStick));

		public static PlayerInputButton jump => impl.GetInputButton(nameof(jump));

		public static PlayerInputButton evade => impl.GetInputButton(nameof(evade));

		public static PlayerInputButton dash => impl.GetInputButton(nameof(dash));

		public static PlayerInputButton superDash => impl.GetInputButton(nameof(superDash));

		public static PlayerInputButton dreamNail => impl.GetInputButton(nameof(dreamNail));

		public static PlayerInputButton attack => impl.GetInputButton(nameof(attack));

		public static PlayerInputButton cast => impl.GetInputButton(nameof(cast));

		public static PlayerInputButton focus => impl.GetInputButton(nameof(focus));

		public static PlayerInputButton quickMap => impl.GetInputButton(nameof(quickMap));

		public static PlayerInputButton quickCast => impl.GetInputButton(nameof(quickCast));

		public static PlayerInputButton textSpeedup => impl.GetInputButton(nameof(textSpeedup));

		public static PlayerInputButton skipCutscene => impl.GetInputButton(nameof(skipCutscene));

		public static PlayerInputButton openInventory => impl.GetInputButton(nameof(openInventory));

		public static PlayerInputButton paneRight => impl.GetInputButton(nameof(paneRight));

		public static PlayerInputButton paneLeft => impl.GetInputButton(nameof(paneLeft));

		public static PlayerInputButton pause => impl.GetInputButton(nameof(pause));
	}
}
