using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.DataTypes;

namespace WeaverCore.Interfaces
{
	public interface IHitEffects
	{
		void PlayHitEffect(HitInfo hit, Vector3 effectsOffset = default);
	}
}
