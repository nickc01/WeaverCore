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

namespace AssemblyManipulator
{
	internal class Program
	{
		/*public static void ReplaceResource(string assembly, string resourceName, byte[] resource)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
			{
				for (var i = 0; i < definition.MainModule.Resources.Count; i++)
					if (definition.MainModule.Resources[i].Name == resourceName)
					{
						definition.MainModule.Resources.RemoveAt(i);
						break;
					}

				var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
				definition.MainModule.Resources.Add(er);
				definition.MainModule.Write();
			}
		}*/

		public static string GetHash(Stream stream)
		{
			var oldPosition = stream.Position;
			stream.Position = 0;
			try
			{
				using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
				{
					return Convert.ToBase64String(sha1.ComputeHash(stream));
				}
			}
			finally
			{
				stream.Position = oldPosition;
			}
		}

		public static void CopyTo(Stream source, Stream destination, int bufferSize = 2048, bool resetPosition = true)
		{
			long oldPosition1 = -1;
			if (source.CanSeek)
			{
				oldPosition1 = source.Position;
			}
			long oldPosition2 = -1;
			if (destination.CanSeek)
			{
				oldPosition2 = destination.Position;
			}
			byte[] buffer = new byte[bufferSize];
			int read = 0;
			do
			{
				read = source.Read(buffer, 0, buffer.GetLength(0));
				if (read > 0)
				{
					destination.Write(buffer, 0, read);
				}
			} while (read != 0);
			if (resetPosition)
			{
				if (oldPosition1 != -1)
				{
					source.Position = oldPosition1;
				}
				if (oldPosition2 != -1)
				{
					destination.Position = oldPosition2;
				}
			}
		}

		struct ResourceMetaData
		{
			public bool compressed;
			public string hash;

			public ResourceMetaData(bool Compressed, string Hash)
			{
				compressed = Compressed;
				hash = Hash;
			}

			public MemoryStream ToStream()
			{
				var stream = new MemoryStream();
				if (compressed)
				{
					stream.WriteByte(1);
				}
				else
				{
					stream.WriteByte(0);
				}
				foreach (var character in hash)
				{
					stream.WriteByte((byte)character);
				}
				stream.Position = 0;
				return stream;
			}

			public static ResourceMetaData FromStream(Stream stream)
			{
				ResourceMetaData meta;
				long oldPosition = stream.Position;
				stream.Position = 0;
				int compressed = stream.ReadByte();
				meta.compressed = compressed == 1;
				meta.hash = "";
				for (int i = 0; i < stream.Length - 1; i++)
				{
					meta.hash += (char)stream.ReadByte();
				}
				stream.Position = oldPosition;
				return meta;
			}
		}

		public static MemoryStream Decompress(Stream compressedStream)
		{
			MemoryStream finalStream = new MemoryStream();
			var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
			CopyTo(decompressionStream, finalStream);
			return finalStream;
		}

		/*public static Stream LoadResource(string resourceName,AssemblyDefinition sourceAssembly)
		{
			var resource = sourceAssembly.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
			if (resource == null)
			{
				return null;
			}

			return LoadResource(resourceName,resource.)

			var metaResource = sourceAssembly.MainModule.Resources.FirstOrDefault(r => r.Name == (resourceName + "_meta"));
			if (metaResource != null && metaResource is EmbeddedResource embeddedHash)
			{

			}
		}*/

		public static Stream LoadResource(string resourceName, Stream sourceStream,AssemblyDefinition sourceAssembly)
		{
			bool compressed = false;

			var metaResource = sourceAssembly.MainModule.Resources.FirstOrDefault(r => r.Name == (resourceName + "_meta"));
			if (metaResource != null && metaResource is EmbeddedResource embeddedHash)
			{
				using (var metaStream = embeddedHash.GetResourceStream())
				{
					var meta = ResourceMetaData.FromStream(metaStream);
					compressed = meta.compressed;
				}
			}
			if (compressed)
			{
				return Decompress(sourceStream);
			}
			else
			{
				return sourceStream;
			}
		}

		public static void ReplaceTypes(string assemblyWithTypes, string sourceAssembly, List<string> assembliesToLook)
		{
			using (var fileStream = new FileStream(assemblyWithTypes,FileMode.Open,FileAccess.Read,FileShare.Read))
			{
				using (var asmWithTypes = AssemblyDefinition.ReadAssembly(fileStream, new ReaderParameters() { ReadWrite = false }))
				{
					using (var replacer = new TypeReplacer(sourceAssembly, assembliesToLook))
					{
						replacer.ReplaceTypes(asmWithTypes.MainModule.Types);
						replacer.WriteChanges();
						//throw new Exception("Replacement Done");
					}
				}
			}
		}

		/*static MemoryStream TextToStream(string text)
		{
			var stream = new MemoryStream();
			foreach (var character in text)
			{
				stream.WriteByte((byte)character);
			}
			stream.Position = 0;
			return stream;
		}*/

		/*static MemoryStream TextToStream(string text)
		{
			var stream = new MemoryStream();
			foreach (var character in text)
			{
				stream.WriteByte((byte)character);
			}
			stream.Position = 0;
			return stream;
		}

		static string StreamToText(Stream stream)
		{
			long oldPosition = stream.Position;
			string result = "";
			for (int i = 0; i < stream.Length; i++)
			{
				result += (char)stream.ReadByte();
			}
			stream.Position = oldPosition;
			return result;
		}*/

