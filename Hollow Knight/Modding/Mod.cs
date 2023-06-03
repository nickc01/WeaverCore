using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
	public abstract class Mod : Loggable, IMod, ILogger
	{
		/// <inheritdoc />
		/// <summary>
		///     Constructs the mod, assigns the instance and sets the name.
		/// </summary>
		protected Mod(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = this.GetType().Name;
			}
			Name = name;
			Log("Initializing");
		}

		/// <inheritdoc />
		/// <summary>
		///     Get's the Mod's Name
		/// </summary>
		/// <returns></returns>
		public string GetName()
		{
			return this.Name;
		}

		/// <inheritdoc />
		/// <summary>
		///     Returns the objects to preload in order for the mod to work.
		/// </summary>
		/// <returns>A List of tuples containing scene name, object name</returns>
		public virtual List<ValueTuple<string, string>> GetPreloadNames()
		{
			return null;
		}

		/// <summary>
		/// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
		/// </summary>
		/// <returns>List of tuples containg scene names and the respective actions.</returns>
		public virtual (string, Func<IEnumerator>)[] PreloadSceneHooks() => Array.Empty<(string, Func<IEnumerator>)>();


        /// <inheritdoc />
        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="T:Modding.Mod" /></param>
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			this.Initialize();
		}

		/// <inheritdoc />
		/// <summary>
		///     Returns version of Mod
		/// </summary>
		/// <returns>Mod Version</returns>
		public virtual string GetVersion()
		{
			return "UNKNOWN";
		}

		/// <inheritdoc />
		/// <summary>
		///     Denotes if the running version is the current version.  Set this with <see cref="T:Modding.GithubVersionHelper" />
		/// </summary>
		/// <returns>If the version is current or not.</returns>
		public virtual bool IsCurrent()
		{
			return true;
		}

		/// <summary>
		///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
		/// </summary>
		/// <returns></returns>
		public virtual int LoadPriority()
		{
			return 1;
		}

		/// <summary>
		///     Called after preloading of all mods.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		///     The Mods Name
		/// </summary>
		public readonly string Name;
	}
}
