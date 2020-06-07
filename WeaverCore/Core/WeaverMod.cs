using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// The definition for a weaver mod. Inherit from this to start mod development
    /// </summary>
    public interface IWeaverMod
    {
        /// <summary>
        /// The name of the mod
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The mod version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The load priority of the mod. The lower it is, the sooner it will load in the mod loading process
        /// </summary>
        int LoadPriority { get; }

        /// <summary>
        /// Whether the mod can be unloaded or not. This is called once when the mod is initially loaded
        /// </summary>
        bool Unloadable { get; }

        /// <summary>
        /// Called when the mod loads up
        /// </summary>
        void Load();
        /// <summary>
        /// Called when the mod unloads. This will only run if <see cref="Unloadable"/> is set to true
        /// </summary>
        void Unload();
    }

    /// <summary>
    /// A class to make it easier to create a Weaver Mod. <seealso cref="IWeaverMod"/>
    /// </summary>
    public abstract class WeaverMod : IWeaverMod
    {
        static string Prettify(string input) => Regex.Replace(input, @"(\S)([A-Z])", @"$1 $2");

        /// <summary>
        /// The name of the mod. Returns the Mod's type name be default
        /// </summary>
        public virtual string Name => Prettify(GetType().Name);

        /// <summary>
        /// The mod version. Returns the assembly version by default
        /// </summary>
        public virtual string Version => GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        /// The load priority of the mod. The lower it is, the sooner it will load in the mod loading process. Returns 0 by default
        /// </summary>
        public virtual int LoadPriority => 0;

        /// <summary>
        /// Whether the mod is unloadable or not. Returns false by default
        /// </summary>
        public virtual bool Unloadable => false;

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public virtual void Load() { }

        /// <summary>
        /// Called when the mod is unloaded
        /// </summary>
        public virtual void Unload() { }
    }
}