		public static void AddResource(string assembly, string resourceName, Stream resource,string hash,bool compressed = true)
		{
			double previousTime = GetTime();
			while (GetTime() - previousTime <= 10.0)
			{
				try
				{
					using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
					{
						var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
						if (finding != null)
						{
							var metaResource = definition.MainModule.Resources.FirstOrDefault(r => r.Name == (resourceName + "_meta"));
							if (metaResource != null && metaResource is EmbeddedResource embeddedHash)
							{
								using (var metaStream = embeddedHash.GetResourceStream())
								{
									//Console.WriteLine("A");
									var meta = ResourceMetaData.FromStream(metaStream);
									if (meta.hash == hash)
									{
										return;
									}
								}
							}
							//Console.WriteLine("B");
							definition.MainModule.Resources.Remove(finding);
							if (metaResource != null)
							{
								definition.MainModule.Resources.Remove(metaResource);
							}
							//Console.WriteLine("C");
						}
						//Console.WriteLine("D");
						if (compressed)
						{
							using (var compressedStream = new MemoryStream())
							{
								//Console.WriteLine("E");
								using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
								{
									CopyTo(resource, compressionStream);
									compressedStream.Position = 0;
									//Console.WriteLine("F");
									//Console.WriteLine("Uncompressed Size = " + resource.Length);
									//Console.WriteLine("Uncompressed Position = " + resource.Position);
									//Console.WriteLine("Compressed Size = " + compressedStream.Length);
									//Console.WriteLine("Compressed Position = " + compressedStream.Position);

									Stream smallestStream = compressedStream;
									bool actuallyCompressed = true;
									if (resource.Length < compressedStream.Length)
									{
										smallestStream = resource;
										actuallyCompressed = false;
									}

									var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, smallestStream);
									definition.MainModule.Resources.Add(er);
									Console.WriteLine("G");

									using (var metaStream = new ResourceMetaData(actuallyCompressed,hash).ToStream())
									{
										//Console.WriteLine("Hash = " + hash);
										//Console.WriteLine("H");
										var hashResource = new EmbeddedResource(resourceName + "_meta", ManifestResourceAttributes.Public, metaStream);
										definition.MainModule.Resources.Add(hashResource);
										//Console.WriteLine("I");
										definition.MainModule.Write();
									}
								}
								//Console.WriteLine("J");
							}
						}
						else
						{
							//Console.WriteLine("K");
							var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
							definition.MainModule.Resources.Add(er);
							//Console.WriteLine("L");
							using (var metaStream = new ResourceMetaData(false, hash).ToStream())
							{
								//Console.WriteLine("Hash = " + hash);
								//Console.WriteLine("M");
								var hashResource = new EmbeddedResource(resourceName + "_meta", ManifestResourceAttributes.Public, metaStream);
								definition.MainModule.Resources.Add(hashResource);
								//Console.WriteLine("N");
								definition.MainModule.Write();
							}
						}
					}
					break;
				}
				catch (Exception e)
				{
					if (e.Message.Contains("because it is being used by another process"))
					{
						continue;
					}
					else
					{
						throw;
					}
				}
			}
			if (GetTime() - previousTime > 10.0)
			{
				throw new Exception("Embedding Timeout");
			}
		}

		static double GetTime()
		{
			return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
		}

		static void AddResource(string[] args)
		{
			string sourceAssembly = args[1];
			string additionFile = args[2];
			string resourcePath = args[3];
			bool compressed = true;
			if (args.GetLength(0) > 4)
			{
				compressed = bool.Parse(args[4]);
			}

			Console.WriteLine($"Embedding {Path.GetFileName(additionFile)} into {Path.GetFileName(sourceAssembly)} at the resource path: ({resourcePath})...");
			using (var additionStream = File.OpenRead(additionFile))
			{
				AddResource(sourceAssembly, resourcePath, additionStream, GetHash(additionStream), compressed);
			}
			Console.WriteLine("Embedding Successful!");
		}

		static int Main(string[] args)
		{
			if (args.GetLength(0) == 0)
			{
				throw new Exception("No args Specified");
			}

			if (args[0] == "addresource")
			{
				AddResource(args);
			}
			/*else if (args[1] == "makepdb")
			{
				MakePDB(args);
			}*/
			
			return 0;
		}

		static void MakePDB(string[] args)
		{

		}

		static void AddMod(string assembly, string @namespace, string typeName, string modName, bool unloadable,string hollowKnightPath, string weaverCorePath)
		{
			string hollowKnightAssemblyPath = hollowKnightPath + @"hollow_knight_Data\Managed\Assembly-CSharp.dll";

			if (!File.Exists(hollowKnightAssemblyPath))
			{
				throw new FileNotFoundException("Unable to find Hollow Knight's Assembly-CSharp.dll. Make sure the hollow knight directory is set correctly.");
			}

			string coreModulePath = hollowKnightPath + @"hollow_knight_Data\Managed\UnityEngine.CoreModule.dll";
			//string hollowKnightPath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Assembly-CSharp.dll";
			//string weaverCorePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods\WeaverCore.dll";
			//string coreModulePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\UnityEngine.CoreModule.dll";
			Console.WriteLine("--ASSEMBLY = " + assembly);
			Console.WriteLine("--HK ASSEMBLY PATH = " + hollowKnightAssemblyPath);
			Console.WriteLine("--WEAVERCORE ASSEMBLY PATH = " + weaverCorePath);
			Console.WriteLine("--CORE MODULE PATH = " + coreModulePath);
			using (var patcher = new ModPatcher(assembly,hollowKnightAssemblyPath,weaverCorePath,coreModulePath))
			{
				string newNamespace = @namespace;
				if (newNamespace != null && newNamespace != "")
				{
					newNamespace = ".";
				}
				patcher.Patch(newNamespace + "_WeaverMod_", typeName,newNamespace + typeName,modName);
			}
		}
	}
}
