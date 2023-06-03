using UnityEngine;

public class TriggerEnterEvent : MonoBehaviour
{
    public delegate void CollisionEvent(Collider2D collider, GameObject sender);

    public bool waitForHeroInPosition;

    private bool active;

    public event CollisionEvent OnTriggerEntered;

    public event CollisionEvent OnTriggerExited;

    public event CollisionEvent OnTriggerStayed;

    private void Start()
    {
        active = false;
        if (waitForHeroInPosition)
        {
            if (HeroController.instance.isHeroInPosition)
            {
                active = true;
                return;
            }
            HeroController.HeroInPosition temp = null;
            temp = delegate
            {
                active = true;
                HeroController.instance.heroInPosition -= temp;
            };
            HeroController.instance.heroInPosition += temp;
        }
        else
        {
            active = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (active && this.OnTriggerEntered != null)
        {
            this.OnTriggerEntered(collision, base.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (active && this.OnTriggerExited != null)
        {
            this.OnTriggerExited(collision, base.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (active && this.OnTriggerStayed != null)
        {
            this.OnTriggerStayed(collision, base.gameObject);
        }
    }
}
