using UnityEngine;

/// <summary>
/// Controla la zona de iluminación que indica al usuario dónde interactuar
/// Puede pulsar y cambiar de color según la cercanía
/// </summary>

public class HighlightZone : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensityMin = 1f;
    [SerializeField] private float pulseIntensityMax = 3f;
    [SerializeField] private bool enablePulse = true;

    [Header("Partículas (Opcional)")]
    [SerializeField] private ParticleSystem highlightParticles;

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private float pulseTime = 0f;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Asegurar que el material soporte emisión
        if (rend != null && rend.material != null)
        {
            rend.material.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (enablePulse && rend != null)
        {
            PulseEmission();
        }
    }

    private void PulseEmission()
    {
        pulseTime += Time.deltaTime * pulseSpeed;
        float intensity = Mathf.Lerp(pulseIntensityMin, pulseIntensityMax,
            (Mathf.Sin(pulseTime) + 1f) * 0.5f);

        rend.GetPropertyBlock(propBlock);
        Color emissionColor = rend.material.GetColor(EmissionColorID);
        propBlock.SetColor(EmissionColorID, emissionColor * intensity);
        rend.SetPropertyBlock(propBlock);
    }

    public void SetEmissionColor(Color color, float intensity)
    {
        if (rend == null) return;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColorID, color * intensity);
        rend.SetPropertyBlock(propBlock);
    }

    public void ActivateParticles(bool active)
    {
        if (highlightParticles != null)
        {
            if (active)
                highlightParticles.Play();
            else
                highlightParticles.Stop();
        }
    }

    private void OnEnable()
    {
        pulseTime = 0f;
        ActivateParticles(true);
    }

    private void OnDisable()
    {
        ActivateParticles(false);
    }
}
