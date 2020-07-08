using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WeaverBuildTools.Commands
{

	public static class StoreFilesToEmbedCMD
	{
		public static void StoreFilesToEmbed(string directoryRelativeTo, string fileToStoreData, string files)
		{
			var file = new FileInfo(fileToStoreData);
			file.Directory.Create();
			using (var fileStream = File.CreateText(file.FullName))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(EmbedData));

				serializer.Serialize(fileStream, new EmbedData()
				{
					directoryRelativeTo = directoryRelativeTo,
					files = files
				});
			}
		}


		public static string GetHelp()
		{
			return "-----storefilestoembed-----\n" +
					"Stores files paths for later embedding\n" +
					"\n" +
					"storefilestoembed {directoryRelativeTo} {filesToStoreData} {files}\n\n" +
					"Note: Files are seperated by ';'\n" +
					"---------------------\n\n";
		}
	}
}
