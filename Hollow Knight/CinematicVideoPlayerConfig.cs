using UnityEngine;

public class CinematicVideoPlayerConfig
{
	private CinematicVideoReference videoReference;

	private MeshRenderer meshRenderer;

	private AudioSource audioSource;

	private CinematicVideoFaderStyles faderStyle;

	private float implicitVolume;

	public CinematicVideoReference VideoReference => videoReference;

	public MeshRenderer MeshRenderer => meshRenderer;

	public AudioSource AudioSource => audioSource;

	public CinematicVideoFaderStyles FaderStyle => faderStyle;

	public float ImplicitVolume => implicitVolume;

	public CinematicVideoPlayerConfig(CinematicVideoReference videoReference, MeshRenderer meshRenderer, AudioSource audioSource, CinematicVideoFaderStyles faderStyle, float implicitVolume)
	{
		this.videoReference = videoReference;
		this.meshRenderer = meshRenderer;
		this.audioSource = audioSource;
		this.faderStyle = faderStyle;
		this.implicitVolume = implicitVolume;
	}
}
