using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Features
{
	[ShowFeature]
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
