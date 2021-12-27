using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Used for loading resources from an assembly, and other related actions
    /// </summary>
    public static class ResourceUtilities
    {
        /// <summary>
        /// Loads an assembly from a resource path in an existing assembly
        /// </summary>
        /// <param name="resourcePath">The path of the assembly resource</param>
        /// <param name="assembly">The assembly to load the resource from</param>
        /// <returns>Returns the loaded assembly</returns>
        public static Assembly LoadAssembly(string resourcePath,Assembly assembly = null)
        {
            if (!HasResource(resourcePath,assembly))
            {
                return null;
            }
            using (Stream stream = Retrieve(resourcePath, assembly))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return Assembly.Load(reader.ReadBytes((int)stream.Length));
                }
            }
        }

        /// <summary>
        /// Checks if the assembly has the specified resource path
        /// </summary>
        /// <param name="resourcePath">The path to check if it exists</param>
        /// <param name="assembly">The assembly to check in</param>
        /// <returns>Returns whether the specified resource path exists in the assembly</returns>
        public static bool HasResource(string resourcePath,Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceUtilities).Assembly;
            }
            foreach (var path in assembly.GetManifestResourceNames())
            {
                if (path == resourcePath)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves a stream of data from the resource path in the assembly
        /// </summary>
        /// <param name="resourcePath">The path of the resource to load</param>
        /// <param name="assembly">The assembly to load from</param>
        /// <returns>Returns a stream containing the data of the resource</returns>
        public static Stream Retrieve(string resourcePath,Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceUtilities).Assembly;
            }

            if (!HasResource(resourcePath))
            {
                return null;
            }

            bool compressed = false;
            if (HasResource(resourcePath + "_meta"))
            {
                using (var metaStream = assembly.GetManifestResourceStream(resourcePath + "_meta"))
                {
                    int compressedByte = metaStream.ReadByte();
                    compressed = compressedByte == 1;
                }
            }

            if (!compressed)
            {
                return assembly.GetManifestResourceStream(resourcePath);
            }
            else
            {
                MemoryStream finalStream = new MemoryStream();
                using (Stream compressedStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    using (var decompressionStream = new GZipStream(compressedStream,CompressionMode.Decompress))
                    {
                        StreamUtilities.CopyTo(decompressionStream, finalStream);
                    }
                }
                return finalStream;
            }
        }
    }
}
