using System.Collections;
using GlobalEnums;
using UnityEngine;

public class HeroAudioController : MonoBehaviour
{
    private HeroController heroCtrl;

    [Header("Sound Effects")]
    public AudioSource softLanding;

    public AudioSource hardLanding;

    public AudioSource jump;

    public AudioSource takeHit;

    public AudioSource backDash;

    public AudioSource dash;

    public AudioSource footStepsRun;

    public AudioSource footStepsWalk;

    public AudioSource wallslide;

    public AudioSource nailArtCharge;

    public AudioSource nailArtReady;

    public AudioSource falling;

    public AudioSource walljump;

    private Coroutine fallingCo;

    private void Awake()
    {
        heroCtrl = GetComponent<HeroController>();

        if (footStepsRun == null)
        {
            var runTransform = transform.Find("Sounds/FootstepsRun");
            if (runTransform != null)
            {
                footStepsRun = runTransform.GetComponent<AudioSource>();
            }
        }

        if (footStepsWalk == null)
        {
            var walkTransform = transform.Find("Sounds/FootstepsWalk");
            if (walkTransform != null)
            {
                footStepsWalk = walkTransform.GetComponent<AudioSource>();
            }
        }
    }

    public void PlaySound(HeroSounds soundEffect)
    {
        if (heroCtrl.cState.isPaused)
        {
            return;
        }
        switch (soundEffect)
        {
            case HeroSounds.SOFT_LANDING:
                RandomizePitch(jump, 0.9f, 1.1f);
                Play(softLanding);
                break;
            case HeroSounds.HARD_LANDING:
                Play(hardLanding);
                break;
            case HeroSounds.JUMP:
                RandomizePitch(jump, 0.9f, 1.1f);
                Play(jump);
                break;
            case HeroSounds.WALLJUMP:
                RandomizePitch(walljump, 0.9f, 1.1f);
                Play(walljump);
                break;
            case HeroSounds.TAKE_HIT:
                Play(takeHit);
                break;
            case HeroSounds.DASH:
                Play(dash);
                break;
            case HeroSounds.WALLSLIDE:
                Play(wallslide);
                break;
            case HeroSounds.BACKDASH:
                Play(backDash);
                break;
            case HeroSounds.FOOTSTEPS_RUN:
                if (!IsPlaying(footStepsRun) && !IsPlaying(softLanding))
                {
                    Play(footStepsRun);
                }
                break;
            case HeroSounds.FOOTSTEPS_WALK:
                if (!IsPlaying(footStepsWalk) && !IsPlaying(softLanding))
                {
                    Play(footStepsWalk);
                }
                break;
            case HeroSounds.NAIL_ART_CHARGE:
                Play(nailArtCharge);
                break;
            case HeroSounds.NAIL_ART_READY:
                Play(nailArtReady);
                break;
            case HeroSounds.FALLING:
                fallingCo = StartCoroutine(FadeInVolume(falling, 0.7f));
                Play(falling);
                break;
        }
    }

    public void StopSound(HeroSounds soundEffect)
    {
        switch (soundEffect)
        {
            case HeroSounds.FOOTSTEPS_RUN:
                Stop(footStepsRun);
                break;
            case HeroSounds.FOOTSTEPS_WALK:
                Stop(footStepsWalk);
                break;
            case HeroSounds.WALLSLIDE:
                Stop(wallslide);
                break;
            case HeroSounds.NAIL_ART_CHARGE:
                Stop(nailArtCharge);
                break;
            case HeroSounds.NAIL_ART_READY:
                Stop(nailArtReady);
                break;
            case HeroSounds.FALLING:
                Stop(falling);
                if (fallingCo != null)
                {
                    StopCoroutine(fallingCo);
                }
                break;
        }
    }

    public void StopAllSounds()
    {
        Stop(softLanding);
        Stop(hardLanding);
        Stop(jump);
        Stop(takeHit);
        Stop(backDash);
        Stop(dash);
        Stop(footStepsRun);
        Stop(footStepsWalk);
        Stop(wallslide);
        Stop(nailArtCharge);
        Stop(nailArtReady);
        Stop(falling);
    }

    public void PauseAllSounds()
    {
        Pause(softLanding);
        Pause(hardLanding);
        Pause(jump);
        Pause(takeHit);
        Pause(backDash);
        Pause(dash);
        Pause(footStepsRun);
        Pause(footStepsWalk);
        Pause(wallslide);
        Pause(nailArtCharge);
        Pause(nailArtReady);
        Pause(falling);
    }

    public void UnPauseAllSounds()
    {
        UnPause(softLanding);
        UnPause(hardLanding);
        UnPause(jump);
        UnPause(takeHit);
        UnPause(backDash);
        UnPause(dash);
        UnPause(footStepsRun);
        UnPause(footStepsWalk);
        UnPause(wallslide);
        UnPause(nailArtCharge);
        UnPause(nailArtReady);
        UnPause(falling);
    }

    private void RandomizePitch(AudioSource src, float minPitch, float maxPitch)
    {
        if (src == null)
        {
            return;
        }
        float pitch = Random.Range(minPitch, maxPitch);
        src.pitch = pitch;
    }

    private void ResetPitch(AudioSource src)
    {
        if (src == null)
        {
            return;
        }
        src.pitch = 1f;
    }

    private IEnumerator FadeInVolume(AudioSource src, float duration)
    {
        if (src == null)
        {
            yield break;
        }
        float elapsedTime = 0f;
        src.volume = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            src.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
    }

    private static bool IsPlaying(AudioSource src) => src != null && src.isPlaying;

    private static void Play(AudioSource src)
    {
        if (src != null)
        {
            src.Play();
        }
    }

    private static void Stop(AudioSource src)
    {
        if (src != null)
        {
            src.Stop();
        }
    }

    private static void Pause(AudioSource src)
    {
        if (src != null)
        {
            src.Pause();
        }
    }

    private static void UnPause(AudioSource src)
    {
        if (src != null)
        {
            src.UnPause();
        }
    }
}
