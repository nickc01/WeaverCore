using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Features
{
	/// <summary>
	/// A boss designed to replace an existing enemy in the game
	/// </summary>
	[ShowFeature]
	public class BossReplacement : Boss, IObjectReplacement
	{
		[SerializeField]
		[Tooltip("The name of the object this boss is replacing")]
		private string enemyToReplace = "";

		/// <summary>
		/// The name of the object this boss is replacing
		/// </summary>
		public string ThingToReplace
		{
			get
			{
				return enemyToReplace;
			}
		}
	}
}
