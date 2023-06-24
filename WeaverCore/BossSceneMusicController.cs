using System.Collections;
using UnityEngine;

namespace WeaverCore.Assets.Components
{

    public class BossSceneMusicController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The delay before the music starts")]
        float musicStartDelay = 2.5f;

        [SerializeField]
        [Tooltip("The music to be played when the boss starts")]
        MusicPack bossMusicPack;

        [SerializeField]
        [Tooltip("A special music pack to be played when on attuned or radiant difficulty. Leave empty to use the regular boss music")]
        MusicPack remixBossMusicPack;

        [Header("Pantheon Only")]
        [SerializeField]
        [Tooltip("If set to true, will disable playing the music when playing in a pantheon. Note: Any music that is already being played will not be affected")]
        bool disableSequenceMusic = false;

        [SerializeField]
        [Tooltip("If set to true, will wait for the \"GG MUSIC\" event to be fired before playing the pantheon sequence music")]
        bool waitForMusicEvent = false;

        [SerializeField]
        [Tooltip("The music pack to be played when in a pantheon. Only used if disableSequenceMusic is false")]
        MusicPack pantheonSequenceMusic;

        [SerializeField]
        AudioClip transitionSound;

        EventListener listener;

        bool ggMusicFired = false;

        private void Awake()
        {
            StartCoroutine(MusicRoutine());
            if (waitForMusicEvent)
            {
                listener = GetComponent<EventListener>();
                if (listener == null)
                {
                    listener = gameObject.AddComponent<EventListener>();
                    listener.ListenForEvent("GG MUSIC", (source, dest) =>
                    {
                        ggMusicFired = true;
                    });
                }
            }
        }

        IEnumerator MusicRoutine()
        {
            if (BossSequenceController.IsInSequence)
            {
                if (!disableSequenceMusic)
                {
                    if (waitForMusicEvent)
                    {
                        yield return new WaitUntil(() => ggMusicFired);
                    }
                    yield return new WaitForSeconds(musicStartDelay);

                    Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 0.5f);

                    if (transitionSound != null)
                    {
                        WeaverAudio.PlayAtPoint(transitionSound, Player.Player1.transform.position);
                    }
                    yield return new WaitForSeconds(0.5f);

                    Music.PlayMusicPack(pantheonSequenceMusic, 0.1f, 0.1f);

                    yield return new WaitForSeconds(0.1f);

                    yield return null;

                    Music.ApplyMusicSnapshot(Music.SnapshotType.Normal, 0f, 0.1f);
                }
            }
            else
            {
                yield return new WaitForSeconds(musicStartDelay);

                if (BossSceneController.Instance != null && BossSceneController.Instance.BossLevel >= 2 && remixBossMusicPack != null)
                {
                    Music.PlayMusicPack(remixBossMusicPack);
                }
                else
                {
                    Music.PlayMusicPack(bossMusicPack);
                }
                Music.ApplyMusicSnapshot(Music.SnapshotType.Normal, 0f, 0f);
            }
        }
    }
}
