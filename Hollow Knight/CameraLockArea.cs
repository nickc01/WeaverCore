using GlobalEnums;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraLockArea : MonoBehaviour
{
	private void Awake()
	{
		if (Application.isPlaying)
		{
            box2d = base.GetComponent<Collider2D>();
        }
	}

	private IEnumerator Start()
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
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
		/*if (!ValidateBounds())
		{
			Debug.LogError("Camera bounds are unspecified for " + name + ", please specify lock area bounds for this Camera Lock Area.");
		}*/
		if (box2d != null)
		{
			leftSideX = box2d.bounds.min.x;
			rightSideX = box2d.bounds.max.x;
			botSideY = box2d.bounds.min.y;
			topSideY = box2d.bounds.max.y;
		}
		yield break;
	}

    private bool IsInApplicableGameState()
	{
		GameManager unsafeInstance = GameManager.instance;
		return !(unsafeInstance == null) && (unsafeInstance.gameState == GameState.PLAYING || unsafeInstance.gameState == GameState.ENTERING_LEVEL);
	}

	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
        if (!Application.isPlaying)
        {
			return;
        }
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

	public void OnTriggerStay2D(Collider2D otherCollider)
	{
        if (!Application.isPlaying)
        {
            return;
        }
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

	public void OnTriggerExit2D(Collider2D otherCollider)
	{
        if (!Application.isPlaying)
        {
            return;
        }
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

	public void OnDisable()
	{
        if (!Application.isPlaying)
        {
            return;
        }
        if (cameraCtrl != null)
		{
			cameraCtrl.ReleaseLock(this);
		}
	}

	private bool ValidateBounds()
	{
		var cameraBounds = GameManager.instance.SceneDimensions;

		Debug.Log("Camera Bounds = " + cameraBounds);

		if (cameraXMin < cameraBounds.xMin)
		{
			cameraXMin = cameraBounds.xMin;
		}
		if (cameraXMax > cameraBounds.xMax)
		{
			cameraXMax = cameraBounds.xMax;
		}
		if (cameraYMin < cameraBounds.yMin)
		{
			cameraYMin = cameraBounds.yMin;
		}
		if (cameraYMax > cameraBounds.yMax)
		{
			cameraYMax = cameraBounds.yMax;
		}
		return cameraXMin != 0f || cameraXMax != 0f || cameraYMin != 0f || cameraYMax != 0f;
	}

	public void SetXMin(float xmin)
	{
		cameraXMin = xmin;
	}

	public void SetXMax(float xmax)
	{
		cameraXMax = xmax;
	}

	private IEnumerator orig_Start()
	{
		gcams = GameCameras.instance;
		cameraCtrl = gcams.cameraController;
		camTarget = gcams.cameraTarget;
		Scene scene = gameObject.scene;

		while (cameraCtrl.sceneWidth == 0f)
		{
			yield return null;
		}
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

	private readonly bool verboseMode;

	public float cameraXMin;

	public float cameraYMin;

	public float cameraXMax;

	public float cameraYMax;

	private float leftSideX;

	private float rightSideX;

	private float topSideY;

	private float botSideY;

	private Vector3 heroPos;

	private readonly bool enteredLeft;

	private readonly bool enteredRight;

	private readonly bool enteredTop;

	private readonly bool enteredBot;

	private readonly bool exitedLeft;

	private readonly bool exitedRight;

	private readonly bool exitedTop;

	private readonly bool exitedBot;

	public bool preventLookUp;

	public bool preventLookDown;

	public bool maxPriority;

	private GameCameras gcams;

	private CameraController cameraCtrl;

	private CameraTarget camTarget;

	[NonSerialized]
	private Collider2D box2d;
}

