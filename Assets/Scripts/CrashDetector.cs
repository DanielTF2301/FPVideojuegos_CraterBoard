using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] private float crashDelay = 1f; // Delay before reloading the scene
    [SerializeField] private ParticleSystem crashEffect;

    PlayerController playerController;

    void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            // --- NUEVO: Si el escudo está activo, ignoramos el choque ---
            if (playerController != null && playerController.IsShieldActive)
            {
                return; // Salimos del método y evitamos el choque/muerte
            }

            playerController.CanControlPlayer = false; // Disable player input
            crashEffect.Play();
            Invoke(nameof(ReloadScene), crashDelay);
        }
    }

    void ReloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}