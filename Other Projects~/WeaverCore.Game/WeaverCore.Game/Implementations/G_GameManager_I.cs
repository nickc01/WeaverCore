using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations.GameManager;

namespace WeaverCore.Game.Implementations
{
	public class G_GameManager_I : GameManager_I
	{
		public override IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		{
			return GameManager.instance.FreezeMoment(rampDownTime, waitTime, rampUpTime, targetSpeed);
		}
	}
}
