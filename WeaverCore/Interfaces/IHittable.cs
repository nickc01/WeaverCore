using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Interfaces
{
	/// <summary>
	/// Interface for objects that can take damage and process hits
	/// </summary>
    public interface IHittable
	{
		/// <summary>
		/// Called when the object is hit
		/// </summary>
		/// <param name="hit">Information about the hit</param>
		/// <returns>Returns true if damage was dealt, false otherwise</returns>
		bool Hit(HitInfo hit);
	}
}
