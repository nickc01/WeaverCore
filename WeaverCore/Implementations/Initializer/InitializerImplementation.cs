using System;
using System.Collections.Generic;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class InitializerImplementation : IImplementation
    {
        public abstract void Initialize();
    }
}
