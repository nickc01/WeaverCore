using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViridianCore.Helpers
{
    public static class Json
    {
        public static string Serialize(object obj)
        {
            object settings = Activator.CreateInstance(ModuleInitializer.JsonSettings);

            object value = Enum.ToObject(ModuleInitializer.JsonReferenceHandling.PropertyType, 1);

            ModuleInitializer.JsonReferenceHandling.SetValue(settings, value, null);


            //return ModuleInitializer.Serialize(obj, settings);
            return (string)ModuleInitializer.Serialize.Invoke(null, new object[] { obj,settings });
        }
    }
}
