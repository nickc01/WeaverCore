using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Interfaces
{
	/// <summary>
	/// Interface for anything that the player can hit
	/// </summary>
    public interface IHittable
	{
		/// <summary>
		/// Called when the player hits the object
		/// </summary>
		/// <param name="hit">Information about how the player hit the object</param>
		/// <returns>Returns whether the hit was a valid hit or not</returns>
		bool Hit(HitInfo hit);
	}
}
