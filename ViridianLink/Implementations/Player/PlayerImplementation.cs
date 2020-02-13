using UnityEngine;
using ViridianLink.Core;

namespace ViridianLink
{

    namespace Implementations
    {
        public abstract class PlayerImplementation : MonoBehaviour, IImplementation
        {
            public abstract void Initialize();
        }
    }
}
