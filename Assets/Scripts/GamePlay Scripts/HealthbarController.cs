using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Referencia a la imagen de relleno

    // Método para actualizar la barra de vida
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;
        fillImage.fillAmount = Mathf.Clamp01(healthPercentage); // Actualiza el relleno
    }
}
