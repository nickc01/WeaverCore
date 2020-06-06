using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations.GameManager
{
	public abstract class GameManagerImplementation : MonoBehaviour, IImplementation
	{
		public abstract IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed);
	}
}
