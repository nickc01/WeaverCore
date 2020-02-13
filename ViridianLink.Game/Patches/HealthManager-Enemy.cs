using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ViridianLink.Core;
using ViridianLink.Features;
using ViridianLink.Extras;
using System.IO;

namespace ViridianLink.Game.Patches
{
	class HealthManager_Enemy : IPatch
	{
		public void Apply()
		{
			On.HealthManager.Start += HealthManager_Start;
		}

		private void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			try
			{
				Debugger.Log("Enemy Name = " + self.gameObject.name);
				self.gameObject.AddComponent<Enemy>();
				Debugger.Log("A");
				var replacement = Registry.GetAllFeatures<EnemyReplacement>(r => r.EnemyToReplace == self.gameObject.name).FirstOrDefault();
				Debugger.Log("B");
				Debugger.Log("Replacement = " + replacement?.name);
				if (replacement != null)
				{
					Debugger.Log("C");
					foreach (var component in replacement.GetComponents<Component>())
					{
						Debugger.Log("Component = " + component?.name);
						Debugger.Log("Component Type = " + component?.GetType());
					}
				}
				Debugger.Log("D");
				if (replacement != null)
				{
					Debugger.Log($"Replacing {self.gameObject.name} with {replacement.gameObject.name}");
					//GameObject.Destroy(self.gameObject);
					Debugger.Log("E");
					Debugger.Log("GameObject = " + replacement.gameObject);
					GameObject.Instantiate(replacement.gameObject);
					Debugger.Log("F");

					var textureDumpFolder = Path.GetTempPath() + "TextureDumpGame\\";
					Directory.CreateDirectory(textureDumpFolder);
					foreach (var renderer in GameObject.FindObjectsOfType<tk2dBaseSprite>())
					{
						try
						{
							var texture = renderer.CurrentSprite.material.mainTexture;
							if (texture is Texture2D)
							{
								var data = UnityEngine.ImageConversion.EncodeToPNG((Texture2D)texture);
								using (var file = File.Open(textureDumpFolder + texture.name + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
								{
									file.Write(data, 0, data.Length);
								}
								Debugger.Log("Texture = " + texture);

							}
						}
						catch (Exception e)
						{
							Debugger.Log("E = " + e);
						}
					}

				}
			}
			catch (Exception e)
			{
				Debugger.LogError("Health Manager Exception = " + e);
			}
			finally
			{
				orig(self);
			}
		}
	}
}
