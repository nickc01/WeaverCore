using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Enums
{
	/// <summary>
	/// Used with <see cref="CameraShaker.Shake(ShakeType)"/> to determine how the camera shakes
	/// </summary>
	public enum ShakeType
	{
		HugeShake,
		SuperDashShake,
		BigShake,
		EnemyKillShake,
		TramShake,
		AverageShake,
		BlizzardShake,
		SmallShake
	}
}
