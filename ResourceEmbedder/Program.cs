using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ResourceEmbedder
{
    class Program
    {
        public static void ReplaceResource(string path, string resourceName, byte[] resource)
        {
            using (var definition = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadWrite = true }))
            {
                for (var i = 0; i < definition.MainModule.Resources.Count; i++)
                    if (definition.MainModule.Resources[i].Name == resourceName)
                    {
                        definition.MainModule.Resources.RemoveAt(i);
                        break;
                    }

                var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
                definition.MainModule.Resources.Add(er);
                definition.Write();
            }
        }

        public static void AddResource(string path, string resourceName, byte[] resource)
        {
            using (var definition = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadWrite = true }))
            {

                var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
                var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
                if (finding != null)
                {
                    definition.MainModule.Resources.Remove(finding);
                }
                definition.MainModule.Resources.Add(er);
                definition.Write();
                //definition.Write(path);
            }

        }

        public static MemoryStream GetResource(string path, string resourceName)
        {
            using (var definition = AssemblyDefinition.ReadAssembly(path))
            {

                foreach (var resource in definition.MainModule.Resources)
                    if (resource.Name == resourceName)
                    {
                        var embeddedResource = (EmbeddedResource)resource;
                        var memStream = new MemoryStream();
                        using (var stream = embeddedResource.GetResourceStream())
                        {
                            var bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, bytes.Length);

                            memStream.Write(bytes, 0, bytes.Length);
                            memStream.Position = 0;
                        }

                        return memStream;
                    }
            }
            return null;
        }

        static int Main(string[] args)
        {
            try
            {
                string sourceAssembly = args[0];
                string additionAssembly = args[1];
                string resourcePath = args[2];

                byte[] additionBytes;

                using (var additionStream = File.OpenRead(additionAssembly))
                {
                    additionBytes = new byte[additionStream.Length];

                    additionStream.Read(additionBytes, 0, (int)additionStream.Length);

                    additionStream.Close();
                }

                AddResource(sourceAssembly, resourcePath, additionBytes);
                Console.WriteLine("Successful Embedding");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR Embedding assembly resource -> " + e);
                return -1;
            }
        }
    }
}
