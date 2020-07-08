namespace WeaverCore.Internal
{
    /// <summary>
    /// The mod class for WeaverCore
    /// </summary>
    public sealed class WeaverCore : WeaverMod
    {

        public override void Initialize()
        {
            base.Initialize();
            InitRunner.RunInitFunctions();
        }

        public override string GetVersion()
        {
            return "0.0.1.1 Alpha";
        }
    }
}
