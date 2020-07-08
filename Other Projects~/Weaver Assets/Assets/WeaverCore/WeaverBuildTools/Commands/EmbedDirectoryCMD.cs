using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeaverBuildTools.Enums;
using WeaverCore.Utilities;

namespace WeaverBuildTools.Commands
{
	public static class EmbedDirectoryCMD
	{
		public static void EmbedDirectory(string directoryToEmbed, string assemblyToEmbedTo, string extensionFilter = "*.*")
		{
			var dir = new DirectoryInfo(directoryToEmbed);

			var files = dir.GetFiles(extensionFilter, SearchOption.AllDirectories);

			foreach (var file in files)
			{
				var relativePath = PathUtilities.MakePathRelative(dir.FullName, file.FullName);

				EmbedResourceCMD.EmbedResource(assemblyToEmbedTo, file.FullName, relativePath,compression: CompressionMethod.NoCompression);
			}
		}



		public static string GetHelp()
		{
			return "-----embeddirectory-----\n" +
					"Embeds an entire directory into an assembly\n" +
					"\n" +
					"embeddirectory {directoryToEmbed} {assemblyToEmbedTo} [extensionFilter]\n\n" +
					"Note: If no extension filter is specified, then all files will be embedded\n" +
					"---------------------\n\n";
		}
	}
}
