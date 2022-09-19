using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// An interface for anything that can be replaced in game
    /// </summary>
    public interface IObjectReplacement
	{
		/// <summary>
		/// The name of the object to be replaced
		/// </summary>
		string ThingToReplace { get; }
	}
}
