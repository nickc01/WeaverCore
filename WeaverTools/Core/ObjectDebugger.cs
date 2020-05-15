using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Helpers;
using WeaverTools.Machine;

namespace WeaverTools
{
	public static class ObjectDebugger
	{
		class BeingDebugged
		{
			public bool beingDebugged = false;
		}

		static string CleanName(string name)
		{
			name = name.Replace(" ", "");
			name = name.Replace("(", "");
			name = name.Replace(")", "");
			name = name.ToLower();
			return name;
		}


		static PropertyTable<GameObject, BeingDebugged> DebuggedObjects = new PropertyTable<GameObject, BeingDebugged>();

		public static bool ObjectBeingDebugged(GameObject gameObject)
		{
			return DebuggedObjects.GetOrCreate(gameObject).beingDebugged;
		}

		public static void DebugObject(GameObject gameObject, string subDirectoryName)
		{
			var directory = WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug").CreateSubdirectory("GameObjects").CreateSubdirectory(subDirectoryName);

			directory.CreateSubdirectory(gameObject.name);

			DebugObject(gameObject, directory);
		}

		public static DirectoryInfo DebugDirectory => WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug");

		public static void DebugObject(GameObject gameObject, DirectoryInfo directory = null, bool objectOnly = false)
		{
			var isBeingDebugged = DebuggedObjects.GetOrCreate(gameObject);
			if (isBeingDebugged.beingDebugged)
			{
				return;
			}
			isBeingDebugged.beingDebugged = true;
			try
			{
				try
				{
					if (directory == null)
					{
						var GameObjectDir = WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug").CreateSubdirectory("GameObjects");

						directory = GameObjectDir.CreateSubdirectory(CleanName(gameObject.name));
					}

					//directory.Create();
					Directory.CreateDirectory(directory.FullName);

					var tempDir = new DirectoryInfo(Path.GetTempPath());

					//var tempFile = tempDir.PathWithSlash() + $"GAMEOBJECT-{CleanName(gameObject.name)}.dat";
					var gmFile = directory.PathWithSlash() + $"GAMEOBJECT-{CleanName(gameObject.name)}.dat";

					//var file = File.Create(gmFile);
					//file.Dispose();

					//File.WriteAllText(gmFile, Json.Serialize(gameObject));

					//File.Move(gmFile, gmFile);
					using (var file = File.Create(gmFile))
					{
						using (var writer = new StreamWriter(file))
						{
							writer.Write(Json.Serialize(gameObject));
						}
					}
					if (!objectOnly)
					{
						foreach (var component in gameObject.GetComponents<Component>())
						{
							DebugComponent(component, directory);
							Debugger.Log("FSM FOUND!!!");
							if (component is PlayMakerFSM fsm)
							{
								DebugFSM(fsm, directory);
							}
							if (component is tk2dSprite sprite)
							{
								DebugSprite(sprite, directory);
							}
						}
					}
				}
				catch (Exception e)
				{
					Debugger.LogError($"Error Debugging Object {gameObject} -> {e}");
				}
				var childCount = gameObject.transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					var child = gameObject.transform.GetChild(i).gameObject;
					var subDirectory = directory.CreateSubdirectory(CleanName(gameObject.name));
					DebugObject(child, subDirectory,objectOnly);
				}
			}
			finally
			{
				foreach (var component in gameObject.GetComponents<Component>())
				{
					var awake = component.GetType().GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (awake != null)
					{
						awake.Invoke(component, null);
					}
				}
				foreach (var component in gameObject.GetComponents<Component>())
				{
					var start = component.GetType().GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (start != null)
					{
						start.Invoke(component, null);
					}
				}
				isBeingDebugged.beingDebugged = false;
			}
		}

		static Vector3 MinVector(params Vector3[] vectors)
		{
			Vector3 final = new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity);

