using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaTitleController : MonoBehaviour
{
    private class Area
    {
        public string identifier = "AREA";

        public int areaID;

        public bool subArea;

        public string visitedBool = "";

        public Action<Area, AreaTitleController> evaluateDelegate;

        public Area(string identifier, int areaID, bool subArea, string visitedBool)
        {
            this.identifier = identifier;
            this.areaID = areaID;
            this.subArea = subArea;
            this.visitedBool = visitedBool;
        }

        public Area(string identifier, int areaID, bool subArea, string visitedBool, Action<Area, AreaTitleController> evaluateDelegate)
        {
            this.identifier = identifier;
            this.areaID = areaID;
            this.subArea = subArea;
            this.visitedBool = visitedBool;
            this.evaluateDelegate = evaluateDelegate;
        }
    }

    private List<Area> areaList = new List<Area>
    {
        new Area("ABYSS", 13, subArea: false, "visitedAbyss"),
        new Area("CROSSROADS", 2, subArea: false, "visitedCrossroads", delegate(Area self, AreaTitleController sender)
        {
            if (GameManager.instance.playerData.GetBool("crossroadsInfected"))
            {
                self.identifier = "CROSSROADS_INF";
                sender.areaEvent = "CROSSROADS_INF";
            }
        }),
        new Area("DEEPNEST", 9, subArea: false, "visitedDeepnest"),
        new Area("DIRTMOUTH", 1, subArea: false, "visitedDirtmouth"),
        new Area("EGGTEMPLE", 0, subArea: true, ""),
        new Area("FOG_CANYON", 4, subArea: false, "visitedFogCanyon"),
        new Area("FUNGUS", 5, subArea: false, "visitedFungus"),
        new Area("GREENPATH", 3, subArea: false, "visitedGreenpath"),
        new Area("HIVE", 11, subArea: false, "visitedHive", delegate(Area self, AreaTitleController sender)
        {
            sender.doFinish = false;
            if (GameManager.instance.playerData.GetBool("visitedHive"))
            {
                sender.StartCoroutine(sender.VisitPause(pause: false));
            }
            else
            {
                sender.StartCoroutine(sender.UnvisitPause(pause: false));
            }
        }),
        new Area("KINGSPASS", 0, subArea: true, "", delegate(Area self, AreaTitleController sender)
        {
            if (!GameManager.instance.playerData.GetBool("visitedCrossroads"))
            {
                sender.doFinish = false;
                sender.gameObject.SetActive(value: false);
            }
        }),
        new Area("MINES", 8, subArea: false, "visitedMines"),
        new Area("RESTING_GROUNDS", 12, subArea: false, "visitedRestingGrounds"),
        new Area("ROYAL_GARDENS", 10, subArea: false, "visitedRoyalGardens"),
        new Area("RUINS", 6, subArea: false, "visitedRuins"),
        new Area("SHAMANTEMPLE", 0, subArea: true, ""),
        new Area("WATERWAYS", 7, subArea: false, "visitedWaterways"),
        new Area("MANTIS_VILLAGE", 0, subArea: true, ""),
        new Area("FUNGUS_CORE", 0, subArea: true, ""),
        new Area("MAGE_TOWER", 0, subArea: true, ""),
        new Area("FUNGUS_SHAMAN", 0, subArea: true, ""),
        new Area("QUEENS_STATION", 0, subArea: true, ""),
        new Area("KINGS_STATION", 0, subArea: true, ""),
        new Area("BLUE_LAKE", 0, subArea: true, ""),
        new Area("ACID_LAKE", 0, subArea: true, ""),
        new Area("OUTSKIRTS", 14, subArea: false, "visitedOutskirts"),
        new Area("LOVE_TOWER", 0, subArea: true, ""),
        new Area("SPIDER_VILLAGE", 0, subArea: true, ""),
        new Area("HEGEMOL_NEST", 0, subArea: true, ""),
        new Area("WHITE_PALACE", 15, subArea: false, "visitedWhitePalace"),
        new Area("COLOSSEUM", 0, subArea: true, "seenColosseumTitle", delegate(Area self, AreaTitleController sender)
        {
            sender.doFinish = false;
            if (GameManager.instance.playerData.GetBool("seenColosseumTitle"))
            {
                sender.StartCoroutine(sender.VisitPause());
            }
            else
            {
                sender.StartCoroutine(sender.UnvisitPause());
            }
        }),
        new Area("ABYSS_DEEP", 16, subArea: false, "visitedAbyssLower"),
        new Area("CLIFFS", 17, subArea: false, "visitedCliffs"),
        new Area("GODHOME", 18, subArea: false, "visitedGodhome"),
        new Area("GODSEEKER_WASTE", 0, subArea: true, "")
    };

    public bool waitForHeroInPosition = true;

    //[Header("Values copied from FSM")]
    public string areaEvent = "";

    public bool displayRight;

    public string doorTrigger = "";

    public bool onlyOnRevisit;

    public float unvisitedPause = 2f;

    public float visitedPause = 2f;

    public bool waitForTrigger;

    [Space]
    public GameObject areaTitle;

    private Area area;

    private bool played;

    private bool doFinish = true;

    private HeroController hc;

    private HeroController.HeroInPosition heroInPositionResponder;

    private void Start()
    {
        /*PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(base.gameObject, "Area Title Controller");
        if ((bool)playMakerFSM)
        {
            areaEvent = FSMUtility.GetString(playMakerFSM, "Area Event");
            displayRight = FSMUtility.GetBool(playMakerFSM, "Display Right");
            doorTrigger = FSMUtility.GetString(playMakerFSM, "Door Trigger");
            onlyOnRevisit = FSMUtility.GetBool(playMakerFSM, "Only On Revisit");
            unvisitedPause = FSMUtility.GetFloat(playMakerFSM, "Unvisited Pause");
            visitedPause = FSMUtility.GetFloat(playMakerFSM, "Visited Pause");
            waitForTrigger = FSMUtility.GetBool(playMakerFSM, "Wait for Trigger");
        }
        else
        {
            Debug.LogError("No FSM attached to " + base.gameObject.name + " to get data from!");
        }*/
        if (waitForHeroInPosition)
        {
            hc = HeroController.instance;
            if (hc != null)
            {
                heroInPositionResponder = delegate
                {
                    FindAreaTitle();
                    DoPlay();
                    hc.heroInPosition -= heroInPositionResponder;
                    heroInPositionResponder = null;
                };
                hc.heroInPosition += heroInPositionResponder;
            }
        }
        else
        {
            FindAreaTitle();
            DoPlay();
        }
    }

    private void FindAreaTitle()
    {
        if ((bool)AreaTitle.instance)
        {
            areaTitle = AreaTitle.instance.gameObject;
        }
    }

    private void DoPlay()
    {
        if (!waitForTrigger)
        {
            Play();
        }
    }

    protected void OnDestroy()
    {
        if (hc != null && heroInPositionResponder != null)
        {
            hc.heroInPosition -= heroInPositionResponder;
            hc = null;
            heroInPositionResponder = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("DISPLAYING TITLE CONTROLLER");
        if (!played && collision.tag == "Player")
        {
            Play();
        }
    }

    private void Play()
    {
        if (!played)
        {
            played = true;
            if (doorTrigger == "")
            {
                CheckArea();
            }
            else if (HeroController.instance.GetEntryGateName() == doorTrigger)
            {
                CheckArea();
            }
            else
            {
                base.gameObject.SetActive(value: false);
            }
        }
    }

    private void CheckArea()
    {
        area = areaList.FirstOrDefault((Area o) => o.identifier == areaEvent);
        if (area != null)
        {
            if (area.evaluateDelegate != null)
            {
                area.evaluateDelegate(area, this);
            }
        }
        else
        {
            //Debug.LogWarning("No area with identifier \"" + areaEvent + "\" found in area list. Creating default SubArea.");
            area = new Area(areaEvent, 0, subArea: true, "");
        }
        if (doFinish)
        {
            Finish();
        }
    }

    private void Finish()
    {
        if (area.subArea)
        {
            StartCoroutine(VisitPause());
            return;
        }
        int @int = GameManager.instance.playerData.GetInt("currentArea");
        bool @bool = GameManager.instance.playerData.GetBool(area.visitedBool);
        bool flag = true;
        if ((!@bool && onlyOnRevisit) || area.areaID == @int)
        {
            flag = false;
            base.gameObject.SetActive(value: false);
        }
        else
        {
            GameManager.instance.playerData.SetInt("currentArea", area.areaID);
        }
        if (flag)
        {
            StartCoroutine(@bool ? VisitPause() : UnvisitPause());
        }
    }

    private IEnumerator VisitPause(bool pause = true)
    {
        if (pause)
        {
            yield return new WaitForSeconds(visitedPause);
        }
        GameManager.instance.StoryRecord_travelledToArea(area.identifier);
        if ((bool)areaTitle)
        {
            areaTitle.SetActive(value: true);

            /*PlayMakerFSM fSM = FSMUtility.GetFSM(areaTitle);
            if ((bool)fSM)
            {
                FSMUtility.SetBool(fSM, "Visited", value: true);
                FSMUtility.SetBool(fSM, "NPC Title", value: false);
                FSMUtility.SetBool(fSM, "Display Right", displayRight);
                FSMUtility.SetString(fSM, "Area Event", areaEvent);
            }*/
        }
    }

    private IEnumerator UnvisitPause(bool pause = true)
    {
        if (pause)
        {
            yield return new WaitForSeconds(unvisitedPause);
        }
        GameManager.instance.StoryRecord_discoveredArea(area.identifier);
        if ((bool)areaTitle)
        {
            areaTitle.SetActive(value: true);
            /*PlayMakerFSM fSM = FSMUtility.GetFSM(areaTitle);
            if ((bool)fSM)
            {
                FSMUtility.SetBool(fSM, "Visited", value: false);
                FSMUtility.SetBool(fSM, "NPC Title", value: false);
                FSMUtility.SetString(fSM, "Area Event", areaEvent);
                GameManager.instance.playerData.SetBool(area.visitedBool, value: true);
            }*/
        }
    }
}
