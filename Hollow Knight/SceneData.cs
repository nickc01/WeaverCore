using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneData
{
    [SerializeField]
    public List<GeoRockData> geoRocks;

    [SerializeField]
    public List<PersistentBoolData> persistentBoolItems;

    [SerializeField]
    public List<PersistentIntData> persistentIntItems;

    private static SceneData _instance;

    public static SceneData instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SceneData();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    protected SceneData()
    {
        SetupNewSceneData();
    }

    public void Reset()
    {
        SetupNewSceneData();
    }

    public void SaveMyState(GeoRockData geoRockData)
    {
        int num = FindGeoRockInList(geoRockData);
        if (num == -1)
        {
            geoRocks.Add(geoRockData);
        }
        else
        {
            geoRocks[num] = geoRockData;
        }
    }

    public void SaveMyState(PersistentBoolData persistentBoolData)
    {
        int num = FindPersistentBoolItemInList(persistentBoolData);
        if (num == -1)
        {
            persistentBoolItems.Add(persistentBoolData);
        }
        else
        {
            persistentBoolItems[num] = persistentBoolData;
        }
    }

    public void SaveMyState(PersistentIntData persistentIntData)
    {
        int num = FindPersistentIntItemInList(persistentIntData);
        if (num == -1)
        {
            persistentIntItems.Add(persistentIntData);
        }
        else
        {
            persistentIntItems[num] = persistentIntData;
        }
    }

    public void ResetSemiPersistentItems()
    {
        for (int i = 0; i < persistentBoolItems.Count; i++)
        {
            if (persistentBoolItems[i].semiPersistent)
            {
                persistentBoolItems[i].activated = false;
            }
        }
        for (int j = 0; j < persistentIntItems.Count; j++)
        {
            if (persistentIntItems[j].semiPersistent)
            {
                persistentIntItems[j].value = -1;
            }
        }
    }

    public GeoRockData FindMyState(GeoRockData grd)
    {
        int num = FindGeoRockInList(grd);
        if (num == -1)
        {
            return null;
        }
        return geoRocks[num];
    }

    public PersistentBoolData FindMyState(PersistentBoolData persistentBoolData)
    {
        int num = FindPersistentBoolItemInList(persistentBoolData);
        if (num == -1)
        {
            return null;
        }
        return persistentBoolItems[num];
    }

    public PersistentIntData FindMyState(PersistentIntData persistentIntData)
    {
        int num = FindPersistentIntItemInList(persistentIntData);
        if (num == -1)
        {
            return null;
        }
        return persistentIntItems[num];
    }

    private void SetupNewSceneData()
    {
        geoRocks = new List<GeoRockData>();
        persistentBoolItems = new List<PersistentBoolData>();
        persistentIntItems = new List<PersistentIntData>();
    }

    private int FindGeoRockInList(GeoRockData grd)
    {
        for (int i = 0; i < geoRocks.Count; i++)
        {
            if (string.Compare(geoRocks[i].sceneName, grd.sceneName, ignoreCase: true) == 0 && geoRocks[i].id == grd.id)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindPersistentBoolItemInList(PersistentBoolData pbd)
    {
        for (int i = 0; i < persistentBoolItems.Count; i++)
        {
            if (string.Compare(persistentBoolItems[i].sceneName, pbd.sceneName, ignoreCase: true) == 0 && persistentBoolItems[i].id == pbd.id)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindPersistentIntItemInList(PersistentIntData pid)
    {
        for (int i = 0; i < persistentIntItems.Count; i++)
        {
            if (string.Compare(persistentIntItems[i].sceneName, pid.sceneName, ignoreCase: true) == 0 && persistentIntItems[i].id == pid.id)
            {
                return i;
            }
        }
        return -1;
    }
}
