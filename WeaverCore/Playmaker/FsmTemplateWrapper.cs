using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
	/// <summary>
	/// Type-safe wrapper for PlayMaker FSM templates.
	/// </summary>
	public struct FsmTemplateWrapper
	{
		public object InternalTemplate { get; }

		public FsmTemplateWrapper(object fsmTemplate)
		{
			InternalTemplate = fsmTemplate;
		}

		public FsmWrapper GetFsm()
		{
			return PlayMakerUtilities.GetTemplateFsmWrapper(InternalTemplate);
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmTemplateWrapper template)
			{
				return template.InternalTemplate == InternalTemplate;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return InternalTemplate != null ? InternalTemplate.GetHashCode() : 0;
		}

		public static bool operator ==(FsmTemplateWrapper a, FsmTemplateWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(FsmTemplateWrapper a, FsmTemplateWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
