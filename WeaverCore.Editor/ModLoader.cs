using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Used for loading mods from within the Editor
    /// </summary>
    static class ModLoader
    {
        [OnRuntimeInit(int.MaxValue)]
        static void RuntimeInit()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    List<IMod> mods = new List<IMod>();
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IMod).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                        {
                            mods.Add((IMod)Activator.CreateInstance(type));
                        }
                    }
                    foreach (var mod in mods.OrderBy(m => m.LoadPriority()))
                    {
                        mod.Initialize(new Dictionary<string, Dictionary<string, UnityEngine.GameObject>>());
                        var methods = ReflectionUtilities.GetMethodsWithAttribute<AfterModLoadAttribute>().ToList();

                        foreach (var method in methods)
                        {
                            try
                            {
                                if (method.attribute.ModType.IsAssignableFrom(mod.GetType()))
                                {
                                    var parameters = method.method.GetParameters();
                                    if (parameters.GetLength(0) == 1 && parameters[0].ParameterType.IsAssignableFrom(mod.GetType()))
                                    {
                                        method.method.Invoke(null, new object[] { mod });
                                    }
                                    else
                                    {
                                        method.method.Invoke(null, null);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                WeaverLog.LogError($"Error running function : {method.method.DeclaringType.FullName}:{method.method.Name}");
                                WeaverLog.LogException(e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    WeaverLog.Log("Error Loading Mod [" + assembly.GetName().Name + " : " + e);
                }
            }

            //ModHooks.OnFinishedLoadingMods();
            typeof(ModHooks).GetMethod("OnFinishedLoadingMods", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }
    }
}
