using UnityEngine;

public class BossStatueLoadManager : MonoBehaviour
{
    private static BossScene currentBossScene;

    private static BossScene previousBossScene;

    private static int activeCount;

    public static bool ShouldUnload
    {
        get
        {
            if (currentBossScene == null || previousBossScene == null || activeCount <= 0)
            {
                return false;
            }
            return currentBossScene != previousBossScene;
        }
    }

    private void OnEnable()
    {
        activeCount++;
        TransitionPoint component = GetComponent<TransitionPoint>();
        if ((bool)component)
        {
            component.OnBeforeTransition += Clear;
        }
    }

    private void OnDisable()
    {
        TransitionPoint component = GetComponent<TransitionPoint>();
        if ((bool)component)
        {
            component.OnBeforeTransition -= Clear;
        }
        activeCount--;
    }

    public static void Clear()
    {
        currentBossScene = null;
        previousBossScene = null;
    }

    public static void RecordBossScene(BossScene bossScene)
    {
        previousBossScene = currentBossScene;
        currentBossScene = bossScene;
        Debug.Log(string.Format("Recorded statue boss scene. Current: {0}, Previous: {1}", currentBossScene ? currentBossScene.name : "null", previousBossScene ? previousBossScene.name : "null"));
    }
}
