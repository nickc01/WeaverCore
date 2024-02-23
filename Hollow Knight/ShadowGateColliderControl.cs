using UnityEngine;

public class ShadowGateColliderControl : MonoBehaviour
{
    public Collider2D disableCollider;

    private bool unlocked;

    private EventRegister eventRegister;

    private void Awake()
    {
        eventRegister = GetComponent<EventRegister>();
    }

    private void Start()
    {
        if ((bool)eventRegister)
        {
            eventRegister.OnReceivedEvent += Setup;
        }
        Setup();
    }

    private void OnDestroy()
    {
        if ((bool)eventRegister)
        {
            eventRegister.OnReceivedEvent -= Setup;
        }
    }

    private void Setup()
    {
        if (PlayerData.instance.GetBool("hasShadowDash"))
        {
            unlocked = true;
        }
    }

    private void FixedUpdate()
    {
        if (unlocked)
        {
            if (HeroController.instance.cState.dashing && disableCollider.enabled)
            {
                disableCollider.enabled = false;
            }
            if (!HeroController.instance.cState.dashing && !disableCollider.enabled)
            {
                disableCollider.enabled = true;
            }
        }
    }
}
