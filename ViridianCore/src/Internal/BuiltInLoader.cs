using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ViridianCore;
using ViridianCore.Hooks;

namespace ViridianCore
{
    internal static class BuiltInLoader
    {


        static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();


        static internal Assembly AssemblyLoader(object sender, ResolveEventArgs args)
        {
            try
            {
                if (LoadedAssemblies.ContainsKey(args.Name))
                {
                    return LoadedAssemblies[args.Name];
                }
                var assemblyName = new AssemblyName(args.Name);

                //var sourceAssembly = typeof(ViridianCore).Assembly;
                foreach (var sourceAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var resource in sourceAssembly.GetManifestResourceNames())
                        {
                            if (resource.Contains(assemblyName.Name))
                            {
                                using (var stream = sourceAssembly.GetManifestResourceStream(resource))
                                {
                                    byte[] assemblyRaw = new byte[stream.Length];
                                    stream.Read(assemblyRaw, 0, assemblyRaw.Length);
                                    LoadedAssemblies.Add(args.Name, Assembly.Load(assemblyRaw));
                                    Modding.Logger.Log("Loaded Assembly : " + args.Name);
                                    return LoadedAssemblies[args.Name];
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
            return null;
        }
    }
}
