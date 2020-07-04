using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace WeaverCore.Editor.Preloader
{
	[Serializable]
	struct HashPair
	{
		public string resourceName;
		public string filePath;
		public string hash;

		public HashPair(string ResourceName = null, string Hash = null, string FilePath = null)
		{
			resourceName = ResourceName;
			hash = Hash;
			filePath = FilePath;
		}

		public static bool operator==(HashPair lhs, HashPair rhs)
		{
			return lhs.resourceName == rhs.resourceName && lhs.hash == rhs.hash;
		}

		public static bool operator !=(HashPair lhs, HashPair rhs)
		{
			return lhs.resourceName != rhs.resourceName || lhs.hash != rhs.hash;
		}

		public override bool Equals(object obj)
		{
			return obj is HashPair pair && this == pair;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + resourceName.GetHashCode();
				hash = hash * 31 + hash.GetHashCode();
				return hash;
			}
		}
	}

	[Serializable]
	class HashList
	{
		public List<HashPair> Hashes = new List<HashPair>();


		public static HashList CalculateDifferences(HashList A, HashList B)
		{
			HashList C = new HashList();

			C.Hashes.AddRange(B.Hashes.Where(h => !A.Hashes.Any(h2 => h2.filePath == h.filePath)));

			return C;
		}
	}



    [InitializeOnLoad]
    public class Preloader
    {
		static Preloader()
		{
			try
			{
				AssetDatabase.StartAssetEditing();

				var assetsFolder = new DirectoryInfo("Assets");

				var weaverCoreFolder = assetsFolder.CreateSubdirectory("WeaverCore");
				var noBuildFolder = weaverCoreFolder.CreateSubdirectory("_NOBUILD_");

				//Debug.Log("Assets Folder = " + assetsFolder.FullName);
				//Debug.Log("Weaver Core Folder = " + weaverCoreFolder.FullName);
				//Debug.Log("No Build Folder = " + noBuildFolder.FullName);

				var preloaderAssembly = typeof(Preloader).Assembly;

				//if (!File.Exists(noBuildFolder.FullName + "\\REBUILDER.cs"))
				//{
				HashList previousHashes = null;
				HashList currentHashes = new HashList();
				if (File.Exists(noBuildFolder.FullName + "\\HASHES.txt"))
				{
					try
					{
						previousHashes = JsonUtility.FromJson<HashList>(File.ReadAllText(noBuildFolder.FullName + "\\HASHES.txt"));
					}
					catch (Exception)
					{
						previousHashes = new HashList();
					}
				}
				else
				{
					previousHashes = new HashList();
				}

				var resourceNames = new HashSet<string>(typeof(Preloader).Assembly.GetManifestResourceNames());

				/*foreach (var name in resourceNames)
				{
					Debug.Log("All Resource Name = " + name);
				}*/

				foreach (var resource in resourceNames.Where(r => !r.EndsWith("_meta")))
				{
					var filePath = "";
					if (resource == "WeaverCore")
					{
						filePath = weaverCoreFolder.FullName + "\\WeaverCore.dll";
					}
					else if (resource == "WeaverBuildTools")
					{
						filePath = weaverCoreFolder.FullName + "\\Editor\\WeaverBuildTools.dll";
					}
					else if (resource == "Mono.Cecil")
					{
						filePath = weaverCoreFolder.FullName + "\\Editor\\Mono.Cecil.dll";
					}
					else
					{
						filePath = noBuildFolder.FullName + "\\" + SanitizeName(resource);
					}

					bool rewrite = true;

					if (File.Exists(filePath))
					{
						if (resourceNames.Contains(resource + "_meta"))
						{
							using (var metaStream = preloaderAssembly.GetManifestResourceStream(resource + "_meta"))
							{
								string hash = RawReadToEnd(metaStream);

								//Debug.Log("Meta Stream Position = " + metaStream.Position);
								//Debug.Log("Meta Stream Length = " + metaStream.Length);
								//Debug.Log("HASH = " + hash);
								//using (var reader = new StreamReader(metaStream))
								//{
								var hashPair = new HashPair(resource, hash,filePath);

								currentHashes.Hashes.Add(hashPair);

								//Debug.Log("HASH for resource" + resource + " = " + hashPair.hash);
								var matchingHash = previousHashes.Hashes.FirstOrDefault(pair => pair.resourceName == resource);
								//If the hash was not found
								if (matchingHash == default)
								{
									previousHashes.Hashes.Add(hashPair);
								}
								//If the hash was found
								else
								{
									if (matchingHash.hash == hashPair.hash)
									{
										rewrite = false;
									}
									else
									{
										previousHashes.Hashes.Remove(matchingHash);
										previousHashes.Hashes.Add(hashPair);
									}
								}
								//}
							}
						}
					}
					if (rewrite)
					{
						using (var resourceStream = preloaderAssembly.GetManifestResourceStream(resource))
						{
							new FileInfo(filePath).Directory.Create();
							using (var stream = File.Create(filePath))
							{
								CopyToStream(resourceStream, stream);
								stream.Close();
							}
							ImportAsset(filePath);
						}
					}
				}
				var differences = HashList.CalculateDifferences(currentHashes, previousHashes);

				foreach (var diff in differences.Hashes)
				{
					Debug.Log("File to be removed = " + diff.filePath);
					DeleteAsset(diff.filePath);
				}
				/*using (var rebuilderStream = File.CreateText(noBuildFolder.FullName + "\\REBUILDER.cs"))
				{
					rebuilderStream.WriteLine("//If you ever need to rebuild the WeaverCore directory, delete this file. Then, the WeaverCore Directory will be rebuilt");
					rebuilderStream.Close();
				}*/
				using (var rebuilderStream = File.CreateText(noBuildFolder.FullName + "\\HASHES.txt"))
				{
					rebuilderStream.Write(JsonUtility.ToJson(currentHashes));
					rebuilderStream.Close();
				}
				ImportAsset(noBuildFolder.FullName + "\\HASHES.txt");
				//}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		static void ImportAsset(string assetPath)
		{
			if (!assetPath.StartsWith("Assets"))
			{
				assetPath = MakePathRelative(new DirectoryInfo("Assets\\..").FullName, assetPath);
			}
			AssetDatabase.ImportAsset(assetPath);
		}

		static void DeleteAsset(string assetPath)
		{
			var directory = new FileInfo(assetPath).Directory;
			if (directory.Exists && File.Exists(assetPath))
			{
				File.Delete(assetPath);
			}
			DirectoryInfo previousDirectory = null;

			if (directory.Exists)
			{
				while (TechnicallyEmpty(directory))
				{
					Directory.Delete(directory.FullName, true);
					previousDirectory = directory;
					directory = directory.Parent;
				}

				if (previousDirectory != null)
				{
					var file = directory.GetFiles("*.meta").FirstOrDefault(f => f.Name == previousDirectory.Name);
					if (file != null)
					{
						file.Delete();
					}
				}
			}
			/*if (!assetPath.StartsWith("Assets"))
			{
				assetPath = MakePathRelative(new DirectoryInfo("Assets\\..").FullName, assetPath);
			}*/
			/*AssetDatabase.DeleteAsset(assetPath);*/
		}


		static bool TechnicallyEmpty(DirectoryInfo directory)
		{
			Debug.Log("Directory to remove = " + directory.FullName);
			var files = directory.GetFiles("*.*", SearchOption.AllDirectories);
			var fileCount = files.GetLength(0);
			Debug.Log("File Count = " + fileCount);
			if (fileCount == 0)
			{
				return true;
			}
			else
			{
				foreach (var file in files)
				{
					Debug.Log("File = " + file.Name);
					Debug.Log("Extension = " + file.Extension);
					if (file.Extension != ".meta")
					{
						return false;
					}
				}
				return true;
			}
		}

		static string MakePathRelative(string relativeTo, string path)
		{
			if (relativeTo.Last() != '\\')
			{
				relativeTo += "\\";
			}

			Uri fullPath = new Uri(path, UriKind.Absolute);
			Uri relRoot = new Uri(relativeTo, UriKind.Absolute);

			return relRoot.MakeRelativeUri(fullPath).ToString();
		}

		static string RawReadToEnd(Stream stream)
		{
			/*SHA256 sha256Hash = SHA256.Create();

			var data = sha256Hash.ComputeHash(stream);

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			var sBuilder = new StringBuilder();

			// Loop through each byte of the hashed data
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();*/

			StringBuilder final = new StringBuilder();
			stream.Position = 1;

			//string final = "";

			int data = 0;
			do
			{
				data = stream.ReadByte();
				//Debug.Log("BYTE READ = " + data);
				//Debug.Log("BYTE READ CHAR = " + ((char)data));
				if (data != -1)
				{
					final.Append((char)data);
					//final += (char)data;
				}
			} while (data != -1);

			return final.ToString();
		}

		static string SanitizeName(string resourceName)
		{
			var final = new StringBuilder(resourceName);

			resourceName.Replace('/', '\\');

			//final = Regex.Replace(final,"")
			/*final = final.Replace("%20", " ");

			return final;*/

			Regex regex = new Regex(@"%([a-zA-Z0-9]{2})");


			var match = regex.Match(final.ToString());

			while (match.Success)
			{
				var numberGroup = match.Groups[1];
				var escapeGroup = match.Captures[0];
				var hexString = numberGroup.Value;

				//Debug.Log("Number Group = " + numberGroup);
				//Debug.Log("Escaoe Group = " + escapeGroup);
				//Debug.Log("Hex = " + hexString);
				//Debug.Log("Decimal = " + ConvertHex(hexString));

				final.Remove(escapeGroup.Index, escapeGroup.Length);
				final.Insert(escapeGroup.Index, ConvertHex(hexString));

				match = match.NextMatch();
			}

			return final.ToString();
		}

		static string ConvertHex(string hexString)
		{
			try
			{
				string ascii = string.Empty;

				for (int i = 0; i < hexString.Length; i += 2)
				{
					string hs = string.Empty;

					hs = hexString.Substring(i, 2);
					uint decval = Convert.ToUInt32(hs, 16);
					char character = Convert.ToChar(decval);
					ascii += character;

				}

				return ascii;
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }

			return string.Empty;
		}

		static void CopyToStream<Source, Dest>(Source source, Dest destination, int bufferSize = 2048, bool resetPosition = true) where Source : Stream where Dest : Stream
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
	}
}
