using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class ModLoader_I : IImplementation
	{
		public abstract IEnumerable<IWeaverMod> LoadAllMods();
	}
}
