using UnityEngine;
using TMPro;

public class DwarfStatsPanelController : MonoBehaviour
{
    public PlayerCharacterController dwarfController; // Vinculado en el inspector
    public TextMeshProUGUI maxHPText;
    public TextMeshProUGUI attackDmgText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI lifeStealText;
    public TextMeshProUGUI resFireText;
    public TextMeshProUGUI resIceText;
    public TextMeshProUGUI resElectricText;
    public TextMeshProUGUI resWaterText;
    public TextMeshProUGUI resNatureText;
    public TextMeshProUGUI resEarthText;
    public GameObject statsPanel;
    public Canvas panelCanvas;
    private Vector3 originalScale;

   
    void Start()
    {
        originalScale = statsPanel.transform.localScale;
        statsPanel.transform.localScale = Vector3.zero; // Iniciar oculto
        statsPanel.SetActive(false);   
    }
    public void ToggleStatsPanel()
    {
        if (!statsPanel.activeSelf)
        {
            ShowStatsPanel();
        }
        else
        {
            HideStatsPanel();
        }
    }
    private void ShowStatsPanel()
    {
        statsPanel.SetActive(true);
        UpdateStatsPanel();
        panelCanvas.overrideSorting = true;
        statsPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(statsPanel, originalScale, 0.3f).setEaseOutBack(); // Animación de crecimiento
    }

    private void HideStatsPanel()
    {
        LeanTween.scale(statsPanel, Vector3.zero, 0.2f).setEaseInBack().setOnComplete(() =>
        {
            statsPanel.SetActive(false);
        });
    }

    public void UpdateStatsPanel()
    {
        SetText(maxHPText, "Max Health: " + dwarfController.maxHealth, dwarfController.maxHealth);
        SetText(armorText, "Armor: " + dwarfController.armor, dwarfController.armor);
        SetText(resFireText, "Fire: " + dwarfController.resFire, dwarfController.resFire);
        SetText(resIceText, "Ice: " + dwarfController.resIce, dwarfController.resIce);
        SetText(resElectricText, "Electric: " + dwarfController.resElectric, dwarfController.resElectric);
        SetText(resWaterText, "Water: " + dwarfController.resWater, dwarfController.resWater);
        SetText(resNatureText, "Nature: " + dwarfController.resNature, dwarfController.resNature);
        SetText(resEarthText, "Earth: " + dwarfController.resEarth, dwarfController.resEarth);
    }

    private void SetText(TextMeshProUGUI textElement, string text, int value)
    {
        textElement.gameObject.SetActive(value > 0);
        if (value > 0)
        {
            textElement.text = text;
        }
    }
}
