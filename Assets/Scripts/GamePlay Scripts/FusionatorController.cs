using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FusionatorController : MonoBehaviour
{
    public GameObject runesGO;
    public GameObject goldGO;
    public GameObject playHandButtonGO;
    public GameObject fusionButtonGO;
    public Button fusionButton;
    public Button backToShopButton;
    public GameObject fusionatorPanelGO;
    public TextMeshProUGUI requiredRunesText;
    public GameObject cardPrefab;
    public Transform leftSocket;
    public Transform rightSocket;
    public Transform centralSocket;
    public Transform playerGemsHand;
    public Image background;
    public TextMeshProUGUI runesText;
    public FusionMatrix fusionMatrix;
    public GemLibrary gemLibrary;
    private Material fusionButtonMaterial;
    public Image fusionButtonImage;
    public TextMeshProUGUI fusionButtonText;
    private Color fusionButtonTextColor;
    private List<CardData> allGemsData;
    public Dictionary<(string, string), CardData> fusionMatrixDict;
    private PlayerCharacterController playerDwarfController;
    public DeckManager deckManager;
    public ShopManager shopManager;
    private Transform leftCardTransform;
    private Transform rightCardTransform;
    private bool isAnimationEnd;
    [SerializeField] public CardData leftCard;
    [SerializeField] public CardData rightCard;
    [SerializeField] private Animator animator;
    private float animationSpeed = 0.2f;
    public float spacing = 0f;
    public float cardWidth = 210;




    public void Awake()
    {
        background.enabled = false;
        fusionatorPanelGO.SetActive(false);
        fusionMatrixDict = BuildFusionDict();
        allGemsData = gemLibrary.GetGems();
        fusionButtonMaterial = fusionButtonImage.material;
        fusionButtonMaterial = new Material(fusionButtonMaterial);
        fusionButtonImage.material = fusionButtonMaterial;
        fusionButtonTextColor = fusionButtonText.color;
        UpdateFusionButton();
    }

    private Dictionary<(string, string), CardData> BuildFusionDict()
    {
        Dictionary<(string, string), CardData> fusionDict = new Dictionary<(string, string), CardData>();
        foreach (var fusion in fusionMatrix.entries)
        {
            if (fusion.gemA == null || fusion.gemB == null || fusion.result == null)
            {
                continue;
            }
            string a = fusion.gemA.cardName;
            string b = fusion.gemB.cardName;
            // El siguiente operador ternario me costó entenderlo
            // CompareTo() devuelve 0 si son iguales, Mayor o menor que 0 si uno es lexicográficamente anterior o posterior respectivamente
            // Entonces esto nos permite crear la tupla de la misma manera que la crea la Matrix y ahorrarnos las 2 permutaciones
            // Ya que las fusiones son CONMUTATIVAS
            var key = a.CompareTo(b) <= 0 ? (a, b) : (b, a);
            fusionDict[key] = fusion.result;
        }
        return fusionDict;
    }

    public void OpenFusionator()
    {
        playHandButtonGO.SetActive(false);
        goldGO.SetActive(false);
        background.enabled = true;
        fusionatorPanelGO.SetActive(true);
        fusionButton.onClick.AddListener(DoTheFusion);
        backToShopButton.onClick.AddListener(CloseFusionator);
        playerDwarfController = PlayerCharacterController.Instance;
        runesText.text = playerDwarfController.Runes.ToString();
        LoadGemsDeck();
        UpdateFusionButton();
    }
    public void CloseFusionator()
    {
        playHandButtonGO.SetActive(true);
        goldGO.SetActive(true);
        background.enabled = false;
        fusionButton.onClick.RemoveAllListeners();
        backToShopButton.onClick.RemoveAllListeners();
        RemoveAllCards(() =>
        {
            fusionatorPanelGO.SetActive(false);
            shopManager.ReturnDestroyCardsTransition();
        });
    }
    public CardData Fusion(CardData a, CardData b)
    {
        if (a == null || b == null)
        {
            return null;
        }
        var key = a.cardName.CompareTo(b.cardName) <= 0 ?
                (a.cardName, b.cardName) :
                (b.cardName, a.cardName);

        // En esta línea a la vez que se comprueba si existe el valor dada la key, si se cumple, lo mete en la variable fusionBase
        fusionMatrixDict.TryGetValue(key, out CardData fusionBase);
        int level = ChosenCardLevel(a.cardLevel, b.cardLevel);
        if (fusionBase == null)
        {
            Debug.Log("fusionBase NULLLLL");
            return null;
        }
        else
        {
            return GetGemByNameAndLevel(fusionBase.cardName, level);
        }
    }
    private CardData GetGemByNameAndLevel(string cardName, int level)
    {
        Debug.Log($"Buscando la carta: {cardName} de nivel {level}");
        foreach (CardData cardData in allGemsData)
        {
            if (cardData.cardName == cardName && cardData.cardLevel == level)
            {
                Debug.Log("Encontrada!");
                return cardData;
            }
        }
        return null;
    }
    private int ChosenCardLevel(int levelA, int levelB)
    {
        // Caso igual: sube nivel fijo
        if (levelA == levelB)
            return levelA + 1;

        // Ordénalos
        int low = Mathf.Min(levelA, levelB);
        int high = Mathf.Max(levelA, levelB);

        // Construye la lista de niveles posibles [low, low+1, ..., high]
        int count = high - low + 1;
        var levels = new List<int>(count);
        for (int i = 0; i < count; i++)
            levels.Add(low + i);

        // Genera pesos
        var weights = new List<float>(count);
        if (count == 2)
        {
            // Sólo dos opciones → 50% / 50%
            weights.Add(0.5f);
            weights.Add(0.5f);
        }
        else
        {
            // Más de dos opciones:
            // - al más alto le damos 10%
            // - el resto reparte el 90% a partes iguales
            float share = 0.90f / (count - 1);
            for (int i = 0; i < count - 1; i++)
                weights.Add(share);
            weights.Add(0.10f);
        }

        // Toma un valor aleatorio y lo comparamos con el acumulado
        float r = UnityEngine.Random.value; // [0,1)
        float acc = 0;
        for (int i = 0; i < count; i++)
        {
            acc += weights[i];
            if (r < acc)
                return levels[i];
        }

        // Por seguridad, si hubo redondeo, devuelve el último
        return levels[count - 1];
    }
    private void ChangeButtonMaterialAndText(bool checkFusion)
    {
        if (checkFusion)
        {
            int requiredRunes = GetRequiresRunes();
            requiredRunesText.text = requiredRunes.ToString();
            fusionButtonMaterial.SetFloat("_EnableSaturation", 0f);
            fusionButtonMaterial.DisableKeyword("_ENABLESATURATION_ON");
            fusionButtonText.color = fusionButtonTextColor;
            if (playerDwarfController.Runes >= requiredRunes)
            {
                fusionButton.interactable = true;
                requiredRunesText.color = Color.black;
            }
            else
            {
                fusionButton.interactable = false;
                requiredRunesText.color = Color.red;
            }            
        }
        else
        {
            fusionButton.interactable = false;
            requiredRunesText.text = "0";
            fusionButtonMaterial.EnableKeyword("_ENABLESATURATION_ON");
            fusionButtonMaterial.SetFloat("_EnableSaturation", 1f);
            fusionButtonText.color = Color.grey;
        }
    }
    private int GetRequiresRunes()
    {        
        int a = leftCard.cardLevel;
        int b = rightCard.cardLevel;
        // Esta media aritmética, al sumarle 1 se convierte en una media aritmética que siempre redondea hacia arriba.
        return (a + b + 1) / 2;
    }
    private bool CheckFusion()
    {
        if (leftCard != null && rightCard != null)
        {
            if (Fusion(leftCard, rightCard) != null)
            {
                return true;
            }
        }
        return false;
    }
    public void UpdateFusionButton()
    {
        GetBothCardData();
        ChangeButtonMaterialAndText(CheckFusion());
    }
    private void DoTheFusion()
    {
        StartCoroutine(DoTheFusionRutine());
    }
    private IEnumerator DoTheFusionRutine()
    {
        isAnimationEnd = false;
        fusionButton.interactable = false;
        Debug.Log("BEGIN THE FUSION!");
        GetBothCardData();
        FusionSocketController leftSocketController = leftSocket.GetComponent<FusionSocketController>();
        FusionSocketController rightSocketController = rightSocket.GetComponent<FusionSocketController>();
        CardData fusionResultData = Fusion(leftCard, rightCard);

        playerDwarfController.SpendRunes(GetRequiresRunes());
        runesText.text = playerDwarfController.Runes.ToString();

        yield return new WaitUntil(() => fusionResultData != null);

        deckManager.RemoveCardFromDeck(leftCard);
        deckManager.RemoveCardFromDeck(rightCard);
        yield return null;
        yield return null;

         // Eliminamos tanto el gameObject como la carta del mazo de la carta izquierda
        DestroyImmediate(leftCardTransform.gameObject);
        leftSocketController.RemoveCard();
        leftCard = null;
        // Eliminamos tanto el gameObject como la carta del mazo de la carta derecha
        DestroyImmediate(rightCardTransform.gameObject);
        rightSocketController.RemoveCard();
        rightCard = null;

        animator.SetTrigger("PlayFusion");

        yield return new WaitUntil(() => isAnimationEnd == true);
       

        // Agregamos al mazo la carta resultante de la fusión y la Instanciamos
        deckManager.AddCardToDeck(fusionResultData);
        GameObject fusionCard = Instantiate(cardPrefab, centralSocket);
        CardController fusionCardController = fusionCard.GetComponent<CardController>();
        fusionCard.name = $"{fusionResultData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        fusionCardController.Initialize(fusionResultData);
        yield return new WaitForSeconds(1f);

        Transform fusionCardTransform = fusionCard.transform;
        fusionCardTransform.SetParent(playerGemsHand);
        UpdateFusionatorCardsPositions();

    }

    private void GetBothCardData()
    {
        FusionSocketController leftSocketController = leftSocket.GetComponent<FusionSocketController>();
        FusionSocketController rightSocketController = rightSocket.GetComponent<FusionSocketController>();
        leftCardTransform = leftSocketController.GetCardTransform();
        rightCardTransform = rightSocketController.GetCardTransform();
        leftCard = leftSocketController.GetCardData();
        rightCard = rightSocketController.GetCardData();
    }
    private void RemoveAllCards(Action onComplete)
    {
        foreach (Transform card in playerGemsHand)
        {
            Destroy(card.gameObject);
        }
        if (rightCardTransform != null)
        {
            Destroy(rightCardTransform.gameObject);
        }
        if (leftCardTransform != null)
        {
            Destroy(leftCardTransform.gameObject);
        }
        onComplete?.Invoke();
    }
    private void LoadGemsDeck()
    {
        List<CardData> fullDeck = deckManager.fullDeck;
        List<CardData> onlyGems = new List<CardData>();
        foreach (CardData card in fullDeck)
        {
            if (card.cardType == CardType.Gem)
            {
                onlyGems.Add(card);
            }
        }
        foreach (CardData gem in onlyGems)
        {
            GameObject newGemCard = Instantiate(cardPrefab, playerGemsHand);
            CardController cardController = newGemCard.GetComponent<CardController>();
            cardController.Initialize(gem);
            newGemCard.name = $"{gem.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
            UpdateFusionatorCardsPositions();
        }
        Invoke("UpdateFusionatorCardsPositions", 0.4f);

    }

    public void UpdateFusionatorCardsPositions()
    {
        RectTransform rectTransformPlayerHand = GetComponent<RectTransform>();
        int totalCards = playerGemsHand.childCount;

        for (int i = 0; i < totalCards; i++)
        {
            Transform card = playerGemsHand.GetChild(i);
            CardController cardController = card.GetComponent<CardController>();
            if (cardController.isMoving) continue;

            // Ajustar Sorting Order para que las cartas más a la izquierda estén arriba
            Canvas cardCanvas = card.GetComponentInChildren<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = true; // Asegurar que se pueda sobrescribir el orden
                cardCanvas.sortingOrder = 7 + totalCards - i; // La carta a la izquierda tiene el orden más alto
            }
            Vector3 originalPosition = card.localPosition;
            Vector3 newPosition = CalculateCardPositionX(i, totalCards, rectTransformPlayerHand.rect.width, cardWidth);
            if (card.gameObject.CompareTag("Placeholder"))
            {
                card.localPosition = newPosition;
            }
            else
            {
                if (originalPosition != newPosition)
                {
                    cardController.isMoving = true;
                    AnimatePosition(card, newPosition);
                }
            }
            if (card.tag != "Placeholder")
            {
                cardController.frameTextGlowToggle();
            }

        }
    }

    private Vector3 CalculateCardPositionX(int index, int totalCards, float panelWidth, float cardWidth)
    {
        float availableWidth = panelWidth - cardWidth;
        float minSpacing = availableWidth / totalCards;
        float idealSpacing = Mathf.Min(cardWidth + spacing, minSpacing);
        float centerOffset = (totalCards - 1) / 2f;
        return new Vector3((index - centerOffset) * idealSpacing, 0, 0);
    }

    private void AnimatePosition(Transform card, Vector3 targetPosition)
    {
        CardController cardController = card.GetComponent<CardController>();

        // Cancelar cualquier animación previa para evitar conflictos
        LeanTween.cancel(card.gameObject);

        // Calcular la distancia entre la posición actual y la de destino
        float distance = Vector3.Distance(card.localPosition, targetPosition);
        float adjustedSpeed = Mathf.Clamp(distance / 100f, 0.1f, animationSpeed);
        // Asegurar que no sea demasiado rápido ni demasiado lento
        Image cardImage = cardController.cardImage;
        float normalizedZ = cardImage.transform.localEulerAngles.z;
        if (normalizedZ > 180f) normalizedZ -= 360f;    
        if (Math.Abs(normalizedZ) > 2.5f)
        {
            LeanTween.rotateZ(cardImage.gameObject, 0f, 0.25f).setEaseOutSine();
        }
        // Iniciar la animación con LeanTween
        LeanTween.moveLocal(card.gameObject, targetPosition, adjustedSpeed)
            .setEase(LeanTweenType.easeOutQuad) // Suavizado natural sin rebote
            .setOnComplete(() =>
            {
                if (card != null)
                {
                    card.localPosition = targetPosition; // Asegurar que termine exactamente en la posición
                    cardController.currentPosition = targetPosition;
                    cardController.isMoving = false; // Marcar que la carta terminó su animación
                    //cardController.StartFloatingEffect();
                }
            });
    }

    public void HandleFusionEnd()
    {
        isAnimationEnd = true;
    }


}
