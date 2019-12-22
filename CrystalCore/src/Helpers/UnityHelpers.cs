using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalCore.Helpers
{
    public static class UnityHelpers
    {
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive { get; private set; } = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");
    }
}
