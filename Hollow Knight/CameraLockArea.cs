using GlobalEnums;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000DE RID: 222
public class CameraLockArea : MonoBehaviour
{
	// Token: 0x06000496 RID: 1174 RVA: 0x00016B90 File Offset: 0x00014D90
	private void Awake()
	{
		box2d = base.GetComponent<Collider2D>();
	}

	// Token: 0x06000497 RID: 1175 RVA: 0x00016B9E File Offset: 0x00014D9E
	private IEnumerator Start()
	{
		gcams = GameCameras.instance;
		if (gcams == null)
		{
			yield break;
		}
		cameraCtrl = gcams.cameraController;
		camTarget = gcams.cameraTarget;
		Scene scene = gameObject.scene;
		if (cameraCtrl == null)
		{
			yield break;
		}
		/*while (cameraCtrl.tilemap == null || cameraCtrl.tilemap.gameObject.scene != scene)
		{
			yield return null;
		}*/
		if (!ValidateBounds())
		{
			Debug.LogError("Camera bounds are unspecified for " + name + ", please specify lock area bounds for this Camera Lock Area.");
		}
		if (box2d != null)
		{
			leftSideX = box2d.bounds.min.x;
			rightSideX = box2d.bounds.max.x;
			botSideY = box2d.bounds.min.y;
			topSideY = box2d.bounds.max.y;
		}
		yield break;
	}

