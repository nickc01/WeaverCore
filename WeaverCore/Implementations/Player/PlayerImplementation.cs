using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class PlayerImplementation : MonoBehaviour, IImplementation
    {
        public abstract void Initialize();
    }
}
