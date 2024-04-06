using UnityEngine;

public class EnviroRegion : MonoBehaviour
{
    public int environmentType;

    private GameManager gm;

    private PlayerData pd;

    private HeroController heroCtrl;

    private void Start()
    {
        gm = GameManager.instance;
        pd = PlayerData.instance;
        heroCtrl = HeroController.instance;
    }

    private void OnTriggerEnter2D()
    {
        pd.SetInt("environmentType", environmentType);
        heroCtrl.checkEnvironment();
    }

    private void OnTriggerExit2D()
    {
        pd.SetInt("environmentType", pd.GetInt("environmentTypeDefault"));
        heroCtrl.checkEnvironment();
    }
}
