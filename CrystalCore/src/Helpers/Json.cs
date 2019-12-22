using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalCore.Helpers
{
    public static class Json
    {
        public static string Serialize(object obj)
        {
            return ModuleInitializer.Serialize(obj);
        }
    }
}
