using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Helpers;

namespace ViridianLink.Core
{
    public interface IViridianMod
    {
        string Name { get; }
        string Version { get; }

        int LoadPriority { get; }

        bool Unloadable { get; }

        void Load();
        void Unload();
    }

    public sealed class ViridianLinkMod : ViridianMod
    {
        public override string Name => nameof(ViridianLink);
    }

    public abstract class ViridianMod : IViridianMod
    {
        public virtual string Name => GetType().Name;

        public virtual string Version => GetType().Assembly.GetName().Version.ToString();

        public virtual int LoadPriority => 0;

        public virtual bool Unloadable => false;

        public virtual void Load() { }

        public virtual void Unload() { }
    }
}
