using Modding;
using System.Collections.Generic;
using System.Linq;
using WeaverCore.Utilities;

namespace WeaverCore.Game
{
	public class WeaverModTemplate : IWeaverMod
	{
		public string Name => "Template Mod";

		public string Version => "1.2.3.4";

		public int LoadPriority => 0;

		public bool Unloadable => true;

		public void Load()
		{

		}

		public void Unload()
		{

		}
	}


	public class ModTemplate : Mod, ITogglableMod
	{
		private IWeaverMod mod;

		private List<Registry> registries;

		public ModTemplate() : base("MODNAMEHERE")
		{
			mod = new WeaverModTemplate();
			registries = RegistryLoader.GetModRegistries(mod.GetType()).ToList();
		}

		public override string GetVersion()
		{
			return mod.Version;
		}

		public override void Initialize()
		{
			foreach (var registry in registries)
			{
				registry.RegistryEnabled = true;
			}
			mod.Load();
		}

		public override bool IsCurrent()
		{
			return true;
		}

		public override int LoadPriority()
		{
			return mod.LoadPriority;
		}

		public void Unload()
		{
			foreach (var registry in registries)
			{
				registry.RegistryEnabled = false;
			}
			mod.Unload();
		}
	}
}
