/*namespace Test
{
    public class TestMod2 : Modding.Mod
    {
        private TestVMod mod;
        private ViridianLink.Core.Registry registry;

        public TestMod2(string name) : base(name)
        {
            mod = new TestVMod();
            registry = ViridianLink.Extras.RegistryLoader.GetModRegistry(mod.GetType());
        }

        public override void Initialize()
        {
            if (registry != null)
            {
                registry.RegistryEnabled = true;
            }
            mod.Load();
        }

        public override int LoadPriority()
        {
            return mod.LoadPriority;
        }

        public override string GetVersion()
        {
            return mod.Version;
        }
    }
}*/

/*namespace Test
{
    public class TestDefinition : Modding.Mod, Modding.ITogglableMod
    {
        private TestVMod mod;
        private ViridianLink.Core.Registry registry;

        public TestDefinition() : base("MODNAMEHERE")
        {
            mod = new TestVMod();
            registry = ViridianLink.Extras.RegistryLoader.GetModRegistry(mod.GetType());
        }

        public override void Initialize()
        {
            if (registry != null)
            {
                registry.RegistryEnabled = true;
            }
            mod.Load();
        }

        public override string GetVersion() => mod.Version;

        public override bool IsCurrent() => true;

        public override int LoadPriority() => mod.LoadPriority;

        public void Unload()
        {
            if (registry != null)
            {
                registry.RegistryEnabled = false;
            }
            mod.Unload();
        }
    }
}

namespace Test
{
    public class TestVMod : ViridianLink.Core.IViridianMod
    {
        public string Name => throw new System.NotImplementedException();

        public string Version => throw new System.NotImplementedException();

        public int LoadPriority => throw new System.NotImplementedException();

        public bool Unloadable => throw new System.NotImplementedException();

        public void Load()
        {
            throw new System.NotImplementedException();
        }

        public void Unload()
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestMod : Modding.IMod, Modding.ITogglableMod
    {
        private TestVMod mod;
        private ViridianLink.Core.Registry registry;

        public TestMod()
        {
            mod = new TestVMod();
            registry = ViridianLink.Extras.RegistryLoader.GetModRegistry(mod.GetType());
        }

        public string GetName() => mod.Name;

        public System.Collections.Generic.List<(string, string)> GetPreloadNames() => null;

        public string GetVersion() => mod.Version;

        public void Initialize(System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>> preloadedObjects)
        {
            if (registry != null)
            {
                registry.RegistryEnabled = true;
            }
            mod.Load();
        }

        public bool IsCurrent() => true;

        public int LoadPriority() => mod.LoadPriority;

        public void Log(string message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void Log(object message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void LogDebug(string message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void LogDebug(object message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void LogError(string message)
        {
            ViridianLink.Extras.Debugger.LogError(mod.Name + " : " + message);
        }

        public void LogError(object message)
        {
            ViridianLink.Extras.Debugger.LogError(mod.Name + " : " + message);
        }

        public void LogFine(string message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void LogFine(object message)
        {
            ViridianLink.Extras.Debugger.Log(mod.Name + " : " + message);
        }

        public void LogWarn(string message)
        {
            ViridianLink.Extras.Debugger.LogWarning(mod.Name + " : " + message);
        }

        public void LogWarn(object message)
        {
            ViridianLink.Extras.Debugger.LogWarning(mod.Name + " : " + message);
        }

        public void Unload()
        {
            if (registry != null)
            {
                registry.RegistryEnabled = false;
            }
            mod.Unload();
        }
    }
}*/
