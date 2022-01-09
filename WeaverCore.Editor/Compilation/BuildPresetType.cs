namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Determines the platform an assembly is getting built for. Used to determine what assembly references should be included or not
	/// </summary>
	public enum BuildPresetType
	{
		None,
		Game,
		Editor
	}
}
