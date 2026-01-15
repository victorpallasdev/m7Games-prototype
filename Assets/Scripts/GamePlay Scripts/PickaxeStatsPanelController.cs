using UnityEngine;
using TMPro;
using System.Collections;

public class PickaxeStatsPanelController : MonoBehaviour
{
    private PlayerCharacterController dwarfController;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI physicalPowerText;
    public TextMeshProUGUI lifeStealText;
    public TextMeshProUGUI firePowerText;
    public TextMeshProUGUI icePowerText;
    public TextMeshProUGUI electricPowertext;
    public TextMeshProUGUI waterPowerText;
    public TextMeshProUGUI naturePowerText;
    public TextMeshProUGUI earthPowerText;
    public GameObject statsPanel;
    private Vector3 originalScale;

    IEnumerator Start()
    {
        dwarfController = PlayerCharacterController.Instance;
        yield return new WaitUntil(() => dwarfController != null);
        originalScale = statsPanel.transform.localScale;
        statsPanel.transform.localScale = Vector3.zero; // Iniciar oculto
        // statsPanel.SetActive(false);   
    }

    public void ShowPickaxeStatsPanel()
    {
        statsPanel.SetActive(true);
        UpdatePickaxeStatsPanel();
        statsPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(statsPanel, originalScale, 0.3f).setEaseOutBack(); // Animación de crecimiento
    }
    public void HidePickaxeStatsPanel()
    {
        LeanTween.scale(statsPanel, Vector3.zero, 0.2f).setEaseInBack().setOnComplete(() =>
        {
            statsPanel.SetActive(false);
        });
    }


    public void UpdatePickaxeStatsPanel()
    {
        // Operador ternario y además funcion Split para dividir el titulo en dos palabras que la separa un espacio, y cogemos la primera palabra solamente
        titleText.text = dwarfController.HaveWeapon ? dwarfController.WeaponTitle.Split(' ')[0] : "NO PICKAXE";

        if (!dwarfController.HaveWeapon)
        {
            physicalPowerText.gameObject.SetActive(false);
            lifeStealText.gameObject.SetActive(false);
            firePowerText.gameObject.SetActive(false);
            icePowerText.gameObject.SetActive(false);
            electricPowertext.gameObject.SetActive(false);
            waterPowerText.gameObject.SetActive(false);
            naturePowerText.gameObject.SetActive(false);
            earthPowerText.gameObject.SetActive(false);
            return;
        }

        SetText(physicalPowerText, "Physical: " + dwarfController.ModifiedPhysicalPower(), dwarfController.ModifiedPhysicalPower());
        SetText(lifeStealText, "Lifesteal: " + dwarfController.LifeSteal, dwarfController.LifeSteal);
        SetText(firePowerText, "Fire: " + dwarfController.weaponFirePower, dwarfController.weaponFirePower);
        SetText(icePowerText, "Ice: " + dwarfController.weaponIcePower, dwarfController.weaponIcePower);
        SetText(electricPowertext, "Electric: " + dwarfController.weaponElectricPower, dwarfController.weaponElectricPower);
        SetText(waterPowerText, "Water: " + dwarfController.weaponWaterPower, dwarfController.weaponWaterPower);
        SetText(naturePowerText, "Nature: " + dwarfController.weaponNaturePower, dwarfController.weaponNaturePower);
        SetText(earthPowerText, "Earth: " + dwarfController.weaponEarthPower, dwarfController.weaponEarthPower);
    }
    private void SetText(TextMeshProUGUI textElement, string text, int value)
    {
        textElement.gameObject.SetActive(value > 0);
        if (value > 0)
        {
            textElement.text = text;
        }
    }
    public bool IsPanelActive()
    {
        return statsPanel.activeSelf;
    }
}
