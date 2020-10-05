using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Features;
using System.IO;
using WeaverCore.Utilities;
using System.Reflection;
using System.Security.Permissions;
using WeaverCore.Interfaces;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	class HealthManager_Enemy
	{
		[OnInit]
		static void Init()
		{
			On.HealthManager.Start += HealthManager_Start;
		}

		private static void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			bool destroyed = false;

			var child = self.transform.Find("White Flash");
			//Debugger.Log("Child = " + child);
			if (child != null)
			{
				//DebugPrinting(child.gameObject);
			}

			//DebugPrinting(self.gameObject);

			try
			{
				self.gameObject.AddComponent<Enemy>();
				var replacement = (Enemy)Registry.GetAllFeatures<IObjectReplacement>(r => r is Enemy && r.ThingToReplace == self.gameObject.name).FirstOrDefault();
				//WeaverLog.Log("Enemy = " + self.gameObject);
				//WeaverLog.Log("Replacement = " + replacement);
				if (replacement != null)
				{
					var instance = GameObject.Instantiate(replacement.gameObject);
					/*var eventReceiver = instance.GetComponent<EventReceiver>();
					if (eventReceiver != null)
					{
						eventReceiver.ReceiveEventsFromObject(self.gameObject);
					}*/
					GameObject.Destroy(self.gameObject);
					destroyed = true;
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogError("Exception occured while spawning enemy replacement : " + e);
			}
			finally
			{
				if (!destroyed)
				{
					orig(self);
				}
			}
		}

		static void DebugPrinting(GameObject self)
		{
			//Debugger.Log("PRETEST");

			var healthManager = self.GetComponent<HealthManager>();
			if (healthManager != null)
			{
				//Debugger.Log($"{nameof(HealthManager)} Type = " + healthManager);
				//Debugger.Log("CCC");
				var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("WeaverTools"));
				//Debugger.Log("Assembly = " +  assembly);
				var type = assembly.GetType("WeaverTools.ObjectDebugger");
				//Debugger.Log("Type = " + type);
				var method = type.GetMethod("DebugObject", BindingFlags.Public | BindingFlags.Static);
				//Debugger.Log("Method = " + method);
				var printObjectM = method;

				printObjectM.Invoke(null, new object[] { self });
				//Debugger.Log("DDD");
				foreach (var field in healthManager.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					//field.Name.ToUpper().Contains("PREFAB")
					if (field.FieldType.Name.Contains("GameObject"))
					{
						//Debugger.Log(field.Name + " Value = " + field.GetValue(healthManager));

						var raw = field.GetValue(healthManager);
						tk2dSprite sprite = null;
						GameObject sourceObject = null;

						if (raw is Component c)
						{
							sprite = c.GetComponent<tk2dSprite>();
							sourceObject = c.gameObject;
						}
						else if (raw is GameObject g)
						{
							sprite = g.GetComponent<tk2dSprite>();
							sourceObject = g;
						}
						else
						{
							continue;
						}

						printObjectM.Invoke(null, new object[] { sourceObject });

						var spriteRenderer = sourceObject.GetComponentInChildren<SpriteRenderer>();
						//Debugger.Log("Sprite Renderer = " + spriteRenderer);
						if (spriteRenderer != null)
						{
							//Debugger.Log("Sprite = " + spriteRenderer.sprite.name);
							//Debugger.Log("Rect = " + spriteRenderer.sprite.rect);
							//Debugger.Log("Texture = " + spriteRenderer.sprite.texture.name);
							//Debugger.Log($"Size = {spriteRenderer.sprite.texture.width}, {spriteRenderer.sprite.texture.height}");
							//Debugger.Log("Pixel Per Unit = " + spriteRenderer.sprite.pixelsPerUnit);

							var animator = sourceObject.GetComponentInChildren<Animator>();
							if (animator != null)
							{
								//Debugger.Log("Animator = " + animator.name);
								foreach (var clip in animator.runtimeAnimatorController.animationClips)
								{
									//Debugger.Log("Clip = " + clip.name);
									//Debugger.Log("Clip Framerate = " + clip.frameRate);
								}
							}
						}

						if (sprite != null)
						{
							Texture mainTexture = sprite.Collection.FirstValidDefinition.material.mainTexture;
							//Debugger.Log("Texture for " + field.Name + " = " + mainTexture.name);
							//Debugger.Log($"Size = {mainTexture.width} , {mainTexture.height}");

							//sprite.Collection.FirstValidDefinition.material.mainTexture.name;
						}

					}
					else if (field.FieldType.Name.Contains("AudioEvent"))
					{
						//Debugger.Log("Audio Field = " + field.Name);
						AudioEvent e = (AudioEvent)field.GetValue(healthManager);

						//Debugger.Log("Clip = " + e.Clip?.name);
						//Debugger.Log("Volume = " + e.Volume);
					}
				}
			}
			else
			{
				var spriteRenderer = self.GetComponentInChildren<SpriteRenderer>();
				//Debugger.Log("Sprite Renderer = " + spriteRenderer);
				if (spriteRenderer != null)
				{
					//Debugger.Log("Sprite = " + spriteRenderer.sprite.name);
					//Debugger.Log("Rect = " + spriteRenderer.sprite.rect);
					//Debugger.Log("Texture = " + spriteRenderer.sprite.texture.name);
					//Debugger.Log($"Size = {spriteRenderer.sprite.texture.width}, {spriteRenderer.sprite.texture.height}");
					//Debugger.Log("Pixel Per Unit = " + spriteRenderer.sprite.pixelsPerUnit);

					var animator = self.GetComponentInChildren<Animator>();
					if (animator != null)
					{
						//Debugger.Log("Animator = " + animator.name);
						foreach (var clip in animator.runtimeAnimatorController.animationClips)
						{
							//Debugger.Log("Clip = " + clip.name);
							//Debugger.Log("Clip Framerate = " + clip.frameRate);
						}
					}
				}
			}
		}
	}
}
