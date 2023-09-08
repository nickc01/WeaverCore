using System;
using System.Reflection;
using UnityEngine;


[Serializable]
public struct AudioEvent
{
    static MethodInfo playAtPointMethod;
    static PropertyInfo audioSourceProperty;

    public AudioClip Clip;

    public float PitchMin;

    public float PitchMax;

    public float Volume;

    public void Reset()
    {
        PitchMin = 0.75f;
        PitchMax = 1.25f;
        Volume = 1f;
    }

    public float SelectPitch()
    {
        if (Mathf.Approximately(PitchMin, PitchMax))
        {
            return PitchMax;
        }
        return UnityEngine.Random.Range(PitchMin, PitchMax);
    }

    public void SpawnAndPlayOneShot(AudioSource prefab, Vector3 position)
    {
        if (!(Clip == null) && !(Volume < Mathf.Epsilon))
        {
#if UNITY_EDITOR
            if (playAtPointMethod == null)
            {
                playAtPointMethod = WeaverTypeHelpers.GetWeaverMethod("WeaverCore.WeaverAudio", "PlayAtPoint");

            }

            Debug.Log("SOUND = " + Clip.name);

            var instance = playAtPointMethod.Invoke(null, new object[] {Clip, position, Volume, 2});

            if (audioSourceProperty == null)
            {
                audioSourceProperty = instance.GetType().GetProperty("AudioSource");
            }

            var audioSource = (AudioSource)audioSourceProperty.GetValue(instance);

            audioSource.pitch = SelectPitch();
#else
            if (prefab != null)
            {
                AudioSource audioSource = GameObject.Instantiate(prefab, position, Quaternion.identity);
                audioSource.volume = Volume;
                audioSource.pitch = SelectPitch();
                audioSource.PlayOneShot(Clip);
            }
#endif
        }
    }
}
