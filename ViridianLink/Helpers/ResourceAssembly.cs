using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ViridianLink
{
    public static class ResourceAssembly
    {
        public static Assembly Load(string resourcePath)
        {
            Assembly assembly = typeof(ResourceAssembly).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return Assembly.Load(reader.ReadBytes((int)stream.Length));
                }
            }
        }
    }
}
