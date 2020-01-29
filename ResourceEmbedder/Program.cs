using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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
            using (var definition = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadWrite = true,ReadingMode = ReadingMode.Immediate }))
            {

                var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
                var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
                if (finding != null)
                {
                    definition.MainModule.Resources.Remove(finding);
                }
                definition.MainModule.Resources.Add(er);
                definition.Write();
            }

        }

        public static MemoryStream GetResource(string path, string resourceName)
        {
            using (var definition = AssemblyDefinition.ReadAssembly(path))
            {

                foreach (var resource in definition.MainModule.Resources)
                {
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
                            stream.Close();
                        }

                        return memStream;
                    }
                }
            }
            return null;
        }

        public static bool IsFileReady(string filename)
        {
			try
			{
				using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
					return inputStream.Length > 0;
			}
			catch (System.IO.IOException e)
			{
				if (e.Message.Contains("because it is being used by another process"))
				{
					return false;
				}
				else
				{
					throw;
				}
			}
        }

        static int Main(string[] args)
        {
            //try
            //{
                string sourceAssembly = args[0];
                string additionFile = args[1];
                string resourcePath = args[2];



                Console.WriteLine($"Embedding {Path.GetFileName(additionFile)} into {Path.GetFileName(sourceAssembly)} at the resource path: ({resourcePath})...");
                byte[] additionBytes;

                while (!IsFileReady(sourceAssembly)) { }
                using (var additionStream = File.OpenRead(additionFile))
                {
                    additionBytes = new byte[additionStream.Length];

                    additionStream.Read(additionBytes, 0, (int)additionStream.Length);

                    additionStream.Close();
                }

                AddResource(sourceAssembly, resourcePath, additionBytes);
                Console.WriteLine("Embedding Successful!");
                return 0;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("ERROR Embedding assembly resource -> " + e);
            //    return -1;
            //}
        }
    }
}
