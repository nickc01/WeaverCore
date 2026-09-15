using UnityEngine;

public class GlobalPrefabDefaults : MonoBehaviour
{
    public static GlobalPrefabDefaults Instance;

    public GameObject bloodSplatterParticle;

    public float speedMultiplier = 1.2f;

    public float amountMultiplier = 1.3f;

    private ParticleSystem.MinMaxGradient initialBloodColour;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if ((bool)bloodSplatterParticle)
        {
            ParticleSystem component = bloodSplatterParticle.GetComponent<ParticleSystem>();
            if ((bool)component)
            {
                initialBloodColour = component.main.startColor;
            }
        }
    }

    public void SpawnBlood(Vector3 position, short minCount, short maxCount, float minSpeed, float maxSpeed, float angleMin = 0f, float angleMax = 360f, Color? colorOverride = null)
    {
        if (!bloodSplatterParticle)
        {
            return;
        }
        ParticleSystem component = bloodSplatterParticle.Spawn().GetComponent<ParticleSystem>();
        if ((bool)component)
        {
            component.Stop();
            component.emission.SetBursts(new ParticleSystem.Burst[1]
            {
                new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt((float)minCount * amountMultiplier), (short)Mathf.RoundToInt((float)maxCount * amountMultiplier))
            });
            ParticleSystem.MainModule main = component.main;
            main.maxParticles = Mathf.RoundToInt((float)maxCount * amountMultiplier);
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed * speedMultiplier, maxSpeed * speedMultiplier);
            if (!colorOverride.HasValue)
            {
                main.startColor = initialBloodColour;
            }
            else
            {
                main.startColor = new ParticleSystem.MinMaxGradient(colorOverride.Value);
            }
            ParticleSystem.ShapeModule shape = component.shape;
            float num = (shape.arc = angleMax - angleMin);
            component.transform.SetRotation2D(angleMin);
            component.transform.position = position;
            component.Play();
        }
    }
}
