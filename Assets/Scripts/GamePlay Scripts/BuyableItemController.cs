using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;


public class BuyableItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI consumableNameText; // Vinculado desde el Inspector
    public Image itemSprite; // Vinculado desde el Inspector
    public TextMeshProUGUI costText; // Vinculado desde el Inspector
    public Button buyButton; // Vinculado desde el Inspector
    private ItemInspectionController itemInspector;
    private ShopTooltipManager tooltipManager;
    private ConsumablesController consumablesController;
    private PlayerCharacterController playerCharacterController;
    private ShopManager shopManager;
    private DeckManager deckManager;
    private DestroyCardsController destroyCardsController;
    private GetCardsController getCardsController;
    private ChooseCardsController chooseCardsController;
    private int cost;
    private string description;
    private List<CardData> possibleGadgets;
    private List<CardData> possibleGems;
    private List<CardData> possibleWeapons;
    private int gemAmount;
    private int healingAmount;
    private BuyableItemData buyableItemData;
    private int hoverTweenId = -1; // Guardamos el ID de la animación de hover
    private int dehoverTweenId = -1; // Guardamos el ID de la animación de dehover
    private int floatTweenId = -1;
    private Vector3 originalScale = Vector3.one;
    private Quaternion originalRotation = Quaternion.identity;

    [Header("Settings of Idle Animation")]
    [SerializeField] public float rotationAmountX = 10f;
    [SerializeField] public float rotationAmountY = 20f;
    [SerializeField] public float rotationAmountZ = 4f;
    [SerializeField] private float expandedScale = 1.2f;
    [SerializeField] private float animationSpeed = 0.6f;
    [SerializeField] public float baseDuration = 3f;
    [SerializeField] public float durationVariation = 0.5f;


    public void Initialize(BuyableItemData data)
    {
        consumablesController = ConsumablesController.Instance;
        playerCharacterController = PlayerCharacterController.Instance;
        deckManager = DeckManager.Instance;
        shopManager = ShopManager.Instance;
        destroyCardsController = DestroyCardsController.Instance;
        getCardsController = GetCardsController.Instance;
        chooseCardsController = ChooseCardsController.Instance;
        itemInspector = ItemInspectionController.Instance;
        tooltipManager = ShopTooltipManager.Instance;
        if (tooltipManager == null) Debug.Log("NO LO ENCUENTRA ostias");
        buyableItemData = data;
        name = $"{buyableItemData.itemName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";

        if (buyableItemData.itemType == BuyableItemType.ConsumableCard)
        {
            consumableNameText.gameObject.SetActive(true);
            cost = buyableItemData.consumableData.cost;
            costText.text = cost.ToString();
            consumableNameText.transform.GetComponent<LocalizedText>().key = buyableItemData.consumableData.consumableName;
            itemSprite.sprite = buyableItemData.consumableData.cardImage;
            description = buyableItemData.consumableData.description;
            LocalizationManager.Instance.UpdateAllLocalizedTexts();
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, () => BuyConsumable(buyableItemData.consumableData), true));
        }
        if (buyableItemData.itemType == BuyableItemType.Gear)
        {
            cost = buyableItemData.gearData.goldCost;
            costText.text = cost.ToString();
            consumableNameText.text = buyableItemData.gearData.gearName;
            itemSprite.sprite = buyableItemData.gearData.gearIcon;
            description = buyableItemData.gearData.description;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, () => BuyGear(buyableItemData.gearData), true));
        }
        if (buyableItemData.itemType == BuyableItemType.Bonfire)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, DestroyCardScene, true));
        }

        if (buyableItemData.itemType == BuyableItemType.GadgetBox)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            possibleGadgets = buyableItemData.possibleGadgets;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyGadgetBox, true));
        }
        if (buyableItemData.itemType == BuyableItemType.WeaponBox)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            possibleWeapons = buyableItemData.possibleWeapons;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyWeaponBox, true));
        }
        if (buyableItemData.itemType == BuyableItemType.GemPack)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            possibleGems = buyableItemData.possibleGems;
            gemAmount = buyableItemData.gemAmount;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyGemPack, true));
        }
        if (buyableItemData.itemType == BuyableItemType.Buff)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyBuff, true));
        }
        if (buyableItemData.itemType == BuyableItemType.Mead)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            healingAmount = buyableItemData.healingAmount;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyMead, true));
        }
        if (buyableItemData.itemType == BuyableItemType.Healing)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            healingAmount = buyableItemData.healingAmount;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, BuyHealing, true));
        }
        if (buyableItemData.itemType == BuyableItemType.Vouche)
        {
            cost = buyableItemData.cost;
            costText.text = cost.ToString();
            itemSprite.sprite = buyableItemData.itemSprite;
            description = buyableItemData.description;
            buyButton.onClick.AddListener(() => itemInspector.OpenPanel(buyableItemData, () => BuyVouche(buyableItemData.fusionatorVouche), true));
        }

        if (buyableItemData.itemType == BuyableItemType.Gear)
        {
            AddTooltip(buyableItemData.gearData);
        }
        else
        {
            AddTooltip();
        }
        StartFloatingEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaleAnimation(ref hoverTweenId, ref dehoverTweenId, originalScale * expandedScale, LeanTweenType.easeOutElastic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleAnimation(ref dehoverTweenId, ref hoverTweenId, originalScale, LeanTweenType.easeOutElastic);
    }

    private void StartScaleAnimation(ref int tweenToStart, ref int tweenToCancel, Vector3 targetScale, LeanTweenType easing)
    {
        // Cancelar la animación que estaba en curso
        if (LeanTween.isTweening(tweenToCancel))
        {
            LeanTween.cancel(tweenToCancel);
        }

        // Cancelar cualquier animación del mismo tipo antes de iniciar una nueva
        if (LeanTween.isTweening(tweenToStart))
        {
            LeanTween.cancel(tweenToStart);
        }

        // Iniciar la nueva animación y guardar el ID
        tweenToStart = LeanTween.scale(gameObject, targetScale, animationSpeed)
            .setEase(easing)
            .id;
    }
    public void StartFloatingEffect()
    {
        float duration = baseDuration + UnityEngine.Random.Range(-durationVariation, durationVariation);

        Vector3 targetRotation = new Vector3(
            originalRotation.x + UnityEngine.Random.Range(-rotationAmountX, rotationAmountX),
            originalRotation.y + UnityEngine.Random.Range(-rotationAmountY, rotationAmountY),
            originalRotation.z + UnityEngine.Random.Range(-rotationAmountZ, rotationAmountZ)
        );

        floatTweenId = LeanTween.rotateLocal(itemSprite.gameObject, targetRotation, duration)
        .setEaseInOutSine()
        .setOnComplete(() => StartFloatingEffect())
        .id;
    }

    public void BuyConsumable(ConsumableData consumableData)
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {consumableData.consumableName} por {cost} Gold.");
            Addressables.LoadAssetAsync<GameObject>("ConsumableCard").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    CombatManager combatManager = CombatManager.Instance;
                    GameObject cardObject = Instantiate(handle.Result, consumablesController.transform);
                    ConsumableCardController cardController = cardObject.GetComponent<ConsumableCardController>();

                    if (cardController != null)
                    {
                        cardController.Initialize(consumableData); // Pasamos los datos al controlador
                    }
                }
            };
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }

    public void BuyGear(GearData gearData)
    {
        if (playerCharacterController.Gold >= gearData.goldCost)
        {
            playerCharacterController.SpendGold(gearData.goldCost);
            Debug.Log($"🛒 Compraste {gearData.gearName} por {gearData.goldCost} Gold.");
            playerCharacterController.EquipGear(gearData);
            //ClearGearButton(button, costText);
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }


    public void BuyGadgetBox()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            CardData gadget = possibleGadgets[UnityEngine.Random.Range(0, possibleGadgets.Count)];
            deckManager.AddCardToDeck(gadget);
            GetACardScene(gadget);
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }
    public void BuyWeaponBox()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            CardData weapon = possibleWeapons[UnityEngine.Random.Range(0, possibleWeapons.Count)];
            deckManager.AddCardToDeck(weapon);
            GetACardScene(weapon);
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }
    public void BuyGemPack()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
    
            ChooseACardScene();
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }
    public void BuyBuff()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            PlayerBuff buff = BuffCatalog.CreateBuff(buyableItemData.itemName, buyableItemData.buffValue, buyableItemData.roundsDuration);
            playerCharacterController.AddPlayerBuff(buff);
            buff.OnBuffSpawn?.Invoke(playerCharacterController);
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }
    public void BuyMead()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            PlayerBuff buff = BuffCatalog.CreateBuff(buyableItemData.itemName, buyableItemData.buffValue, buyableItemData.roundsDuration);
            playerCharacterController.AddPlayerBuff(buff);
            buff.OnBuffSpawn?.Invoke(playerCharacterController);
            StartCoroutine(playerCharacterController.TakeHeal(healingAmount, () => Destroy(gameObject)));
        }
        tooltipManager.Hide();
    }

    public void BuyHealing()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            StartCoroutine(playerCharacterController.TakeHeal(healingAmount, () => Destroy(gameObject)));
        }
        tooltipManager.Hide();
    }

    public void BuyVouche(bool fusionatorCheck)
    {
        if (fusionatorCheck && playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            Debug.Log($"🛒 Compraste {buyableItemData.itemName} por {cost} Gold.");
            playerCharacterController.getFusionatorAcces();
            shopManager.UpdateFusionatorButton();
            tooltipManager.Hide();
            Destroy(gameObject);
        }
    }
    public void DestroyCardScene()
    {
        if (playerCharacterController.Gold >= cost)
        {
            playerCharacterController.SpendGold(cost);
            shopManager.HideTransition();
            destroyCardsController.StartScene();
            Destroy(gameObject);
        }
        tooltipManager.Hide();
    }
    public void GetACardScene(CardData cardData)
    {
        shopManager.HideTransition();
        getCardsController.StartScene(cardData);
        tooltipManager.Hide();
    }
    public void ChooseACardScene()
    {
        shopManager.HideTransition();
        List<CardData> cardsToChoose = GetRandomCardSelection(possibleGems, gemAmount);
        chooseCardsController.StartScene(cardsToChoose, itemSprite.sprite);
        tooltipManager.Hide();
    }
    public List<CardData> GetRandomCardSelection(List<CardData> availableCards, int amount)
    {
        List<CardData> result = new List<CardData>();

        if (availableCards == null || availableCards.Count == 0 || amount <= 0) return result;

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
            result.Add(availableCards[randomIndex]);
        }

        return result;
    }
    private void AddTooltip()
    {
        //Debug.Log("Añade el tooltip");
        EventTrigger eventTrigger = buyButton.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = buyButton.gameObject.AddComponent<EventTrigger>();
        }
        // Evento PointerEnter (Ratón sobre el botón)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((eventData) => tooltipManager.Show(buyableItemData));

        // Evento PointerExit (Ratón sale del botón)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) => tooltipManager.Hide());

        eventTrigger.triggers.Add(entryEnter);
        eventTrigger.triggers.Add(entryExit);
    }

    private void AddTooltip(GearData gearData)
    {
        EventTrigger eventTrigger = buyButton.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = buyButton.gameObject.AddComponent<EventTrigger>();

        // Evento PointerEnter (Ratón sobre el botón)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((eventData) => shopManager.ShowGearTooltip(gearData));

        // Evento PointerExit (Ratón sale del botón)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) => shopManager.HideTooltip());

        eventTrigger.triggers.Add(entryEnter);
        eventTrigger.triggers.Add(entryExit);
    }


  

}
