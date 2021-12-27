using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverBuildTools.Commands
{
	public static class LoadResourceCMD
	{
		public static void LoadResource(string resourceName, string outputFilePath,string assemblyToReadFrom)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assemblyToReadFrom))
			{
				using (var outputStream = File.Create(outputFilePath))
				{
					var resource = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
					if (resource == null || resource.Name != resourceName || !(resource is EmbeddedResource))
					{
						throw new Exception("The resource of " + resourceName + " was not found in " + assemblyToReadFrom);
					}
					using (var resourceStream = ((EmbeddedResource)resource).GetResourceStream())
					{
						var uncompressedStream = CheckIfCompressed(resourceName, resourceStream, definition);
                        StreamUtilities.CopyTo(uncompressedStream, outputStream);
					}
				}
			}

		}

		public static Stream CheckIfCompressed(string resourceName, Stream sourceStream, AssemblyDefinition sourceAssembly)
		{
			bool compressed = false;

			var metaResource = sourceAssembly.MainModule.Resources.FirstOrDefault(r => r.Name == (resourceName + "_meta"));
			if (metaResource != null && metaResource is EmbeddedResource)
			{
				var embeddedHash = (EmbeddedResource)metaResource;
				using (var metaStream = embeddedHash.GetResourceStream())
				{
					var meta = ResourceMetaData.FromStream(metaStream);
					compressed = meta.compressed;
				}
			}
			if (compressed)
			{
				MemoryStream finalStream = new MemoryStream();
				sourceStream.Decompress(finalStream);
				return finalStream;
			}
			else
			{
				return sourceStream;
			}
		}

		public static string GetHelp()
		{
			return "-----loadresource-----\n" +
					"Loads a resource from an assembly and dumps it into a file\n" +
					"\n" +
					"loadresource {resourceName} {outputFilePath} {assemblyToReadFrom}\n\n" +
					"---------------------\n\n";
		}
	}
}
