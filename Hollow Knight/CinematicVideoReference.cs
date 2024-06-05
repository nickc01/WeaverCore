using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Hollow Knight/Cinematic Video Reference", fileName = "CinematicVideoReference", order = 1000)]
public class CinematicVideoReference : ScriptableObject
{
	[SerializeField]
	private string videoAssetPath;

	[SerializeField]
	private string audioAssetPath;

	[SerializeField]
	private VideoClip embeddedVideoClip;

	public string VideoFileName => base.name;

	public string VideoAssetPath => videoAssetPath;

	public string AudioAssetPath => audioAssetPath;

	public VideoClip EmbeddedVideoClip => embeddedVideoClip;
}
