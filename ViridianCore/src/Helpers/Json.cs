using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ViridianCore.Helpers
{
    public static class Json
    {
        static Assembly NewtonsoftJson;
        static Type JsonConvert;
        static Func<object, string> SerializeMethod;
        static Func<string, Type, object> DeserializeMethod;

        static void Initialize()
        {
            if (NewtonsoftJson == null)
            {
                NewtonsoftJson = Assembly.Load("Newtonsoft.Json");
                JsonConvert = NewtonsoftJson.GetType("Newtonsoft.Json.JsonConvert");
                SerializeMethod = Methods.GetFunction<Func<object, string>>(JsonConvert.GetMethod("SerializeObject", new Type[] { typeof(object) }));
                DeserializeMethod = Methods.GetFunction<Func<string, Type, object>>(JsonConvert.GetMethod("DeserializeObject", new Type[] { typeof(string), typeof(Type) }));
            }
        }

        public static string Serialize(object obj)
        {
            Initialize();
            return SerializeMethod(obj);
        }

        public static T Deserialize<T>(string source)
        {
            Initialize();
            return (T)DeserializeMethod(source, typeof(T));
        }
    }
}
