using ViridianLink.Core;

namespace ViridianLink.Implementations.Allocators
{
    public abstract class PlayerAllocator : IImplementation
    {
        public abstract PlayerImplementation Allocate();
    }
}