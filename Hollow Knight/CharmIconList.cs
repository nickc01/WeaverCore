using UnityEngine;

public class CharmIconList : MonoBehaviour
{
    public static CharmIconList Instance;

    public Sprite[] spriteList;

    public Sprite unbreakableHeart;

    public Sprite unbreakableGreed;

    public Sprite unbreakableStrength;

    public Sprite grimmchildLevel1;

    public Sprite grimmchildLevel2;

    public Sprite grimmchildLevel3;

    public Sprite grimmchildLevel4;

    public Sprite nymmCharm;

    private PlayerData playerData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerData = PlayerData.instance;
    }

    public Sprite GetSprite(int id)
    {
        playerData = PlayerData.instance;
        switch (id)
        {
            case 23:
                if (playerData.GetBool("fragileHealth_unbreakable"))
                {
                    return unbreakableHeart;
                }
                break;
            case 24:
                if (playerData.GetBool("fragileGreed_unbreakable"))
                {
                    return unbreakableGreed;
                }
                break;
            case 25:
                if (playerData.GetBool("fragileStrength_unbreakable"))
                {
                    return unbreakableStrength;
                }
                break;
            case 40:
                if (playerData.GetInt("grimmChildLevel") == 1)
                {
                    return grimmchildLevel1;
                }
                if (playerData.GetInt("grimmChildLevel") == 2)
                {
                    return grimmchildLevel2;
                }
                if (playerData.GetInt("grimmChildLevel") == 3)
                {
                    return grimmchildLevel3;
                }
                if (playerData.GetInt("grimmChildLevel") == 4)
                {
                    return grimmchildLevel4;
                }
                if (playerData.GetInt("grimmChildLevel") == 5)
                {
                    return nymmCharm;
                }
                break;
        }
        return spriteList[id];
    }
}
