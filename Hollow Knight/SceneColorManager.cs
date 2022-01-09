using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(ColorCorrectionCurves))]
public class SceneColorManager : MonoBehaviour
{
	public float Factor;

	public float SaturationA = 1f;

	public AnimationCurve RedA = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve GreenA = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve BlueA = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public Color AmbientColorA = Color.white;

	public float AmbientIntensityA = 1f;

	public Color HeroLightColorA = Color.white;

	public float SaturationB = 1f;

	public AnimationCurve RedB = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve GreenB = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve BlueB = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public Color AmbientColorB = Color.white;

	public float AmbientIntensityB = 1f;

	public Color HeroLightColorB = Color.white;

	private List<Keyframe[]> RedPairedKeyframes;

	private List<Keyframe[]> GreenPairedKeyframes;

	private List<Keyframe[]> BluePairedKeyframes;

	private ColorCorrectionCurves CurvesScript;

	private const float PAIRING_DISTANCE = 0.01f;

	private const float TANGENT_DISTANCE = 0.0012f;

	private const float UPDATE_RATE = 1f;

	private bool gameplayScene;

	private HeroController hero;

	private GameManager gm;

	private static List<Keyframe> tempA;

	private static List<Keyframe> tempB;

	private static List<Keyframe> finalFramesList;

	private static List<Keyframe[]> simplePairList;

	private bool ChangesInEditor = true;

	private float LastFactor;

	private float LastSaturationA;

	private float LastSaturationB;

	private float LastAmbientIntensityA;

	private float LastAmbientIntensityB;

	private float startBufferDuration = 0.5f;

	public bool markerActive
	{
		get;
		private set;
	}

	public bool startBufferActive
	{
		get;
		private set;
	}

	public void SetFactor(float factor)
	{
		Factor = factor;
	}

	public void SetSaturationA(float saturationA)
	{
		SaturationA = saturationA;
	}

	public void SetSaturationB(float saturationB)
	{
		SaturationB = saturationB;
	}

	public void GameInit()
	{
		CurvesScript = GetComponent<ColorCorrectionCurves>();
		tempA = new List<Keyframe>(128);
		tempB = new List<Keyframe>(128);
		finalFramesList = new List<Keyframe>(128);
		simplePairList = new List<Keyframe[]>(128);
		gm = GameManager.instance;
		gm.UnloadingLevel += OnLevelUnload;
		UpdateSceneType();
		LastFactor = Factor;
		LastSaturationA = SaturationA;
		LastSaturationB = SaturationB;
		LastAmbientIntensityA = AmbientIntensityA;
		LastAmbientIntensityB = AmbientIntensityB;
		PairCurvesKeyframes();
	}

	public void SceneInit()
	{
		UpdateSceneType();
		if (!gameplayScene)
		{
			Factor = 0f;
			return;
		}
		startBufferActive = true;
		markerActive = true;
		UpdateScript(forceUpdate: true);
		Invoke("FinishBufferPeriod", startBufferDuration);
	}

	private void Update()
	{
		if ((markerActive || startBufferActive) && (float)Time.frameCount % 1f == 0f)
		{
			UpdateScript();
		}
	}

	private void OnLevelUnload()
	{
		Factor = 0f;
		markerActive = false;
	}

	private void OnDisable()
	{
		if (gm != null)
		{
			gm.UnloadingLevel -= OnLevelUnload;
		}
	}

	public IEnumerator ForceRefresh()
	{
		UpdateScript();
		SetFactor(0.0002f);
		yield return new WaitForSeconds(0.1f);
		UpdateScript();
	}

	private void FinishBufferPeriod()
	{
		UpdateScript(forceUpdate: true);
		startBufferActive = false;
	}

	public void MarkerActive(bool active)
	{
		if (!startBufferActive)
		{
			markerActive = active;
		}
	}

	public void UpdateScript(bool forceUpdate = false)
	{
		if (CurvesScript == null)
		{
			GetComponent<ColorCorrectionCurves>();
		}
		if (!PairedListsInitiated())
		{
			PairCurvesKeyframes();
		}
		if (ChangesInEditor)
		{
			PairCurvesKeyframes();
			UpdateScriptParameters();
			CurvesScript.UpdateParameters();
			ChangesInEditor = false;
		}
		else if (forceUpdate)
		{
			PairCurvesKeyframes();
			UpdateScriptParameters();
			CurvesScript.UpdateParameters();
		}
		else if (Factor != LastFactor || SaturationA != LastSaturationA || SaturationB != LastSaturationB || AmbientIntensityA != LastAmbientIntensityA || AmbientIntensityB != LastAmbientIntensityB)
		{
			UpdateScriptParameters();
			CurvesScript.UpdateParameters();
			LastFactor = Factor;
			LastSaturationA = SaturationA;
			LastSaturationB = SaturationB;
			LastAmbientIntensityA = AmbientIntensityA;
			LastAmbientIntensityB = AmbientIntensityB;
		}
	}

	private void EditorHasChanged()
	{
		ChangesInEditor = true;
		UpdateScript();
	}

