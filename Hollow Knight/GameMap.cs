using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameMap : MonoBehaviour
{
	private void OnEnable()
	{
		gm = GameManager.instance;
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
		gm = GameManager.instance;
		pd = PlayerData.instance;
		hero = HeroController.instance.gameObject;
		if (gm.IsGameplayScene())
		{
			GetTilemapDimensions();
		}
	}

	public void LevelReady()
	{
		inRoom = false;
		if (gm.IsGameplayScene())
		{
			GetTilemapDimensions();
		}
	}

	private void OnLevelWasLoaded()
	{
		inRoom = false;
		if (gm.IsGameplayScene())
		{
			GetTilemapDimensions();
		}
	}

	public void SetCompassPoint()
	{
		pd.SetFloatSwappedArgs(doorX, "gMap_doorX");
		pd.SetFloatSwappedArgs(doorY, "gMap_doorY");
		pd.SetStringSwappedArgs(doorScene, "gMap_doorScene");
		pd.SetStringSwappedArgs(doorMapZone, "gMap_doorMapZone");
		pd.SetFloatSwappedArgs(doorOriginOffsetX, "gMap_doorOriginOffsetX");
		pd.SetFloatSwappedArgs(doorOriginOffsetY, "gMap_doorOriginOffsetY");
		pd.SetFloatSwappedArgs(doorSceneWidth, "gMap_doorSceneWidth");
		pd.SetFloatSwappedArgs(doorSceneHeight, "gMap_doorSceneHeight");
	}

	public void GetDoorValues()
	{
		doorX = pd.GetFloat("gMap_doorX");
		doorY = pd.GetFloat("gMap_doorY");
		doorScene = pd.GetString("gMap_doorScene");
		doorMapZone = pd.GetString("gMap_doorMapZone");
		doorOriginOffsetX = pd.GetFloat("gMap_doorOriginOffsetX");
		doorOriginOffsetY = pd.GetFloat("gMap_doorOriginOffsetY");
		doorSceneWidth = pd.GetFloat("gMap_doorSceneWidth");
		doorSceneHeight = pd.GetFloat("gMap_doorSceneHeight");
	}

	public void SetupMap(bool pinsOnly = false)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject gameObject = transform.GetChild(i).gameObject;
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				GameObject gameObject2 = gameObject.transform.GetChild(j).gameObject;
				if (pd.GetVariable<List<string>>("scenesMapped").Contains(gameObject2.transform.name) || pd.GetBool("mapAllRooms"))
				{
					if (pd.GetBool("hasQuill") && !pinsOnly)
					{
						gameObject2.SetActive(true);
					}
					for (int k = 0; k < gameObject2.transform.childCount; k++)
					{
						GameObject gameObject3 = gameObject2.transform.GetChild(k).gameObject;
						if (gameObject3.name == "pin_blue_health" && !gameObject3.activeSelf && pd.GetVariable<List<string>>("scenesEncounteredCocoon").Contains(gameObject2.transform.name) && pd.GetBool("hasPinCocoon"))
						{
							gameObject3.SetActive(true);
						}
						if (gameObject3.name == "pin_dream_tree" && !gameObject3.activeSelf && pd.GetVariable<List<string>>("scenesEncounteredDreamPlant").Contains(gameObject2.transform.name) && pd.GetBool("hasPinDreamPlant"))
						{
							gameObject3.SetActive(true);
						}
						if (gameObject3.name == "pin_dream_tree" && gameObject3.activeSelf && pd.GetVariable<List<string>>("scenesEncounteredDreamPlantC").Contains(gameObject2.transform.name))
						{
							gameObject3.SetActive(false);
						}
					}
				}
			}
		}
	}

	private void GetTilemapDimensions()
	{
		originOffsetX = 0f;
		originOffsetY = 0f;

		sceneWidth = gm.sceneWidth;
		sceneHeight = gm.sceneHeight;
	}

	public void WorldMap()
	{
		string currentMapZone = gm.GetCurrentMapZone();
		displayNextArea = false;
		shadeMarker.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		SetupMapMarkers();
		panMinX = -1.44f;
		panMaxX = 4.55f;
		panMinY = -8.642f;
		panMaxY = -5.58f;
		if (pd.GetBool("mapAbyss") || (currentMapZone == "ABYSS" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapAbyss"))
			{
				areaAncientBasin.SetActive(true);
			}
			if (panMinX > -13.44f)
			{
				panMinX = -13.44f;
			}
			if (panMaxY < 15.6913f)
			{
				panMaxY = 15.6913f;
			}
		}
		if (pd.GetBool("mapCity") || ((currentMapZone == "CITY" || currentMapZone == "KINGS_STATION" || currentMapZone == "SOUL_SOCIETY" || currentMapZone == "LURIENS_TOWER") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapCity"))
			{
				areaCity.SetActive(true);
			}
			if (panMinX > -14.26f)
			{
				panMinX = -14.26f;
			}
			if (panMaxY < 2.27f)
			{
				panMaxY = 2.27f;
			}
		}
		if (pd.GetBool("mapCliffs") || (currentMapZone == "CLIFFS" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapCliffs"))
			{
				areaCliffs.SetActive(true);
			}
			if (panMaxX < 9.07f)
			{
				panMaxX = 9.07f;
			}
			if (panMinY > -10.6653f)
			{
				panMinY = -10.6653f;
			}
		}
		if (pd.GetBool("mapCrossroads"))
		{
			areaCrossroads.SetActive(true);
		}
		if (pd.GetBool("mapMines") || (currentMapZone == "MINES" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapMines"))
			{
				areaCrystalPeak.SetActive(true);
			}
			if (panMinX > -13.47f)
			{
				panMinX = -13.47f;
			}
			if (panMinY > -12.58548f)
			{
				panMinY = -12.58548f;
			}
		}
		if (pd.GetBool("mapDeepnest") || ((currentMapZone == "DEEPNEST" || currentMapZone == "BEASTS_DEN") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapDeepnest"))
			{
				areaDeepnest.SetActive(true);
			}
			if (panMaxX < 17.3f)
			{
				panMaxX = 17.3f;
			}
			if (panMaxY < 8.29f)
			{
				panMaxY = 8.29f;
			}
		}
		if (pd.GetBool("mapFogCanyon") || ((currentMapZone == "FOG_CANYON" || currentMapZone == "MONOMON_ARCHIVE") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapFogCanyon"))
			{
				areaFogCanyon.SetActive(true);
			}
			if (panMaxX < 7.13f)
			{
				panMaxX = 7.13f;
			}
			if (panMaxY < -2.49f)
			{
				panMaxY = -2.49f;
			}
		}
		if (pd.GetBool("mapFungalWastes") || ((currentMapZone == "WASTES" || currentMapZone == "QUEENS_STATION") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapFungalWastes"))
			{
				areaFungalWastes.SetActive(true);
			}
			if (panMaxY < 4.14f)
			{
				panMaxY = 4.14f;
			}
		}
		if (pd.GetBool("mapGreenpath") || (currentMapZone == "GREEN_PATH" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapGreenpath"))
			{
				areaGreenpath.SetActive(true);
			}
			if (panMaxX < 17.26f)
			{
				panMaxX = 17.26f;
			}
			if (panMaxY < -6.22f)
			{
				panMaxY = -6.22f;
			}
		}
		if (pd.GetBool("mapOutskirts") || ((currentMapZone == "OUTSKIRTS" || currentMapZone == "HIVE" || currentMapZone == "COLOSSEUM") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapOutskirts"))
			{
				areaKingdomsEdge.SetActive(true);
			}
			if (panMinX > -24.16f)
			{
				panMinX = -24.16f;
			}
			if (panMaxY < 8.16f)
			{
				panMaxY = 8.16f;
			}
		}
		if (pd.GetBool("mapRoyalGardens") || (currentMapZone == "ROYAL_GARDENS" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapRoyalGardens"))
			{
				areaQueensGardens.SetActive(true);
			}
			if (panMaxX < 17.3f)
			{
				panMaxX = 17.3f;
			}
			if (panMaxY < 1.53f)
			{
				panMaxY = 1.53f;
			}
		}
		if (pd.GetBool("mapRestingGrounds") || (currentMapZone == "RESTING_GROUNDS" && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapRestingGrounds"))
			{
				areaRestingGrounds.SetActive(true);
			}
			if (panMinX > -14.59f)
			{
				panMinX = -14.59f;
			}
			if (panMaxY < -5.77f)
			{
				panMaxY = -5.77f;
			}
		}
		areaDirtmouth.SetActive(true);
		if (pd.GetBool("mapWaterways") || ((currentMapZone == "WATERWAYS" || currentMapZone == "GODSEEKER_WASTE") && pd.GetBool("equippedCharm_2")))
		{
			if (pd.GetBool("mapWaterways"))
			{
				areaWaterways.SetActive(true);
			}
			if (panMinX > -11.39f)
			{
				panMinX = -11.39f;
			}
			if (panMaxY < 7.06f)
			{
				panMaxY = 7.06f;
			}
		}
		flamePins.SetActive(true);
		PositionCompass(false);
	}

	public void QuickMapAncientBasin()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(true);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapCity()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(true);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapCliffs()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(true);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapCrossroads()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(true);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapCrystalPeak()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(true);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapDeepnest()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(true);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapFogCanyon()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(true);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapFungalWastes()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(true);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapGreenpath()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(true);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapKingdomsEdge()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(true);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapQueensGardens()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(true);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapRestingGrounds()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(true);
		areaRestingGrounds.SetActive(true);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapDirtmouth()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(true);
		areaWaterways.SetActive(false);
		PositionCompass(false);
	}

	public void QuickMapWaterways()
	{
		shadeMarker.SetActive(true);
		flamePins.SetActive(true);
		dreamGateMarker.SetActive(true);
		dreamerPins.SetActive(true);
		displayNextArea = true;
		SetupMapMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		areaWaterways.SetActive(true);
		areaDirtmouth.SetActive(false);
		PositionCompass(false);
	}

	public void CloseQuickMap()
	{
		shadeMarker.SetActive(false);
		dreamGateMarker.SetActive(false);
		dreamerPins.SetActive(false);
		DisableMarkers();
		areaAncientBasin.SetActive(false);
		areaCity.SetActive(false);
		areaCliffs.SetActive(false);
		areaCrossroads.SetActive(false);
		areaCrystalPeak.SetActive(false);
		areaDeepnest.SetActive(false);
		areaFogCanyon.SetActive(false);
		areaFungalWastes.SetActive(false);
		areaGreenpath.SetActive(false);
		areaKingdomsEdge.SetActive(false);
		areaQueensGardens.SetActive(false);
		areaRestingGrounds.SetActive(false);
		areaDirtmouth.SetActive(false);
		flamePins.SetActive(false);
		areaWaterways.SetActive(false);
		compassIcon.SetActive(false);
		displayNextArea = false;
		displayingCompass = false;
	}

	public void PositionDreamGateMarker()
	{
		posGate = true;
		PositionCompass(false);
		posGate = false;
	}

	public void PositionCompass(bool posShade)
	{
		GameObject gameObject = null;
		string currentMapZone = gm.GetCurrentMapZone();
		if (currentMapZone == "DREAM_WORLD" || currentMapZone == "WHITE_PALACE" || currentMapZone == "GODS_GLORY")
		{
			compassIcon.SetActive(false);
			displayingCompass = false;
			return;
		}
		string sceneName;
		if (!inRoom)
		{
			sceneName = gm.sceneName;
		}
		else
		{
			currentMapZone = doorMapZone;
			sceneName = doorScene;
		}
		if (currentMapZone == "ABYSS")
		{
			gameObject = areaAncientBasin;
			for (int i = 0; i < areaAncientBasin.transform.childCount; i++)
			{
				GameObject gameObject2 = areaAncientBasin.transform.GetChild(i).gameObject;
				if (gameObject2.name == sceneName)
				{
					currentScene = gameObject2;
					break;
				}
			}
		}
		else if (currentMapZone == "CITY" || currentMapZone == "KINGS_STATION" || currentMapZone == "SOUL_SOCIETY" || currentMapZone == "LURIENS_TOWER")
		{
			gameObject = areaCity;
			for (int j = 0; j < areaCity.transform.childCount; j++)
			{
				GameObject gameObject3 = areaCity.transform.GetChild(j).gameObject;
				if (gameObject3.name == sceneName)
				{
					currentScene = gameObject3;
					break;
				}
			}
		}
		else if (currentMapZone == "CLIFFS")
		{
			gameObject = areaCliffs;
			for (int k = 0; k < areaCliffs.transform.childCount; k++)
			{
				GameObject gameObject4 = areaCliffs.transform.GetChild(k).gameObject;
				if (gameObject4.name == sceneName)
				{
					currentScene = gameObject4;
					break;
				}
			}
		}
		else if (currentMapZone == "CROSSROADS" || currentMapZone == "SHAMAN_TEMPLE")
		{
			gameObject = areaCrossroads;
			for (int l = 0; l < areaCrossroads.transform.childCount; l++)
			{
				GameObject gameObject5 = areaCrossroads.transform.GetChild(l).gameObject;
				if (gameObject5.name == sceneName)
				{
					currentScene = gameObject5;
					break;
				}
			}
		}
		else if (currentMapZone == "MINES")
		{
			gameObject = areaCrystalPeak;
			for (int m = 0; m < areaCrystalPeak.transform.childCount; m++)
			{
				GameObject gameObject6 = areaCrystalPeak.transform.GetChild(m).gameObject;
				if (gameObject6.name == sceneName)
				{
					currentScene = gameObject6;
					break;
				}
			}
		}
		else if (currentMapZone == "DEEPNEST" || currentMapZone == "BEASTS_DEN")
		{
			gameObject = areaDeepnest;
			for (int n = 0; n < areaDeepnest.transform.childCount; n++)
			{
				GameObject gameObject7 = areaDeepnest.transform.GetChild(n).gameObject;
				if (gameObject7.name == sceneName)
				{
					currentScene = gameObject7;
					break;
				}
			}
		}
		else if (currentMapZone == "FOG_CANYON" || currentMapZone == "MONOMON_ARCHIVE")
		{
			gameObject = areaFogCanyon;
			for (int num = 0; num < areaFogCanyon.transform.childCount; num++)
			{
				GameObject gameObject8 = areaFogCanyon.transform.GetChild(num).gameObject;
				if (gameObject8.name == sceneName)
				{
					currentScene = gameObject8;
					break;
				}
			}
		}
		else if (currentMapZone == "WASTES" || currentMapZone == "QUEENS_STATION")
		{
			gameObject = areaFungalWastes;
			for (int num2 = 0; num2 < areaFungalWastes.transform.childCount; num2++)
			{
				GameObject gameObject9 = areaFungalWastes.transform.GetChild(num2).gameObject;
				if (gameObject9.name == sceneName)
				{
					currentScene = gameObject9;
					break;
				}
			}
		}
		else if (currentMapZone == "GREEN_PATH")
		{
			gameObject = areaGreenpath;
			for (int num3 = 0; num3 < areaGreenpath.transform.childCount; num3++)
			{
				GameObject gameObject10 = areaGreenpath.transform.GetChild(num3).gameObject;
				if (gameObject10.name == sceneName)
				{
					currentScene = gameObject10;
					break;
				}
			}
		}
		else if (currentMapZone == "OUTSKIRTS" || currentMapZone == "HIVE" || currentMapZone == "COLOSSEUM")
		{
			gameObject = areaKingdomsEdge;
			for (int num4 = 0; num4 < areaKingdomsEdge.transform.childCount; num4++)
			{
				GameObject gameObject11 = areaKingdomsEdge.transform.GetChild(num4).gameObject;
				if (gameObject11.name == sceneName)
				{
					currentScene = gameObject11;
					break;
				}
			}
		}
		else if (currentMapZone == "ROYAL_GARDENS")
		{
			gameObject = areaQueensGardens;
			for (int num5 = 0; num5 < areaQueensGardens.transform.childCount; num5++)
			{
				GameObject gameObject12 = areaQueensGardens.transform.GetChild(num5).gameObject;
				if (gameObject12.name == sceneName)
				{
					currentScene = gameObject12;
					break;
				}
			}
		}
		else if (currentMapZone == "RESTING_GROUNDS")
		{
			gameObject = areaRestingGrounds;
			for (int num6 = 0; num6 < areaRestingGrounds.transform.childCount; num6++)
			{
				GameObject gameObject13 = areaRestingGrounds.transform.GetChild(num6).gameObject;
				if (gameObject13.name == sceneName)
				{
					currentScene = gameObject13;
					break;
				}
			}
		}
		else if (currentMapZone == "TOWN" || currentMapZone == "KINGS_PASS")
		{
			gameObject = areaDirtmouth;
			for (int num7 = 0; num7 < areaDirtmouth.transform.childCount; num7++)
			{
				GameObject gameObject14 = areaDirtmouth.transform.GetChild(num7).gameObject;
				if (gameObject14.name == sceneName)
				{
					currentScene = gameObject14;
					break;
				}
			}
		}
		else if (currentMapZone == "WATERWAYS" || currentMapZone == "GODSEEKER_WASTE")
		{
			gameObject = areaWaterways;
			for (int num8 = 0; num8 < areaWaterways.transform.childCount; num8++)
			{
				GameObject gameObject15 = areaWaterways.transform.GetChild(num8).gameObject;
				if (gameObject15.name == sceneName)
				{
					currentScene = gameObject15;
					break;
				}
			}
		}
		if (currentScene != null)
		{
			currentScenePos = new Vector3(currentScene.transform.localPosition.x + gameObject.transform.localPosition.x, currentScene.transform.localPosition.y + gameObject.transform.localPosition.y, 0f);
			if (!posShade && !posGate)
			{
				if (pd.GetBool("equippedCharm_2"))
				{
					displayingCompass = true;
					compassIcon.SetActive(true);
				}
				else
				{
					compassIcon.SetActive(false);
					displayingCompass = false;
				}
			}
			if (posShade)
			{
				if (!inRoom)
				{
					shadeMarker.transform.localPosition = new Vector3(currentScenePos.x, currentScenePos.y, 0f);
				}
				else
				{
					float num9 = currentScenePos.x - currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.x / 100f / 2f + (doorX + doorOriginOffsetX) / doorSceneWidth * (currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.x / 100f * transform.localScale.x) / transform.localScale.x;
					float num10 = currentScenePos.y - currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.y / 100f / 2f + (doorY + doorOriginOffsetY) / doorSceneHeight * (currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.y / 100f * transform.localScale.y) / transform.localScale.y;
					shadeMarker.transform.localPosition = new Vector3(num9, num10, 0f);
				}
				pd.SetVector3SwappedArgs(new Vector3(currentScenePos.x, currentScenePos.y, 0f), "shadeMapPos");
			}
			if (posGate)
			{
				dreamGateMarker.transform.localPosition = new Vector3(currentScenePos.x, currentScenePos.y, 0f);
				pd.SetVector3SwappedArgs(new Vector3(currentScenePos.x, currentScenePos.y, 0f), "dreamgateMapPos");
				return;
			}
		}
		else
		{
			Debug.Log("Couldn't find current scene object!");
			if (posShade)
			{
				pd.SetVector3SwappedArgs(new Vector3(-10000f, -10000f, 0f), "shadeMapPos");
				shadeMarker.transform.localPosition = pd.GetVector3("shadeMapPos");
			}
		}
	}

	Func<bool> down;
	Func<bool> up;
	Func<bool> left;
	Func<bool> right;

	Func<bool> rs_down;
	Func<bool> rs_up;
	Func<bool> rs_left;
	Func<bool> rs_right;

	static void GetIsPressedFunc(string name, ref Func<bool> isPressed)
	{
		if (isPressed == null)
		{
			var prop = WeaverTypeHelpers.GetWeaverProperty("WeaverCore.PlayerInput", name);
			var buttonType = prop.GetGetMethod().ReturnType;
			var isPressedProp = buttonType.GetType().GetProperty("IsPressed");

			isPressed = () => (bool)isPressedProp.GetValue(prop.GetValue(null));
		}
	}

	private void Update()
	{
		if (displayingCompass)
		{
			Vector2 vector = currentScene.GetComponent<SpriteRenderer>().sprite.bounds.size;
			if (!inRoom)
			{
				float num = currentScenePos.x - vector.x / 2f + (hero.transform.position.x + originOffsetX) / sceneWidth * (vector.x * transform.localScale.x) / transform.localScale.x;
				float num2 = currentScenePos.y - vector.y / 2f + (hero.transform.position.y + originOffsetY) / sceneHeight * (vector.y * transform.localScale.y) / transform.localScale.y;
				compassIcon.transform.localPosition = new Vector3(num, num2, -1f);
			}
			else
			{
				float num = currentScenePos.x - vector.x / 2f + (doorX + doorOriginOffsetX) / doorSceneWidth * (vector.x * transform.localScale.x) / transform.localScale.x;
				float num2 = currentScenePos.y - vector.y / 2f + (doorY + doorOriginOffsetY) / doorSceneHeight * (vector.y * transform.localScale.y) / transform.localScale.y;
				compassIcon.transform.localPosition = new Vector3(num, num2, -1f);
				displayingCompass = false;
			}
			if (!compassIcon.activeSelf)
			{
				compassIcon.SetActive(true);
			}
		}
		if (canPan)
		{
			GetIsPressedFunc(nameof(rs_down), ref rs_down);
			GetIsPressedFunc(nameof(down), ref down);
			GetIsPressedFunc(nameof(rs_up), ref rs_up);
			GetIsPressedFunc(nameof(up), ref up);
			GetIsPressedFunc(nameof(rs_left), ref rs_left);
			GetIsPressedFunc(nameof(left), ref left);
			GetIsPressedFunc(nameof(rs_right), ref rs_right);
			GetIsPressedFunc(nameof(right), ref right);

			if (rs_down())
			{
				transform.position = new Vector3(transform.position.x, transform.position.y + panSpeed * 2f * Time.deltaTime, transform.position.z);
			}
			else if (down())
			{
				transform.position = new Vector3(transform.position.x, transform.position.y + panSpeed * Time.deltaTime, transform.position.z);
			}
			if (rs_up())
			{
				transform.position = new Vector3(transform.position.x, transform.position.y - panSpeed * 2f * Time.deltaTime, transform.position.z);
			}
			else if (up())
			{
				transform.position = new Vector3(transform.position.x, transform.position.y - panSpeed * Time.deltaTime, transform.position.z);
			}
			if (rs_left())
			{
				transform.position = new Vector3(transform.position.x + panSpeed * 2f * Time.deltaTime, transform.position.y, transform.position.z);
			}
			else if (left())
			{
				transform.position = new Vector3(transform.position.x + panSpeed * Time.deltaTime, transform.position.y, transform.position.z);
			}
			if (rs_right())
			{
				transform.position = new Vector3(transform.position.x - panSpeed * 2f * Time.deltaTime, transform.position.y, transform.position.z);
			}
			else if (right())
			{
				transform.position = new Vector3(transform.position.x - panSpeed * Time.deltaTime, transform.position.y, transform.position.z);
			}
			KeepWithinBounds();
			if (transform.position.x == panMinX && panArrowR.activeSelf)
			{
				panArrowR.SetActive(false);
			}
			else if (transform.position.x > panMinX && !panArrowR.activeSelf)
			{
				panArrowR.SetActive(true);
			}
			if (transform.position.x == panMaxX && panArrowL.activeSelf)
			{
				panArrowL.SetActive(false);
			}
			else if (transform.position.x < panMaxX && !panArrowL.activeSelf)
			{
				panArrowL.SetActive(true);
			}
			if (transform.position.y == panMinY && panArrowU.activeSelf)
			{
				panArrowU.SetActive(false);
			}
			else if (transform.position.y > panMinY && !panArrowU.activeSelf)
			{
				panArrowU.SetActive(true);
			}
			if (transform.position.y == panMaxY && panArrowD.activeSelf)
			{
				panArrowD.SetActive(false);
				return;
			}
			if (transform.position.y < panMaxY && !panArrowD.activeSelf)
			{
				panArrowD.SetActive(true);
			}
		}
	}

	private void DisableMarkers()
	{
		for (int i = 0; i < mapMarkersBlue.Length; i++)
		{
			mapMarkersBlue[i].SetActive(false);
		}
		for (int j = 0; j < mapMarkersRed.Length; j++)
		{
			mapMarkersRed[j].SetActive(false);
		}
		for (int k = 0; k < mapMarkersYellow.Length; k++)
		{
			mapMarkersYellow[k].SetActive(false);
		}
		for (int l = 0; l < mapMarkersWhite.Length; l++)
		{
			mapMarkersWhite[l].SetActive(false);
		}
	}

	public void SetManualTilemap(float offsetX, float offsetY, float width, float height)
	{
		originOffsetX = offsetX;
		originOffsetY = offsetY;
		sceneWidth = width;
		sceneHeight = height;
	}

	public void SetDoorValues(float x, float y, string scene, string mapZone)
	{
		doorX = x;
		doorY = y;
		doorScene = scene;
		doorMapZone = mapZone;
		doorOriginOffsetX = originOffsetX;
		doorOriginOffsetY = originOffsetY;
		doorSceneWidth = sceneWidth;
		doorSceneHeight = sceneHeight;
	}

	public void SetCustomCompassPos(float x, float y, string scene, string mapZone, float offsetX, float offsetY, float width, float height)
	{
		inRoom = true;
		doorX = x;
		doorY = y;
		doorScene = scene;
		doorMapZone = mapZone;
		doorOriginOffsetX = offsetX;
		doorOriginOffsetY = offsetY;
		doorSceneWidth = width;
		doorSceneHeight = height;
	}

	public void SetInRoom(bool room)
	{
		inRoom = room;
	}

	public void SetCanPan(bool pan)
	{
		canPan = pan;
	}

	public string GetDoorMapZone()
	{
		return doorMapZone;
	}

	public bool GetInRoom()
	{
		return inRoom;
	}

	public void SetPanArrows(GameObject arrowU, GameObject arrowD, GameObject arrowL, GameObject arrowR)
	{
		panArrowU = arrowU;
		panArrowD = arrowD;
		panArrowL = arrowL;
		panArrowR = arrowR;
	}

	public void KeepWithinBounds()
	{
		if (transform.position.x < panMinX)
		{
			transform.position = new Vector3(panMinX, transform.position.y, transform.position.z);
		}
		if (transform.position.x > panMaxX)
		{
			transform.position = new Vector3(panMaxX, transform.position.y, transform.position.z);
		}
		if (transform.position.y < panMinY)
		{
			transform.position = new Vector3(transform.position.x, panMinY, transform.position.z);
		}
		if (transform.position.y > panMaxY)
		{
			transform.position = new Vector3(transform.position.x, panMaxY, transform.position.z);
		}
	}

	public void StopPan()
	{
		canPan = false;
		panArrowU.SetActive(false);
		panArrowL.SetActive(false);
		panArrowR.SetActive(false);
		panArrowD.SetActive(false);
	}

	public void StartPan()
	{
		canPan = true;
	}

	public void SetupMapMarkers()
	{
		DisableMarkers();
		for (int i = 0; i < pd.GetVariable<List<Vector3>>("placedMarkers_b").Count; i++)
		{
			mapMarkersBlue[i].SetActive(true);
			mapMarkersBlue[i].transform.localPosition = pd.GetVariable<List<Vector3>>("placedMarkers_b")[i];
		}
		for (int j = 0; j < pd.GetVariable<List<Vector3>>("placedMarkers_r").Count; j++)
		{
			mapMarkersRed[j].SetActive(true);
			mapMarkersRed[j].transform.localPosition = pd.GetVariable<List<Vector3>>("placedMarkers_r")[j];
		}
		for (int k = 0; k < pd.GetVariable<List<Vector3>>("placedMarkers_y").Count; k++)
		{
			mapMarkersYellow[k].SetActive(true);
			mapMarkersYellow[k].transform.localPosition = pd.GetVariable<List<Vector3>>("placedMarkers_y")[k];
		}
		for (int l = 0; l < pd.GetVariable<List<Vector3>>("placedMarkers_w").Count; l++)
		{
			mapMarkersWhite[l].SetActive(true);
			mapMarkersWhite[l].transform.localPosition = pd.GetVariable<List<Vector3>>("placedMarkers_w")[l];
		}
	}

	private GameManager gm;

	private PlayerData pd;

	private GameObject hero;

	public GameObject compassIcon;

	private float originOffsetX;

	private float originOffsetY;

	private float sceneWidth;

	private float sceneHeight;

	public float doorX;

	public float doorY;

	public string doorScene;

	public string doorMapZone;

	public float doorOriginOffsetX;

	public float doorOriginOffsetY;

	public float doorSceneWidth;

	public float doorSceneHeight;

	public bool inRoom;

	public GameObject areaAncientBasin;

	public GameObject areaCity;

	public GameObject areaCliffs;

	public GameObject areaCrossroads;

	public GameObject areaCrystalPeak;

	public GameObject areaDeepnest;

	public GameObject areaFogCanyon;

	public GameObject areaFungalWastes;

	public GameObject areaGreenpath;

	public GameObject areaKingdomsEdge;

	public GameObject areaQueensGardens;

	public GameObject areaRestingGrounds;

	public GameObject areaDirtmouth;

	public GameObject areaWaterways;

	public GameObject flamePins;

	public GameObject dreamerPins;

	public GameObject shadeMarker;

	public GameObject dreamGateMarker;

	private bool posGate;

	public bool displayNextArea;

	private bool displayingCompass;

	public Vector3 currentScenePos;

	public GameObject currentScene;

	public bool canPan;

	public float panSpeed;

	public float panMinX;

	public float panMaxX;

	public float panMinY;

	public float panMaxY;

	public GameObject panArrowU;

	public GameObject panArrowD;

	public GameObject panArrowL;

	public GameObject panArrowR;

	public GameObject[] mapMarkersBlue;

	public GameObject[] mapMarkersRed;

	public GameObject[] mapMarkersYellow;

	public GameObject[] mapMarkersWhite;
}