	// Token: 0x06000498 RID: 1176 RVA: 0x00016BB0 File Offset: 0x00014DB0
	private bool IsInApplicableGameState()
	{
		GameManager unsafeInstance = GameManager.instance;
		return !(unsafeInstance == null) && (unsafeInstance.gameState == GameState.PLAYING || unsafeInstance.gameState == GameState.ENTERING_LEVEL);
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x00016BE4 File Offset: 0x00014DE4
	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		if (IsInApplicableGameState() && otherCollider.CompareTag("Player"))
		{
			heroPos = otherCollider.gameObject.transform.position;
			if (box2d != null)
			{
				if (heroPos.x > leftSideX - 1f && heroPos.x < leftSideX + 1f)
				{
					camTarget.enteredLeft = true;
				}
				else
				{
					camTarget.enteredLeft = false;
				}
				if (heroPos.x > rightSideX - 1f && heroPos.x < rightSideX + 1f)
				{
					camTarget.enteredRight = true;
				}
				else
				{
					camTarget.enteredRight = false;
				}
				if (heroPos.y > topSideY - 2f && heroPos.y < topSideY + 2f)
				{
					camTarget.enteredTop = true;
				}
				else
				{
					camTarget.enteredTop = false;
				}
				if (heroPos.y > botSideY - 1f && heroPos.y < botSideY + 1f)
				{
					camTarget.enteredBot = true;
				}
				else
				{
					camTarget.enteredBot = false;
				}
			}
			cameraCtrl.LockToArea(this);
			if (verboseMode)
			{
				Debug.Log("Lockzone Enter Lock " + base.name);
			}
		}
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x00016D8C File Offset: 0x00014F8C
	public void OnTriggerStay2D(Collider2D otherCollider)
	{
		if (!base.isActiveAndEnabled || !box2d.isActiveAndEnabled)
		{
			Debug.LogWarning("Fix for Unity trigger event queue!");
			return;
		}
		if (IsInApplicableGameState() && otherCollider.CompareTag("Player"))
		{
			if (verboseMode)
			{
				Debug.Log("Lockzone Stay Lock " + base.name);
			}
			cameraCtrl.LockToArea(this);
		}
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x00016DF8 File Offset: 0x00014FF8
	public void OnTriggerExit2D(Collider2D otherCollider)
	{
		if (otherCollider.CompareTag("Player"))
		{
			heroPos = otherCollider.gameObject.transform.position;
			if (box2d != null)
			{
				if (heroPos.x > leftSideX - 1f && heroPos.x < leftSideX + 1f)
				{
					camTarget.exitedLeft = true;
				}
				else
				{
					camTarget.exitedLeft = false;
				}
				if (heroPos.x > rightSideX - 1f && heroPos.x < rightSideX + 1f)
				{
					camTarget.exitedRight = true;
				}
				else
				{
					camTarget.exitedRight = false;
				}
				if (heroPos.y > topSideY - 2f && heroPos.y < topSideY + 2f)
				{
					camTarget.exitedTop = true;
				}
				else
				{
					camTarget.exitedTop = false;
				}
				if (heroPos.y > botSideY - 1f && heroPos.y < botSideY + 1f)
				{
					camTarget.exitedBot = true;
				}
				else
				{
					camTarget.exitedBot = false;
				}
			}
			cameraCtrl.ReleaseLock(this);
			if (verboseMode)
			{
				Debug.Log("Lockzone Exit Lock " + base.name);
			}
		}
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x00016F95 File Offset: 0x00015195
	public void OnDisable()
	{
		if (cameraCtrl != null)
		{
			cameraCtrl.ReleaseLock(this);
		}
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x00016FB4 File Offset: 0x000151B4
	private bool ValidateBounds()
	{
		if (cameraXMin == -1f)
		{
			cameraXMin = 14.6f;
		}
		if (cameraXMax == -1f)
		{
			cameraXMax = cameraCtrl.xLimit;
		}
		if (cameraYMin == -1f)
		{
			cameraYMin = 8.3f;
		}
		if (cameraYMax == -1f)
		{
			cameraYMax = cameraCtrl.yLimit;
		}
		return cameraXMin != 0f || cameraXMax != 0f || cameraYMin != 0f || cameraYMax != 0f;
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x00017064 File Offset: 0x00015264
	public void SetXMin(float xmin)
	{
		cameraXMin = xmin;
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x0001706D File Offset: 0x0001526D
	public void SetXMax(float xmax)
	{
		cameraXMax = xmax;
	}

	// Token: 0x060004A1 RID: 1185 RVA: 0x00017076 File Offset: 0x00015276
	private IEnumerator orig_Start()
	{
		gcams = GameCameras.instance;
		cameraCtrl = gcams.cameraController;
		camTarget = gcams.cameraTarget;
		Scene scene = gameObject.scene;
		/*while (cameraCtrl.tilemap == null || cameraCtrl.tilemap.gameObject.scene != scene)
		{
			yield return null;
		}*/
		if (!ValidateBounds())
		{
			Debug.LogError("Camera bounds are unspecified for " + name + ", please specify lock area bounds for this Camera Lock Area.");
		}
		if (box2d != null)
		{
			leftSideX = box2d.bounds.min.x;
			rightSideX = box2d.bounds.max.x;
			botSideY = box2d.bounds.min.y;
			topSideY = box2d.bounds.max.y;
		}
		yield break;
	}

	// Token: 0x04000434 RID: 1076
	private readonly bool verboseMode;

	// Token: 0x04000435 RID: 1077
	public float cameraXMin;

	// Token: 0x04000436 RID: 1078
	public float cameraYMin;

	// Token: 0x04000437 RID: 1079
	public float cameraXMax;

	// Token: 0x04000438 RID: 1080
	public float cameraYMax;

	// Token: 0x04000439 RID: 1081
	private float leftSideX;

	// Token: 0x0400043A RID: 1082
	private float rightSideX;

	// Token: 0x0400043B RID: 1083
	private float topSideY;

	// Token: 0x0400043C RID: 1084
	private float botSideY;

	// Token: 0x0400043D RID: 1085
	private Vector3 heroPos;

	// Token: 0x0400043E RID: 1086
	private readonly bool enteredLeft;

	// Token: 0x0400043F RID: 1087
	private readonly bool enteredRight;

	// Token: 0x04000440 RID: 1088
	private readonly bool enteredTop;

	// Token: 0x04000441 RID: 1089
	private readonly bool enteredBot;

	// Token: 0x04000442 RID: 1090
	private readonly bool exitedLeft;

	// Token: 0x04000443 RID: 1091
	private readonly bool exitedRight;

	// Token: 0x04000444 RID: 1092
	private readonly bool exitedTop;

	// Token: 0x04000445 RID: 1093
	private readonly bool exitedBot;

	// Token: 0x04000446 RID: 1094
	public bool preventLookUp;

	// Token: 0x04000447 RID: 1095
	public bool preventLookDown;

	// Token: 0x04000448 RID: 1096
	public bool maxPriority;

	// Token: 0x04000449 RID: 1097
	private GameCameras gcams;

	// Token: 0x0400044A RID: 1098
	private CameraController cameraCtrl;

	// Token: 0x0400044B RID: 1099
	private CameraTarget camTarget;

	// Token: 0x0400044C RID: 1100
	private Collider2D box2d;
}

