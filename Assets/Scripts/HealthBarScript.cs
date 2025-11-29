using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Character character;
    public Image fillImg;
    private void LateUpdate()
    {
        fillImg.fillAmount = (float)(character.GetCurrentHealth() / character.maxHealth);
    }
}
