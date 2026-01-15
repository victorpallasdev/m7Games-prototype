using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Configuration")]
    public static DeckManager Instance;
    public bool isDeckLoaded = false;
    public List<CardData> fullDeck = new List<CardData>();  // Mazo original completo
    public List<CardData> roundDeck { get; private set; } = new List<CardData>(); // Mazo actual de la ronda
    public GameObject cardPrefab; // Prefab de la carta
    public Transform playerHand; // Transform donde se instancian las cartas
    public Transform destroyCardsTransform; // Vinculado desde el inspector
    public TextMeshProUGUI displayNumberOfCardsText;
    public SoundsFXManager soundFXManager;
    public int CardsOnFullDeck { get; private set; }
    public int CardsOnDeck { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    /// Roba una carta del mazo de la ronda
    public void DrawCard()
    {

        soundFXManager = SoundsFXManager.Instance;
        soundFXManager.PlayCardSound();
        if (roundDeck.Count == 0)
        {
            Debug.LogWarning("El mazo de la ronda está vacío. No se pueden robar más cartas.");
            return;
        }
        // Obtener la primera carta del mazo de la ronda
        CardData drawnCardData = roundDeck[0];
        // Instanciar la carta en la PlayerHand
        GameObject newCard = Instantiate(cardPrefab, playerHand);
        CardController cardComponent = newCard.GetComponent<CardController>();
        if (cardComponent != null)
        {
            cardComponent.Initialize(drawnCardData);
            newCard.transform.SetSiblingIndex(0);
            newCard.name = $"{drawnCardData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        }
        // Eliminar la carta de roundDeck (ya ha sido robada)
        roundDeck.RemoveAt(0);
        CardsOnDeck = roundDeck.Count;
        UpdateNumberOfCardsText();
    }

    public void LoadFullDeck(List<CardData> deck)
    {
        fullDeck = new List<CardData>(deck);
        CardsOnFullDeck = fullDeck.Count;
        isDeckLoaded = true;
    }

    public void PutCardOnHand(CardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, playerHand);
        CardController cardComponent = newCard.GetComponent<CardController>();
        if (cardComponent != null)
        {
            cardComponent.Initialize(cardData);
            newCard.transform.SetSiblingIndex(0);
            newCard.name = $"{cardData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        }
    }
    /// <summary>
    /// Roba múltiples cartas al inicio de la ronda
    /// </summary>
    public IEnumerator DrawNumberOfCards(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            DrawCard();
            yield return new WaitForSeconds(0.05f);
        }
    }
    public List<CardData> GetSortedRoundDeck()
    {
        return roundDeck
            .OrderBy(card => card.cardName)
            .ThenBy(card => card.cardLevel)
            .ToList();
    }

    public void InstantiateACardOnPlayerHand(CardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, playerHand);
        CardController cardController = newCard.GetComponent<CardController>();
        PlayerHandController playerHandController = FindFirstObjectByType<PlayerHandController>();

        cardController.Initialize(cardData);
        playerHandController.UpdatePlayerHandPositions();
    }
    public Transform DrawCardToDestroy()
    {
        // Obtener la primera carta del mazo
        CardData drawnCardData = roundDeck[0];
        // Instanciar la carta en el destroyCardsPanel
        GameObject newCard = Instantiate(cardPrefab, destroyCardsTransform);
        RectTransform rect = newCard.GetComponent<RectTransform>();
        RectTransform parentRect = destroyCardsTransform.GetComponent<RectTransform>();
        float offscreenX = parentRect.rect.width + (rect.rect.width / 2f);
        float offscreenY = -(parentRect.rect.height / 2f) - (rect.rect.height / 2f);
        rect.anchoredPosition = new Vector2(offscreenX, offscreenY);
        CardController cardComponent = newCard.GetComponent<CardController>();
        if (cardComponent != null)
        {
            cardComponent.Initialize(drawnCardData);
            newCard.transform.SetSiblingIndex(0);
            newCard.name = $"{drawnCardData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        }
        // Eliminar la carta de roundDeck (ya ha sido robada)
        roundDeck.RemoveAt(0);
        CardsOnDeck = roundDeck.Count;
        UpdateNumberOfCardsText();
        return newCard.transform;        
    }

    /// <summary>
    /// Añade una carta nueva al mazo original y la tendrá disponible en futuras rondas
    /// </summary>
    public void AddCardToDeck(CardData newCardData)
    {
        fullDeck.Add(newCardData);
        Debug.Log($"Se ha añadido la carta {newCardData.cardName} al mazo.");
        CardsOnFullDeck = fullDeck.Count;
        UpdateNumberOfCardsText();
    }
    public void RemoveCardFromDeck(CardData cardData)
    {
        fullDeck.Remove(cardData);
        Debug.Log($"Carta eliminada del mazo: {cardData.cardName}");
        UpdateNumberOfCardsText();
    }
    public void ReplaceCardInDeck(CardData oldCard, CardData newCard)
    {
        int index = fullDeck.IndexOf(oldCard);
        fullDeck[index] = newCard;
        Debug.Log($"Carta reemplazada: {oldCard.cardName} → {newCard.cardName}");
        UpdateNumberOfCardsText();
    }

    /// <summary>
    /// Reinicia el mazo de la ronda con todas las cartas del mazo original y lo baraja
    /// </summary>
    public void ShuffleDeck()
    {
        roundDeck = new List<CardData>(fullDeck); // Copiar todas las cartas del mazo original

        // Algoritmo Fisher-Yates para barajar el mazo
        // Todas las permutaciones son equiprobables
        // Porqué son equiprobables:
        // Fisher-Yates se recorre de atrás a alante y además restringe el intercambio a los index que no han sido ya intercambiados.
        // int randomIndex = Random.Range(0, i + 1); esto es la clave para que no esté sesgado como un iterador que intercambie
        // posiciones como yo habría hecho. Ya que da a pie a que muchas veces las cartas vuelvan a su posición original.
        // eso provoca que haya permutaciones mas comunes y otras raras o imposibles.
        for (int i = roundDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = roundDeck[i];
            roundDeck[i] = roundDeck[randomIndex];
            roundDeck[randomIndex] = temp;
        }
        Debug.Log("El mazo ha sido mezclado.");
        CardsOnDeck = roundDeck.Count;
    }

    private void UpdateNumberOfCardsText()
    {
        displayNumberOfCardsText.text = $"{CardsOnDeck} / {CardsOnFullDeck}";
    }
}
