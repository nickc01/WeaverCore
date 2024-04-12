using UnityEngine;

public class ReduceParticleEffects : MonoBehaviour
{
    private GameManager gm;

    private ParticleSystem emitter;

    private float emissionRateHigh;

    private float emissionRateLow;

    private int maxParticlesHigh;

    private int maxParticlesLow;

    private bool init;

    private void Start()
    {
        gm = GameManager.instance;
        gm.RefreshParticleLevel += SetEmission;
        emitter = GetComponent<ParticleSystem>();
        emissionRateHigh = ((emitter != null) ? emitter.emissionRate : 1f);
        emissionRateLow = emissionRateHigh / 2f;
        maxParticlesHigh = ((emitter != null) ? emitter.maxParticles : 20);
        maxParticlesLow = maxParticlesHigh / 2;
        SetEmission();
        init = true;
    }

    private void SetEmission()
    {
        if (emitter != null)
        {
            emitter.emissionRate = emissionRateHigh;
            emitter.maxParticles = maxParticlesHigh;
            /*if (gm.gameSettings.particleEffectsLevel == 0)
            {
                emitter.emissionRate = emissionRateLow;
                emitter.maxParticles = maxParticlesLow;
            }
            else
            {
                emitter.emissionRate = emissionRateHigh;
                emitter.maxParticles = maxParticlesHigh;
            }*/
        }
    }

    private void OnEnable()
    {
        if (init)
        {
            gm.RefreshParticleLevel += SetEmission;
        }
    }

    private void OnDisable()
    {
        if (init)
        {
            gm.RefreshParticleLevel -= SetEmission;
        }
    }
}
