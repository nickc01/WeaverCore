using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations.GameManager;

namespace WeaverCore.Game.Implementations
{
	public class GameGameManagerImplementation : GameManagerImplementation
	{
		public override IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		{
			return GameManager.instance.FreezeMoment(rampDownTime, waitTime, rampUpTime, targetSpeed);
		}
	}
}
