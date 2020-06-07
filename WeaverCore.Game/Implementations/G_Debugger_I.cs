using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Modding;

namespace WeaverCore.Game.Implementations
{
    public class G_Debugger_I : WeaverCore.Implementations.Debugger_I
    {
        public override void Log(object obj)
        {
            Logger.Log(obj);
        }

        public override void Log(string str)
        {
            Logger.Log(str);
        }

        public override void LogError(object obj)
        {
            Logger.LogError(obj);
        }

        public override void LogError(string str)
        {
            Logger.LogError(str);
        }

        public override void LogWarning(object obj)
        {
            Logger.LogWarn(obj);
        }

        public override void LogWarning(string str)
        {
            Logger.LogWarn(str);
        }
    }
}
