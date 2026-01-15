using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System;

public class ItemInspectionController : MonoBehaviour, IPointerDownHandler
{
    public static ItemInspectionController Instance;
    public Image itemImage;
    public TextMeshProUGUI itemTitleText;
    public TextMeshProUGUI itemArmorText;
    public TextMeshProUGUI itemResistancesText;
    public TextMeshProUGUI itemAbilityText;
    public Transform panelTransform;
    public Transform backgroundClickCatcher;
    public Transform textsContainerTransform;
    public Transform buttonsContainerTransform;
    public Button sellButton;
    public Button buyButton;
    public Material buyButtonMaterial;
    public TextMeshProUGUI sellButtonText;
    public TextMeshProUGUI buyButtonText;
    public CombatManager combatManager;
    public ShopManager shopManager;
    public CameraController cameraController;
    private PlayerCharacterController playerDwarfController;
    private GearData gearData;
    private BuyableItemData itemData;
    private int sellPrice;

    void Awake()
    {
        Instance = this;
        panelTransform.gameObject.SetActive(false);
        backgroundClickCatcher.gameObject.SetActive(false);
    }
    // Esta sobrecarga es para el GearSocket, desde el cual como son los GearItems equipados, pues solo podemos pasarle el GearData, no el buyableitemData
    // Solo se pueden vender los que estan equipados, así que ya está bien que buyable = false siempre.
    public void OpenPanel(GearData gearData)
    {
        if (gearData == null)
        {
            return;
        }
        this.gearData = gearData;
        panelTransform.localScale = Vector3.one / cameraController.cameraZoomFactor();
        panelTransform.localPosition = new Vector3(cameraController.transform.localPosition.x, cameraController.transform.localPosition.y, 0f);
        panelTransform.gameObject.SetActive(true);
        itemArmorText.gameObject.SetActive(true);
        itemResistancesText.gameObject.SetActive(true);
        StartCoroutine(ActivateCatcherNextFrame());
        playerDwarfController = PlayerCharacterController.Instance;
        shopManager = ShopManager.Instance;
        combatManager.hideUIInspectorItem();
        LoadGearItem(null, false);
    }
    public void OpenPanel(BuyableItemData itemData, Action buyMethod, bool buyable = false)
    {
        if (itemData == null)
        {
            return;
        }        
        this.itemData = itemData;
        panelTransform.localScale = Vector3.one / cameraController.cameraZoomFactor();
        panelTransform.localPosition = new Vector3(cameraController.transform.localPosition.x, cameraController.transform.localPosition.y, 0f);
        StartCoroutine(ActivateCatcherNextFrame());
        playerDwarfController = PlayerCharacterController.Instance;
        combatManager.hideUIInspectorItem();
        panelTransform.gameObject.SetActive(true);

        if (itemData.itemType == BuyableItemType.Gear)
        {
            gearData = itemData.gearData;
            itemArmorText.gameObject.SetActive(true);
            itemResistancesText.gameObject.SetActive(true);
            LoadGearItem(buyMethod, buyable);
        }
        else
        {
            itemArmorText.gameObject.SetActive(false);
            itemResistancesText.gameObject.SetActive(false);
            LoadItem(buyMethod, buyable);
        }
        
    }    
    // Tengo que activar y desactivar el ClickCatcher con Corutinas para que suceda unos frames después. Ya que sinó detecta el mismo click que
    // lo activa a él y entonces cierra el panel en el momento de abrirlo. Cosa que molesta, ya que a veces no se abria.
    // Y otras veces se quedaba activado si se clicaba encima de un collider para salir del ItemPanel.
    private IEnumerator ActivateCatcherNextFrame()
    {
        yield return null;
        backgroundClickCatcher.gameObject.SetActive(true);
    }
    private IEnumerator DeactivateCatcherNextFrame()
    {
        yield return null;
        backgroundClickCatcher.gameObject.SetActive(false);
    }
    public void HidePanel()
    {
        panelTransform.gameObject.SetActive(false);
        backgroundClickCatcher.gameObject.SetActive(false);
        combatManager.showUIInspectorItem();
        StartCoroutine(DeactivateCatcherNextFrame());
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Lista de resultados del raycast
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        // Comprobamos si el clic fue sobre un UI interactivo (como un botón)
        foreach (var result in raycastResults)
        {
            if (result.gameObject.GetComponent<Button>() != null)
            {
                Button resultButton = result.gameObject.GetComponent<Button>();
                if (resultButton == sellButton || resultButton == sellButton)
                {
                    result.gameObject.GetComponent<Button>().onClick.Invoke();
                    return; // Hiciste clic sobre un botón, no cierres el panel
                }
            }
        }
        HidePanel();
    }
    private void LoadGearItem(Action buyMethod, bool buyable = false)
    {
        itemTitleText.transform.GetComponent<LocalizedText>().key = gearData.gearName;
        itemArmorText.text = LocalizationManager.Instance.GetText("armor") + ": " + gearData.armor.ToString();
        itemResistancesText.text = LocalizationManager.Instance.GetText("resistances") + ": ";
        if (gearData.elementalResistances.Count > 0)
        {
            for (int i = 0; i < gearData.elementalResistances.Count; i++)
            {
                if (gearData.elementalResistances[i].value > 0)
                {
                    Element elementName = gearData.elementalResistances[i].element;
                    int resistancePower = gearData.elementalResistances[i].value;
                    // Obtenemos el Color del elemento segun nuestro mapeado
                    Color elementColor = FloatingText.ElementColorMap[elementName];
                    // Lo convertimos de RGB a HEX para utilizarlo en el texto de formato y así tener varios colores en la misma cadena
                    string colorHex = ColorUtility.ToHtmlStringRGB(elementColor);
                    itemResistancesText.text += $" <color=#{colorHex}>{resistancePower} {LocalizationManager.Instance.GetText(elementName)}</color>\n";
                }
            }
        }
        else
        {
            itemResistancesText.text += "0";
        }
        itemAbilityText.transform.GetComponent<LocalizedText>().key = gearData.description;
        itemImage.sprite = gearData.gearIcon;
        itemImage.SetNativeSize();        

        if (buyable)
        {
            CheckAffordableGear();
            buyButton.gameObject.SetActive(true);
            sellButton.gameObject.SetActive(false);
            buyButtonText.text = LocalizationManager.Instance.GetText("buy") + " " + gearData.goldCost.ToString() + "G";
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
                    {
                        HidePanel();
                        buyMethod();
                    });
        }
        else
        {
            float decimalSellPrice = gearData.goldCost * 0.4f;
            sellPrice = (int)decimalSellPrice;
            sellButton.gameObject.SetActive(true);
            sellButtonText.text = LocalizationManager.Instance.GetText("sell") + " " + ((int)decimalSellPrice).ToString() + "G";
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellGear);
            buyButton.gameObject.SetActive(false);
        }

        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        LayoutRebuilder.ForceRebuildLayoutImmediate(textsContainerTransform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buyButton.transform.GetComponent<RectTransform>());
        StartCoroutine(ForceUpdateButtonCorutine());      
    }
    private IEnumerator ForceUpdateButtonCorutine()
    {        
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(sellButtonText.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(sellButton.transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsContainerTransform.GetComponent<RectTransform>());
    }

    private void LoadItem(Action buyMethod, bool buyable = false)
    {
        if (itemData.itemType == BuyableItemType.ConsumableCard)
        {
            itemTitleText.transform.GetComponent<LocalizedText>().key = itemData.consumableData.consumableName;
            itemAbilityText.transform.GetComponent<LocalizedText>().key = itemData.consumableData.description;
            itemImage.sprite = itemData.consumableData.cardImage;
            itemImage.SetNativeSize();

            float decimalSellPrice = itemData.consumableData.cost * 0.4f;
            sellPrice = (int)decimalSellPrice;

            if (buyable)
            {
                CheckAffordableItem();
                buyButton.gameObject.SetActive(true);
                sellButton.gameObject.SetActive(false);
                buyButtonText.text = LocalizationManager.Instance.GetText("buy") + " " + itemData.consumableData.cost.ToString() + "G";
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() =>
                    {
                        HidePanel();
                        buyMethod();
                    });
            }
            else
            {
                buyButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(true);
            }
        }
        else
        {
            itemTitleText.transform.GetComponent<LocalizedText>().key = itemData.itemName;
            itemAbilityText.transform.GetComponent<LocalizedText>().key = itemData.description;
            itemImage.sprite = itemData.itemSprite;
            itemImage.SetNativeSize();

            float decimalSellPrice = itemData.cost * 0.4f;
            sellPrice = (int)decimalSellPrice;

            if (buyable)
            {
                CheckAffordableItem();
                buyButton.gameObject.SetActive(true);
                sellButton.gameObject.SetActive(false);
                buyButtonText.text = LocalizationManager.Instance.GetText("buy") + " " + itemData.cost.ToString() + "G";
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() =>
                    {
                        HidePanel();
                        buyMethod();
                    });
            }
            else
            {
                buyButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(true);
            }
        }

        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        LayoutRebuilder.ForceRebuildLayoutImmediate(textsContainerTransform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buyButton.transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsContainerTransform.GetComponent<RectTransform>());

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellItem);
    }
    private void CheckAffordableGear()
    {
        if (playerDwarfController.Gold < gearData.goldCost)
        {
            buyButton.interactable = false;
            buyButtonMaterial.EnableKeyword("_ENABLESATURATION_ON");
            buyButtonMaterial.SetFloat("_EnableSaturation", 1f);
        }
        else
        {
            buyButton.interactable = true;
            buyButtonMaterial.SetFloat("_EnableSaturation", 0f);
            buyButtonMaterial.DisableKeyword("_ENABLESATURATION_ON");
        }
    }

    private void CheckAffordableItem()
    {
        if (playerDwarfController.Gold < itemData.cost)
        {
            buyButton.interactable = false;
            buyButtonMaterial.EnableKeyword("_ENABLESATURATION_ON");
            buyButtonMaterial.SetFloat("_EnableSaturation", 1f);
        }
        else
        {
            buyButton.interactable = true;
            buyButtonMaterial.SetFloat("_EnableSaturation", 0f);
            buyButtonMaterial.DisableKeyword("_ENABLESATURATION_ON");
        }
    }

    private void SellGear()
    {
        playerDwarfController.AddGold(sellPrice);
        playerDwarfController.DisEquip(gearData);
        HidePanel();
    }

    private void SellItem()
    {
        playerDwarfController.AddGold(sellPrice);
        HidePanel();
    }


}
