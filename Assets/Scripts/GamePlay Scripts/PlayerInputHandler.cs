using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerCharacterController playerDwarfController;
    private Transform lastHoveredObject = null;

    IEnumerator Start()
    {
        playerInput = GetComponent<PlayerInput>();
        while (playerDwarfController == null)
        {
            playerDwarfController = FindFirstObjectByType<PlayerCharacterController>();
            yield return null; // Espera un frame antes de volver a comprobar
        }
        
        playerInput.onActionTriggered += OnActionTriggered;
        AssignInputEvents();        
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        LayerMask fieldEntityLayer = LayerMask.GetMask("FieldEntity");
        LayerMask golemLayer = LayerMask.GetMask("Golem");
        Vector2 pointerPosition = Vector2.zero;
        // 🔹 Verifica si el input proviene de `Touchscreen` o `Mouse`
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
           // Debug.Log($"📱 Touch detectado en posición: {pointerPosition}");
        }
        else if (Mouse.current != null && Mouse.current.position.ReadValue() != Vector2.zero)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            //Debug.Log($"🖱 Mouse detectado en posición: {pointerPosition}");
        }
        else
        {
            Debug.LogWarning("⚠ No se detectó ningún input válido.");
            return; // No hay input válido, salimos
        }

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(pointerPosition);
        // Primero se intenta en el layer de los fieldEntity ya que tienen prioridad
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, fieldEntityLayer);
        // Sinó, pues el collider que sea
        if (hit.collider == null)
        {
            hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        }
        Transform newHoveredObject = hit.collider != null ? hit.collider.transform : null;

        if (context.action.name == "PointerMove")
        {
            // Si el cursor ha cambiado de objeto, ocultamos el anterior si tenía un panel
            if (lastHoveredObject != null && lastHoveredObject != newHoveredObject)
            {
                PickaxeStatsPanelController previousPickaxeSocket = lastHoveredObject.GetComponent<PickaxeStatsPanelController>();
                if (previousPickaxeSocket != null && previousPickaxeSocket.IsPanelActive())
                {
                    previousPickaxeSocket.HidePickaxeStatsPanel();
                }

                TurnsRemainingPanelController previousTurnsPanel = lastHoveredObject.GetComponent<TurnsRemainingPanelController>();
                if (previousTurnsPanel != null && previousTurnsPanel.IsPanelActive())
                {
                    previousTurnsPanel.HidePickaxeStatsPanel();
                }

                GearSocketTooltip previousGearSocket = lastHoveredObject.GetComponent<GearSocketTooltip>();
                if (previousGearSocket != null && GearSocketTooltip.isActive)
                {
                    previousGearSocket.HideTooltip();
                }

                FieldEntityController previousEntity = lastHoveredObject.GetComponent<FieldEntityController>();
                if (previousEntity != null)
                {
                    previousEntity.RestoreMaterial();
                }

                GolemController previousGolemController = lastHoveredObject.GetComponent<GolemController>();
                if (previousGolemController != null)
                {
                    previousGolemController.RestoreMaterial();
                }

            }

            // Si el nuevo objeto tiene un panel, lo mostramos SOLO si no está ya activo
            if (newHoveredObject != null)
            {
                PickaxeStatsPanelController pickaxeSocket = newHoveredObject.GetComponent<PickaxeStatsPanelController>();
                if (pickaxeSocket != null && !pickaxeSocket.IsPanelActive())
                {
                    pickaxeSocket.ShowPickaxeStatsPanel();
                }

                TurnsRemainingPanelController turnsRemainingPanel = newHoveredObject.GetComponent<TurnsRemainingPanelController>();
                if (turnsRemainingPanel != null && !turnsRemainingPanel.IsPanelActive())
                {
                    turnsRemainingPanel.ShowPickaxeStatsPanel();
                }

                GearSocketTooltip gearSocket = newHoveredObject.GetComponent<GearSocketTooltip>();
                if (gearSocket != null && !GearSocketTooltip.isActive)
                {
                    gearSocket.ShowTooltip();
                }

                FieldEntityController entity = newHoveredObject.GetComponent<FieldEntityController>();
                if (entity != null)
                {
                    entity.ApplyOutline();
                }

                GolemController golemController = newHoveredObject.GetComponent<GolemController>();
                if (golemController != null)
                {
                    golemController.ApplyOutline();
                }
            }

            // Actualizamos la referencia del objeto sobre el que está el cursor
            lastHoveredObject = newHoveredObject;
        }

        // 🔹 Mantener el RightClick funcional
        if (context.action.name == "RightClick" && hit.collider != null)
        {
            PlayerCharacterController dwarf = hit.collider.GetComponent<PlayerCharacterController>();
            if (dwarf != null)
            {
                dwarf.OnRightClick();
                return;
            }

            GolemController golem = hit.collider.GetComponent<GolemController>();
            if (golem != null)
            {
                golem.OnRightClick();
                return;
            }
        }

        if (context.action.name == "LeftClick" && hit.collider != null)
        {
            Debug.Log("Click Izquierdo detectado en: ");
            GolemController golem = hit.collider.GetComponent<GolemController>();
            if (golem != null)
            {
                
                golem.SetTarget();
                return;
            }
            FieldEntityController entity = hit.collider.GetComponent<FieldEntityController>();
            if (entity != null)
            {
                
                entity.SetTarget();
                return;
            }

            GearSocketTooltip gearSocket = newHoveredObject.GetComponent<GearSocketTooltip>();
            if (gearSocket != null)
            {
                
                gearSocket.HideTooltip();
                gearSocket.ShowInspector();
            }
    }

    }


    void AssignInputEvents()
    {
        playerInput.actions["PointerMove"].performed += OnActionTriggered;
        playerInput.actions["RightClick"].performed += OnActionTriggered;
        playerInput.actions["LeftClick"].performed += OnActionTriggered;

        Debug.Log("✅ Eventos asignados correctamente.");
    }
}
