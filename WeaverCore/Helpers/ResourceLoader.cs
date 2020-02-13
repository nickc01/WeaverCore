using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Helpers
{
    public static class ResourceLoader
    {
        public static Assembly LoadAssembly(string resourcePath)
        {
            Assembly assembly = typeof(ResourceLoader).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return Assembly.Load(reader.ReadBytes((int)stream.Length));
                }
            }
        }

        public static Stream Retrieve(string resourcePath)
        {
            Assembly assembly = typeof(ResourceLoader).Assembly;
            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}
