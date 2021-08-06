using GlobalEnums;
using System;
using UnityEngine;

[Serializable]
public class SceneManagerSettings
{
	public SceneManagerSettings(MapZone mapZone, Color defaultColor, float defaultIntensity, float saturation, AnimationCurve redChannel, AnimationCurve greenChannel, AnimationCurve blueChannel, Color heroLightColor)
	{
		this.mapZone = mapZone;
		this.defaultColor = defaultColor;
		this.defaultIntensity = defaultIntensity;
		this.saturation = saturation;
		this.redChannel = redChannel;
		this.greenChannel = greenChannel;
		this.blueChannel = blueChannel;
		this.heroLightColor = heroLightColor;
	}

	public SceneManagerSettings() { }

	[SerializeField]
	public MapZone mapZone;

	[SerializeField]
	public Color defaultColor;

	public float defaultIntensity;

	public float saturation;

	[SerializeField]
	public AnimationCurve redChannel;

	[SerializeField]
	public AnimationCurve greenChannel;

	[SerializeField]
	public AnimationCurve blueChannel;

	[SerializeField]
	public Color heroLightColor;
}