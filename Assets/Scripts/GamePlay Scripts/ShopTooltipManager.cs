using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopTooltipManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public GameObject tooltipPanel;
    public RectTransform shopPanelRectTransform;
    public Transform canvasTransform;
    public CameraController cameraController;
    public Camera mainCamera;
    public static ShopTooltipManager Instance; // Esto se llama SINGLETON, lo asigno en el Awake y así puedo obtenerlo desde cualquier otra clase aún que esté desactivado sin problemas.
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
        tooltipPanel.transform.position = new Vector3(mousePosition.x + 5f * cameraZoomFactor, mousePosition.y - 0.3f * cameraZoomFactor, 0);
        ClampTooltipInsideShopPanel();
    }
    
    public void Show(BuyableItemData data)
    {
        Vector3 screenMousePos = Pointer.current.position.ReadValue();
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 0));
        tooltipPanel.transform.position = new Vector3(mousePosition.x + 100f * cameraZoomFactor, mousePosition.y - 50f * cameraZoomFactor, 0);

        this.enabled = true;
        gameObject.SetActive(true);
        if (data.itemType == BuyableItemType.ConsumableCard)
        {
            titleText.transform.GetComponent<LocalizedText>().key = data.consumableData.consumableName;
            descriptionText.transform.GetComponent<LocalizedText>().key = data.consumableData.description;
        }
        else
        {
            titleText.transform.GetComponent<LocalizedText>().key = data.itemName;
            descriptionText.transform.GetComponent<LocalizedText>().key = data.description;
        }
        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        // Esto fuerza la actualización del RectTransform así nos aseguramos que coge las medidas según el Content Size Fitter, porqué a veces no le da tiempo.
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasTransform.GetComponent<RectTransform>());
    }

    public void Hide()
    {
        titleText.text = "";
        descriptionText.text = "";
        gameObject.SetActive(false);
        this.enabled = false;
    }

    private void ClampTooltipInsideShopPanel()
    {
        RectTransform tooltipRT = canvasTransform.GetComponent<RectTransform>();

        // 2) Sacar world corners
        Vector3[] shopCorners = new Vector3[4];
        Vector3[] tipCorners = new Vector3[4];
        shopPanelRectTransform.GetWorldCorners(shopCorners);
        tooltipRT.GetWorldCorners(tipCorners);

        // 3) Calcular min/max
        float shopMinX = shopCorners[0].x, shopMaxX = shopCorners[2].x;
        float shopMinY = shopCorners[0].y, shopMaxY = shopCorners[2].y;
        float tipMinX = tipCorners[0].x, tipMaxX = tipCorners[2].x;
        float tipMinY = tipCorners[0].y, tipMaxY = tipCorners[2].y;

        // 4) Overflow
        float overflowLeft = shopMinX - tipMinX;
        float overflowRight = tipMaxX - shopMaxX;
        float overflowBottom = shopMinY - tipMinY;
        float overflowTop = tipMaxY - shopMaxY;

        // 5) Desplazamiento
        Vector3 shift = Vector3.zero;
        if (overflowLeft > 0f) shift.x += overflowLeft;
        if (overflowRight > 0f) shift.x -= overflowRight;
        if (overflowBottom > 0f) shift.y += overflowBottom;
        if (overflowTop > 0f) shift.y -= overflowTop;

        // 6) Aplicar
        if (shift != Vector3.zero)
            tooltipPanel.transform.position += shift;      
    }

}
