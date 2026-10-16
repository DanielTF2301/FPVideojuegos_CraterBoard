using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] private float crashDelay = 1f;
    [SerializeField] private ParticleSystem crashEffect;

    private PlayerController playerController;

    void Start()
    {
        // Como el CrashDetector y el PlayerController están en el mismo objeto (Player), usamos GetComponent directamente
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        if (playerController == null)
        {
            Debug.LogError("¡CrashDetector no encuentra ningún PlayerController en la escena!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            // Comprobamos el estado del escudo
            if (playerController != null)
            {
                Debug.Log("Estado actual del escudo: " + playerController.IsShieldActive);

                if (playerController.IsShieldActive)
                {
                    Debug.Log("¡El escudo está activo! El choque ha sido bloqueado.");
                    return; // Sale del método y evita la muerte
                }
            }
            else
            {
                Debug.LogWarning("PlayerController es null en el momento del impacto.");
            }

            // Si no hay escudo activo, procede a reiniciar
            if (playerController != null)
            {
                playerController.CanControlPlayer = false;
            }

            if (crashEffect != null) crashEffect.Play();
            Invoke(nameof(ReloadScene), crashDelay);
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}