using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Character character;
    private Image fillImg;

    void Start()
    {
        fillImg = GetComponentInChildren<Image>();
    }

    private void LateUpdate()
    {
        fillImg.fillAmount = (int)(character.currentHealth / character.maxHealth);
    }
}
