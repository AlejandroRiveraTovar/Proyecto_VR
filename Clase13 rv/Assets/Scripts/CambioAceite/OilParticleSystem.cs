using UnityEngine;

/// <summary>
/// Sistema de partículas para simular aceite cayendo
/// Se activa cuando la botella está inclinada correctamente
/// </summary>
public class OilParticleSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform oilBottle;
    [SerializeField] private Transform spoutPosition; // Pico de la botella
    [SerializeField] private ParticleSystem particleSystem;

    [Header("Configuración de Detección")]
    [SerializeField] private float tiltAngleThreshold = 45f; // Ángulo para activar
    [SerializeField] private Transform targetLocation; // Donde debe caer el aceite
    [SerializeField] private float maxDistance = 0.3f; // Distancia máxima para verter

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pouringSound;
    [SerializeField] private float volumeFadeSpeed = 2f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject splashEffect; // Efecto de salpicadura
    [SerializeField] private Transform splashSpawnPoint; // Donde aparece splash

    private ParticleSystem.EmissionModule emission;
    private bool isPouring = false;
    private float currentVolume = 0f;
    private float targetVolume = 0f;

    private void Start()
    {
        if (particleSystem != null)
        {
            emission = particleSystem.emission;
            emission.enabled = false;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = 0f;

        if (splashEffect != null)
        {
            splashEffect.SetActive(false);
        }
    }

    private void Update()
    {
        CheckPouringConditions();
        UpdateAudioVolume();
        UpdateParticlePosition();
    }

    private void CheckPouringConditions()
    {
        if (oilBottle == null) return;

       

        // Verificar distancia al objetivo
        bool isNearTarget = true;
        if (targetLocation != null && spoutPosition != null)
        {
            float distance = Vector3.Distance(spoutPosition.position, targetLocation.position);
            isNearTarget = distance <= maxDistance;
        }

        // Activar/desactivar vertido
        bool shouldPour = isNearTarget;

        if (shouldPour && !isPouring)
        {
            StartPouring();
        }
        else if (!shouldPour && isPouring)
        {
            StopPouring();
        }
    }

    private void StartPouring()
    {
        isPouring = true;

        // Activar partículas
        if (particleSystem != null)
        {
            emission.enabled = true;
            particleSystem.Play();
        }

        // Activar splash
        if (splashEffect != null)
        {
            splashEffect.SetActive(true);
        }

        // Activar audio
        if (audioSource != null && pouringSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = pouringSound;
                audioSource.Play();
            }
            targetVolume = 1f;
        }

        Debug.Log("[OilParticles] Vertido iniciado");
    }

    private void StopPouring()
    {
        isPouring = false;

        // Desactivar partículas
        if (particleSystem != null)
        {
            emission.enabled = false;
            particleSystem.Stop();
        }

        // Desactivar splash
        if (splashEffect != null)
        {
            splashEffect.SetActive(false);
        }

        // Fade out audio
        targetVolume = 0f;

        Debug.Log("[OilParticles] Vertido detenido");
    }

    private void UpdateAudioVolume()
    {
        if (audioSource == null) return;

        // Fade suave del volumen
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * volumeFadeSpeed);
        audioSource.volume = currentVolume;

        // Detener audio si volumen es muy bajo
        if (currentVolume < 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void UpdateParticlePosition()
    {
        // Actualizar posición de partículas al pico de la botella
        if (particleSystem != null && spoutPosition != null)
        {
            particleSystem.transform.position = spoutPosition.position;
            particleSystem.transform.rotation = spoutPosition.rotation;
        }

        // Actualizar posición de splash
        if (splashEffect != null && splashSpawnPoint != null)
        {
            splashEffect.transform.position = splashSpawnPoint.position;
        }
        else if (splashEffect != null && targetLocation != null)
        {
            splashEffect.transform.position = targetLocation.position;
        }
    }

    public bool IsPouring()
    {
        return isPouring;
    }

    public void ForceStop()
    {
        StopPouring();
    }

    private void OnDrawGizmos()
    {
        // Dibujar área de vertido
        if (targetLocation != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetLocation.position, maxDistance);
        }

        // Dibujar línea desde pico a objetivo
        if (spoutPosition != null && targetLocation != null)
        {
            Gizmos.color = isPouring ? Color.green : Color.red;
            Gizmos.DrawLine(spoutPosition.position, targetLocation.position);
        }
    }
}