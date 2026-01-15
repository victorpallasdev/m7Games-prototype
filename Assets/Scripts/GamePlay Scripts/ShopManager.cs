using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

using UnityEngine.InputSystem;
using System.Linq;





public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public GameObject shopPanelGO; // Vinculado desde el inspector
    public RectTransform shopPanel; // Vinculado desde el inspector
    public RectTransform canvasRect; // Vinculado desde el inspector
    public Button continueButton; // Vinculado desde el inspector
    public Button fusionatorButton; // Vinculado desde el inspector
    public Button restockButton;
    public CameraController cameraController; // Vinculado desde el inspector
    public Camera mainCamera;
    public PlayerHandController playerHandController; // Vinculado desde el inspector
    public GearTooltipManager gearTooltipManager; // Vinculado desde el inspector
    public Transform gearPanelTransform;
    public Transform gemPanelTransform;
    public Transform boxesPanelTransform;
    public Transform consumablesPanelTransform;
    public Transform barPanelTransform;
    public Transform vouchesPanelTransform;
    public GearSocketsController gearSocketsController; // Vinculado desde el inspector
    public FusionatorController fusionatorController; // Vinculado desde el inspector
    private PlayerCharacterController playerDwarfController;   
    private GolemController golemController;
     public Animator shopSignAnimator;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI restockCostText;
    private DeckManager deckManager;
    private GameObject fusionatorButtonGO;
    public GameObject buyableItemPrefab;
    public GearLibrary gearLibrary;
    public List<BuyableItemData> allGemPacks;
    public List<BuyableItemData> allBoxes;
    public List<BuyableItemData> allConsumables;
    public List<BuyableItemData> allComodities;
    public BuyableItemData bonfireData;
    public BuyableItemData fusionatorVoucheData;   
    
    [SerializeField] private int restockPrize = 10;


    void Awake()
    {
        Instance = this;
    }
    void Start()
    {        
        fusionatorButtonGO = fusionatorButton.gameObject;
        fusionatorButtonGO.SetActive(false);
        shopPanelGO.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClick);
        restockButton.onClick.AddListener(RestockShop);
    }
    public void ShopOpen()
    {        
        shopPanelGO.SetActive(true);
        ShopInitialize();
        
        float targetX = -640f;
        float startX = shopPanel.anchoredPosition.x;
        shopSignAnimator.SetTrigger("Open");
        GameplayAudioManager.Instance.FadeOutInShopMusic();
        LeanTween.value(shopPanel.gameObject, startX, targetX, 1f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((float value) =>
            {
                shopPanel.anchoredPosition = new Vector2(value, shopPanel.anchoredPosition.y);
            })
            .setOnComplete(() =>
                {
                    MoveAndShowGearEquippedSockets();          
                });           
    }
    
    public void ShopClose()
    {
        float targetX = 640f;
        float startX = shopPanel.anchoredPosition.x;
        restockPrize = 10;
        GameplayAudioManager.Instance.FadeOutInLevelMusic();
        gearSocketsController.ReturnFromShop();
        LeanTween.value(shopPanel.gameObject, startX, targetX, 0.6f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float value) =>
            {
                shopPanel.anchoredPosition = new Vector2(value, shopPanel.anchoredPosition.y);
            })
            .setOnComplete(() =>
            {
                shopPanelGO.SetActive(false);
                ShopReset();
            });
    }
    public void HideTransition()
    {
        float targetX = 640f;
        float startX = shopPanel.anchoredPosition.x;
        gearSocketsController.ReturnFromShop();
        LeanTween.value(shopPanel.gameObject, startX, targetX, 0.6f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float value) =>
            {
                shopPanel.anchoredPosition = new Vector2(value, shopPanel.anchoredPosition.y);
            });
    }

    public void ReturnDestroyCardsTransition()
    {
        float targetX = -640f;
        float startX = shopPanel.anchoredPosition.x;

        LeanTween.value(shopPanel.gameObject, startX, targetX, 0.6f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float value) =>
            {
                shopPanel.anchoredPosition = new Vector2(value, shopPanel.anchoredPosition.y);
            })
            .setOnComplete(() =>
            {
                MoveAndShowGearEquippedSockets();
            });
    }
    private void MoveAndShowGearEquippedSockets()
    {
        Vector3[] shopWorldCorners = new Vector3[4];
        RectTransform gearSocketsRect = gearSocketsController.GetComponent<RectTransform>();

        shopPanel.GetWorldCorners(shopWorldCorners);
        Vector3 centerLeft = (shopWorldCorners[0] + shopWorldCorners[1]) / 2f;
        Vector3 targetScaleGearSockets = canvasRect.localScale * 1.5f / EntityInitializer.newScale.x;
        Vector3 targetPosGearSockets = new Vector3(centerLeft.x + gearSocketsRect.rect.width * targetScaleGearSockets.x, centerLeft.y, centerLeft.z);                    
        
        gearSocketsController.MoveToShop(targetPosGearSockets, targetScaleGearSockets);
        targetPosGearSockets.x = centerLeft.x - gearSocketsRect.rect.width / 7 * targetScaleGearSockets.x;
        gearSocketsController.ShowInShop(targetPosGearSockets);
    }
    private void ShopInitialize()
    {
        golemController = GolemController.Instance;
        playerDwarfController = PlayerCharacterController.Instance;
        deckManager = DeckManager.Instance;
        goldText.text = playerDwarfController.Gold.ToString();
        restockCostText.text = restockPrize.ToString() + "G";
        ShopReset();
        GearGenerator();
        GemPacksGenerator();
        BoxGenerator();
        ConsumableCardsGenerator();
        BarGenerator();
        VouchesBonfireGenerator(); 
    }

    private void ShopReset()
    {
        foreach (Transform buyable in gearPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
        foreach (Transform buyable in gemPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
        foreach (Transform buyable in boxesPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
        foreach (Transform buyable in consumablesPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
        foreach (Transform buyable in barPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
        foreach (Transform buyable in vouchesPanelTransform)
        {
            Destroy(buyable.gameObject);
        }
    }

    private void RestockShop()
    {
        restockPrize = (int)(restockPrize * 1.1f);
        restockCostText.text = restockPrize.ToString() + "G";
        ShopReset();
        GearGenerator();
        GemPacksGenerator();
        BoxGenerator();
        ConsumableCardsGenerator();
        BarGenerator();
        VouchesBonfireGenerator();
    }
    public void UpdateFusionatorButton()
    {
        fusionatorButtonGO.SetActive(playerDwarfController.gotFusionatorAcces);
        fusionatorButton.onClick.AddListener(GoToFusionator);
    }
    private void GearGenerator()
    {
        List<BuyableItemData> gearList = new List<BuyableItemData>();
        gearList = gearLibrary.allBuyableGear
        .OrderBy(x => Random.value)
        .Take(2)
        .ToList();

        for (int i = 0; i < gearList.Count; i++)
        {
            Vector3 localPosition = CalculatePositionY(i, gearList.Count, gearPanelTransform);
            Vector3 worldPosition = gearPanelTransform.TransformPoint(localPosition);
            GameObject newGearItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, gearPanelTransform);
            BuyableItemController itemController = newGearItem.GetComponent<BuyableItemController>();

            itemController.Initialize(gearList[i]);
        }       
    }

    private void GemPacksGenerator()
    {
        List<BuyableItemData> result = new List<BuyableItemData>();

        // Pesos asociados a cada pack (suman 100)
        int[] probabilities = new int[] { 75, 15, 9, 1 };
        int totalWeight = probabilities.Sum();

        for (int i = 0; i < 3; i++)
        {
            int roll = Random.Range(0, totalWeight); // Random de 0 a 99
            int cumulative = 0;
            int selectedIndex = 0;

            for (int j = 0; j < probabilities.Length; j++)
            {
                cumulative += probabilities[j];
                if (roll < cumulative)
                {
                    selectedIndex = j;
                    break;
                }
            }

            BuyableItemData pack = allGemPacks[selectedIndex];
            result.Add(pack);
        }

        for (int i = 0; i < result.Count; i++)
        {
            Vector3 localPosition = CalculatePositionX(i, result.Count, gemPanelTransform);
            Vector3 worldPosition = gemPanelTransform.TransformPoint(localPosition);
            GameObject newGearItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, gemPanelTransform);
            BuyableItemController itemController = newGearItem.GetComponent<BuyableItemController>();

            itemController.Initialize(result[i]);
        }  
    }
    private void BoxGenerator()
    {
        int index = Random.Range(0, 2);
        BuyableItemData result = allBoxes[index]; 
        Vector3 worldPosition = boxesPanelTransform.TransformPoint(Vector3.zero);

        GameObject newBoxItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, boxesPanelTransform);
        BuyableItemController itemController = newBoxItem.GetComponent<BuyableItemController>();
        newBoxItem.transform.localScale = Vector3.one * 0.9f;
        itemController.Initialize(result);        
    }
    private void ConsumableCardsGenerator()
    {
        List<BuyableItemData> result = new List<BuyableItemData>();

        for (int i = 0; i < 2; i++)
        {
            // Elegir aleatoriamente entre índice 0 y 1
            int index = Random.Range(0, allConsumables.Count);
            result.Add(allConsumables[index]);
        }

        for (int i = 0; i < result.Count; i++)
        {
            Vector3 localPosition = CalculatePositionYCards(i, result.Count, 250f, 0f);
            Vector3 worldPosition = consumablesPanelTransform.TransformPoint(localPosition);

            GameObject newBoxItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, consumablesPanelTransform);
            BuyableItemController itemController = newBoxItem.GetComponent<BuyableItemController>();

            itemController.Initialize(result[i]);
        }

    }
    private void BarGenerator()
    {
        List<BuyableItemData> result = new List<BuyableItemData>();

        for (int i = 0; i < 2; i++)
        {
            // Elegir aleatoriamente entre índice 0 y 1
            int index = Random.Range(0, allComodities.Count);
            result.Add(allComodities[index]);
        }

        for (int i = 0; i < result.Count; i++)
        {
            Vector3 localPosition = CalculatePositionYCards(i, result.Count, 250f, 0f);
            Vector3 worldPosition = barPanelTransform.TransformPoint(localPosition);

            GameObject newBoxItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, barPanelTransform);
            BuyableItemController itemController = newBoxItem.GetComponent<BuyableItemController>();

            itemController.Initialize(result[i]);
        }
    }
    private void VouchesBonfireGenerator()
    {
        List<BuyableItemData> result;
        if (playerDwarfController.gotFusionatorAcces)
        {
            result = new List<BuyableItemData>() {bonfireData};
        }
        else
        {
            result = new List<BuyableItemData>(){fusionatorVoucheData, bonfireData};
        }        

        for (int i = 0; i < result.Count; i++)
        {
            Vector3 localPosition = CalculatePositionX(i, result.Count, vouchesPanelTransform);
            Vector3 worldPosition = vouchesPanelTransform.TransformPoint(localPosition);

            GameObject newBoxItem = Instantiate(buyableItemPrefab, worldPosition, Quaternion.identity, vouchesPanelTransform);
            BuyableItemController itemController = newBoxItem.GetComponent<BuyableItemController>();

            itemController.Initialize(result[i]);
        }
    }
    private Vector3 CalculatePositionX(int position, int total, Transform panel)
    {
        RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
        float panelWidth = panelRectTransform.rect.width;
        float spacing = panelWidth / (total + 1);
        float x = (spacing * (position + 1)) - (panelWidth / 2);

        return new Vector3(x, 0f, 0f);
    }
    private Vector3 CalculatePositionY(int position, int total, Transform panel)
    {
        RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
        float panelHeight = panelRectTransform.rect.height;
        float spacing = panelHeight / (total + 1);
        float y = (panelHeight / 2) - (spacing * (position + 1));

        return new Vector3(0f, y, 0f);
    }
    private Vector3 CalculatePositionYCards(int position, int total, float cardHeight, float margin)
    {
        // Espaciado total necesario para mostrar todas las cartas sin solaparse
        float totalSpacing = (cardHeight + margin) * total;

        // Posición del centro de la carta actual, alineadas desde arriba
        float startY = totalSpacing / 2f - (cardHeight / 2f);
        float y = startY - (position * (cardHeight + margin));

        return new Vector3(0f, y, 0f);
    }    
    public void ShowGearTooltip(GearData gearData)
    {
        // Lo muevo antes de activarlo para que no se vea un efecto raro de desplazamiento del tooltip
        float cameraZoomFactor = 1 / cameraController.cameraZoomFactor();
        Vector3 screenMousePos = Pointer.current.position.ReadValue();
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 0));
        gearTooltipManager.transform.position = new Vector3(mousePosition.x + 0.5f * cameraZoomFactor, mousePosition.y - 0.3f * cameraZoomFactor , 0) ;
        // Entonces activo el gameObject, luego habilito su script y por último llamo a su metodo para mostrar el texto
        gearTooltipManager.gameObject.SetActive(true);
        gearTooltipManager.enabled = true;
        gearTooltipManager.Show(gearData);
    }
    public void HideTooltip()
    {
        gearTooltipManager.gameObject.SetActive(false);
        gearTooltipManager.enabled = false;
    }
    public void UpdateGoldTextOfShop()
    {
        playerDwarfController = PlayerCharacterController.Instance;
        if (goldText == null)
        {
            Debug.Log("goldText == null, seguramente la tienda aún no se inicializó");
            return;
        }
        goldText.text = playerDwarfController.Gold.ToString();  
    }
    private void GoToFusionator()
    {
        HideTransition();
        fusionatorController.OpenFusionator();

    }
    private void OnContinueClick()
    {
        // Volvemos a activar la UI
        // combatManager.toggleUI();
        // Desbloqueamos la playerHand que se queda bloqueada en la última jugada
        playerHandController.ActivatePlayerHand();
        // Destruimos todas las cartas que quedaron en mano antes de hacer zoom
        playerHandController.DestroyAllCards();
        // Zoom Out   
        cameraController.ResetCamera();
        // Se levanta el golem con la siguiente barra de vida
        golemController.IsKnockedOut = false;
        golemController.NextRound();
        // Y se cierra la tienda desactivando su GameObject   
        ShopClose();
        // Barajamos otra vez el mazo
        deckManager.ShuffleDeck();
        playerHandController.turnDrawing();
    }
}
