using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_Language_I : Language_I
    {
        public override string GetString(string sheetName, string convoName, string fallback = null)
        {
            return fallback;
        }

        public override string GetString(string convoName, string fallback = null)
        {
            return fallback;
        }

        public override bool HasString(string sheetName, string convoName)
        {
            return false;
        }

        public override bool HasString(string convoName)
        {
            return false;
        }
    }
}