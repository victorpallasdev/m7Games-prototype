using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GearSocketsController : MonoBehaviour
{
    public static GearSocketsController Instance;
    public Image helmetImage;  // Vinculado desde el inspector
    public Image chestImage;  // Vinculado desde el inspector
    public Image glovesImage;  // Vinculado desde el inspector
    public Image leggingsImage;  // Vinculado desde el inspector
    public Image bootsImage;  // Vinculado desde el inspector
    private PlayerCharacterController playerDwarfController;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerDwarfController = PlayerCharacterController.Instance;
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }
    public void MoveToShop(Vector3 targetPosition, Vector3 targetScale)
    {
        LeanTween.cancel(gameObject);
        transform.position = targetPosition;
        transform.localScale = targetScale;
    }
    public void ShowInShop(Vector3 targetPosition)
    {
        transform.LeanMove(targetPosition, 1f).setEaseOutExpo();
    }
    public void ReturnFromShop()
    {
        LeanTween.cancel(gameObject);
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
    }

    public void UpdateGearUI()
    {
        List<GearData> gearList = playerDwarfController.GetDwarfGear();

        foreach (var gear in gearList)
        {
            if (gear != null)
            {
                switch (gear.gearType)
                {
                    case GearType.Helmet:
                        helmetImage.sprite = gear.gearIcon;
                        helmetImage.enabled = true;
                        break;

                    case GearType.Chestplate:
                        chestImage.sprite = gear.gearIcon;
                        chestImage.enabled = true;
                        break;

                    case GearType.Gloves:
                        glovesImage.sprite = gear.gearIcon;
                        glovesImage.enabled = true;
                        break;

                    case GearType.Leggings:
                        leggingsImage.sprite = gear.gearIcon;
                        leggingsImage.enabled = true;
                        break;

                    case GearType.Boots:
                        bootsImage.sprite = gear.gearIcon;
                        bootsImage.enabled = true;
                        break;
                }
            }
        }

        if (playerDwarfController.helmetGear == null)
        {
            helmetImage.sprite = null;
            helmetImage.enabled = false;
            helmetImage.transform.GetComponent<GearSocketTooltip>().DisEquipGear();
        }
        if (playerDwarfController.chestGear == null)
        {
            chestImage.sprite = null;
            chestImage.enabled = false;
            chestImage.transform.GetComponent<GearSocketTooltip>().DisEquipGear();
        }
        if (playerDwarfController.glovesGear == null)
        {
            glovesImage.sprite = null;
            glovesImage.enabled = false;
            glovesImage.transform.GetComponent<GearSocketTooltip>().DisEquipGear();
        }
        if (playerDwarfController.leggingsGear == null)
        {
            leggingsImage.sprite = null;
            leggingsImage.enabled = false;
            leggingsImage.transform.GetComponent<GearSocketTooltip>().DisEquipGear();
        }
        if (playerDwarfController.bootsGear == null)
        {
            bootsImage.sprite = null;
            bootsImage.enabled = false;
            bootsImage.transform.GetComponent<GearSocketTooltip>().DisEquipGear();
        }                        
    }
}
