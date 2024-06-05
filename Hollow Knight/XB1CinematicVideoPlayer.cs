using UnityEngine;
using UnityEngine.Video;

public class XB1CinematicVideoPlayer : CinematicVideoPlayer
{
	private VideoPlayer videoPlayer;

	private Texture originalMainTexture;

	private RenderTexture renderTexture;

	private const string TexturePropertyName = "_MainTex";

	private bool isPlayEnqueued;

	public override float Volume
	{
		get
		{
			if (base.Config.AudioSource != null)
			{
				return base.Config.AudioSource.volume;
			}
			return 1f;
		}
		set
		{
			if (base.Config.AudioSource != null)
			{
				base.Config.AudioSource.volume = value;
			}
		}
	}

	public override bool IsLoading => false;

	public override bool IsLooping
	{
		get
		{
			if (videoPlayer != null)
			{
				return videoPlayer.isLooping;
			}
			return false;
		}
		set
		{
			if (videoPlayer != null)
			{
				videoPlayer.isLooping = value;
			}
		}
	}

	public override bool IsPlaying
	{
		get
		{
			if (videoPlayer != null && videoPlayer.isPrepared)
			{
				return videoPlayer.isPlaying;
			}
			return isPlayEnqueued;
		}
	}

	public XB1CinematicVideoPlayer(CinematicVideoPlayerConfig config)
		: base(config)
	{
		originalMainTexture = config.MeshRenderer.material.GetTexture("_MainTex");
		renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
		Graphics.Blit((config.FaderStyle == CinematicVideoFaderStyles.White) ? Texture2D.whiteTexture : Texture2D.blackTexture, renderTexture);
		videoPlayer = config.MeshRenderer.gameObject.AddComponent<VideoPlayer>();
		videoPlayer.playOnAwake = false;
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer.SetTargetAudioSource(0, config.AudioSource);
		videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		videoPlayer.targetTexture = renderTexture;
		config.MeshRenderer.material.SetTexture("_MainTex", renderTexture);
		VideoClip embeddedVideoClip = config.VideoReference.EmbeddedVideoClip;
		videoPlayer.clip = embeddedVideoClip;
		videoPlayer.prepareCompleted += OnPrepareCompleted;
		videoPlayer.Prepare();
	}

	public override void Dispose()
	{
		base.Dispose();
		if (videoPlayer != null)
		{
			videoPlayer.Stop();
			Object.Destroy(videoPlayer);
			videoPlayer = null;
			MeshRenderer meshRenderer = base.Config.MeshRenderer;
			if (meshRenderer != null)
			{
				meshRenderer.material.SetTexture("_MainTex", originalMainTexture);
			}
		}
		if (renderTexture != null)
		{
			Object.Destroy(renderTexture);
			renderTexture = null;
		}
	}

	public override void Play()
	{
		if (videoPlayer != null && videoPlayer.isPrepared)
		{
			videoPlayer.Play();
		}
		isPlayEnqueued = true;
	}

	public override void Stop()
	{
		if (videoPlayer != null)
		{
			videoPlayer.Stop();
		}
		isPlayEnqueued = false;
	}

	private void OnPrepareCompleted(VideoPlayer source)
	{
		if (source == videoPlayer && videoPlayer != null && isPlayEnqueued)
		{
			videoPlayer.Play();
			isPlayEnqueued = false;
		}
	}
}
