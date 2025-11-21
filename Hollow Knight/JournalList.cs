using UnityEngine;

public class JournalList : MonoBehaviour
{
    public GameObject[] list;

    public GameObject[] listInv;

    private GameObject[] currentList;

    private PlayerData pd;

    public float yDistance = -1.5f;

    private Vector3 selfPos;

    public int itemCount = -1;

    public int firstNewItem;

    public void BuildEnemyList()
    {
        pd = PlayerData.instance;
        firstNewItem = -1;
        listInv = new GameObject[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
            {
                itemCount++;
                GameObject gameObject = Object.Instantiate(list[i]);
                gameObject.transform.SetParent(base.transform, worldPositionStays: false);
                listInv[itemCount] = gameObject;
            }
            else
            {
                Debug.LogErrorFormat("JournalList cannot instantiate item {0} in enemyList as it is NULL", i);
            }
        }
    }

    public void UpdateEnemyList()
    {
        firstNewItem = -1;
        itemCount = -1;
        float num = 0f;
        currentList = new GameObject[listInv.Length];
        for (int i = 0; i < listInv.Length; i++)
        {
            GameObject gameObject = listInv[i];
            JournalEntryStats component = gameObject.GetComponent<JournalEntryStats>();
            if (pd.GetBool(listInv[i].GetComponent<JournalEntryStats>().GetPlayerDataBoolName()) || pd.GetBool("fillJournal"))
            {
                itemCount++;
                gameObject.SetActive(value: true);
                gameObject.transform.localPosition = new Vector3(0f, num, 0f);
                component.itemNumber = itemCount;
                currentList[itemCount] = gameObject;
                num += yDistance;
                if (pd.GetBool(component.GetPlayerDataNewDataName()) && firstNewItem == -1)
                {
                    firstNewItem = itemCount;
                }
            }
            else
            {
                gameObject.SetActive(value: false);
                gameObject.GetComponent<JournalEntryStats>().itemNumber = -10;
            }
        }
    }

    public int GetItemCount()
    {
        return itemCount;
    }

    public string GetNameConvo(int itemNum)
    {
        string nameConvo = currentList[itemNum].GetComponent<JournalEntryStats>().GetNameConvo();
        if (nameConvo != null)
        {
            return nameConvo;
        }
        return "";
    }

    public string GetDescConvo(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetDescConvo();
    }

    public bool GetWarriorGhost(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetWarriorGhost();
    }

    public bool GetGrimm(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetGrimm();
    }

    public string GetNotesConvo(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetNotesConvo();
    }

    public string GetPlayerDataBoolName(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetPlayerDataBoolName();
    }

    public string GetPlayerDataKillsName(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetPlayerDataKillsName();
    }

    public string GetPlayerDataNewDataName(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetPlayerDataNewDataName();
    }

    public Sprite GetSprite(int itemNum)
    {
        return currentList[itemNum].GetComponent<JournalEntryStats>().GetSprite();
    }

    public float GetYDistance()
    {
        return yDistance;
    }

    public int GetFirstNewItem()
    {
        return firstNewItem;
    }
}
