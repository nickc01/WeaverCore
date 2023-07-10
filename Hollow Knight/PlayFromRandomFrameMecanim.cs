using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayFromRandomFrameMecanim : MonoBehaviour
{
    private Animator animator;

    [Tooltip("The name of the Animator state to play randomly.")]
    public string stateToPlay;

    public bool onEnable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!onEnable)
        {
            DoPlay();
        }
    }

    private void OnEnable()
    {
        if (onEnable)
        {
            DoPlay();
        }
    }

    private void DoPlay()
    {
        if (!string.IsNullOrEmpty(stateToPlay))
        {
            StartCoroutine(DelayStart());
        }
        else
        {
            Debug.LogError("PlayFromRandomFrameMecanim: No state name specified to play." + base.gameObject.name);
        }
    }

    private IEnumerator DelayStart()
    {
        yield return null;
        animator.Play(stateToPlay, 0, Random.Range(0f, 1f));
    }
}
