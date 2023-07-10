using System.Collections.Generic;
using UnityEngine;

public class TrackTriggerObjects : MonoBehaviour
{
    [SerializeField]
    private LayerMask ignoreLayers;

    private List<GameObject> insideGameObjects = new List<GameObject>();

    private int layerMask = -1;

    private bool gottenOverlappedColliders;

    private bool subscribed;

    private static readonly Collider2D[] _tempResults = new Collider2D[10];

    private static readonly List<GameObject> _refreshTemp = new List<GameObject>();

    public int InsideCount
    {
        get
        {
            int num = 0;
            foreach (GameObject insideGameObject in insideGameObjects)
            {
                if ((bool)insideGameObject)
                {
                    num++;
                }
            }
            return num;
        }
    }

    private void OnDisable()
    {
        insideGameObjects.Clear();
        gottenOverlappedColliders = false;
        if (subscribed)
        {
            HeroController silentInstance = HeroController.SilentInstance;
            if ((bool)silentInstance)
            {
                silentInstance.heroInPosition -= OnHeroInPosition;
            }
            subscribed = false;
        }
    }

    private void OnEnable()
    {
        if (layerMask < 0)
        {
            layerMask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics2D.GetIgnoreLayerCollision(gameObject.layer, i))
                {
                    layerMask |= 1 << i;
                }
            }
        }
        HeroController instance = HeroController.instance;
        if ((bool)instance && !instance.isHeroInPosition)
        {
            HeroController.instance.heroInPosition += OnHeroInPosition;
            subscribed = true;
        }
        else
        {
            GetOverlappedColliders();
        }
    }

    private void FixedUpdate()
    {
        for (int num = insideGameObjects.Count - 1; num >= 0; num--)
        {
            GameObject gameObject = insideGameObjects[num];
            if (!gameObject || !gameObject.activeInHierarchy)
            {
                insideGameObjects.RemoveAt(num);
            }
        }
    }

    private void OnHeroInPosition(bool forceDirect)
    {
        if (subscribed)
        {
            HeroController.instance.heroInPosition -= OnHeroInPosition;
            subscribed = false;
        }
        if (!this)
        {
            Debug.LogError("TrackTriggerObjects native Object was destroyed! This should not happen...", this);
        }
        else
        {
            GetOverlappedColliders();
        }
    }

    private void GetOverlappedColliders(bool isRefresh = false)
    {
        if (!base.enabled || !base.gameObject.activeInHierarchy || (gottenOverlappedColliders && !isRefresh))
        {
            return;
        }
        gottenOverlappedColliders = true;
        Collider2D[] components = GetComponents<Collider2D>();
        Collider2D[] array = components;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].OverlapCollider(new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = layerMask
            }, _tempResults) <= 0)
            {
                continue;
            }
            Collider2D[] tempResults = _tempResults;
            foreach (Collider2D collider2D in tempResults)
            {
                if ((bool)collider2D)
                {
                    OnTriggerEnter2D(collider2D);
                }
            }
        }
        for (int k = 0; k < _tempResults.Length; k++)
        {
            _tempResults[k] = null;
        }
        if (!isRefresh)
        {
            return;
        }
        _refreshTemp.AddRange(insideGameObjects);
        foreach (GameObject item in _refreshTemp)
        {
            bool flag = false;
            array = components;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].gameObject == item)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                OnExit(item);
            }
        }
        _refreshTemp.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gottenOverlappedColliders)
        {
            GameObject gameObject = collision.gameObject;
            if (!IsIgnored(gameObject) && !insideGameObjects.Contains(gameObject))
            {
                insideGameObjects.Add(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        OnExit(obj);
    }

    private void OnExit(GameObject obj)
    {
        insideGameObjects.Remove(obj);
    }

    private bool IsIgnored(GameObject obj)
    {
        int layer = obj.layer;
        int num = 1 << layer;
        return (ignoreLayers.value & num) == num;
    }
}
