using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using NLayer;

internal static class AudioLoader
{
    class MpegHolder : IDisposable
    {
        MpegFile reader;
        Stream InputStream;
        int position = 0;

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                reader.Dispose();
                InputStream.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }


        public void ReadData(float[] data)
        {
            //position += reader.ReadSamples(data, 0, data.Length);
            position += reader.ReadSamples(data, 0, data.Length);
        }

        public void SetPosition(int newPosition)
        {
            position = newPosition;
        }

        public MpegHolder(MpegFile Reader,Stream inputStream)
        {
            reader = Reader;
            InputStream = inputStream;
        }

        ~MpegHolder()
        {
            Dispose();
        }
    }



    internal static void LoadAudio(string name, Stream inputAudioStream, out AudioClip outputClip, out IDisposable holder)
    {
        
        var mpeg = new MpegFile(inputAudioStream);
        var newHolder = new MpegHolder(mpeg,inputAudioStream);
        holder = newHolder;
        outputClip = AudioClip.Create(name, (int)mpeg.Length / sizeof(float) / mpeg.Channels, mpeg.Channels, mpeg.SampleRate, true, newHolder.ReadData, newHolder.SetPosition);
    }
}

