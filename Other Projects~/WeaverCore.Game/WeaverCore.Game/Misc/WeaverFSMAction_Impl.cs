/*using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections;
using WeaverCore.Playmaker;

public class WeaverFSMAction_Impl : FsmStateAction
{
    private WeaverFSMAction weaverAction;

    public WeaverFSMAction_Impl(WeaverFSMAction weaverAction)
    {
        this.weaverAction = weaverAction;
        // Set the FSMActionBase to this implementation
        weaverAction.FSMActionBase = this;
    }

    // FsmStateAction lifecycle methods
    public override void Init(FsmState state)
    {
        base.Init(state);
        weaverAction.Init(state);
    }

    public override void Reset()
    {
        base.Reset();
        weaverAction.Reset();
    }

    public override void OnPreprocess()
    {
        base.OnPreprocess();
        weaverAction.OnPreprocess();
    }

    public override void Awake()
    {
        base.Awake();
        weaverAction.Awake();
    }

    public override bool Event(FsmEvent fsmEvent)
    {
        bool baseResult = base.Event(fsmEvent);
        bool weaverResult = weaverAction.Event(fsmEvent);
        return baseResult || weaverResult;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        weaverAction.OnEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        weaverAction.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        weaverAction.OnUpdate();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        weaverAction.OnGUI();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        weaverAction.OnLateUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
        weaverAction.OnExit();
    }

    // Collision methods
    public override void DoCollisionEnter(Collision collisionInfo)
    {
        base.DoCollisionEnter(collisionInfo);
        weaverAction.DoCollisionEnter(collisionInfo);
    }

    public override void DoCollisionStay(Collision collisionInfo)
    {
        base.DoCollisionStay(collisionInfo);
        weaverAction.DoCollisionStay(collisionInfo);
    }

    public override void DoCollisionExit(Collision collisionInfo)
    {
        base.DoCollisionExit(collisionInfo);
        weaverAction.DoCollisionExit(collisionInfo);
    }

    public override void DoTriggerEnter(Collider other)
    {
        base.DoTriggerEnter(other);
        weaverAction.DoTriggerEnter(other);
    }

    public override void DoTriggerStay(Collider other)
    {
        base.DoTriggerStay(other);
        weaverAction.DoTriggerStay(other);
    }

    public override void DoTriggerExit(Collider other)
    {
        base.DoTriggerExit(other);
        weaverAction.DoTriggerExit(other);
    }

    public override void DoParticleCollision(GameObject other)
    {
        base.DoParticleCollision(other);
        weaverAction.DoParticleCollision(other);
    }

    public override void DoCollisionEnter2D(Collision2D collisionInfo)
    {
        base.DoCollisionEnter2D(collisionInfo);
        weaverAction.DoCollisionEnter2D(collisionInfo);
    }

    public override void DoCollisionStay2D(Collision2D collisionInfo)
    {
        base.DoCollisionStay2D(collisionInfo);
        weaverAction.DoCollisionStay2D(collisionInfo);
    }

    public override void DoCollisionExit2D(Collision2D collisionInfo)
    {
        base.DoCollisionExit2D(collisionInfo);
        weaverAction.DoCollisionExit2D(collisionInfo);
    }

    public override void DoTriggerEnter2D(Collider2D other)
    {
        base.DoTriggerEnter2D(other);
        weaverAction.DoTriggerEnter2D(other);
    }

    public override void DoTriggerStay2D(Collider2D other)
    {
        base.DoTriggerStay2D(other);
        weaverAction.DoTriggerStay2D(other);
    }

    public override void DoTriggerExit2D(Collider2D other)
    {
        base.DoTriggerExit2D(other);
        weaverAction.DoTriggerExit2D(other);
    }

    public override void DoControllerColliderHit(ControllerColliderHit collider)
    {
        base.DoControllerColliderHit(collider);
        weaverAction.DoControllerColliderHit(collider);
    }

    public override void DoJointBreak(float force)
    {
        base.DoJointBreak(force);
        weaverAction.DoJointBreak(force);
    }

    public override void DoJointBreak2D(Joint2D joint)
    {
        base.DoJointBreak2D(joint);
        weaverAction.DoJointBreak2D(joint);
    }

    public override void DoAnimatorMove()
    {
        base.DoAnimatorMove();
        weaverAction.DoAnimatorMove();
    }

    public override void DoAnimatorIK(int layerIndex)
    {
        base.DoAnimatorIK(layerIndex);
        weaverAction.DoAnimatorIK(layerIndex);
    }
}*/