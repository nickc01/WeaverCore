using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverLog_I : IImplementation
    {
        public abstract void Log(object obj);
        public abstract void Log(string str);
        public abstract void LogError(object obj);
        public abstract void LogError(string str);
        public abstract void LogWarning(object obj);
        public abstract void LogWarning(string str);
    }
}
