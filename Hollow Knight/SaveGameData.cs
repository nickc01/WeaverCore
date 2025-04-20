using System;

[Serializable]
public class SaveGameData
{
	public PlayerData playerData;

	public SceneData sceneData;

	public SaveGameData(PlayerData playerData, SceneData sceneData)
	{
		this.playerData = playerData;
		this.sceneData = sceneData;
	}
}
