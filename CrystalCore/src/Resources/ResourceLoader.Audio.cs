using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CrystalCore
{
    public class LoadedAudio : IDisposable
    {
        public AudioClip Clip { get; internal set; }
        IDisposable ExtraResources;
        internal LoadedAudio(AudioClip clip,IDisposable extraResources)
        {
            Clip = clip;
            ExtraResources = extraResources;
        }


        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~LoadedAudio()
        {
            Dispose();
        }
    }


    public static partial class ResourceLoader
    {
        delegate void AudioLoader(string name, Stream inputAudioStream, out AudioClip outputClip, out IDisposable holder);

        static AudioLoader loader;
        static Assembly Mp3AudioAssembly;

        static void Initialize()
        {
            Mp3AudioAssembly = Assembly.Load("CrystalCore.NLayer");
            loader = (AudioLoader)Delegate.CreateDelegate(typeof(AudioLoader),Mp3AudioAssembly.GetType("AudioLoader").GetMethod("LoadAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),true);
        }


        static bool Started = false;

        /// <summary>
        /// Loads a resource as a AudioClip
        /// </summary>
        /// <param name="ResourcePath">The path to the resource in the project</param>
        /// <param name="searchAssembly">The assembly to search in. If left null, it searches the Executing Assembly</param>
        /// <returns>The stream to the resource file</returns>
        public static LoadedAudio LoadResourceAudio(string ResourcePath, Assembly searchAssembly = null)
        {
            if (!Started)
            {
                Started = true;
                Initialize();
            }

            var audioStream = LoadResource(ResourcePath, searchAssembly);

            var groups = Regex.Match(ResourcePath, @"(.+?)\.").Groups;

            AudioClip outputClip;
            IDisposable holder;


            loader(groups[groups.Count - 1].Value, audioStream, out outputClip, out holder);

            return new LoadedAudio(outputClip, holder);
        }
    }
}