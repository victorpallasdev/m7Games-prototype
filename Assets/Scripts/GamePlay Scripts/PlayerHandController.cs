using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;


public class PlayerHandController : MonoBehaviour
{
    public static PlayerHandController Instance;
    [Header("Settings to modify")] 
    [SerializeField] public float spacing = 0f;
    [SerializeField] private int maxCardsInHand = 8;
    [SerializeField] private int  maxCardsSelected = 5;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private float selectedHeightPercent = 0.15f;


    [Header("Only monitoring")] 
    [SerializeField] public float cardWidth = 210;
    [SerializeField] public float cardHeight; 
    [SerializeField] private bool isBlocked = false;  
    public bool updatesBlocked = false;
    DeckManager deckManager;
    PlayHandButtonController playHandButtonController;
    PlayerCharacterController playerDwarfController;
    float cardScalerModifier;


    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        deckManager = DeckManager.Instance;
        playHandButtonController = PlayHandButtonController.Instance;
        playerDwarfController = PlayerCharacterController.Instance;
        cardScalerModifier = playHandButtonController.cardScalerModifier;
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleBlockPlayerHand()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (!isBlocked)
        {           
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            isBlocked = true;
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            isBlocked = false;
        }
    }

    public void ActivatePlayerHand()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        isBlocked = false;
    }
    public void LowerPlayerHand()
    {
        RectTransform handRect = GetComponent<RectTransform>();
        Vector2 currentPos = handRect.anchoredPosition;
        Vector2 targetPos = new Vector2(currentPos.x, -310f);

        LeanTween.moveLocalY(handRect.gameObject, targetPos.y, 0.5f)
            .setEase(LeanTweenType.easeInOutSine);
    }
    public void RaisePlayerHand()
    {
        RectTransform handRect = GetComponent<RectTransform>();
        Vector2 currentPos = handRect.anchoredPosition;
        Vector2 targetPos = new Vector2(currentPos.x, 0f); // Ajusta si es otro valor

        LeanTween.moveLocalY(handRect.gameObject, targetPos.y, 0.5f)
            .setEase(LeanTweenType.easeInOutSine);
    }

    public void turnDrawing()
    {
        int cardsToDraw = maxCardsInHand - cardsInHandCount();
        // Debug.Log($"cardsToDraw = {cardsToDraw} // maxCardsInHand = {maxCardsInHand} // cardsInHandCount = {cardsInHandCount()}");
        StartCoroutine(deckManager.DrawNumberOfCards(cardsToDraw));      
        UpdatePlayerHandPositions();        
    }

    private int cardsInHandCount()
    {
        return gameObject.transform.childCount;
    }
    public void DelayedUpdatePlayerHandPositions(float delay)
    {
        StartCoroutine(DelayUpdatePositions(delay));
    }
    private IEnumerator DelayUpdatePositions(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdatePlayerHandPositions();
    }

    public void UpdatePlayerHandPositions()
    {            
        Transform playerHand = transform;
        RectTransform rectTransformPlayerHand = GetComponent<RectTransform>();
        int totalCards = playerHand.childCount;

        for (int i = 0; i < totalCards; i++)
        {                
            Transform card = playerHand.GetChild(i);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            CardController cardController = card.GetComponent<CardController>();
            if(cardController.isMoving) continue;

            // Ajustar Sorting Order para que las cartas más a la izquierda estén arriba
            Canvas cardCanvas = card.GetComponentInChildren<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = true; // Asegurar que se pueda sobrescribir el orden
                cardCanvas.sortingOrder = 7 + totalCards - i; // La carta a la izquierda tiene el orden más alto
            }
            Vector2 originalPosition = cardRect.anchoredPosition;
            Vector2 newPosition = CalculateCardPositionX(i, totalCards, rectTransformPlayerHand.rect.width, cardWidth);
            if (cardController.IsSelected)
            {
                newPosition.y += cardHeight * selectedHeightPercent;
            }
            if (card.gameObject.CompareTag("Placeholder"))
            {
                cardRect.anchoredPosition = newPosition;
            }
            else
            {
                if (originalPosition != newPosition)
                {
                    cardController.isMoving = true;
                    AnimatePosition(cardRect, newPosition);                    
                }                
            }
            if (card.tag != "Placeholder")
            {
                cardController.frameTextGlowToggle(); 
            }
                      
        }
    }
    private Vector2 CalculateCardPositionX(int index, int totalCards, float panelWidth, float cardWidth)
    {
        float availableWidth = panelWidth - cardWidth;
        float minSpacing = availableWidth / totalCards;
        float idealSpacing = Mathf.Min(cardWidth + spacing, minSpacing);
        float centerOffset = (totalCards - 1) / 2f;
        return new Vector2((index - centerOffset) * idealSpacing, 0);
    }
    private void AnimatePosition(RectTransform card, Vector2 targetPosition)
    {
        CardController cardController = card.GetComponent<CardController>();
        // Cancelar la animación de flotar y la animación de movimiento previas para evitar conflictos
        cardController.CancelMovingAnimationEffect();
        cardController.CancelFloatingEffect();

        // Calcular la distancia entre la posición actual y la de destino
        float distance = Vector3.Distance(card.anchoredPosition, targetPosition);
        float adjustedSpeed = Mathf.Clamp(distance / 100f, 0.1f, animationSpeed);
        // Asegurar que no sea demasiado rápido ni demasiado lento
        // Arreglar su rotacion si hace falta
        // Pero es la cardImage lo que rota.
        Image cardImage = cardController.cardImage;
        float normalizedZ = cardImage.transform.localEulerAngles.z;
        if (normalizedZ > 180f) normalizedZ -= 360f;
        if (Math.Abs(normalizedZ) > 2.5f)
        {
            LeanTween.rotateZ(cardImage.gameObject, 0f, 0.25f).setEaseOutSine();
        }
        // Iniciar la animación con LeanTween

        cardController.movingTweenId = LeanTween.move(card, targetPosition, adjustedSpeed)
            .setEase(LeanTweenType.easeOutQuad) // Suavizado natural sin rebote
            .setOnComplete(() =>
            {
                if (card != null)
                {
                    card.localPosition = targetPosition; // Asegurar que termine exactamente en la posición
                    cardController.currentPosition = targetPosition;
                    cardController.isMoving = false; // Marcar que la carta terminó su animación
                    cardController.StartFloatingEffect();
                }
            })
            .id;
    }
    public List<CardData> GetSelectedCardsList(Transform actualCard)
    {
        List<CardData> cardsList = new List<CardData>();
        
        for (int i = actualCard.GetSiblingIndex(); i < transform.childCount; i++)
        {
            //Debug.Log($"Actual index = {i}");
            Transform card = transform.GetChild(i);
            CardController cardController = card.GetComponent<CardController>();            
            if(cardController.IsSelected)
            {
                CardData cardData = card.GetComponent<CardController>().cardData;
                cardsList.Add(cardData);
            }
        }
        return cardsList;
    }
    public List<CardController> GetAllSelectedCardsList()
    {
        List<CardController> cardsList = new List<CardController>();
        for (int i = 0; i < transform.childCount ; i++)
        {
            // Debug.Log($"Actual index = {i}");
            Transform card = transform.GetChild(i);
            CardController cardController = card.GetComponent<CardController>();            
            if(cardController.IsSelected)
            {                
                cardsList.Add(cardController);
            }
        }
        return cardsList;
    }

    public bool GetCardEffectType(List<CardData> cardList)
    {
        //Debug.Log($"cardList = {cardList.Count}");
        CardData activeCard = cardList[0];
        bool textType = activeCard.defaultTextType;

        if (activeCard.cardType == CardType.Weapon || activeCard.cardType == CardType.Support || activeCard.cardType == CardType.DefensiveGem || activeCard.cardType == CardType.Gadget)
        {
            return textType;
        }

        for (int i = 1; i < cardList.Count; i++)
        {
            if (cardList[i].cardType == CardType.Weapon)
            {
                return false;
            }
            else if (cardList[i].cardType == CardType.DefensiveGem)
            {
                return true;
            }
        }
        return textType;       
    }
  

    public void DestroyAllCards()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) // Recorrer en orden inverso
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    public int MaxCardsSelected
    {
        get => maxCardsSelected; // Proporciona acceso de lectura
        set => maxCardsSelected = value; // Proporciona acceso de escritura
    }
    public int MaxCardsInHand
    {
        get => maxCardsInHand; // Proporciona acceso de lectura
        set => maxCardsInHand = value; // Proporciona acceso de escritura
    }


}
