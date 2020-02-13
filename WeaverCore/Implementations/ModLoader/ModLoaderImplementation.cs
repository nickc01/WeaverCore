using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Implementations
{
	public abstract class ModLoaderImplementation : IImplementation
	{
		public abstract IEnumerable<IWeaverMod> LoadAllMods();
	}
}
