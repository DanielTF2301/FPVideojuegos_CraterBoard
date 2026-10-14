using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueAmount = 1f;
    [SerializeField] private float bootsSpeed = 35f;
    [SerializeField] private ParticleSystem snowEffect;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float maxAngularVelocity = 500f;
    
    SurfaceEffector2D surfaceEffector2D;
    Rigidbody2D rb;

    float baseSpeed;    
    float baseGravityScale; 
    InputAction moveAction;
    Vector2 moveInput;
    float previousRotation; 
    float totalRotation; 
    int flipCount; 

    int activePowerUpsCount; 
    
    // Variables de power-ups y puntuación acumulada real
    bool isScoreMultiplierActive = false;
    float currentScoreMultiplier = 1f;
    bool isShieldActive = false;
    public bool IsShieldActive { get => isShieldActive; }

    int totalScore = 0; // Acumulador fijo para que la puntuación nunca baje

    private bool canControlPlayer = true; 
    public bool CanControlPlayer { get => canControlPlayer; set => canControlPlayer = value; }

    void Start()
    {
        Transform characterContainer = transform.GetChild(0);
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        
        foreach (Transform child in characterContainer)
        {
            child.gameObject.SetActive(false);
        }

        if (selectedCharacter < characterContainer.childCount)
        {
            characterContainer.GetChild(selectedCharacter).gameObject.SetActive(true);
        }
        else if (characterContainer.childCount > 0)
        {
            characterContainer.GetChild(0).gameObject.SetActive(true);
        }

        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
        
        baseGravityScale = rb.gravityScale; 

        surfaceEffector2D = FindAnyObjectByType<SurfaceEffector2D>();
        if (surfaceEffector2D != null)
        {
            baseSpeed = surfaceEffector2D.speed;
        }

        previousRotation = transform.rotation.eulerAngles.z;
    }

    void Update()
    {
        if (!canControlPlayer) return;
        
        PlayerTorque();    
        BoostPlayer();
        CalculateFlips();        
    }

    private void CalculateFlips()
    {
        float currentRotation = transform.rotation.eulerAngles.z; 
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation); 
        
        if (Math.Abs(totalRotation) > 340)
        {
            flipCount++; 
            if (scoreManager != null)
            {
                // Puntos base por cada voltereta completada
                int baseFlipPoints = 100;
                
                // Si el multiplicador está activo, duplicamos los puntos de esta voltereta
                if (isScoreMultiplierActive)
                {
                    baseFlipPoints = Mathf.RoundToInt(baseFlipPoints * currentScoreMultiplier); 
                }

                // Acumulamos de forma segura sumando a lo que ya teníamos
                totalScore += baseFlipPoints;

                // Enviamos la puntuación total acumulada al ScoreManager
                scoreManager.AddScore(totalScore); 
            }
            totalRotation = 0;            
        }

        previousRotation = currentRotation; 
    }

    void PlayerTorque()
    {
        if (rb == null) return; 
        
        moveInput = moveAction.ReadValue<Vector2>(); 
        if (moveInput.x < 0)
        {
            rb.AddTorque(torqueAmount);
        }
        else if (moveInput.x > 0)
        {
            rb.AddTorque(-torqueAmount);
        }

        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
    }

    void BoostPlayer()
    {
        if (surfaceEffector2D == null) return;

        if (moveInput.y > 0)
        {    
            surfaceEffector2D.speed = bootsSpeed; 
        }
        else
        {        
            surfaceEffector2D.speed = baseSpeed; 
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {            
            if (snowEffect != null) snowEffect.Play();            
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {            
            if (snowEffect != null) snowEffect.Stop();
        }
    }

    public void ApplyPowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUpsCount++;

        if (powerUpData.PowerUpType == "Speed")
        {
            baseSpeed += powerUpData.PowerUpValue;
            bootsSpeed += powerUpData.PowerUpValue;
        }
        else if (powerUpData.PowerUpType == "Gravity")
        {
            rb.gravityScale -= powerUpData.PowerUpValue; 
        }
        else if (powerUpData.PowerUpType == "ScoreMultiplier")
        {
            isScoreMultiplierActive = true;
            currentScoreMultiplier = powerUpData.PowerUpValue; 
        }
        else if (powerUpData.PowerUpType == "Shield")
        {
            isShieldActive = true;
        }
    }

    public void DeactivatePowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUpsCount--;

        if (powerUpData.PowerUpType == "Speed")
        {
            baseSpeed -= powerUpData.PowerUpValue;
            bootsSpeed -= powerUpData.PowerUpValue;
        }
        else if (powerUpData.PowerUpType == "Gravity")
        {
            rb.gravityScale = baseGravityScale; 
        }
        else if (powerUpData.PowerUpType == "ScoreMultiplier")
        {
            isScoreMultiplierActive = false;
            currentScoreMultiplier = 1f; 
        }
        else if (powerUpData.PowerUpType == "Shield")
        {
            isShieldActive = false;
        }
    }
}