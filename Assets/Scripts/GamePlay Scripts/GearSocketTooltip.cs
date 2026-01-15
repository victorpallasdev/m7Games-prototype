using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GearSocketTooltip : MonoBehaviour
{
    public GearTooltipManager gearTooltipManager; // Vinculado desde el inspector.
    public CameraController cameraController; // Vinculado desde el inspector.
    public Camera mainCamera; // Vinculado desde el inspector.
    private GearData equippedGear; // La pieza equipada
    public static bool isActive = false;


    public void EquipGear(GearData gearData)
    {
        equippedGear = gearData;
    }
    public void DisEquipGear()
    {
        equippedGear = null;
    }
    public void ShowTooltip()
    {
        isActive = true;
        if (equippedGear != null)
        {
            // Lo muevo antes de activarlo para que no se vea un efecto raro de desplazamiento del tooltip
            float cameraZoomFactor = 1 / cameraController.cameraZoomFactor();
            Vector3 screenMousePos = Pointer.current.position.ReadValue();
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 0));
            gearTooltipManager.transform.position = new Vector3(mousePosition.x + 0.5f * cameraZoomFactor, mousePosition.y - 0.3f * cameraZoomFactor, 0);
            // Entonces activo el gameObject, luego habilito su script y por último llamo a su metodo para mostrar el texto
            gearTooltipManager.gameObject.SetActive(true);
            gearTooltipManager.enabled = true;
            gearTooltipManager.Show(equippedGear);
        }
    }
    public void ShowInspector()
    {
        ItemInspectionController.Instance.OpenPanel(equippedGear);
    }

    public void HideTooltip()
    {
        isActive = false;
        gearTooltipManager.gameObject.SetActive(false);
        gearTooltipManager.enabled = false;
    }
}
