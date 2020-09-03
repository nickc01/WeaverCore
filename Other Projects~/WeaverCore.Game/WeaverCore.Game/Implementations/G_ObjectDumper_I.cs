using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	struct TextureDump
	{
		public Texture Texture;
		public FileInfo FileLocation;
		public string FileName;
	}

	[Serializable]
	struct GameObjectDump
	{
		public string Name;
		public int Layer;
		public string Tag;
		public string SceneName;
		public int SceneBuildIndex;

		public GameObjectDump(GameObject gameObject)
		{
			Name = gameObject.name;
			Layer = gameObject.layer;
			Tag = gameObject.tag;
			SceneName = gameObject.scene.name;
			SceneBuildIndex = gameObject.scene.buildIndex;
		}
	}

	[Serializable]
	struct SingleComponentDump
	{
		public string Name;
		public string Type;
		public string Contents;

		public SingleComponentDump(Component component)
		{
			Name = component.name;
			Type = component.GetType().FullName;
			if (component is MonoBehaviour)
			{
				Contents = JsonUtility.ToJson(component, true);
				//Contents = co
			}
			else
			{
				Contents = "";
			}
		}
	}

	[Serializable]
	struct MultiComponentDump
	{
		public List<SingleComponentDump> Components;

		public MultiComponentDump(GameObject gameObject)
		{
			Components = new List<SingleComponentDump>();
			foreach (var component in gameObject.GetComponents<Component>())
			{
				Components.Add(new SingleComponentDump(component));
			}
		}
	}




	public class G_ObjectDumper_I : ObjectDumper_I
	{
		public static DirectoryInfo GetDumpLocation()
		{
			if (SystemInfo.operatingSystem.Contains("Windows"))
			{
				return new DirectoryInfo("C:\\WeaverDump");
			}
			else if (SystemInfo.operatingSystem.Contains("Mac"))
			{
				return new DirectoryInfo(Application.dataPath + "/Resources/Data/Managed/Mods/WeaverCore/Dump");
			}
			else if (SystemInfo.operatingSystem.Contains("Linux"))
			{
				return new DirectoryInfo(Application.dataPath + "/Managed/Mods/WeaverCore/Dump");
			}
			else
			{
				return null;
			}

		}


		static string RemoveInvalidChars(string str)
		{
			//string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

			foreach (char c in invalid)
			{
				str = str.Replace(c.ToString(), "");
			}

			return str;
		}

		public override void Dump(GameObject obj)
		{
			DumpObject(obj);
		}


		void DumpObject(GameObject obj, DirectoryInfo dumpLocation = null)
		{
			if (dumpLocation == null)
			{
				dumpLocation = GetDumpLocation();
			}

			dumpLocation.Create();

			var objDir = dumpLocation.CreateSubdirectory(RemoveInvalidChars(obj.name));

			DumpGameObjectInfo(obj, objDir);
			DumpComponentInfo(obj, objDir);
			var textures = DumpImages(obj, objDir);

			for (int i = 0; i < obj.transform.childCount; i++)
			{
				var child = obj.transform.GetChild(i).gameObject;
				var childDirectory = objDir.CreateSubdirectory(RemoveInvalidChars(child.name));
				childDirectory.Create();

				DumpObject(child, childDirectory);
			}
		}



		FileInfo DumpGameObjectInfo(GameObject obj, DirectoryInfo directory)
		{
			WeaverLog.Log("DUMPING OBJECT = " + obj);
			var fileLocation = new FileInfo(directory.FullName + "\\GM_INFO.dat");
			var dump = new GameObjectDump(obj);
			using (var stream = fileLocation.Create())
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(JsonUtility.ToJson(dump,true));
				}
			}
			return fileLocation;
		}

		List<FileInfo> DumpComponentInfo(GameObject obj, DirectoryInfo directory)
		{
			List<FileInfo> ComponentDumps = new List<FileInfo>();
			foreach (var component in obj.GetComponents<Component>())
			{
				WeaverLog.Log("DDDD");
				WeaverLog.Log("DUMPING Component = " + component.GetType().FullName);
				var fileLocation = new FileInfo(directory.FullName + "\\COMPONENT_" + component.GetType().FullName + ".dat");
				var dump = new SingleComponentDump(component);
				using (var stream = fileLocation.Create())
				{
					using (var writer = new StreamWriter(stream))
					{
						writer.Write(JsonUtility.ToJson(dump,true));
					}
				}
				ComponentDumps.Add(fileLocation);
			}
			return ComponentDumps;
		}

		List<TextureDump> DumpImages(GameObject obj, DirectoryInfo directory)
		{
			List<TextureDump> files = new List<TextureDump>();


			var spriteFolder = directory.FullName + "\\Sprites";

			var tkSprite = obj.GetComponent<tk2dSprite>();
			if (tkSprite == null)
			{
				return files;
			}

			foreach (var texture in tkSprite.Collection.textures)
			{
				if (texture is Texture2D)
				{
					var t2D = (Texture2D)texture;
					RenderTexture dumpTexture = new RenderTexture(t2D.width, t2D.height,24,RenderTextureFormat.ARGB32);
					Graphics.Blit(t2D, dumpTexture);

					Texture2D myTexture2D = new Texture2D(t2D.width, t2D.height, TextureFormat.ARGB32, false);

					myTexture2D.ReadPixels(new Rect(0, 0, dumpTexture.width, dumpTexture.height), 0, 0);
					myTexture2D.Apply();


					var data = myTexture2D.EncodeToPNG();

					var fileName = t2D.name + "-" + StreamUtilities.GetHash(data);

					//RemoveInvalidChars(ref fileName);
					fileName = RemoveInvalidChars(fileName);

					var dataLocation = new FileInfo(spriteFolder + "\\" + fileName + ".png");
					dataLocation.Directory.Create();
					using (var fileStream = dataLocation.Create())
					{
						using (var writer = new BinaryWriter(fileStream))
						{
							writer.Write(data);
						}
					}
					files.Add(new TextureDump()
					{
						FileLocation = dataLocation,
						FileName = fileName,
						Texture = texture
					});
				}
			}
			return files;
		}
	}
}
