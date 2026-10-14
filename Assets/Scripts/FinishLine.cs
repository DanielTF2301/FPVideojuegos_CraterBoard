using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private float reloadDelay = 1f; // Delay before reloading the scene
    [SerializeField] private ParticleSystem finishEffect; // Particle effect to play when the player crosses the finish line

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has crossed the finish line!");
            if (finishEffect != null) finishEffect.Play(); // Play the finish effect particles            
            
            Invoke(nameof(NextLevel), reloadDelay); // Reload the scene after the specified delay
        }
    }

    void NextLevel()
    {
        // 1. Averiguar en qué nivel estamos basándonos en el nombre de la escena actual (ej. "Level1", "Level2")
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (currentSceneName.StartsWith("Level") && int.TryParse(currentSceneName.Replace("Level", ""), out int currentLevelNumber))
        {
            int nextLevelNumber = currentLevelNumber + 1;
            
            // 2. Actualizar el progreso desbloqueado solo si superamos nuestro récord
            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (currentLevelNumber >= unlockedLevel)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevelNumber);
                PlayerPrefs.Save();
            }
            
            // 3. Cargar el siguiente nivel de forma segura (asumiendo un total de 4 niveles)
            if (nextLevelNumber > 3) 
            {
                SceneManager.LoadScene("Menu"); // TODO: Go to Win Scene
            }
            else
            {
                SceneManager.LoadScene($"Level{nextLevelNumber}"); // Carga exactamente el siguiente nivel correlativo
            }
        }
        else
        {
            // Fallback de seguridad por si acaso
            SceneManager.LoadScene("Menu");
        }
    }
}