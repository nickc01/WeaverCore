using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
//using System.Reflection.Emit;
using System.Text;
using System.Threading;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;

namespace WeaverBuildTools
{
	internal class Program
	{
		static int Main(string[] args)
		{
			foreach (var arg in args)
			{
				Console.WriteLine("Arg = " + arg);
			}
			if (args.GetLength(0) == 0 || args[0] == "help" || args[0] == "/?" || args[0] == "?")
			{
				Console.WriteLine(GetHelpText());
				return 0;
			}
			switch (args[0])
			{
				case "loadresource":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(LoadResourceCMD.GetHelp());
					}
					else
					{
						var resourceName = args[1];
						if (args.GetLength(0) < 3)
						{
							PrintError("No output file Path specified");
							return -10;
						}
						var outputFilePath = args[2];
						if (args.GetLength(0) < 4)
						{
							PrintError("No assembly To read from has been specified");
							return -20;
						}
						var assemblyToReadFrom = args[3];
						LoadResourceCMD.LoadResource(resourceName, outputFilePath, assemblyToReadFrom);
					}
					break;
				case "embedresource":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(EmbedResourceCMD.GetHelp());
					}
					else
					{
						var assemblyToEmbedTo = args[1];
						if (args.GetLength(0) < 3)
						{
							PrintError("No file to embed has been specified");
							return -30;
						}
						var pathToFileToEmbed = args[2];
						if (args.GetLength(0) < 4)
						{
							PrintError("No resource name has been specified");
							return -40;
						}
						var resourceName = args[3];

						CompressionMethod compression = CompressionMethod.Auto;

						if (args.GetLength(0) >= 5)
						{
							var argument = args[4].ToLower();
							if (argument == "true")
							{
								compression = CompressionMethod.UseCompression;
							}
							else if (argument == "false")
							{
								compression = CompressionMethod.NoCompression;
							}
							else
							{
								PrintError("Invalid option entered for useCompression");
								return -50;
							}
						}

						string hash = null;
						if (args.GetLength(0) >= 6)
						{
							hash = args[5];
						}

						EmbedResourceCMD.EmbedResource(assemblyToEmbedTo, pathToFileToEmbed, resourceName, hash, compression);
						Console.WriteLine("Successfully embedded [" + pathToFileToEmbed + "] as [" + resourceName + "] into " + assemblyToEmbedTo);

					}
					break;
				case "embeddirectory":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(EmbedDirectoryCMD.GetHelp());
					}
					else
					{
						var directoryToEmbed = args[1];
						if (args.GetLength(0) < 3)
						{
							PrintError("No assembly to embed to has been specified");
							return -60;
						}
						var assemblyToEmbedTo = args[2];
						string extensionFilter = "*.*";
						if (args.GetLength(0) >= 4)
						{
							extensionFilter = args[3];
						}

						EmbedDirectoryCMD.EmbedDirectory(directoryToEmbed, assemblyToEmbedTo, extensionFilter);
						Console.WriteLine("Successfully embedded the directory [" + directoryToEmbed + "] into " + assemblyToEmbedTo);

					}
					break;
				case "storefilestoembed":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(StoreFilesToEmbedCMD.GetHelp());
					}
					else
					{
						var directoryRelativeTo = args[1];
						if (args.GetLength(0) < 3)
						{
							PrintError("No file to store that data been specified");
							return -70;
						}
						var fileToStoreData = args[2];
						if (args.GetLength(0) < 4)
						{
							PrintError("No files been specified");
							return -80;
						}
						var files = args[3];
						StoreFilesToEmbedCMD.StoreFilesToEmbed(directoryRelativeTo, fileToStoreData, files);
						Console.WriteLine("Successfully stored the files to [" + fileToStoreData + "]");

					}
					break;
				case "embedmanyfiles":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(EmbedManyFilesCMD.GetHelp());
					}
					else
					{
						var fileToReadFrom = args[1];
						if (args.GetLength(0) < 3)
						{
							PrintError("No assembly to embed to been specified");
							return -90;
						}
						var assemblyToEmbedTo = args[2];
						EmbedManyFilesCMD.EmbedManyFiles(fileToReadFrom, assemblyToEmbedTo);
						Console.WriteLine("Successfully embed files into [" + assemblyToEmbedTo + "] from data stored in [" + fileToReadFrom + "]");

					}
					break;
				case "removeembeddedfiles":
					if (args.GetLength(0) == 1 || args[1] == "help" || args[1] == "/?" || args[1] == "?")
					{
						Console.WriteLine(EmbedManyFilesCMD.GetHelp());
					}
					else
					{
						var assemblyToRemoveFrom = args[1];
						RemoveEmbeddedFilesCMD.RemoveEmbeddedFiles(assemblyToRemoveFrom);
						Console.WriteLine("Successfully removed all embedded files from [" + assemblyToRemoveFrom + "]");

					}
					break;
				default:
					Console.WriteLine(GetHelpText());
					break;
			}

			return 0;
		}

		public static void PrintError(string message)
		{
			Console.WriteLine("Error : " + message);
			Console.WriteLine(GetHelpText());
		}

		public static string GetHelpText()
		{
			var helpText = "Commands:\n";
			foreach (var type in typeof(Program).Assembly.GetTypes())
			{
				if (type.Name.Contains("CMD") && type.Namespace.Contains("Commands"))
				{
					var method = type.GetMethod("GetHelp", BindingFlags.Public | BindingFlags.Static);
					if (method != null && method.IsStatic && method.ReturnType == typeof(string))
					{
						var text = method.Invoke(null, null);
						if (text != null)
						{
							helpText += "\n" + text;
						}
					}
				}
			}
			return helpText;
		}
	}
}
