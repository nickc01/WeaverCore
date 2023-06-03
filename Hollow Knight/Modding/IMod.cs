using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    public interface IMod : ILogger
    {
        string GetName();

        List<System.ValueTuple<string,string>> GetPreloadNames();

        /// <summary>
        /// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
        /// </summary>
        /// <returns>List of tuples containg scene names and the respective actions.</returns>
        (string, Func<IEnumerator>)[] PreloadSceneHooks();

        void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);

        string GetVersion();

        bool IsCurrent();

        int LoadPriority();
    }
}