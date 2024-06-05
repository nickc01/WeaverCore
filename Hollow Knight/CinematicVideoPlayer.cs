using System;

public abstract class CinematicVideoPlayer : IDisposable
{
	private CinematicVideoPlayerConfig config;

	protected CinematicVideoPlayerConfig Config => config;

	public abstract bool IsLoading { get; }

	public abstract bool IsPlaying { get; }

	public abstract bool IsLooping { get; set; }

	public abstract float Volume { get; set; }

	public virtual float CurrentTime => 0f;

	public CinematicVideoPlayer(CinematicVideoPlayerConfig config)
	{
		this.config = config;
	}

	public virtual void Dispose()
	{
	}

	public abstract void Play();

	public abstract void Stop();

	public virtual void Update()
	{
	}

	public static CinematicVideoPlayer Create(CinematicVideoPlayerConfig config)
	{
		return new XB1CinematicVideoPlayer(config);
	}
}
