namespace WeaverCore
{
    public abstract class ExtraHitWrapper
	{
		public abstract object SourceObj { get; }
		public abstract void Hit(HitInfo hit);
	}
}