using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerData
{
    private enum MapBools
	{
		MapDirtmouth = 0,
		MapCrossroads = 1,
		MapGreenpath = 2,
		MapFogCanyon = 3,
		MapRoyalGardens = 4,
		MapFungalWastes = 5,
		MapCity = 6,
		MapWaterways = 7,
		MapMines = 8,
		MapDeepnest = 9,
		MapCliffs = 10,
		MapOutskirts = 11,
		MapRestingGrounds = 12,
		MapAbyss = 13
	}

    private static PlayerData dummyInstance;
    public static PlayerData instance
    {
        get
        {
            if (PlayerData.dummyInstance == null)
            {
                PlayerData.dummyInstance = new PlayerData();
            }
            return PlayerData.dummyInstance;
        }
        set => PlayerData.dummyInstance = value;
    }

    [NonSerialized]
    public Vector3 hazardRespawnLocation;
    public bool hazardRespawnFacingRight;
    public string respawnMarkerName;
    public bool respawnFacingRight;
    public string respawnScene;
    public int respawnType;
    public int prevHealth;
    public int health;
    public int maxHealth;
    public int maxHealthBase;
    public int healthBlue;
    public int joniHealthBlue;
    public bool damagedBlue;
    public bool isInvincible;
    public bool atBench;
    public MapZone mapZone;
    public List<string> scenesVisited;
    public bool hasDoubleJump;
    public List<string> scenesEncounteredBench;
    public string currentBossStatueCompletionKey;
    public int bossStatueTargetLevel;
    public bool equippedCharm_23;
    public bool brokenCharm_23;
    public int nailDamage;
    public BossSequenceController.BossSequenceData currentBossSequence;
    public int MPCharge;
    public int MPReserve;
    public string bossReturnEntryGate;
    public List<string> scenesEncounteredCocoon;
    public bool mapDirtmouth;
	public bool mapCrossroads;
	public bool mapGreenpath;
	public bool mapFogCanyon;
	public bool mapRoyalGardens;
	public bool mapFungalWastes;
	public bool mapCity;
	public bool mapWaterways;
	public bool mapMines;
	public bool mapDeepnest;
	public bool mapCliffs;
	public bool mapOutskirts;
	public bool mapRestingGrounds;
	public bool mapAbyss;
    public bool hasMap;
	public bool mapAllRooms;
	public bool atMapPrompt;
	public List<string> scenesMapped;
	public List<string> scenesGrubRescued;
	public List<string> scenesFlameCollected;
	public List<string> scenesEncounteredDreamPlant;
	public List<string> scenesEncounteredDreamPlantC;
	public int charmSlots;
    
    private Dictionary<string, MapBools> mapZoneBools;

    public int CurrentMaxHealth
    {
        get
        {
            if (BossSequenceController.BoundShell)
            {
                return Mathf.Min(maxHealth, BossSequenceController.BoundMaxHealth);
            }
            return maxHealth;
        }
    }

    protected PlayerData()
    {
        SetupNewPlayerData();
    }

    public void CheckAllMaps()
	{
		if (mapCrossroads && mapGreenpath && mapFogCanyon && mapRoyalGardens && mapFungalWastes && mapCity && mapWaterways && mapMines && mapDeepnest && mapCliffs && mapOutskirts && mapRestingGrounds && mapAbyss)
		{
            SetBool("corniferAtHome", true);
		}
	}

    private void SetupNewPlayerData()
    {
        atBench = false;
        hazardRespawnLocation = Vector3.zero;
        hazardRespawnFacingRight = false;
        respawnMarkerName = "Death Respawn Marker";
        respawnFacingRight = false;
        respawnScene = "Tutorial_01";
        respawnType = 0;
        health = int.MaxValue / 2;
        maxHealth = int.MaxValue / 2;
        maxHealthBase = int.MaxValue / 2;
        healthBlue = 0;
        joniHealthBlue = 0;
        damagedBlue = false;
        prevHealth = health;
        isInvincible = false;
        mapZone = MapZone.GODS_GLORY;
        scenesVisited = new List<string>();
        hasDoubleJump = true;
        scenesEncounteredBench = new List<string>();
        nailDamage = 5;
        currentBossStatueCompletionKey = "";
        bossStatueTargetLevel = -1;
        equippedCharm_23 = false;
        brokenCharm_23 = false;
        currentBossSequence = null;
        bossReturnEntryGate = "";
        MPCharge = 0;
        MPReserve = 0;
        scenesEncounteredCocoon = new List<string>();
    }

	public void CalculateNotchesUsed()
	{
		
	}

    private Dictionary<string, MapBools> InitMapBools()
	{
		return new Dictionary<string, MapBools>
		{
			{
				"Town",
				MapBools.MapDirtmouth
			},
			{
				"Tutorial_01",
				MapBools.MapDirtmouth
			},
			{
				"Abyss_03",
				MapBools.MapAbyss
			},
			{
				"Abyss_04",
				MapBools.MapAbyss
			},
			{
				"Abyss_05",
				MapBools.MapAbyss
			},
			{
				"Abyss_06_Core",
				MapBools.MapAbyss
			},
			{
				"Abyss_06_Core_b",
				MapBools.MapAbyss
			},
			{
				"Abyss_08",
				MapBools.MapAbyss
			},
			{
				"Abyss_09",
				MapBools.MapAbyss
			},
			{
				"Abyss_10",
				MapBools.MapAbyss
			},
			{
				"Abyss_12",
				MapBools.MapAbyss
			},
			{
				"Abyss_16",
				MapBools.MapAbyss
			},
			{
				"Abyss_17",
				MapBools.MapAbyss
			},
			{
				"Abyss_18",
				MapBools.MapAbyss
			},
			{
				"Abyss_18_b",
				MapBools.MapAbyss
			},
			{
				"Abyss_19",
				MapBools.MapAbyss
			},
			{
				"Abyss_20",
				MapBools.MapAbyss
			},
			{
				"Abyss_21",
				MapBools.MapAbyss
			},
			{
				"Abyss_22",
				MapBools.MapAbyss
			},
			{
				"Crossroads_49b",
				MapBools.MapCity
			},
			{
				"Ruins1_01",
				MapBools.MapCity
			},
			{
				"Ruins1_02",
				MapBools.MapCity
			},
			{
				"Ruins1_03",
				MapBools.MapCity
			},
			{
				"Ruins1_04",
				MapBools.MapCity
			},
			{
				"Ruins1_05",
				MapBools.MapCity
			},
			{
				"Ruins1_05b",
				MapBools.MapCity
			},
			{
				"Ruins1_05c",
				MapBools.MapCity
			},
			{
				"Ruins1_06",
				MapBools.MapCity
			},
			{
				"Ruins1_09",
				MapBools.MapCity
			},
			{
				"Ruins1_17",
				MapBools.MapCity
			},
			{
				"Ruins1_18",
				MapBools.MapCity
			},
			{
				"Ruins1_18_b",
				MapBools.MapCity
			},
			{
				"Ruins1_23",
				MapBools.MapCity
			},
			{
				"Ruins1_24",
				MapBools.MapCity
			},
			{
				"Ruins1_25",
				MapBools.MapCity
			},
			{
				"Ruins1_27",
				MapBools.MapCity
			},
			{
				"Ruins1_28",
				MapBools.MapCity
			},
			{
				"Ruins1_29",
				MapBools.MapCity
			},
			{
				"Ruins1_30",
				MapBools.MapCity
			},
			{
				"Ruins1_31",
				MapBools.MapCity
			},
			{
				"Ruins1_31b",
				MapBools.MapCity
			},
			{
				"Ruins1_31_top",
				MapBools.MapCity
			},
			{
				"Ruins1_31_top_2",
				MapBools.MapCity
			},
			{
				"Ruins1_32",
				MapBools.MapCity
			},
			{
				"Ruins2_01",
				MapBools.MapCity
			},
			{
				"Ruins2_01_b",
				MapBools.MapCity
			},
			{
				"Ruins2_03",
				MapBools.MapCity
			},
			{
				"Ruins2_03b",
				MapBools.MapCity
			},
			{
				"Ruins2_04",
				MapBools.MapCity
			},
			{
				"Ruins2_05",
				MapBools.MapCity
			},
			{
				"Ruins2_06",
				MapBools.MapCity
			},
			{
				"Ruins2_07",
				MapBools.MapCity
			},
			{
				"Ruins2_07_left",
				MapBools.MapCity
			},
			{
				"Ruins2_07_right",
				MapBools.MapCity
			},
			{
				"Ruins2_08",
				MapBools.MapCity
			},
			{
				"Ruins2_09",
				MapBools.MapCity
			},
			{
				"Ruins2_10_b",
				MapBools.MapCity
			},
			{
				"Ruins2_11",
				MapBools.MapCity
			},
			{
				"Ruins2_11_b",
				MapBools.MapCity
			},
			{
				"Ruins2_Watcher_Room",
				MapBools.MapCity
			},
			{
				"Ruins_Bathhouse",
				MapBools.MapCity
			},
			{
				"Ruins_Elevator",
				MapBools.MapCity
			},
			{
				"Cliffs_01",
				MapBools.MapCliffs
			},
			{
				"Cliffs_01_b",
				MapBools.MapCliffs
			},
			{
				"Cliffs_02",
				MapBools.MapCliffs
			},
			{
				"Cliffs_02_b",
				MapBools.MapCliffs
			},
			{
				"Cliffs_04",
				MapBools.MapCliffs
			},
			{
				"Cliffs_05",
				MapBools.MapCliffs
			},
			{
				"Cliffs_06",
				MapBools.MapCliffs
			},
			{
				"Cliffs_06_b",
				MapBools.MapCliffs
			},
			{
				"Fungus1_28",
				MapBools.MapCliffs
			},
			{
				"Fungus1_28_b",
				MapBools.MapCliffs
			},
			{
				"Crossroads_01",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_02",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_03",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_04",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_05",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_06",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_07",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_08",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_09",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_10",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_11_alt",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_12",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_13",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_14",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_15",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_16",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_18",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_19",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_21",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_22",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_25",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_27",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_30",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_31",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_33",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_35",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_36",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_37",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_38",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_39",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_40",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_42",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_43",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_45",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_46",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_47",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_48",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_49",
				MapBools.MapCrossroads
			},
			{
				"Crossroads_52",
				MapBools.MapCrossroads
			},
			{
				"Mines_01",
				MapBools.MapMines
			},
			{
				"Mines_02",
				MapBools.MapMines
			},
			{
				"Mines_03",
				MapBools.MapMines
			},
			{
				"Mines_04",
				MapBools.MapMines
			},
			{
				"Mines_05",
				MapBools.MapMines
			},
			{
				"Mines_06",
				MapBools.MapMines
			},
			{
				"Mines_07",
				MapBools.MapMines
			},
			{
				"Mines_10",
				MapBools.MapMines
			},
			{
				"Mines_11",
				MapBools.MapMines
			},
			{
				"Mines_13",
				MapBools.MapMines
			},
			{
				"Mines_16",
				MapBools.MapMines
			},
			{
				"Mines_17",
				MapBools.MapMines
			},
			{
				"Mines_18",
				MapBools.MapMines
			},
			{
				"Mines_19",
				MapBools.MapMines
			},
			{
				"Mines_20",
				MapBools.MapMines
			},
			{
				"Mines_20_b",
				MapBools.MapMines
			},
			{
				"Mines_23",
				MapBools.MapMines
			},
			{
				"Mines_24",
				MapBools.MapMines
			},
			{
				"Mines_25",
				MapBools.MapMines
			},
			{
				"Mines_28",
				MapBools.MapMines
			},
			{
				"Mines_28_b",
				MapBools.MapMines
			},
			{
				"Mines_29",
				MapBools.MapMines
			},
			{
				"Mines_30",
				MapBools.MapMines
			},
			{
				"Mines_31",
				MapBools.MapMines
			},
			{
				"Mines_32",
				MapBools.MapMines
			},
			{
				"Mines_34",
				MapBools.MapMines
			},
			{
				"Mines_36",
				MapBools.MapMines
			},
			{
				"Mines_37",
				MapBools.MapMines
			},
			{
				"Abyss_03_b",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_01b",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_02",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_03",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_09",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_10",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_14",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_16",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_17",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_26",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_26b",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_30",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_30_b",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_31",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_32",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_33",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_34",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_35",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_36",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_37",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_38",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_39",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_40",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_41",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_41_b",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_42",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_44",
				MapBools.MapDeepnest
			},
			{
				"Deepnest_44_b",
				MapBools.MapDeepnest
			},
			{
				"Fungus2_25",
				MapBools.MapDeepnest
			},
			{
				"Room_Mask_maker",
				MapBools.MapDeepnest
			},
			{
				"Fungus3_01",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_02",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_03",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_24",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_25",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_25b",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_26",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_27",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_28",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_30",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_35",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_44",
				MapBools.MapFogCanyon
			},
			{
				"Fungus3_47",
				MapBools.MapFogCanyon
			},
			{
				"Deepnest_01",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_01",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_02",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_03",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_04",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_05",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_06",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_07",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_08",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_09",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_10",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_11",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_12",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_13",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_14",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_14_b",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_14_c",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_15",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_17",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_18",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_19",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_20",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_21",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_23",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_26",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_28",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_29",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_30",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_31",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_32",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_29_b",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_33",
				MapBools.MapFungalWastes
			},
			{
				"Fungus2_34",
				MapBools.MapFungalWastes
			},
			{
				"Fungus1_01",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_01b",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_02",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_03",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_04",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_05",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_06",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_07",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_08",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_09",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_09_b",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_10",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_11",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_12",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_13",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_14",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_14_b",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_15",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_16_alt",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_17",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_19",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_20_v02",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_21",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_22",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_25",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_26",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_29",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_30",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_31",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_32",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_34",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_37",
				MapBools.MapGreenpath
			},
			{
				"Fungus1_Slug",
				MapBools.MapGreenpath
			},
			{
				"Abyss_03_c",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_01",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_02",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_02b",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_03",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_04",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_06",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_07",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_08",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_09",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_09_b",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_10",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_11",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_12",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_13",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_14",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_15",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_16",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_18",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_Hornet",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_East_Hornet_b",
				MapBools.MapOutskirts
			},
			{
				"Hive_01",
				MapBools.MapOutskirts
			},
			{
				"Hive_02",
				MapBools.MapOutskirts
			},
			{
				"Hive_03",
				MapBools.MapOutskirts
			},
			{
				"Hive_03_b",
				MapBools.MapOutskirts
			},
			{
				"Hive_03_c",
				MapBools.MapOutskirts
			},
			{
				"Hive_04",
				MapBools.MapOutskirts
			},
			{
				"Hive_04_b",
				MapBools.MapOutskirts
			},
			{
				"Hive_05",
				MapBools.MapOutskirts
			},
			{
				"Deepnest_43",
				MapBools.MapRoyalGardens
			},
			{
				"Deepnest_43_b",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus1_23",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus1_24",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_04",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_05",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_08",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_10",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_11",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_13",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_21",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_22",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_22_b",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_23",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_23_b",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_34",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_39",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_40",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_48",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_48_bot",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_48_left",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_48_top",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_49",
				MapBools.MapRoyalGardens
			},
			{
				"Fungus3_50",
				MapBools.MapRoyalGardens
			},
			{
				"Crossroads_46b",
				MapBools.MapRestingGrounds
			},
			{
				"Crossroads_50",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_02",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_04",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_05",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_06",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_08",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_09",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_10_b",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_10_c",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_10_d",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_12",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_17",
				MapBools.MapRestingGrounds
			},
			{
				"Ruins2_10",
				MapBools.MapRestingGrounds
			},
			{
				"RestingGrounds_10",
				MapBools.MapRestingGrounds
			},
			{
				"Abyss_01",
				MapBools.MapWaterways
			},
			{
				"Abyss_02",
				MapBools.MapWaterways
			},
			{
				"Waterways_01",
				MapBools.MapWaterways
			},
			{
				"Waterways_02",
				MapBools.MapWaterways
			},
			{
				"Waterways_02b",
				MapBools.MapWaterways
			},
			{
				"Waterways_03",
				MapBools.MapWaterways
			},
			{
				"Waterways_04",
				MapBools.MapWaterways
			},
			{
				"Waterways_04_part_b",
				MapBools.MapWaterways
			},
			{
				"Waterways_04b",
				MapBools.MapWaterways
			},
			{
				"Waterways_05",
				MapBools.MapWaterways
			},
			{
				"Waterways_06",
				MapBools.MapWaterways
			},
			{
				"Waterways_07",
				MapBools.MapWaterways
			},
			{
				"Waterways_08",
				MapBools.MapWaterways
			},
			{
				"Waterways_09",
				MapBools.MapWaterways
			},
			{
				"Waterways_12",
				MapBools.MapWaterways
			},
			{
				"Waterways_13",
				MapBools.MapWaterways
			},
			{
				"Waterways_14",
				MapBools.MapWaterways
			},
			{
				"Waterways_15",
				MapBools.MapWaterways
			}
		};
	}

	private bool HasMapForScene(string sceneName)
	{
		if (mapZoneBools == null)
		{
			mapZoneBools = InitMapBools();
		}
		if (mapZoneBools.ContainsKey(sceneName))
		{
			return mapZoneBools[sceneName] switch
			{
				MapBools.MapDirtmouth => GetBool("mapDirtmouth"), 
				MapBools.MapCrossroads => GetBool("mapCrossroads"), 
				MapBools.MapGreenpath => GetBool("mapGreenpath"), 
				MapBools.MapFogCanyon => GetBool("mapFogCanyon"), 
				MapBools.MapRoyalGardens => GetBool("mapRoyalGardens"), 
				MapBools.MapFungalWastes => GetBool("mapFungalWastes"), 
				MapBools.MapCity => GetBool("mapCity"), 
				MapBools.MapWaterways => GetBool("mapWaterways"), 
				MapBools.MapMines => GetBool("mapMines"), 
				MapBools.MapDeepnest => GetBool("mapDeepnest"), 
				MapBools.MapCliffs => GetBool("mapCliffs"), 
				MapBools.MapOutskirts => GetBool("mapOutskirts"), 
				MapBools.MapRestingGrounds => GetBool("mapRestingGrounds"), 
				MapBools.MapAbyss => GetBool("mapAbyss"), 
				_ => false, 
			};
		}
		return true;
	}

	public void CountJournalEntries()
	{

	}

    public bool UpdateGameMap()
	{
		bool result = false;
		if (GetBool("hasQuill"))
		{
			foreach (string item in scenesVisited)
			{
				if (!scenesMapped.Contains(item) && HasMapForScene(item))
				{
					scenesMapped.Add(item);
					result = true;
				}
			}
		}
		return result;
	}

    public void ClearMP()
    {
        MPCharge = 0;
        MPReserve = 0;
    }

    public bool GetBool(string boolName)
    {
        return ModHooks.GetPlayerBool(boolName);
    }

    public float GetFloat(string floatName)
    {
        return ModHooks.GetPlayerFloat(floatName);
    }

    public int GetInt(string intName)
    {
        return ModHooks.GetPlayerInt(intName);
    }

    public string GetString(string stringName)
    {
        return ModHooks.GetPlayerString(stringName);
    }

    public T GetVariable<T>(string fieldName)
    {
        return ModHooks.GetPlayerVariable<T>(fieldName);
    }

    public Vector3 GetVector3(string vectorName)
    {
        return ModHooks.GetPlayerVector3(vectorName);
    }

    public void SetBool(string boolName, bool value)
    {
        ModHooks.SetPlayerBool(boolName, value);
    }

    public void SetFloat(string floatName, float value)
    {
        ModHooks.SetPlayerFloat(floatName, value);
    }

    public void SetInt(string intName, int value)
    {
        ModHooks.SetPlayerInt(intName, value);
    }

    public void SetString(string stringName, string value)
    {
        ModHooks.SetPlayerString(stringName, value);
    }

    public void SetVariable<T>(string fieldName, T value)
    {
        ModHooks.SetPlayerVariable(fieldName, value);
    }

    public void SetVector3(string vectorName, Vector3 value)
    {
        ModHooks.SetPlayerVector3(vectorName, value);
    }

    private void SetField(string fieldName, object value)
    {
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(this, value);
        }
    }

    private T GetField<T>(string fieldName)
    {
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            object value = field.GetValue(this);
            if (value is T castedVal)
            {
                return castedVal;
            }
        }
        return default;
    }

    public void SetBoolInternal(string boolName, bool value)
    {
        SetField(boolName, value);
    }

    public void IncrementInt(string intName)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) + 1);
        }
    }

    public void IntAdd(string intName, int amount)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) + amount);
        }

    }

    public void DecrementInt(string intName)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) - 1);
        }
    }

    public bool GetBoolInternal(string boolName)
    {
        return GetField<bool>(boolName);
    }

    public void SetIntInternal(string intName, int value)
    {
        SetField(intName, value);
    }

    public int GetIntInternal(string intName)
    {
        return GetField<int>(intName);
    }

    public void SetFloatInternal(string floatName, float value)
    {
        SetField(floatName, value);
    }

    public float GetFloatInternal(string floatName)
    {
        return GetField<float>(floatName);
    }

    public void SetStringInternal(string stringName, string value)
    {
        SetField(stringName, value);
    }

    public string GetStringInternal(string stringName)
    {
        return GetField<string>(stringName);
    }

    public void SetVector3Internal(string vector3Name, Vector3 value)
    {
        SetField(vector3Name, value);
    }

    public Vector3 GetVector3Internal(string vector3Name)
    {
        return GetField<Vector3>(vector3Name);
    }

    public void SetVariableInternal<T>(string variableName, T value)
    {
        SetField(variableName, value);
    }

    public T GetVariableInternal<T>(string variableName)
    {
        return GetField<T>(variableName);
    }

    internal void SetBoolSwappedArgs(bool value, string name)
    {
        SetBool(name, value);
    }

    internal void SetFloatSwappedArgs(float value, string name)
    {
        SetFloat(name, value);
    }

    internal void SetIntSwappedArgs(int value, string name)
    {
        SetInt(name, value);
    }

    internal void SetStringSwappedArgs(string value, string name)
    {
        SetString(name, value);
    }

    internal void SetVector3SwappedArgs(Vector3 value, string name)
    {
        SetVector3(name, value);
    }

    internal void SetVariableSwappedArgs<T0>(T0 value, string name)
    {
        SetVariable<T0>(name, value);
    }

    public void AddToMaxHealth(int amount)
	{
		SetIntSwappedArgs(GetInt("maxHealthBase") + amount, "maxHealthBase");
		if (!GetBool("equippedCharm_27"))
		{
			SetIntSwappedArgs(GetInt("maxHealth") + amount, "maxHealth");
		}
		SetIntSwappedArgs(GetInt("health"), "prevHealth");
		SetIntSwappedArgs(GetInt("maxHealth"), "health");
		if (GetInt("maxHealthBase") == GetInt("maxHealthCap"))
		{
			SetBoolSwappedArgs(true, "heartPieceMax");
		}
	}

    public void AddToMaxMPReserve(int amount)
    {
        SetInt("MPReserveMax", amount);
        if (GetInt("MPReserveMax") == GetInt("MPReserveCap"))
        {
            SetBool("vesselFragmentMax", true);
        }
    }

    public void AddHealth(int amount)
    {
        amount = ModHooks.BeforeAddHealth(amount);
        if (GetInt("health") + amount >= GetInt("maxHealth"))
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
        else
        {
            SetIntSwappedArgs(GetInt("health") + amount, "health");
        }
        if (GetInt("health") >= maxHealth)
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
    }

    public void MaxHealth()
    {
        SetIntSwappedArgs(GetInt("health"), "prevHealth");
        SetIntSwappedArgs(maxHealth, "health");
        SetIntSwappedArgs(4, "blockerHits");
    }

    public void TakeHealth(int amount)
    {
        if (amount > 0 && GetInt("health") == GetInt("maxHealth") && GetInt("health") != maxHealth)
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
        if (GetInt("healthBlue") > 0)
        {
            int num = amount - GetInt("healthBlue");
            SetBoolSwappedArgs(true, "damagedBlue");
            SetIntSwappedArgs(GetInt("healthBlue") - amount, "healthBlue");
            if (GetInt("healthBlue") < 0)
            {
                SetIntSwappedArgs(0, "healthBlue");
            }
            if (num > 0)
            {
                TakeHealth(num);
                return;
            }
        }
        else
        {
            SetBoolSwappedArgs(false, "damagedBlue");
            if (GetInt("health") - amount <= 0)
            {
                SetIntSwappedArgs(0, "health");
                return;
            }
            SetIntSwappedArgs(GetInt("health") - amount, "health");
        }
    }

    public void SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType)
    {
        SetStringSwappedArgs(spawnMarker.name, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetIntSwappedArgs(spawnType, "respawnType");
        SetBoolSwappedArgs(spawnMarker.respawnFacingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetBenchRespawn(string spawnMarker, string sceneName, bool facingRight)
    {
        SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetBoolSwappedArgs(facingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
    {
        SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetIntSwappedArgs(spawnType, "respawnType");
        SetBoolSwappedArgs(facingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetHazardRespawn(HazardRespawnMarker location)
    {
        SetVector3SwappedArgs(location.transform.position, "hazardRespawnLocation");
        SetBoolSwappedArgs(location.respawnFacingRight, "hazardRespawnFacingRight");
    }

    public void SetHazardRespawn(Vector3 position, bool facingRight)
    {
        SetVector3SwappedArgs(position, "hazardRespawnLocation");
        SetBoolSwappedArgs(facingRight, "hazardRespawnFacingRight");
    }
}


