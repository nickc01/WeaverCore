using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    public interface IMod : ILogger
    {
        string GetName();

        List<System.ValueTuple<string,string>> GetPreloadNames();

        void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);

        string GetVersion();

        bool IsCurrent();

        int LoadPriority();
    }

    public interface IMod<T> : IMod where T : ModSettings
    {
        T Settings { get; set; }
    }

    public interface IMod<T, TG> : IMod where T : ModSettings where TG : ModSettings
    {
        T Settings { get; set; }

        TG GlobalSettings { get; set; }
    }
}