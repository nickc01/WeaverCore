using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WeaverBuildTools.Commands
{
	public static class EmbedManyFilesCMD
	{
		public static void EmbedManyFiles(string fileToReadFrom, string assemblyToEmbedTo)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(EmbedData));
			EmbedData data = default;
			using (var fileStream = File.OpenRead(fileToReadFrom))
			{
				data = (EmbedData)serializer.Deserialize(fileStream);
			}

			var directory = new DirectoryInfo(data.directoryRelativeTo);

			//Console.WriteLine("All Files = " + data.files);

			foreach (var file in data.files.Split(';'))
			{
				if (file != "")
				{
					//Console.WriteLine("File = " + file);
					//Console.WriteLine("Directory = " + directory.FullName);
					//Console.WriteLine("Full File Name = " + directory.FullName + "\\" + file);
					EmbedResourceCMD.EmbedResource(assemblyToEmbedTo, directory.FullName + "\\" + file, file, compression: Enums.CompressionMethod.NoCompression);
				}
			}
		}


		public static string GetHelp()
		{
			return "-----embedmanyfiles-----\n" +
					"Embeds many files using the file created by the storefilestoembed command\n" +
					"\n" +
					"embedmanyfiles {fileToReadFrom} {assemblyToEmbedTo}\n\n" +
					"Note: Files are seperated by ';'\n" +
					"---------------------\n\n";
		}
	}
}
