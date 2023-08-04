using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build.Content;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace WeaverCore.Editor.Compilation
{
    public abstract class BuildPipelineCustomizer
    {
        static BuildPipelineCustomizer _currentCustomizer;

        public static Type GetCurrentPipelineCustomizerType()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => typeof(BuildPipelineCustomizer).IsAssignableFrom(t) && !t.IsAbstract && !t.ContainsGenericParameters && t.IsClass);
        }

        public static BuildPipelineCustomizer GetCurrentPipelineCustomizer()
        {
            return _currentCustomizer ??= (BuildPipelineCustomizer)Activator.CreateInstance(GetCurrentPipelineCustomizerType());
        }

        public static bool TryGetCurrentCustomizer(out BuildPipelineCustomizer customizer)
        {
            customizer = GetCurrentPipelineCustomizer();
            return customizer != null;
        }




        /// <summary>
        /// Called to save any persistent settings
        /// </summary>
        public virtual void OnSaveSettings()
        {

        }

        /// <summary>
        /// Called to load any persistent settings
        /// </summary>
        public virtual void OnLoadSettings()
        {

        }


        /// <summary>
        /// Used for adding any extra properties to the Build Screen
        /// </summary>
        public virtual void BuildScreenOnGUIExtension(BuildScreen sourceScreen)
        {

        }

        /// <summary>
        /// Used to verify the build settings on the build screen before starting the build
        /// </summary>
        public virtual bool PreBuildVerify()
        {
            return true;
        }

        /// <summary>
        /// Runs a tast before the build begins. Returns false if an error occurs
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> BeforeBuildBegin()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// After all the assemblies have been build, the names of these assemblies will get changed. This is useful for making sure assetbundles point to the right assemblies when finding scripts
        /// </summary>
        public virtual void ChangeAssemblyNames(Dictionary<string, string> assemblyNamesToChange)
        {

        }

        /// <summary>
        /// Used to modify which asset bundles get injected into which assemblies.
        /// </summary>
        public virtual void ChangeBundleAssemblyPairings(Dictionary<string, AssemblyName> bundleToAssemblyPairings, List<AssemblyName> assemblyNames)
        {

        }

        /// <summary>
        /// Used to run any extra steps after all the other build steps have been finished (compiling and bundling of assets)
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> OnAfterBuildFinished()
        {
            return Task.FromResult(true);
        }

    }
}
