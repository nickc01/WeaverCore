using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Implementations
{
    public abstract class RuntimeInitializerImplementation : IImplementation
    {
        public abstract void Awake();
    }
}
