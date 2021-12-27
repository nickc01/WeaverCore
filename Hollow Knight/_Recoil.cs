using UnityEngine;

#if UNITY_EDITOR
[System.ComponentModel.Browsable(false)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
public class Recoil : MonoBehaviour
{
    private Rigidbody2D body;
    private Collider2D bodyCollider;

    [SerializeField]
    public bool freezeInPlace;
    [SerializeField]
    private bool stopVelocityXWhenRecoilingUp;
    [SerializeField]
    private bool preventRecoilUp;
    [SerializeField]
    private float recoilSpeedBase = 15f;
    [SerializeField]
    private float recoilDuration = 0.15f;
    private bool skipFreezingByController;
    private States state;
    private float recoilTimeRemaining;
    private float recoilSpeed;

    public event FreezeEvent OnHandleFreeze;
    public event CancelRecoilEvent OnCancelRecoil;

    private const int SweepLayerMask = 256;
    private Vector2 recoilDirection;

    public bool SkipFreezingByController
    {
        get => skipFreezingByController;
        set => skipFreezingByController = value;
    }

    public bool IsRecoiling => state == States.Recoiling || state == States.Frozen;

    private static Vector2 DirectionToVector(int direction)
    {
        switch (direction)
        {
            case 1:
                return new Vector2(0f, 1f);
            case 2:
                return new Vector2(-1f, 0f);
            case 3:
                return new Vector2(0f, -1f);
            default:
                return new Vector2(1f, 0f);
        }
    }

    protected void Reset()
    {
        freezeInPlace = false;
        stopVelocityXWhenRecoilingUp = false;
        recoilDuration = 0.15f;
        recoilSpeedBase = 15f;
        preventRecoilUp = false;
    }

    protected void Awake()
    {
        body = base.GetComponent<Rigidbody2D>();
        bodyCollider = base.GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        CancelRecoil();
    }

    public void RecoilByDirection(int attackDirection, float attackMagnitude)
    {
        if (state != States.Ready)
        {
            return;
        }
        if (freezeInPlace)
        {
            Freeze();
            return;
        }
        if (attackDirection == 1 && preventRecoilUp)
        {
            return;
        }
        if (bodyCollider == null)
        {
            bodyCollider = base.GetComponent<Collider2D>();
        }
        state = States.Recoiling;
        recoilSpeed = recoilSpeedBase * attackMagnitude;
        recoilTimeRemaining = recoilDuration;
        recoilDirection = DirectionToVector(attackDirection);
        UpdatePhysics(0f);
    }

    public void CancelRecoil()
    {
        if (state != States.Ready)
        {
            state = States.Ready;
            if (OnCancelRecoil != null)
            {
                OnCancelRecoil();
            }
        }
    }

    private void Freeze()
    {
        if (skipFreezingByController)
        {
            if (OnHandleFreeze != null)
            {
                OnHandleFreeze();
            }
            state = States.Ready;
            return;
        }
        state = States.Frozen;
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }
        recoilTimeRemaining = recoilDuration;
        UpdatePhysics(0f);
    }

    protected void FixedUpdate()
    {
        UpdatePhysics(Time.fixedDeltaTime);
    }

    private void UpdatePhysics(float deltaTime)
    {
        if (state == States.Frozen)
        {
            if (body != null)
            {
                body.velocity = Vector2.zero;
            }
            recoilTimeRemaining -= deltaTime;
            if (recoilTimeRemaining <= 0f)
            {
                CancelRecoil();
            }
        }
        else if (state == States.Recoiling)
        {
            transform.Translate(recoilDirection * recoilSpeed * Time.deltaTime, Space.World);
            recoilTimeRemaining -= deltaTime;
            if (recoilTimeRemaining <= 0f)
            {
                CancelRecoil();
            }
        }
    }

    public void SetRecoilSpeed(float newSpeed)
    {
        recoilSpeedBase = newSpeed;
    }

    public delegate void FreezeEvent();

    public delegate void CancelRecoilEvent();

    private enum States
    {
        Ready,
        Frozen,
        Recoiling
    }
}
