using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


public class GearTooltipManager : MonoBehaviour
{
    public static GearTooltipManager Instance;
    public GameObject tooltipPanel;
    public RectTransform canvasRectTransform;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI resistanceText;
    public TextMeshProUGUI effectText;
    public CameraController cameraController;
    public Camera mainCamera;
    private float cameraZoomFactor;

    private void Awake()
    {
        Instance = this;
        this.enabled = false; // Desactiva el script al inicio
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // Si el script está activo, significa que el GameObejct también, se ejecuta el Update para que siga al ratón y cambie la escala del canvas segun si hay Zoom de Cámara.
        cameraZoomFactor = 1 / cameraController.cameraZoomFactor(); // Le hacemos la inversa al zoomFactor para que sea más sencillo.
        tooltipPanel.transform.localScale = new Vector3(cameraZoomFactor * EntityInitializer.newScale.x, cameraZoomFactor * EntityInitializer.newScale.x, 1);
        Vector3 screenMousePos = Pointer.current.position.ReadValue();
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 0));
        tooltipPanel.transform.position = new Vector3(mousePosition.x + 0.5f * cameraZoomFactor, mousePosition.y - 0.3f * cameraZoomFactor, 0);
    }
    public void Show(GearData gearData)
    {
        titleText.transform.GetComponent<LocalizedText>().key = gearData.gearName;
        armorText.text = LocalizationManager.Instance.GetText("armor") + ": " + gearData.armor.ToString();
        // Si tiene alguna resistencia se muestra el texto de Resistances
        resistanceText.text = LocalizationManager.Instance.GetText("resistances") + ": ";
        if (gearData.elementalResistances.Count > 0)
        {
            for (int i = 0; i < gearData.elementalResistances.Count; i++)
            {
                if (gearData.elementalResistances[i].value > 0)
                {
                    Element element = gearData.elementalResistances[i].element;
                    int resistancePower = gearData.elementalResistances[i].value;
                    // Obtenemos el Color del elemento segun nuestro mapeado
                    Color elementColor = FloatingText.ElementColorMap[element];
                    // Lo convertimos de RGB a HEX para utilizarlo en el texto de formato y así tener varios colores en la misma cadena
                    string colorHex = ColorUtility.ToHtmlStringRGB(elementColor);
                    resistanceText.text += $" <color=#{colorHex}>{resistancePower} {LocalizationManager.Instance.GetText(element)}</color>\n";
                }
            }
        }
        else
        {
            resistanceText.text += "0";
        }
        effectText.transform.GetComponent<LocalizedText>().key = gearData.description;
        // Esto fuerza la actualización del RectTransform así nos aseguramos que coge las medidas según el Content Size Fitter, porqué a veces no le da tiempo.
        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRectTransform);
    }

    public void ChangeScale(float newScale)
    {
        transform.localScale = Vector3.one * newScale;
        ShopTooltipManager.Instance.transform.localScale = Vector3.one * newScale;
    }
}
       
