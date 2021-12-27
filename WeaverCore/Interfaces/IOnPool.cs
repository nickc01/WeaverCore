namespace WeaverCore.Interfaces
{
	/// <summary>
	/// When added to a component, the <see cref="OnPool"/> function will be called when the object gets sent back to a pool
	/// </summary>
	public interface IOnPool
	{
		/// <summary>
		/// Called when the object is sent back to a pool
		/// </summary>
		void OnPool();
	}
}
