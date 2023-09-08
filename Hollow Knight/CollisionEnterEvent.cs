using System.Collections.Generic;
using UnityEngine;

public class CollisionEnterEvent : MonoBehaviour
{
    public delegate void CollisionEvent(Collision2D collision, GameObject sender);

    public enum Direction
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }

    public delegate void DirectionalCollisionEvent(Direction direction, Collision2D collision);

    public bool checkDirection;

    public bool ignoreTriggers;

    public int otherLayer = 9;

    [HideInInspector]
    public bool doCollisionStay;

    private Collider2D col2d;

    private const float RAYCAST_LENGTH = 0.08f;

    private List<Vector2> topRays;

    private List<Vector2> rightRays;

    private List<Vector2> bottomRays;

    private List<Vector2> leftRays;

    public event CollisionEvent OnCollisionEntered;

    public event DirectionalCollisionEvent OnCollisionEnteredDirectional;

    private void Awake()
    {
        col2d = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (doCollisionStay)
        {
            HandleCollision(collision);
        }
    }

    private void HandleCollision(Collision2D collision)
    {
        if (this.OnCollisionEntered != null)
        {
            this.OnCollisionEntered(collision, base.gameObject);
        }
        if (checkDirection)
        {
            CheckTouching(otherLayer, collision);
        }
    }

    private void CheckTouching(LayerMask layer, Collision2D collision)
    {
        topRays = new List<Vector2>();
        topRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
        topRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.max.y));
        topRays.Add(col2d.bounds.max);
        rightRays = new List<Vector2>();
        rightRays.Add(col2d.bounds.max);
        rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.center.y));
        rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
        bottomRays = new List<Vector2>();
        bottomRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
        bottomRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.min.y));
        bottomRays.Add(col2d.bounds.min);
        leftRays = new List<Vector2>();
        leftRays.Add(col2d.bounds.min);
        leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.center.y));
        leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
        foreach (Vector2 topRay in topRays)
        {
            RaycastHit2D raycastHit2D = Physics2D.Raycast((Vector3)topRay, Vector2.up, 0.08f, 1 << (int)layer);
            if (raycastHit2D.collider != null && (!ignoreTriggers || !raycastHit2D.collider.isTrigger))
            {
                if (this.OnCollisionEnteredDirectional != null)
                {
                    this.OnCollisionEnteredDirectional(Direction.Top, collision);
                }
                break;
            }
        }
        foreach (Vector2 rightRay in rightRays)
        {
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast((Vector3)rightRay, Vector2.right, 0.08f, 1 << (int)layer);
            if (raycastHit2D2.collider != null && (!ignoreTriggers || !raycastHit2D2.collider.isTrigger))
            {
                if (this.OnCollisionEnteredDirectional != null)
                {
                    this.OnCollisionEnteredDirectional(Direction.Right, collision);
                }
                break;
            }
        }
        foreach (Vector2 bottomRay in bottomRays)
        {
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast((Vector3)bottomRay, -Vector2.up, 0.08f, 1 << (int)layer);
            if (raycastHit2D3.collider != null && (!ignoreTriggers || !raycastHit2D3.collider.isTrigger))
            {
                if (this.OnCollisionEnteredDirectional != null)
                {
                    this.OnCollisionEnteredDirectional(Direction.Bottom, collision);
                }
                break;
            }
        }
        foreach (Vector2 leftRay in leftRays)
        {
            RaycastHit2D raycastHit2D4 = Physics2D.Raycast((Vector3)leftRay, -Vector2.right, 0.08f, 1 << (int)layer);
            if (raycastHit2D4.collider != null && (!ignoreTriggers || !raycastHit2D4.collider.isTrigger))
            {
                if (this.OnCollisionEnteredDirectional != null)
                {
                    this.OnCollisionEnteredDirectional(Direction.Left, collision);
                }
                break;
            }
        }
    }
}
