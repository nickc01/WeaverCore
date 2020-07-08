using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.DataTypes;

namespace WeaverCore.Interfaces
{
	public interface IHittable
	{
		bool Hit(HitInfo hit);
	}
}
