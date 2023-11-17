using UnityEngine;

public class WalkArea : MonoBehaviour
{
    private Collider2D myCollider;

    private GameManager gm;

    private HeroController heroCtrl;

    private bool activated;

    private bool verboseMode;

    protected void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        gm = GameManager.instance;
        gm.UnloadingLevel += Deactivate;
        heroCtrl = HeroController.instance;
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.layer == 9)
        {
            if (verboseMode)
            {
                Debug.Log("ENTER Activated Walk Zone");
            }
            activated = true;
            heroCtrl.SetWalkZone(inWalkZone: true);
        }
    }

    private void OnTriggerStay2D(Collider2D otherCollider)
    {
        if (!activated && myCollider.enabled && otherCollider.gameObject.layer == 9)
        {
            if (verboseMode)
            {
                Debug.Log("STAY Activated Walk Zone");
            }
            activated = true;
            heroCtrl.SetWalkZone(inWalkZone: true);
        }
    }

    private void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.layer == 9)
        {
            if (verboseMode)
            {
                Debug.Log("EXIT Deactivated Walk Zone");
            }
            activated = false;
            heroCtrl.SetWalkZone(inWalkZone: false);
        }
    }

    private void Deactivate()
    {
        if (verboseMode)
        {
            Debug.Log("UNLOAD Deactivated Walk Zone");
        }
        activated = false;
        heroCtrl.SetWalkZone(inWalkZone: false);
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.UnloadingLevel -= Deactivate;
        }
    }
}
