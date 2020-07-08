using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
	public abstract class Mod : Loggable, IMod, ILogger
	{
		/// <summary>
		/// Gets or sets the save settings of this Mod
		/// </summary>
		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x0600441E RID: 17438 RVA: 0x00186D88 File Offset: 0x00184F88
		// (set) Token: 0x0600441F RID: 17439 RVA: 0x00186D8C File Offset: 0x00184F8C
		public virtual ModSettings SaveSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the global settings of this Mod
		/// </summary>
		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x06004420 RID: 17440 RVA: 0x00186D90 File Offset: 0x00184F90
		// (set) Token: 0x06004421 RID: 17441 RVA: 0x00186D94 File Offset: 0x00184F94
		public virtual ModSettings GlobalSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		/// <inheritdoc />
		/// <summary>
		///     Legacy constructor instead of optional argument to not break old mods
		/// </summary>
		// Token: 0x06004422 RID: 17442 RVA: 0x00186D98 File Offset: 0x00184F98
		protected Mod() : this(null)
		{
		}

		/// <inheritdoc />
		/// <summary>
		///     Constructs the mod, assigns the instance and sets the name.
		/// </summary>
		// Token: 0x06004423 RID: 17443 RVA: 0x00186DA4 File Offset: 0x00184FA4
		protected Mod(string name)
		{
			//bool flag = string.IsNullOrEmpty(name);
			if (string.IsNullOrEmpty(name))
			{
				name = this.GetType().Name;
			}
			Name = name;
			Log("Initializing");
			/*bool flag2 = ModLoader.IsSubclassOfRawGeneric(typeof(Mod<>), base.GetType());
			if (!flag2)
			{
				bool flag3 = this._globalSettingsPath == null;
				if (flag3)
				{
					this._globalSettingsPath = Application.persistentDataPath + ModHooks.PathSeperator.ToString() + base.GetType().Name + ".GlobalSettings.json";
				}
				this.LoadGlobalSettings();
				this.HookSaveMethods();
			}*/
		}

		/// <inheritdoc />
		/// <summary>
		///     Get's the Mod's Name
		/// </summary>
		/// <returns></returns>
		// Token: 0x06004424 RID: 17444 RVA: 0x00186E4C File Offset: 0x0018504C
		public string GetName()
		{
			return this.Name;
		}

		/// <inheritdoc />
		/// <summary>
		///     Returns the objects to preload in order for the mod to work.
		/// </summary>
		/// <returns>A List of tuples containing scene name, object name</returns>
		// Token: 0x06004425 RID: 17445 RVA: 0x00186E64 File Offset: 0x00185064
		public virtual List<ValueTuple<string, string>> GetPreloadNames()
		{
			return null;
		}

		/// <inheritdoc />
		/// <summary>
		///     Called after preloading of all mods.
		/// </summary>
		/// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="T:Modding.Mod" /></param>
		// Token: 0x06004426 RID: 17446 RVA: 0x00186E78 File Offset: 0x00185078
		public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			this.Initialize();
		}

		/// <inheritdoc />
		/// <summary>
		///     Returns version of Mod
		/// </summary>
		/// <returns>Mod Version</returns>
		// Token: 0x06004427 RID: 17447 RVA: 0x00186E84 File Offset: 0x00185084
		public virtual string GetVersion()
		{
			return "UNKNOWN";
		}

		/// <inheritdoc />
		/// <summary>
		///     Denotes if the running version is the current version.  Set this with <see cref="T:Modding.GithubVersionHelper" />
		/// </summary>
		/// <returns>If the version is current or not.</returns>
		// Token: 0x06004428 RID: 17448 RVA: 0x00186E9C File Offset: 0x0018509C
		public virtual bool IsCurrent()
		{
			return true;
		}

		/// <summary>
		///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
		/// </summary>
		/// <returns></returns>
		// Token: 0x06004429 RID: 17449 RVA: 0x00186EB0 File Offset: 0x001850B0
		public virtual int LoadPriority()
		{
			return 1;
		}

		/// <summary>
		///     Called after preloading of all mods.
		/// </summary>
		// Token: 0x0600442A RID: 17450 RVA: 0x00186EC4 File Offset: 0x001850C4
		public virtual void Initialize()
		{
		}

		// Token: 0x0600442B RID: 17451 RVA: 0x00186EC8 File Offset: 0x001850C8
		private void HookSaveMethods()
		{
		}

		// Token: 0x0600442C RID: 17452 RVA: 0x00186F1C File Offset: 0x0018511C
		private void LoadGlobalSettings()
		{

		}

		// Token: 0x0600442D RID: 17453 RVA: 0x0018701C File Offset: 0x0018521C
		private void SaveGlobalSettings()
		{

		}

		// Token: 0x0600442E RID: 17454 RVA: 0x0018716C File Offset: 0x0018536C
		private void LoadSaveSettings(SaveGameData data)
		{

		}

		// Token: 0x0600442F RID: 17455 RVA: 0x00187240 File Offset: 0x00185440
		private void SaveSaveSettings(SaveGameData data)
		{

		}

		// Token: 0x04004D8B RID: 19851
		private readonly string _globalSettingsPath;

		/// <summary>
		///     The Mods Name
		/// </summary>
		// Token: 0x04004D8C RID: 19852
		public readonly string Name;
	}
}
