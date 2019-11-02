using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VoidCore
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
        static Assembly AudioAssembly;

        static void Initialize()
        {
            ModLog.Log("A");
            Assembly.Load("NLayer");
            AudioAssembly = Assembly.Load("VoidCore.NLayer");
            ModLog.Log("B");
            ModLog.Log("Assembly = " + AudioAssembly);
            ModLog.Log("TYPE = " + AudioAssembly.GetType("AudioLoader"));
            ModLog.Log("Method = " + AudioAssembly.GetType("AudioLoader").GetMethod("LoadAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
            loader = (AudioLoader)Delegate.CreateDelegate(typeof(AudioLoader),AudioAssembly.GetType("AudioLoader").GetMethod("LoadAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),true);
            ModLog.Log("C");
        }


        static bool Started = false;

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
            
            /*if (!RunBefore)
            {
                RunBefore = true;
                AudioSetup();
            }
            ModLog.Log("G");
            var audioStream = LoadResource(ResourcePath, searchAssembly);
            //NAudio.Wave.Mp3FileReader mp3File = new NAudio.Wave.Mp3FileReader(audioStream);
            ModLog.Log("H");
            Stream mp3File = CreateMp3Stream(audioStream);
            ModLog.Log("I");
            var loadedAudio = new LoadedAudio();
            loadedAudio.resourceAudioStream = audioStream;
            loadedAudio.readerStream = mp3File;
            ModLog.Log("J");


            var groups = Regex.Match(ResourcePath, @"(.+?)\.").Groups;
            //loadedAudio.sampler = NAudio.Wave.WaveExtensionMethods.ToSampleProvider(mp3File);
            var waveFormat = GetWaveFormat(mp3File);
            ModLog.Log("K");
            loadedAudio.sampler = GetSampler(mp3File,waveFormat);
            ModLog.Log("L");
            ModLog.Log("M");
            var clip = AudioClip.Create(groups[groups.Count - 1].Value, (int)(mp3File.Length / (GetBitsPerSample(waveFormat) / 8)), GetChannels(waveFormat), GetSampleRate(waveFormat), true,loadedAudio.pcmReader,loadedAudio.SetPosition);
            return loadedAudio;*/
        }
    }
}