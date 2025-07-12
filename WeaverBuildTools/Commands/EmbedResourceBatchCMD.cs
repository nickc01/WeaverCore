using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using WeaverBuildTools.Enums;
using WeaverCore.Utilities;

namespace WeaverBuildTools.Commands
{
    /// <summary>
    /// Batch version of EmbedResourceCMD that can embed multiple resources in a single operation
    /// </summary>
    public static class EmbedResourceBatchCMD
    {
        /// <summary>
        /// Represents a resource to be embedded
        /// </summary>
        public class ResourceToEmbed
        {
            public string ResourceName { get; set; }
            public string FilePath { get; set; }
            public string Hash { get; set; }
            public CompressionMethod Compression { get; set; } = CompressionMethod.Auto;
        }

        /// <summary>
        /// Embeds multiple resources into an assembly in a single operation
        /// </summary>
        /// <param name="assemblyToEmbedTo">Path to the assembly to embed resources into</param>
        /// <param name="resourcesToEmbed">List of resources to embed</param>
        public static void EmbedResourcesBatch(string assemblyToEmbedTo, List<ResourceToEmbed> resourcesToEmbed)
        {
            if (resourcesToEmbed == null || resourcesToEmbed.Count == 0)
            {
                return;
            }

            UnityEngine.Debug.Log($"Batch embedding {resourcesToEmbed.Count} resources into {assemblyToEmbedTo}");

            double previousTime = GetTime();
            while (GetTime() - previousTime <= 20.0)
            {
                try
                {
                    using (var resolver = new MainResolver())
                    {
                        using (var definition = AssemblyDefinition.ReadAssembly(assemblyToEmbedTo, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver }))
                        {
                            bool hasChanges = false;

                            foreach (var resource in resourcesToEmbed)
                            {
                                // Calculate hash if not provided
                                if (string.IsNullOrEmpty(resource.Hash))
                                {
                                    using (var fileStream = File.OpenRead(resource.FilePath))
                                    {
                                        resource.Hash = HashUtilities.GetHash(fileStream);
                                    }
                                }

                                // Check if resource already exists with same hash
                                var existingResource = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resource.ResourceName);
                                if (existingResource != null)
                                {
                                    var metaResource = definition.MainModule.Resources.FirstOrDefault(r => r.Name == (resource.ResourceName + "_meta"));
                                    if (metaResource != null && metaResource is EmbeddedResource)
                                    {
                                        var embeddedHash = (EmbeddedResource)metaResource;
                                        using (var metaStream = embeddedHash.GetResourceStream())
                                        {
                                            var meta = ResourceMetaData.FromStream(metaStream);
                                            if (meta.hash == resource.Hash)
                                            {
                                                // Resource already exists with same hash, skip
                                                continue;
                                            }
                                        }
                                    }
                                    // Remove existing resource and metadata
                                    definition.MainModule.Resources.Remove(existingResource);
                                    if (metaResource != null)
                                    {
                                        definition.MainModule.Resources.Remove(metaResource);
                                    }
                                }

                                // Embed the resource
                                EmbedSingleResource(definition, resource);
                                hasChanges = true;
                            }

                            // Write assembly only once if there were changes
                            if (hasChanges)
                            {
                                definition.MainModule.Write();
                            }
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("because it is being used by another process"))
                    {
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (GetTime() - previousTime > 20.0f)
            {
                throw new Exception("Batch Embedding Timeout");
            }
        }

        /// <summary>
        /// Embeds a single resource into the assembly definition
        /// </summary>
        private static void EmbedSingleResource(AssemblyDefinition definition, ResourceToEmbed resource)
        {
            byte[] dataToEmbed;
            bool actuallyCompressed = false;

            if (resource.Compression == CompressionMethod.Auto || resource.Compression == CompressionMethod.UseCompression)
            {
                // Read original file data
                var originalData = File.ReadAllBytes(resource.FilePath);
                
                // Try compression
                using (var compressedStream = new MemoryStream())
                {
                    using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        compressionStream.Write(originalData, 0, originalData.Length);
                    }
                    var compressedData = compressedStream.ToArray();
                    
                    // Choose best option
                    if (compressedData.Length < originalData.Length || resource.Compression == CompressionMethod.UseCompression)
                    {
                        dataToEmbed = compressedData;
                        actuallyCompressed = true;
                    }
                    else
                    {
                        dataToEmbed = originalData;
                        actuallyCompressed = false;
                    }
                }
            }
            else
            {
                // No compression
                dataToEmbed = File.ReadAllBytes(resource.FilePath);
                actuallyCompressed = false;
            }

            // Create embedded resource from byte array
            var er = new EmbeddedResource(resource.ResourceName, ManifestResourceAttributes.Public, dataToEmbed);
            definition.MainModule.Resources.Add(er);

            // Create metadata resource
            using (var metaStream = new ResourceMetaData(actuallyCompressed, resource.Hash).ToStream())
            {
                var metaData = new byte[metaStream.Length];
                metaStream.Read(metaData, 0, metaData.Length);
                var hashResource = new EmbeddedResource(resource.ResourceName + "_meta", ManifestResourceAttributes.Public, metaData);
                definition.MainModule.Resources.Add(hashResource);
            }
        }

        private static double GetTime()
        {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
        }
    }
}