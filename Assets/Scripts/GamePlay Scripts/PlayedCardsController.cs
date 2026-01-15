using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
public class PlayedCardsController : MonoBehaviour
{
    public static PlayedCardsController Instance;
    [SerializeField] public float spacing = 10f;
    [Header("Modifiers")]
    [SerializeField] public List<TemporaryModifier> temporaryDefensiveModifiers = new List<TemporaryModifier>();
    [SerializeField] public List<TemporaryModifier> temporaryOffensiveModifiers = new List<TemporaryModifier>();
    [SerializeField] public float cardWidth;
    private float animationSpeed = 0.2f;
    CombatManager combatManager;
    private bool handPlayed = false;
    public bool isProcessingCards = false;
    private PlayerHandController playerHandController;
    private PlayerCharacterController playerDwarfController;
    private SocketController socketController;
    public static event Action CombatReady;
    public static CardController processingCard;
    public bool combatReadyFlag = false;
    public GameObject cardPrefab; // Vinculado desde el Inspector
    
    float cardScalerModifier;
    PlayHandButtonController playHandButtonController;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        combatManager = CombatManager.Instance;
        playerHandController = PlayerHandController.Instance;
        socketController = SocketController.Instance;
        playHandButtonController = PlayHandButtonController.Instance;
        cardScalerModifier = playHandButtonController.cardScalerModifier;
        StartCoroutine(InitializeCardInfo());
    }
    void Update()
    {
        // Esto está escuchando a que la flag cambie para activar todas las cartas, la flag la cambia el botón Play Hand
        if (handPlayed && !isProcessingCards)
        {
            StartCoroutine(ProcessPlayedCards());
            handPlayed = false; // Reseteamos el flag aquí            
        }
    }
    // En este script solo necesitamos el ancho de las cartas
    private IEnumerator InitializeCardInfo()
    {
        // Esperar un frame
        yield return null;
        cardWidth = playerHandController.cardWidth;
    }
    public void UpdatePlayedCardsPositions()
    {
        // Esta cambiado ya que al no poder interactuar con ellas ni haber placaeholder por medio es más fácil y más corto           
        Transform playedCards = transform;
        int totalCards = playedCards.childCount;

        for (int i = 0; i < totalCards; i++)
        {
            Transform card = playedCards.GetChild(i);
            CardController cardController = card.GetComponent<CardController>();
            if (cardController.isMoving) continue;
            Vector3 originalPosition = card.localPosition;
            Vector3 newPosition = CalculateCardPositionX(i, spacing * cardScalerModifier, cardWidth * cardScalerModifier);
            if (originalPosition != newPosition)
            {
                cardController.isMoving = true;
                StartCoroutine(AnimatePosition(card, newPosition));
            }
        }
    }
    private Vector3 CalculateCardPositionX(int index, float spacing, float cardWidth)
    {
        //float cardSpacing = cardWidth + spacing;
        //return new Vector3(index * cardSpacing + cardWidth / 2, 0, 0);
        Transform playedCardsTransform = transform;
        int totalCards = playedCardsTransform.childCount;

        float cardSpacing = cardWidth + spacing;
        float centerOffset = (totalCards - 1) / 2f;
        return new Vector3((index - centerOffset) * cardSpacing, 0, 0);
    }
    // Es el mismo método de animación de la PlayerHand
    private IEnumerator AnimatePosition(Transform card, Vector3 targetPosition)
    {
        CardController cardController = card.GetComponent<CardController>();

        Vector3 initialPosition = card.transform.localPosition;
        float elapsedTime = 0f;
        while (elapsedTime < animationSpeed)
        {
            card.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / animationSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Te aseguras que termina en la posicion deseada
        card.localPosition = targetPosition;
        cardController.isMoving = false;
    }

    public bool HandPlayed
    {
        get => handPlayed; // Proporciona acceso de lectura
        set => handPlayed = value; // Proporciona acceso de escritura
    }

    // MÉTODO QUE ACTIVA LAS CARTAS
    private IEnumerator ProcessPlayedCards()
    {
        playerDwarfController = PlayerCharacterController.Instance;        
        isProcessingCards = true; // Bloqueamos mientras procesamos
        Transform playedCardsTransform = gameObject.transform;

        // Esto es importante
        // Recorremos todas las cartas que se han jugado para ver si hay un arma
        // Si hay un arma ponemos la flag en false para que se reproduzca la animación
        // El flujo del combate tiene que esperar a que se ejecute esta animación de GiveWeapon para empezar el Attack
        // Pero si no hay arma en las cartas jugadas, se queda en true y no tenemos que preocuparnos de esta flag.
        // Ya que nada más termine de procesar las cartas ya se puede atacar.
        combatReadyFlag = true;
        foreach (Transform card in playedCardsTransform)
        {
            CardController controller = card.GetComponent<CardController>();
            CardData cardData = controller.cardData;
            if (cardData.cardType == CardType.Weapon)
            {
                combatReadyFlag = false;
            }
        }

        // PROCESAMIENTO DE CARTAS DE IZQUIERDA A DERECHA
        // Iterará hasta que haya procesado todas las cartas jugadas
        while (playedCardsTransform.childCount > 0)
        {
            Transform card = playedCardsTransform.GetChild(0);
            GameObject cardPlayed = card.gameObject;
            CardController cardController = cardPlayed.GetComponent<CardController>();
            processingCard = cardController;
            CardData cardData = cardController.cardData;
            List<GearEffect> gearEffects = new List<GearEffect>();

            // Esperamos que la carta llegue a su sitio para activarla
            yield return new WaitUntil(() => cardController.isMoving == false);
            // Invocar la habilidad de la carta
            CallAbilityByName(cardPlayed, cardData.cardAbility, cardData, temporaryOffensiveModifiers, temporaryDefensiveModifiers);
            List<CardData> remainingCards = new List<CardData>();
            for (int i = 0; i < playedCardsTransform.childCount; i++)
            {
                CardData remainingCardData = playedCardsTransform.GetChild(i).GetComponent<CardController>().cardData;
                remainingCards.Add(remainingCardData);
            }
            // Ejecutar la animación de fade-out y esperar a que termine
            if (cardData.cardType == CardType.Gem)
            {
                cardController.FadeOutAndDestroy(GetCardEffectType(remainingCards));
            }
            else
            {
                cardController.FadeOutAndDestroy();
            }

            // Espera a que la carta sea destruida     
            yield return new WaitUntil(() => cardPlayed == null);
            if (!cardController.isADuplicate)
            {
                gearEffects = playerDwarfController.GearEffectsToActivateWhenCardPlayed(cardData);
                if (gearEffects.Count > 0)
                {
                    foreach (GearEffect effect in gearEffects)
                    {
                        effect.OnPlayedCard?.Invoke(this);
                    }
                }
            }
            else
            {
                Debug.Log($"This {cardData.cardName} it is a duplicate, no GearEffects triggered");
            }
            UpdatePlayedCardsPositions();
        }
        playerDwarfController.AddElementalPowers(GetActiveOffensiveModifiers(), GetActiveDeffensiveModifiers());
        socketController.StartCoroutine(socketController.AbsorbingElementsAnimation());
        playerDwarfController.StartCoroutine(playerDwarfController.AbsorbingElementsAnimation());

        // Se borran siempre los modificadores.
        ClearTemporaryModifiers();
        // Liberamos la bandera cuando acabemos     
        isProcessingCards = false;

        // Esta espera es obligatoria, necesitamos que se ejecuten las animaciones pertinentes de GiveWeapon y luego la flag se libera
        // Si no es necesario hacer las animations, entonces ya será true.
        // Pero a demás de esas animaciones, hay que esperar a que se absorban los elementos tanto del arma como las resistencias del player
               
        yield return new WaitUntil(() => combatReadyFlag && socketController.isAbsorbed && playerDwarfController.areModifiersAbsorbed);

        playerHandController.RaisePlayerHand();
                    
        CombatReady?.Invoke();     
    }
    public List<CardData> GetNextCardsList(Transform actualCard)
    {
        List<CardData> cardsList = new List<CardData>();
        
        for (int i = actualCard.GetSiblingIndex(); i < transform.childCount; i++)
        {
            Transform card = transform.GetChild(i);           
            CardData cardData = card.GetComponent<CardController>().cardData;
            cardsList.Add(cardData);
        }
        return cardsList;
    }
    public bool GetCardEffectType(List<CardData> cardList)
    {        
        CardData activeCard = cardList[0];
        bool textType = activeCard.defaultTextType;
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
        Debug.Log($"{activeCard.cardName} mode = {textType}");
        return textType;
    }

    // Este metodo consigue invocar otros metodos pasándole el string del nombre del metodo, además soporta parametros.
    void CallAbilityByName(GameObject cardObject, string methodName, CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        // Como además el metodo está en otra class diferente (la CardAbility), no en esta:
        // Obtener la instancia de la clase donde reside el método
        CardAbility cardAbility = cardObject.GetComponent<CardAbility>();
        if (cardAbility == null)
        {
            Debug.LogWarning($"CardAbility no encontrado en {cardObject.name}.");
            return;
        }

        // Buscar el método en esa instancia
        var method = cardAbility.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            // Invoca el método con los parámetros
            method.Invoke(cardAbility, new object[] { cardData, temporaryOffensiveModifiers, temporaryDefensiveModifiers });
        }
        else
        {   // Warning por si no lo encuentra
            Debug.LogWarning($"Método '{methodName}' no encontrado en {cardObject.name}.");
        }
    }
    // Añade 1 modificador temporal a la Lista de esta clase
    public void AddTemporaryOffensiveModifier(TemporaryModifier modifier, Vector3 worldPos)
    {
        temporaryOffensiveModifiers.Add(modifier);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);        
        DisplayTemporaryOffensiveModifier(temporaryOffensiveModifiers.Count - 1, screenPos);
    }
    public void DisplayTemporaryOffensiveModifier(int index, Vector3 screenPos)
    {
        SocketController.Instance.AddWeaponModifier(temporaryOffensiveModifiers[index], screenPos);
    }
    public void DisplayTemporaryDefensiveModifier(int index, Vector3 screenPos)
    {
        playerDwarfController.AddResistanceModifier(temporaryDefensiveModifiers[index], screenPos);
    }
    public void AddTemporaryDefensiveModifier(TemporaryModifier modifier, Vector3 worldPos)
    {
        Debug.Log($"Adding Defensive Modifier {modifier.resistanceElement} = {modifier.damage}");
        temporaryDefensiveModifiers.Add(modifier);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        DisplayTemporaryDefensiveModifier(temporaryDefensiveModifiers.Count - 1, screenPos);
    }

    public void ClearTemporaryModifiers()
    {
        temporaryOffensiveModifiers.Clear();
        temporaryDefensiveModifiers.Clear();
    }
    public int GetLifeStealModifier()
    {
        return temporaryOffensiveModifiers.Sum(modifier => modifier.lifeSteal);
    }
    // Este método devuelve la Lista en forma de diccionario, hay que pensar que es una Lista llena del nuevo tipo TemporaryModifier
    public Dictionary<Element, int> GetActiveOffensiveModifiers()
    {
        Dictionary<Element, int> activeModifiers = new Dictionary<Element, int>();
        foreach (var modifier in temporaryOffensiveModifiers)
        {           
            if (activeModifiers.ContainsKey(modifier.attackElement))
            {
                activeModifiers[modifier.attackElement] += modifier.damage;
            }
            else
            {
                activeModifiers[modifier.attackElement] = modifier.damage;
            }            
        }
        return activeModifiers;
    }
    public Dictionary<Element, int> GetActiveDeffensiveModifiers()
    {
        Dictionary<Element, int> activeModifiers = new Dictionary<Element, int>();
        foreach (var modifier in temporaryDefensiveModifiers)
        {
            if (activeModifiers.ContainsKey(modifier.resistanceElement))
            {
                activeModifiers[modifier.resistanceElement] += modifier.damage;
            }
            else
            {
                activeModifiers[modifier.resistanceElement] = modifier.damage;
            }   
        }
        return activeModifiers;
    }

    public void InstantiateCardOnFirstPlace(CardData cardData, bool duplicate = false)
    {
        GameObject newCard = Instantiate(cardPrefab, transform);        
        CardController cardComponent = newCard.GetComponent<CardController>();
        if (cardComponent != null)
        {
            cardComponent.Initialize(cardData, duplicate);
            newCard.transform.SetSiblingIndex(0);
            newCard.name = $"{cardData.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        }
    }
}
