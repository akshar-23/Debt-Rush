using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Character character;
    public Image fillImg;

    [Header("Scaling Settings")]
    public RectTransform healthBarContainer;

    private float baseHealth = 100f;
    private float previousMaxHealth;
    
    private float initialWidth; 

    private void Start()
    {
        if (healthBarContainer == null)
            healthBarContainer = GetComponent<RectTransform>();

        initialWidth = healthBarContainer.sizeDelta.x;
        previousMaxHealth = character.maxHealth;

        UpdateHealthBarWidth();
    }

    private void LateUpdate()
    {
        if (character == null) return;

        // Update the visual fill
        fillImg.fillAmount = character.GetCurrentHealth() / character.maxHealth;

        if (character.maxHealth != previousMaxHealth)
        {
            UpdateHealthBarWidth();
        }
    }

    private void UpdateHealthBarWidth()
    {
        if (character == null || healthBarContainer == null) return;

        float scaleRatio = character.maxHealth / baseHealth;
        
        float newWidth = initialWidth * scaleRatio;

        healthBarContainer.sizeDelta = new Vector2(newWidth, healthBarContainer.sizeDelta.y);

        previousMaxHealth = character.maxHealth;
    }
}