			foreach (var vector in vectors)
			{
				if (vector.x < final.x)
				{
					final.x = vector.x;
				}
				if (vector.y < final.y)
				{
					final.y = vector.y;
				}
				if (vector.z < final.z)
				{
					final.z = vector.z;
				}
			}
			return final;
		}

		static Vector3 MaxVector(params Vector3[] vectors)
		{
			Vector3 final = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

			foreach (var vector in vectors)
			{
				if (vector.x > final.x)
				{
					final.x = vector.x;
				}
				if (vector.y > final.y)
				{
					final.y = vector.y;
				}
				if (vector.z > final.z)
				{
					final.z = vector.z;
				}
			}
			return final;
		}

		public static void DebugSprite(tk2dSprite sprite, DirectoryInfo directory)
		{
			try
			{
				foreach (var texture in sprite.Collection.textures)
				{
					TextureSheet sheet = new TextureSheet()
					{
						Height = texture.height,
						Width = texture.width,
						TextureName = texture.name
					};
					sheet.Sprites = new List<SpriteSheet>();
					foreach (var spriteDef in sprite.Collection.spriteDefinitions)
					{
						//spriteDef.positions
						//	spriteDef.
						var materialTexture = spriteDef.material.mainTexture;
						if (texture.GetNativeTexturePtr() == materialTexture.GetNativeTexturePtr())
						{

							//spriteDef.pos
							//var bounds = spriteDef.GetBounds();

							//Vector3 TopRight = bounds.max;
							//Vector3 BottomLeft = bounds.min;
							Vector3 BottomLeft = MinVector(spriteDef.positions);
							Vector3 TopRight = MaxVector(spriteDef.positions);

							float pivotX = Mathf.InverseLerp(BottomLeft.x, TopRight.x, 0.0f);
							float pivotY = Mathf.InverseLerp(BottomLeft.y, TopRight.y, 0.0f);

							//float width = TopRight.x - BottomLeft.x;

							//float unitLength = Mathf.InverseLerp(0.0f, width, 1.0f);

							//float pixelsPerUnit = spriteDef.
							//float pixelPerunits = mySprite.rect.width / mySprite.bounds.size.x;

							//float pixelsPerUnit = unitLength * texture.width;



							sheet.Sprites.Add(new SpriteSheet()
							{

								//BottomLeftX = spriteDef.uvs[0].x,//Mathf.RoundToInt(spriteDef.uvs[0].x * texture.width),
								//BottomLeftY = spriteDef.uvs[0].y,//Mathf.RoundToInt(spriteDef.uvs[0].y * texture.height),
								//TopRightX = spriteDef.uvs[3].x,//Mathf.RoundToInt(spriteDef.uvs[3].x * texture.width),
								//TopRightY = spriteDef.uvs[3].y,//Mathf.RoundToInt(spriteDef.uvs[3].y * texture.height),
								SpriteName = spriteDef.name,
								UVs = spriteDef.uvs.ToList(),
								Flipped = spriteDef.flipped == tk2dSpriteDefinition.FlipMode.None,
								Pivot = new Vector2(1.0f - pivotX, 1.0f - pivotY),
								WorldSize = (TopRight - BottomLeft)
								//PixelsPerUnit = pixelsPerUnit


								//BottomLeft = new Vector2(spriteDef.uvs[0].x * texture.width, spriteDef.uvs[0].y * texture.height),
								//TopRight = new Vector2(spriteDef.uvs[3].x * texture.width, spriteDef.uvs[3].y * texture.height)
							});
						}
					}
					var sheetFile = directory.PathWithSlash() + $"{texture.name} - {sprite.name}.sheet";

					File.WriteAllText(sheetFile, Json.Serialize(sheet));
				}
			}
			catch (Exception e)
			{
				Debugger.LogError($"Error Debugging Object {sprite} -> {e}");
				tk2dSpriteAnimation test;
			}
		}

		public static void DebugFSM(Fsm fsm,DirectoryInfo directory)
		{
			try
			{
				var machine = new XMachine(fsm.Name);

				machine.InitialState = new XState(fsm.StartState);
				foreach (var state in fsm.States)
				{
					XState currentState = new XState(state.Name);
					foreach (var transition in state.Transitions)
					{
						currentState.AddEvent(new XEvent(transition.FsmEvent.Name, new XState(transition.ToState)));
					}
					foreach (var action in state.Actions)
					{
						currentState.AddAction(action.GetType().Name);
					}
					machine.AddState(currentState);
				}

				var fsmFile = directory.PathWithSlash() + $"FSM - {fsm.Name} - {fsm.GameObject.name}.fsm";

				var result = machine.Serialize();

				Debugger.Log("Serialization Result = " + result);

				File.WriteAllText(fsmFile, result);

				var test = XMachine.Deserialize(result);

				Debugger.Log("After Test");
			}
			catch (Exception e)
			{
				Debugger.LogError($"Error Debugging FSM {fsm?.Name} -> {e}");
			}
		}

		public static void DebugFSM(PlayMakerFSM fsm, DirectoryInfo directory)
		{
			DebugFSM(fsm.Fsm,directory);
		}

		public static void DebugComponent(Component component, DirectoryInfo directory)
		{
			try
			{
				directory.Create();

				Type componentType = component.GetType();
				using (var file = File.CreateText($"{directory.PathWithSlash()}{component.GetType().Name} - {CleanName(component.name)}.dat"))
				{
					List<MemberInfo> Members = new List<MemberInfo>();

					Members.AddRange(componentType.GetFields(BindingFlags.Public | BindingFlags.Instance));
					Members.AddRange(componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance));

					foreach (var member in Members)
					{
						var accessibility = GetAccessibility(member);

						object value = null;
						string stringValue = "";
						try
						{
							value = GetValue(member, component);
						}
						catch (Exception e)
						{
							//Debugger.Log("Component = " + component);
							//Debugger.LogWarning($"Warning - Exception thrown when trying to get the value of {member.Name} from type {componentType.FullName} -> {e}");
						}

						if (value == null)
						{
							stringValue = "NULL";
						}
						else
						{
							try
							{
								stringValue = JsonUtility.ToJson(value, true);//Json.Serialize(value);
								if (stringValue == "{}")
								{
									stringValue = value.ToString();
								}
							}
							catch (Exception e)
							{
								bool validException = true;
								Exception inner = e;
								do
								{
									if (inner.Message.Contains("JsonUtility.ToJson does not support engine types"))
									{
										validException = false;
										break;
									}
									inner = inner.InnerException;
								} while (inner != null);
								if (validException)
								{
									Debugger.LogWarning($"Warning - Exception thrown when trying to serialize the value of {member.Name} from type {componentType.FullName} -> {e}");
								}
							}

							if (stringValue == null)
							{
								stringValue = value.ToString();
							}
						}

						file.WriteLine($"{accessibility} {GetMemberType(member)} {member.Name} = {stringValue}");
					}
				}
			}
			catch (Exception e)
			{
				Debugger.LogError($"Error Debugging Component {component} -> {e}");
			}
		}

		static string GetAccessibility(MemberInfo member)
		{
			if (member is FieldInfo field)
			{
				if (field.IsPublic)
				{
					return "public";
				}
				else
				{
					return "private";
				}
			}
			else if (member is PropertyInfo property)
			{
				if (property.GetGetMethod().IsPublic)
				{
					return "public";
				}
				else
				{
					return "private";
				}
			}
			return "";
		}

		static object GetValue(MemberInfo member,object instance)
		{
			if (member is FieldInfo fieldInfo)
			{
				return fieldInfo.GetValue(instance);
			}
			else if (member is PropertyInfo propertyInfo)
			{
				return propertyInfo.GetValue(instance, null);
			}
			return null;
		}

		static Type GetMemberType(MemberInfo member)
		{
			if (member is FieldInfo field)
			{
				return field.FieldType;
			}
			else if (member is PropertyInfo property)
			{
				return property.PropertyType;
;			}
			return null;
		}
	}
}
