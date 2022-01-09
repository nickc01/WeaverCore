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
}