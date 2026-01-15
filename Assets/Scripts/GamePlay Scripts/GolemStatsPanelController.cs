using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class GolemStatsPanelController : MonoBehaviour
{
    public GolemController golemController; // Vinculado en el inspector
    public GameObject statsPanel;
    private Vector3 originalScale;

    public TextMeshProUGUI physicalText;
    public TextMeshProUGUI firePowerText;
    public TextMeshProUGUI icePowerText;
    public TextMeshProUGUI electricPowerText;
    public TextMeshProUGUI waterPowerText;
    public TextMeshProUGUI naturePowerText;
    public TextMeshProUGUI earthPowerText;
    public TextMeshProUGUI fireResText;
    public TextMeshProUGUI iceResText;
    public TextMeshProUGUI electricResText;
    public TextMeshProUGUI waterResText;
    public TextMeshProUGUI natureResText;
    public TextMeshProUGUI earthResText;
    public TextMeshProUGUI fireWeakText;
    public TextMeshProUGUI iceWeakText;
    public TextMeshProUGUI electricWeakText;
    public TextMeshProUGUI waterWeakText;
    public TextMeshProUGUI natureWeakText;
    public TextMeshProUGUI earthWeakText;

   
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
        SetText(physicalText, "Physical: " + golemController.golemPower, golemController.golemPower);
        SetText(firePowerText, "Fire: " + golemController.golemElementalPowers[Element.Fire], golemController.golemElementalPowers[Element.Fire]);
        SetText(icePowerText, "Ice: " + golemController.golemElementalPowers[Element.Ice], golemController.golemElementalPowers[Element.Ice]);
        SetText(electricPowerText, "Electric: " + golemController.golemElementalPowers[Element.Electric], golemController.golemElementalPowers[Element.Electric]);
        SetText(waterPowerText, "Water: " + golemController.golemElementalPowers[Element.Water], golemController.golemElementalPowers[Element.Water]);
        SetText(naturePowerText, "Nature: " + golemController.golemElementalPowers[Element.Nature], golemController.golemElementalPowers[Element.Nature]);
        SetText(earthPowerText, "Earth: " + golemController.golemElementalPowers[Element.Earth], golemController.golemElementalPowers[Element.Earth]);

        SetText(fireResText, "Fire: " + golemController.golemElementalResistances[Element.Fire], golemController.golemElementalResistances[Element.Fire]);
        SetText(iceResText, "Ice: " + golemController.golemElementalResistances[Element.Ice], golemController.golemElementalResistances[Element.Ice]);
        SetText(electricResText, "Electric: " + golemController.golemElementalResistances[Element.Electric], golemController.golemElementalResistances[Element.Electric]);
        SetText(waterResText, "Water: " + golemController.golemElementalResistances[Element.Water], golemController.golemElementalResistances[Element.Water]);
        SetText(natureResText, "Nature: " + golemController.golemElementalResistances[Element.Nature], golemController.golemElementalResistances[Element.Nature]);
        SetText(earthResText, "Earth: " + golemController.golemElementalResistances[Element.Earth], golemController.golemElementalResistances[Element.Earth]);

        SetText(fireWeakText, "Fire: " + golemController.golemElementalWeaknesses[Element.Fire], golemController.golemElementalWeaknesses[Element.Fire]);
        SetText(iceWeakText, "Ice: " + golemController.golemElementalWeaknesses[Element.Ice], golemController.golemElementalWeaknesses[Element.Ice]);
        SetText(electricWeakText, "Electric: " + golemController.golemElementalWeaknesses[Element.Electric], golemController.golemElementalWeaknesses[Element.Electric]);
        SetText(waterWeakText, "Water: " + golemController.golemElementalWeaknesses[Element.Water], golemController.golemElementalWeaknesses[Element.Water]);
        SetText(natureWeakText, "Nature: " + golemController.golemElementalWeaknesses[Element.Nature], golemController.golemElementalWeaknesses[Element.Nature]);
        SetText(earthWeakText, "Earth: " + golemController.golemElementalWeaknesses[Element.Earth], golemController.golemElementalWeaknesses[Element.Earth]);
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
