using UnityEngine;

public class RandomAudioStart : MonoBehaviour
{
    public AudioSource audioSource;

    public float timeMin;

    public float timeMax = 1f;

    private float time;

    private float timer;

    private bool started;

    private void Start()
    {
        time = Random.Range(timeMin, timeMax);
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!started)
        {
            if (timer >= time)
            {
                audioSource.Play();
                started = true;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }
}
