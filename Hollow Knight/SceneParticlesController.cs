using GlobalEnums;
using UnityEngine;

public class SceneParticlesController : MonoBehaviour
{
    public SceneParticles defaultParticles;

    public SceneParticles[] sceneParticles;

    private GameManager gm;

    private SceneManager sm;

    private GameCameras gc;

    private bool foundMatch;

    private MapZone sceneParticleZoneType;

    public void SceneInit()
    {
        BeginScene();
    }

    public void EnableParticles()
    {
        foundMatch = false;
        if (sm.overrideParticlesWith == MapZone.NONE)
        {
            sceneParticleZoneType = sm.mapZone;
        }
        else
        {
            sceneParticleZoneType = sm.overrideParticlesWith;
        }
        for (int i = 0; i < sceneParticles.Length; i++)
        {
            if (sceneParticles[i].mapZone == sceneParticleZoneType)
            {
                if (sceneParticles[i].particleObject != null)
                {
                    foundMatch = true;
                    sceneParticles[i].particleObject.gameObject.SetActive(value: true);
                }
                else
                {
                    Debug.LogError("Trying to enable Particle Object for MapZone: " + sceneParticleZoneType.ToString() + " but Particle Object is not set.");
                }
            }
            else if (sceneParticles[i].particleObject != null)
            {
                sceneParticles[i].particleObject.gameObject.SetActive(value: false);
            }
            else
            {
                Debug.LogError("Trying to disable Particle Object for MapZone: " + sceneParticleZoneType.ToString() + " but Particle Object is not set.");
            }
        }
        if (!foundMatch)
        {
            if (defaultParticles.particleObject != null)
            {
                defaultParticles.particleObject.gameObject.SetActive(value: true);
            }
            else
            {
                Debug.LogError("Trying to enable Default Particle Object but Default Particle Object is not set.");
            }
        }
        else if (defaultParticles.particleObject != null)
        {
            defaultParticles.particleObject.gameObject.SetActive(value: false);
        }
        else
        {
            Debug.LogError("Trying to disable Default Particle Object but Default Particle Object is not set.");
        }
    }

    public void DisableParticles()
    {
        for (int i = 0; i < sceneParticles.Length; i++)
        {
            if (sceneParticles[i].particleObject != null)
            {
                sceneParticles[i].particleObject.gameObject.SetActive(value: false);
            }
            else
            {
                Debug.LogError("Trying to disable Particle Object for MapZone: " + sceneParticleZoneType.ToString() + " but Particle Object is not set.");
            }
        }
        if (defaultParticles.particleObject != null)
        {
            defaultParticles.particleObject.gameObject.SetActive(value: false);
        }
        else
        {
            Debug.LogError("Trying to disable Default Particle Object but Default Particle Object is not set.");
        }
    }

    private void BeginScene()
    {
        gm = GameManager.instance;
        sm = gm.sm;
        if (sm == null)
        {
            sm = Object.FindObjectOfType<SceneManager>();
        }
        gc = GameCameras.instance;
        if (gm.IsGameplayScene() && !gm.IsCinematicScene())
        {
            if (!sm.noParticles)
            {
                EnableParticles();
            }
            else
            {
                DisableParticles();
            }
        }
        else
        {
            DisableParticles();
        }
    }
}
