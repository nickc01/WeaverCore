using System;
using UnityEngine;


[Serializable]
public struct AudioEvent
{
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
        if (!(Clip == null) && !(Volume < Mathf.Epsilon) && !(prefab == null))
        {
            AudioSource audioSource = GameObject.Instantiate(prefab, position, Quaternion.identity); //prefab.Spawn(position);
            audioSource.volume = Volume;
            audioSource.pitch = SelectPitch();
            audioSource.PlayOneShot(Clip);
        }
    }
}
