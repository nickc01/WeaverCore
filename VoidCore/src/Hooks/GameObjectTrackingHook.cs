using Modding;
using VoidCore.Hooks.Internal;
using VoidCore.Hooks.Utility;

namespace VoidCore.Hooks
{
    public abstract class GMTrackingHook<Mod> : IHook<Mod> where Mod : IMod
    {
        protected virtual void Initialize() { }
        protected virtual void Uninitialize() { }

        protected abstract void TrackingEnabled();
        protected abstract void TrackingDisabled();

        void TrackingEvent(bool enabled)
        {
            if (enabled)
            {
                TrackingEnabled();
            }
            else
            {
                TrackingDisabled();
            }
        }



        void IHookBase.LoadHook(IMod mod)
        {
            Initialize();
            if (Settings.GMTracking)
            {
                TrackingEnabled();
            }
            Events.GMTrackingEvent += TrackingEvent;
        }

        void IHookBase.UnloadHook(IMod mod)
        {
            Uninitialize();
            if (Settings.GMTracking)
            {
                TrackingDisabled();
            }
            Events.GMTrackingEvent -= TrackingEvent;
        }
    }
}
