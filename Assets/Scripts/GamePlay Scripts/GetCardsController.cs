using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetCardsController : MonoBehaviour
{
    public static GetCardsController Instance;
    public Image backgroundImage; // Vinculado desde el Inspector
    public Transform panelTransform; // Vinculado desde el Inspector
    public ShopManager shopManager; // Vinculado desde el Inspector
    public Button continueButton; // Vinculado desde el Inspector
    public Material outlineMat; // Vinculado desde el Inspector
    public GameObject cardPrefab; // Vinculado desde el Inspector    
    private Transform cardTransform; // Vinculado desde el Inspector


    void Awake()
    {
        Instance = this;
    }

    public void StartScene(CardData cardData)
    {
        backgroundImage.enabled = true;
        continueButton.gameObject.SetActive(true);
        InitializeCard(cardData);
        continueButton.onClick.AddListener(CloseScene);
        ShowAnimation();
    }
    private void InitializeCard(CardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, panelTransform);
        RectTransform cardRect = newCard.GetComponent<RectTransform>();
        cardTransform = newCard.transform;
        cardRect.localScale = Vector3.zero;
        cardRect.anchoredPosition = new Vector2(0f, 80f);
        CardController cardComponent = newCard.GetComponent<CardController>();
        cardComponent.Initialize(cardData);
        newCard.name = $"{cardData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
    }
    private void ShowAnimation()
    {
        GraphicRaycaster grCard = cardTransform.GetComponentInChildren<GraphicRaycaster>();
        grCard.enabled = false;
        LeanTween.scale(cardTransform.gameObject, Vector3.one * 2f, 1f)
            .setEase(LeanTweenType.easeOutElastic);

        LeanTween.scale(continueButton.gameObject, Vector3.one, 1f)
            .setEase(LeanTweenType.easeOutElastic);
    }
    private void HideAnimation()
    {
        LeanTween.scale(cardTransform.gameObject, Vector3.zero, 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => 
                {
                    cardTransform.gameObject.SetActive(false);
                }
            );

        LeanTween.scale(continueButton.gameObject, Vector3.zero, 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => 
                {
                    continueButton.gameObject.SetActive(false);
                }
            );
    }

    public void CloseScene()
    {
        HideAnimation();
        backgroundImage.enabled = false;
        continueButton.onClick.RemoveAllListeners();
        shopManager.ReturnDestroyCardsTransition();
    }








}
