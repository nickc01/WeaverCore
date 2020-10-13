namespace WeaverCore.Enums
{
	/// <summary>
	/// Determines how the pool is loaded
	/// </summary>
	public enum PoolLoadType
	{
		/// <summary>
		/// This makes it so that the pool is only available in the active scene. The pool resets when a new scene is loaded
		/// </summary>
		Local,
		/// <summary>
		/// This makes the it so that the pool is not bound to a specific scene, allowing it to be used anywhere
		/// </summary>
		Global
	}
}
