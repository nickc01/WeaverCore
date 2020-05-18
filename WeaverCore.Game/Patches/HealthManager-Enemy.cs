using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Features;
using System.IO;
using WeaverCore.Helpers;
using System.Reflection;
using System.Security.Permissions;

namespace WeaverCore.Game.Patches
{
	class HealthManager_Enemy : IPatch
	{
		public void Apply()
		{
			On.HealthManager.Start += HealthManager_Start;
		}

		private void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			bool destroyed = false;
			try
			{
				self.gameObject.AddComponent<Enemy>();

				Debugger.Log("PRETEST");

				var hitEffects = self.GetComponent<IHitEffectReciever>();
				if (hitEffects != null)
				{
					Debugger.Log("Hit Effects Type = " + hitEffects);
					//Debugger.Log("CCC");
					var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("WeaverTools"));
					//Debugger.Log("Assembly = " +  assembly);
					var type = assembly.GetType("WeaverTools.ObjectDebugger");
					//Debugger.Log("Type = " + type);
					var method = type.GetMethod("DebugObject", BindingFlags.Public | BindingFlags.Static);
					//Debugger.Log("Method = " + method);
					var printObjectM = method;
					//Debugger.Log("DDD");
					foreach (var field in hitEffects.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						//field.Name.ToUpper().Contains("PREFAB")
						if (field.FieldType.Name.Contains("GameObject"))
						{
							Debugger.Log(field.Name + " Value = " + field.GetValue(hitEffects));

							var raw = field.GetValue(hitEffects);
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
							Debugger.Log("Sprite Renderer = " + spriteRenderer);
							if (spriteRenderer != null)
							{
								Debugger.Log("Sprite = " + spriteRenderer.sprite.name);
								Debugger.Log("Rect = " + spriteRenderer.sprite.rect);
								Debugger.Log("Texture = " + spriteRenderer.sprite.texture.name);
								Debugger.Log($"Size = {spriteRenderer.sprite.texture.width}, {spriteRenderer.sprite.texture.height}");
								Debugger.Log("Pixel Per Unit = " + spriteRenderer.sprite.pixelsPerUnit);

								var animator = sourceObject.GetComponentInChildren<Animator>();
								if (animator != null)
								{
									Debugger.Log("Animator = " + animator.name);
									foreach (var clip in animator.runtimeAnimatorController.animationClips)
									{
										Debugger.Log("Clip = " + clip.name);
										Debugger.Log("Clip Framerate = " + clip.frameRate);
									}
								}
							}

							if (sprite != null)
							{
								Texture mainTexture = sprite.Collection.FirstValidDefinition.material.mainTexture;
								Debugger.Log("Texture for " + field.Name + " = " + mainTexture.name);
								Debugger.Log($"Size = {mainTexture.width} , {mainTexture.height}");

								//sprite.Collection.FirstValidDefinition.material.mainTexture.name;
							}
							
						}
						else if (field.FieldType.Name.Contains("AudioEvent"))
						{
							Debugger.Log("Audio Field = " + field.Name);
							AudioEvent e = (AudioEvent)field.GetValue(hitEffects);

							Debugger.Log("Clip = " + e.Clip?.name);
							Debugger.Log("Volume = " + e.Volume);
						}
					}
				}

				var replacement = Registry.GetAllFeatures<EnemyReplacement>(r => r.EnemyToReplace == self.gameObject.name).FirstOrDefault();

				if (replacement != null)
				{
					var instance = GameObject.Instantiate(replacement.gameObject);
					EventReceiver.ReceiveEventsFromObject(self.gameObject, instance);
					GameObject.Destroy(self.gameObject);
					destroyed = true;
				}
			}
			catch (Exception e)
			{
				Debugger.LogError("Exception occured while spawning enemy replacement : " + e);
			}
			finally
			{
				if (!destroyed)
				{
					orig(self);
				}
			}
		}
	}
}