	public static List<Keyframe[]> PairKeyframes(AnimationCurve curveA, AnimationCurve curveB)
	{
		if (curveA.length == curveB.length)
		{
			return SimplePairKeyframes(curveA, curveB);
		}
		List<Keyframe[]> list = new List<Keyframe[]>();
		tempA.Clear();
		tempA.AddRange(curveA.keys);
		tempB.Clear();
		tempB.AddRange(curveB.keys);
		int num = 0;
		bool flag = false;
		Keyframe aKeyframe = tempA[num];
		while (num < tempA.Count)
		{
			if (flag)
			{
				aKeyframe = tempA[num];
			}
			int num2 = tempB.FindIndex((Keyframe bKeyframe) => Mathf.Abs(aKeyframe.time - bKeyframe.time) < 0.01f);
			if (num2 >= 0)
			{
				Keyframe[] item = new Keyframe[2]
				{
					tempA[num],
					tempB[num2]
				};
				list.Add(item);
				tempA.RemoveAt(num);
				tempB.RemoveAt(num2);
				flag = false;
			}
			else
			{
				num++;
				flag = true;
			}
		}
		for (int i = 0; i < tempA.Count; i++)
		{
			Keyframe keyframe = CreatePair(tempA[i], curveB);
			list.Add(new Keyframe[2]
			{
				tempA[i],
				keyframe
			});
		}
		for (int j = 0; j < tempB.Count; j++)
		{
			Keyframe keyframe2 = CreatePair(tempB[j], curveA);
			list.Add(new Keyframe[2]
			{
				keyframe2,
				tempB[j]
			});
		}
		return list;
	}

	private static List<Keyframe[]> SimplePairKeyframes(AnimationCurve curveA, AnimationCurve curveB)
	{
		if (curveA.length != curveB.length)
		{
			throw new UnityException("Simple Pair cannot work with curves with different number of Keyframes.");
		}
		List<Keyframe[]> list = new List<Keyframe[]>();
		Keyframe[] keys = curveA.keys;
		Keyframe[] keys2 = curveB.keys;
		for (int i = 0; i < curveA.length; i++)
		{
			list.Add(new Keyframe[2]
			{
				keys[i],
				keys2[i]
			});
		}
		return list;
	}

	private static Keyframe CreatePair(Keyframe kf, AnimationCurve curve)
	{
		Keyframe result = default(Keyframe);
		result.time = kf.time;
		result.value = curve.Evaluate(kf.time);
		if (kf.time >= 0.0012f)
		{
			float num = kf.time - 0.0012f;
			result.inTangent = (curve.Evaluate(num) - curve.Evaluate(kf.time)) / (num - kf.time);
		}
		if (kf.time + 0.0012f <= 1f)
		{
			float num2 = kf.time + 0.0012f;
			result.outTangent = (curve.Evaluate(num2) - curve.Evaluate(kf.time)) / (num2 - kf.time);
		}
		return result;
	}

	public static AnimationCurve CreateCurveFromKeyframes(IList<Keyframe[]> keyframePairs, float factor)
	{
		finalFramesList.Clear();
		for (int i = 0; i < keyframePairs.Count; i++)
		{
			Keyframe[] array = keyframePairs[i];
			finalFramesList.Add(AverageKeyframe(array[0], array[1], factor));
		}
		return new AnimationCurve(finalFramesList.ToArray());
	}

	public static Keyframe AverageKeyframe(Keyframe a, Keyframe b, float factor)
	{
		Keyframe result = default(Keyframe);
		result.time = a.time * (1f - factor) + b.time * factor;
		result.value = a.value * (1f - factor) + b.value * factor;
		result.inTangent = a.inTangent * (1f - factor) + b.inTangent * factor;
		result.outTangent = a.outTangent * (1f - factor) + b.outTangent * factor;
		return result;
	}

	private void PairCurvesKeyframes()
	{
		RedPairedKeyframes = PairKeyframes(RedA, RedB);
		GreenPairedKeyframes = PairKeyframes(GreenA, GreenB);
		BluePairedKeyframes = PairKeyframes(BlueA, BlueB);
	}

	private void UpdateScriptParameters()
	{
		if (CurvesScript == null)
		{
			CurvesScript = GetComponent<ColorCorrectionCurves>();
		}
		Factor = Mathf.Clamp01(Factor);
		SaturationA = Mathf.Clamp(SaturationA, 0f, 5f);
		SaturationB = Mathf.Clamp(SaturationB, 0f, 5f);
		CurvesScript.saturation = Mathf.Lerp(SaturationA, SaturationB, Factor);
		CurvesScript.redChannel = CreateCurveFromKeyframes(RedPairedKeyframes, Factor);
		CurvesScript.greenChannel = CreateCurveFromKeyframes(GreenPairedKeyframes, Factor);
		CurvesScript.blueChannel = CreateCurveFromKeyframes(BluePairedKeyframes, Factor);
		SceneManager.SetLighting(Color.Lerp(AmbientColorA, AmbientColorB, Factor), Mathf.Lerp(AmbientIntensityA, AmbientIntensityB, Factor));
		if (gameplayScene)
		{
			if (hero == null)
			{
				hero = HeroController.instance;
			}
		}
	}

	private bool PairedListsInitiated()
	{
		if (RedPairedKeyframes != null && GreenPairedKeyframes != null)
		{
			return BluePairedKeyframes != null;
		}
		return false;
	}

	private void UpdateSceneType()
	{
		if (gm == null)
		{
			gm = GameManager.instance;
		}
		if (gm.IsGameplayScene())
		{
			gameplayScene = true;
			if (hero == null)
			{
				hero = HeroController.instance;
			}
		}
		else
		{
			gameplayScene = false;
		}
	}
}
