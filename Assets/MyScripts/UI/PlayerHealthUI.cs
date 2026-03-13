using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Slider healthSlider;

    // public TextMeshProUGUI healthText;

    void Start()
    {
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (playerHealth == null)
            return;

        int current = playerHealth.currentHealth;
        int max = playerHealth.maxHealth;

        healthSlider.maxValue = max;
        healthSlider.value = current;

        // healthText.text = current + " / " + max;
    }
}