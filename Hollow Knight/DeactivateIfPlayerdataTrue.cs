using UnityEngine;

public class DeactivateIfPlayerdataTrue : MonoBehaviour
{
    public string boolName;

    private GameManager gm;

    private PlayerData pd;

    private void Start()
    {
        gm = GameManager.instance;
        pd = gm.playerData;
    }

    private void OnEnable()
    {
        if (gm == null)
        {
            gm = GameManager.instance;
        }
        if (pd == null)
        {
            pd = gm.playerData;
        }
        if (pd.GetBool(boolName))
        {
            base.gameObject.SetActive(value: false);
        }
    }
}
