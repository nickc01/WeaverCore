using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Utilities
{
    /*public static class Json
    {
        static Assembly NewtonsoftJson;
        static Type JsonConvert;
        static Type FormattingT;
        static Type JsonSerializerSettingsT;
        static Type ReferenceLoopHandlingT;

        static PropertyInfo ReferenceLoopHandlingProp;

        static object Indented;
        static object IgnoreLoopHandling;
        static object DefaultSettings;
        //static Func<object, string> SerializeMethod;
        static Func<string, Type, object> DeserializeMethod;

        static MethodInfo SerializeMethod;

        static Json()
        {
            //Find the Assembly Loader Type
            var assemblyLoader = typeof(Json).Assembly.GetType("Costura.AssemblyLoader");

            //Find the method that resolves the assembly
            var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

            //Resolve the assembly. Mono.Cecil should be loaded beyond this point
            NewtonsoftJson = (Assembly)resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed") });


            //NewtonsoftJson = Assembly.Load("Newtonsoft.Json");
            JsonConvert = NewtonsoftJson.GetType("Newtonsoft.Json.JsonConvert");
            FormattingT = NewtonsoftJson.GetType("Newtonsoft.Json.Formatting");
            JsonSerializerSettingsT = NewtonsoftJson.GetType("Newtonsoft.Json.JsonSerializerSettings");
            ReferenceLoopHandlingT = NewtonsoftJson.GetType("Newtonsoft.Json.ReferenceLoopHandling");

            ReferenceLoopHandlingProp = JsonSerializerSettingsT.GetProperty("ReferenceLoopHandling");

            Indented = Enum.Parse(FormattingT, "Indented");
            DefaultSettings = Activator.CreateInstance(JsonSerializerSettingsT);
            IgnoreLoopHandling = Enum.Parse(ReferenceLoopHandlingT,"Ignore");

            ReferenceLoopHandlingProp.SetValue(DefaultSettings, IgnoreLoopHandling, null);

            SerializeMethod = JsonConvert.GetMethod("SerializeObject", new Type[] { typeof(object),FormattingT,JsonSerializerSettingsT });
            //SerializeMethod = Methods.GetFunction<Func<object, string>>(JsonConvert.GetMethod("SerializeObject", new Type[] { typeof(object) }));
            DeserializeMethod = Methods.GetFunction<Func<string, Type, object>>(JsonConvert.GetMethod("DeserializeObject", new Type[] { typeof(string), typeof(Type) }));
        }

        public static string Serialize(object obj)
        {
            return (string)SerializeMethod.Invoke(null, new object[] { obj,Indented,DefaultSettings });
            //return SerializeMethod(obj);
        }

        public static T Deserialize<T>(string source)
        {
            return (T)DeserializeMethod(source, typeof(T));
        }
    }*/
}
