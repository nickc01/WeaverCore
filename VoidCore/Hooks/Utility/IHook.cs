namespace VoidCore.Hooks
{
    namespace Utility
    {
        public interface IHook
        {
            void LoadHook();
        }

        public interface IHook<Alloc> : IHook where Alloc : Allocator
        {

        }
    }
}
