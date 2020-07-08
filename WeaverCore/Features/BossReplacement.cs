using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Features
{
	public class BossReplacement : Boss, IObjectReplacement
	{
		[SerializeField]
		private string enemyToReplace = "";

		public string ThingToReplace
		{
			get
			{
				return enemyToReplace;
			}
		}
	}
}
