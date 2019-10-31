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
    public sealed class LoadedAudio : IDisposable
    {
        public AudioClip clip { get; internal set; }
        internal Stream resourceAudioStream;
        internal Stream readerStream;

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                readerStream.Dispose();
                resourceAudioStream.Dispose();

                disposed = true;
            }
            GC.SuppressFinalize(this);
        }


        internal NAudio.Wave.ISampleProvider sampler;
        internal int position = 0;

        internal void pcmReader(float[] data)
        {
            sampler.Read(data, position, data.Length);
            position += data.Length;
        }

        internal void SetPosition(int newPosition)
        {
            position = newPosition;
        }

        ~LoadedAudio()
        {
            Dispose();
        }
    }

    public static partial class ResourceLoader
    {

        public static LoadedAudio LoadResourceAudio(string ResourcePath, Assembly searchAssembly = null)
        {
            var audioStream = LoadResource(ResourcePath, searchAssembly);
            NAudio.Wave.Mp3FileReader mp3File = new NAudio.Wave.Mp3FileReader(audioStream);
            var loadedAudio = new LoadedAudio();
            loadedAudio.resourceAudioStream = audioStream;
            loadedAudio.readerStream = mp3File;

            var groups = Regex.Match(ResourcePath, @"(.+?)\.").Groups;
            loadedAudio.sampler = NAudio.Wave.WaveExtensionMethods.ToSampleProvider(mp3File);

            var clip = AudioClip.Create(groups[groups.Count - 1].Value, (int)(mp3File.Length / (mp3File.Mp3WaveFormat.BitsPerSample / 8)), mp3File.Mp3WaveFormat.Channels, mp3File.Mp3WaveFormat.SampleRate, true,loadedAudio.pcmReader,loadedAudio.SetPosition);
            return loadedAudio;
        }
    }
}
