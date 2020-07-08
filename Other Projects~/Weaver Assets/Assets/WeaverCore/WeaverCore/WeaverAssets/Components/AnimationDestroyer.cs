using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.WeaverAssets.Components
{
	public class AnimationDestroyer : MonoBehaviour
	{
		public void Destroy()
		{
			Destroy(gameObject);
		}
	}
}
