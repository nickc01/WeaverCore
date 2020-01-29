using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Core;

namespace ViridianLink.Implementations
{
	public abstract class ModLoaderImplementation : IImplementation
	{
		public abstract IEnumerable<IViridianMod> LoadAllMods();
	}
}
