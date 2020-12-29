using System;
using System.Diagnostics;
using UnityEngine;

// Token: 0x02000840 RID: 2112

#if !UNITY_EDITOR
public class Recoil : MonoBehaviour
{
	// Token: 0x1700027F RID: 639
	// (get) Token: 0x060026AB RID: 9899 RVA: 0x000DC440 File Offset: 0x000DA640
	// (set) Token: 0x060026AC RID: 9900 RVA: 0x000DC448 File Offset: 0x000DA648
	public bool SkipFreezingByController
	{
		get
		{
			return this.skipFreezingByController;
		}
		set
		{
			this.skipFreezingByController = value;
		}
	}
	public event Recoil.FreezeEvent OnHandleFreeze;

	public event Recoil.CancelRecoilEvent OnCancelRecoil;

	public bool IsRecoiling
	{
		get
		{
			return this.state == Recoil.States.Recoiling || this.state == Recoil.States.Frozen;
		}
	}

	// Token: 0x060026B2 RID: 9906 RVA: 0x000DC53C File Offset: 0x000DA73C
	protected void Reset()
	{
		this.freezeInPlace = false;
		this.stopVelocityXWhenRecoilingUp = true;
		this.recoilDuration = 0.5f;
		this.recoilSpeedBase = 15f;
		this.preventRecoilUp = false;
	}

	// Token: 0x060026B3 RID: 9907 RVA: 0x000DC56C File Offset: 0x000DA76C
	protected void Awake()
	{
		this.body = base.GetComponent<Rigidbody2D>();
		this.bodyCollider = base.GetComponent<Collider2D>();
	}

	// Token: 0x060026B4 RID: 9908 RVA: 0x000DC588 File Offset: 0x000DA788
	private void OnEnable()
	{
		this.CancelRecoil();
	}

	// Token: 0x060026B5 RID: 9909 RVA: 0x000DC590 File Offset: 0x000DA790
	public void RecoilByHealthManagerFSMParameters()
	{

	}

	// Token: 0x060026B7 RID: 9911 RVA: 0x000DC628 File Offset: 0x000DA828
	public void RecoilByDirection(int attackDirection, float attackMagnitude)
	{

	}

	// Token: 0x060026B8 RID: 9912 RVA: 0x000DC748 File Offset: 0x000DA948
	public void CancelRecoil()
	{

	}

	// Token: 0x060026BA RID: 9914 RVA: 0x000DC800 File Offset: 0x000DAA00
	protected void FixedUpdate()
	{
		
	}

	// Token: 0x060026BC RID: 9916 RVA: 0x000DC8FC File Offset: 0x000DAAFC
	public void SetRecoilSpeed(float newSpeed)
	{

	}

	// Token: 0x04002A65 RID: 10853
	private Rigidbody2D body;

	// Token: 0x04002A66 RID: 10854
	private Collider2D bodyCollider;

	// Token: 0x04002A67 RID: 10855
	[SerializeField]
	public bool freezeInPlace;

	// Token: 0x04002A68 RID: 10856
	[SerializeField]
	private bool stopVelocityXWhenRecoilingUp;

	// Token: 0x04002A69 RID: 10857
	[SerializeField]
	private bool preventRecoilUp;

	// Token: 0x04002A6A RID: 10858
	[SerializeField]
	private float recoilSpeedBase;

	// Token: 0x04002A6B RID: 10859
	[SerializeField]
	private float recoilDuration;

	// Token: 0x04002A6C RID: 10860
	private bool skipFreezingByController;

	// Token: 0x04002A6F RID: 10863
	private Recoil.States state;

	// Token: 0x04002A70 RID: 10864
	private float recoilTimeRemaining;

	// Token: 0x04002A71 RID: 10865
	private float recoilSpeed;

	// Token: 0x04002A73 RID: 10867
	private bool isRecoilSweeping;

	// Token: 0x04002A74 RID: 10868
	private const int SweepLayerMask = 256;

	// Token: 0x02000841 RID: 2113
	// (Invoke) Token: 0x060026BE RID: 9918
	public delegate void FreezeEvent();

	// Token: 0x02000842 RID: 2114
	// (Invoke) Token: 0x060026C2 RID: 9922
	public delegate void CancelRecoilEvent();

	// Token: 0x02000843 RID: 2115
	private enum States
	{
		// Token: 0x04002A76 RID: 10870
		Ready,
		// Token: 0x04002A77 RID: 10871
		Frozen,
		// Token: 0x04002A78 RID: 10872
		Recoiling
	}
}

#endif
