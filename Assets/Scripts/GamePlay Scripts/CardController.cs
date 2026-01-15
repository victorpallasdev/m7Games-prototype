using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem; // New Input System
using TMPro;
using System.Linq;


public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Card UI Elements")]
    public TextMeshProUGUI nameText;       // Referencia al texto del nombre
    public TextMeshProUGUI attackText;  
    public TextMeshProUGUI deffenseText;
    public LocalizedText nameKey;
    public Image cardImage;               // Referencia a la imagen de la carta
    public Image cardClickGraphic;
    public List<Image> starsList;

    [Header("Settings of Idle Animation")] 
    [SerializeField] public float rotationAmountX = 20f;
    [SerializeField] public float rotationAmountY = 20f;
    [SerializeField] public float rotationAmountZ = 2.5f;

    [SerializeField] public float baseDuration = 3f;
    [SerializeField] public float durationVariation = 0.5f;

    private Vector3 originalRotation;

    [Header("Card Data")]
    public CardData cardData;             // Referencia al ScriptableObject de esta carta

    [Header("Effects Settings")]  
    [SerializeField] private float fadeOutSpeed = 0.4f;
    [SerializeField] public float animationSpeed = 0.6f;
    [SerializeField] public float expandedScale = 1.15f;

    [Header("Only monitoring")] 
    private int maxCardsSelected;
    private float spacing;
    [SerializeField] public float cardWidth;
    [SerializeField] private float cardHeight;
    private RectTransform cardRectTransform;
    private RectTransform playerHandRectTransform;
    private PlayerHandController playerHandController;    
    public bool isMoving = false;
    public bool IsHovered;
    public bool isADuplicate = false;
    [SerializeField] private bool isSelected = false;
    [SerializeField] private bool isPlayed = false;
    private AssetReference placeholderPrefabReference;
    private Transform playerHandTransform;
    private Transform cardTransform;
    public Vector3 originalScale;
    private static GameObject placeholder;
    private Canvas mainCanvas;
    private RectTransform canvasRectTransform;
    public bool isDraggingThisCard = false;
    public static CardController CurrentDraggingCard { get; private set; }
    private bool readyToDrag = false;
    private bool placeHolderCreated = false;
    public bool IsProcesed = false;
    public GameObject attackTextGO;   // Vinculado desde el inspector
    public GameObject deffenseTextGO;   // Vinculado desde el inspector
    public CardData upgradedCard;
    public CardType cardType;
    public Vector3 currentPosition;
    public Material outlineCardMaterial;    
    private bool isOnFusionator = false;
    private bool isOnDestroyPanel = false;
    private ConsumablesController consumablesController;
    private DestroyCardsController destroyCardsController;
    private ChooseCardsController chooseCardsController;
    private FusionatorController fusionatorController;
    private Transform fusionatorHandTransform;
    private FusionSocketController lastSocket;
    private GraphicRaycaster graphicRaycasterSockets;
    private CanvasGroup canvasGroup;
    private Transform socketDetectado;
    private FusionSocketController[] fusionSocketsArray;
    private List<FusionSocketController> allFusionSockets;
    private Camera mainCamera;
    private int hoverTweenId = -1; // Guardamos el ID de la animación de hover
    private int dehoverTweenId = -1; // Guardamos el ID de la animación de dehover
    private int floatTweenId = -1;
    public int movingTweenId = -1;
    private CardInspectionManager cardInspectionManager;
    private Coroutine holdCoroutine;
    private SoundsFXManager soundsFXManager;
    private const float holdThreshold = 1f;
    private static readonly List<RaycastResult> s_RaycastResults  = new List<RaycastResult>(8);

    public void Initialize(CardData data, bool duplicate = false)
    {
        soundsFXManager = SoundsFXManager.Instance;
        cardTransform = transform;
        originalScale = Vector3.one;
        playerHandController = PlayerHandController.Instance;
        playerHandTransform = playerHandController.transform;
        playerHandRectTransform = playerHandTransform.GetComponent<RectTransform>();
        maxCardsSelected = playerHandController.MaxCardsSelected;
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        consumablesController = ConsumablesController.Instance;
        destroyCardsController = DestroyCardsController.Instance;
        chooseCardsController = ChooseCardsController.Instance;
        outlineCardMaterial = new Material(outlineCardMaterial);
        mainCanvas = GetMainCanvas();
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        cardRectTransform = GetComponent<RectTransform>();
        CameraController camController = CameraController.Instance;
        mainCamera = camController.transform.GetComponent<Camera>();
        cardInspectionManager = CardInspectionManager.Instance;
        originalRotation = transform.localEulerAngles;


        if (!gameObject.CompareTag("Placeholder"))
        {
            attackTextGO.SetActive(false);
            deffenseTextGO.SetActive(false);
        }

        //StartCoroutine(InitializeCardInfoAndPosition());



        InitializePlaceholderReference();
        //StartFloatingEffect();

        cardData = data;
        nameKey.key = cardData.cardName;
        cardImage.sprite = cardData.cardImage;
        attackText.text = cardData.attackText;
        deffenseText.text = cardData.deffenseText;
        cardType = cardData.cardType;
        isADuplicate = duplicate;
        upgradedCard = cardData.upgradedCard;
        if (cardType == CardType.Gadget)
        {
            SetAllTextsToWhite();
        }
        if (transform.parent.CompareTag("Fusionator"))
        {
            isOnFusionator = true;
            GameObject fusionatorHandGO = GameObject.Find("Shop Panel/Canvas/Fusionator Canvas/Fusionator Panel/Player Gem Cards");
            fusionatorHandTransform = fusionatorHandGO.transform;
            fusionatorController = FindFirstObjectByType<FusionatorController>();
            fusionSocketsArray = FindObjectsByType<FusionSocketController>(FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
            allFusionSockets = fusionSocketsArray.ToList();
            GameObject raycasterGO = GameObject.Find("Shop Panel/Canvas/Fusionator Canvas/Fusionator Panel/Canvas Sockets");
            graphicRaycasterSockets = raycasterGO.GetComponent<GraphicRaycaster>();
        }
        if (transform.parent.CompareTag("DestroyPanel"))
        {
            isOnDestroyPanel = true;
        }
        ActivateStars();
        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        playerHandController.DelayedUpdatePlayerHandPositions(0.2f);

    }

    private void ActivateStars()
    {
        for (int i = 0; i < 4; i++)
        {
            starsList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < cardData.cardLevel; i++)
        {
            starsList[i].gameObject.SetActive(true);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        soundsFXManager.PlayCardSound();
        StartScaleAnimation(ref hoverTweenId, ref dehoverTweenId, originalScale * expandedScale, LeanTweenType.easeOutElastic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
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
        float duration = baseDuration + Random.Range(-durationVariation, durationVariation);

        Vector3 targetRotation = new Vector3(
            originalRotation.x + Random.Range(-rotationAmountX, rotationAmountX),
            originalRotation.y + Random.Range(-rotationAmountY, rotationAmountY),
            originalRotation.z + Random.Range(-rotationAmountZ, rotationAmountZ)
        );

        floatTweenId = LeanTween.rotateLocal(cardImage.gameObject, targetRotation, duration)
        .setEaseInOutSine()
        .setOnComplete(() => StartFloatingEffect())
        .id;
    }
    public void CancelFloatingEffect()
    {
        LeanTween.cancel(floatTweenId);
    }
    public void CancelMovingAnimationEffect()
    {
        LeanTween.cancel(floatTweenId);
    }
    public void ResetRotation()
    {
        cardImage.transform.localRotation = Quaternion.identity;
    }
    private IEnumerator InitializeCardInfoAndPosition()
    {
        // Esperar un frame
        yield return null;
        cardWidth = playerHandController.cardWidth;
        cardHeight = playerHandController.cardHeight;
        spacing = playerHandController.spacing;
        playerHandController.UpdatePlayerHandPositions();
    }
    private void InitializePlaceholderReference()
    {
        // Inicializa el prefab gracias al Addressables
        string placeholderKey = "CardPlaceholder";
        placeholderPrefabReference = new AssetReference(placeholderKey);
        placeholderPrefabReference.LoadAssetAsync<GameObject>().Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                placeholder = handle.Result;
            }
            else
            {
                Debug.LogError($"No se pudo cargar el prefab Addressable con el nombre '{placeholderKey}'.");
            }
        };
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (transform.parent.CompareTag("FusionSocket"))
        {
            lastSocket = transform.parent.GetComponent<FusionSocketController>();
        }
        else
        {            
            lastSocket = null;
        }
        // Bloquear clics si una animación está en curso, sinó hace efectos raros
        if (isMoving)
        {
            //Debug.Log("Clicks bloqueados!!");
            return;
        }
        
        
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            cardInspectionManager.ShowDetails(cardData, transform.position);
            StartScaleAnimation(ref dehoverTweenId, ref hoverTweenId, originalScale, LeanTweenType.easeOutElastic);
            return;
        }
      

        holdCoroutine = StartCoroutine(HoldToShowDetails());
      
        
        // Si los clics no estan bloqueados entonces está lista para hacer el arrastre
        readyToDrag = true;
        // En móviles, bloquear múltiples toques para evitar errores
        if (eventData.pointerId > 0) return;      
        // Lo siguiente no hace falta.      
        //transform.localPosition = mainCanvas.transform.InverseTransformPoint(globalPosition);
    }
    private IEnumerator HoldToShowDetails()
    {
        float timer = 0f;

        while (timer < holdThreshold)
        {
            if (isDraggingThisCard) yield break;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        cardInspectionManager.ShowDetails(cardData, transform.position);
        StartScaleAnimation(ref dehoverTweenId, ref hoverTweenId, originalScale, LeanTweenType.easeOutElastic);
    }
    public void OnDrag(PointerEventData eventData)
    {
        CardDragVisual dragVisual = GetComponent<CardDragVisual>();
        // Si no estaba lista para hacer el arrastre vuelven las variables a false y no se arrastra.
        if (isMoving || !readyToDrag || isOnDestroyPanel)
        {
            readyToDrag = false;
            isDraggingThisCard = false;
            CurrentDraggingCard = null;
            return;
        }

        canvasGroup.blocksRaycasts = false;
        if (!placeHolderCreated && placeholderPrefabReference != null)
        {
            // Instanciamos el placeholder según la referencia al prefab
            GameObject newPlaceholder;
            if (isOnFusionator)
            {
                newPlaceholder = Instantiate(placeholder, fusionatorHandTransform, false);
            }
            else
            {
                newPlaceholder = Instantiate(placeholder, playerHandTransform, false);
            }
            newPlaceholder.name = "CardPlaceholder";
            newPlaceholder.tag = "Placeholder";
            // Lo posiciona en el mismo sitio que estaba la carta
            newPlaceholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
            newPlaceholder.transform.localPosition = transform.localPosition;
            // Actualiza la referencia al nuevo placeholder
            placeholder = newPlaceholder;
            // Cogemos su posicion global antes de nada
            Vector3 globalPosition = transform.position;
            // Cancelamos la animacion de floating
            LeanTween.cancel(floatTweenId);
            // Pasamos la carta al Canvas Principal
            transform.SetParent(mainCanvas.transform, true);
            // Una vez en el canvas principal la dejamos en la misma posicion global
            transform.position = globalPosition;
            placeHolderCreated = true;
        }
        dragVisual.StartDragging();
        // Como se empieza a arrastrar establecemos la bandera de que sí está arrastrando esta carta.
        isDraggingThisCard = true;
        // Tambien se establece el objeto de esta carta como la que se está arrastrando en la variable static para futuros usos.
        CurrentDraggingCard = this;
        // Y reseteamos su rotacion para que no atraviese el fondo.

        // Aquí seteamos el sorting order para que renderice la carta que estamos arrastrando encima de las demas
        // Obtenemos el sorting order más alto de las cartas de la mano utilizando el mismo calculo que para organizarlas.
        int totalCardsInHand = playerHandController.transform.childCount;
        int maxSortingOrder = 7 + totalCardsInHand;
        // Obtenemos el canvas de esta carta arrastrada
        Canvas cardCanvas = GetComponentInChildren<Canvas>();
        // Y simplemente hacemos que sea 1 nivel mas alto en el sorting order.
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = maxSortingOrder + 1;
        LeanTween.cancel(gameObject);
        // Cogemos la posición del ratón
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
            out Vector2 localPointerPosition
        );
        // se la pasamos a la carta para que siga al ratón
        dragVisual.UpdateDraggedPosition(eventData.position, canvasRectTransform);
        // newIndex para monitorear el indice del placeholder según donde está el ratón
        int newIndex;
        if (isOnFusionator)
        {
            foreach (FusionSocketController socket in allFusionSockets)
            {
                if (!socket.HasCard())
                {
                    socket.ActiveGlow();
                }
            }
            RectTransform fusionatorHandRectTransform = fusionatorHandTransform.GetComponent<RectTransform>();
            newIndex = GetNextCardIndex(localPointerPosition.x, fusionatorHandTransform.childCount, fusionatorHandRectTransform.rect.width, cardWidth);
        }
        else
        {
            newIndex = GetNextCardIndex(localPointerPosition.x, playerHandTransform.childCount, playerHandRectTransform.rect.width, cardWidth);
        }
        // Buscamos la referencia al gameobject placeholder por su tag
        placeholder = GameObject.FindWithTag("Placeholder");
        // Movemos el placeholder a su posicion según el índice que le toca
        if (placeholder != null && placeholder.transform.GetSiblingIndex() != newIndex)
        {
            placeholder.transform.SetSiblingIndex(newIndex);
        }
        // Actualizamos la posicion de las cartas en cada tick del arrastre.
        if (isOnFusionator)
        {
            fusionatorController.UpdateFusionatorCardsPositions();
        }
        else
        {
            playerHandController.UpdatePlayerHandPositions();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        CardDragVisual dragVisual = GetComponent<CardDragVisual>();
        socketDetectado = null;
        FusionSocketController fs = null;
        PlayerHandController handController = FindFirstObjectByType<PlayerHandController>();
        FusionatorController fusionatorController = FindFirstObjectByType<FusionatorController>();
        // Si esta carta no se estaba arrastrando, representa que ha sido un click de selección
        dragVisual.StopDragging();

        if (!isDraggingThisCard && !isOnFusionator && !isOnDestroyPanel)
        {
            ToggleSelection();
            handController.UpdatePlayerHandPositions();
        }
        else if (!isDraggingThisCard && (isOnFusionator || isOnDestroyPanel))
        {
            ToggleSelection();
            return;
        }
        else if (isOnFusionator)
        {
            PointerEventData pd = new PointerEventData(EventSystem.current) { position = eventData.position };
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycasterSockets.Raycast(pd, results);

            foreach (var rr in results)
            {
                fs = rr.gameObject.GetComponent<FusionSocketController>();
                if (fs != null)
                {
                    socketDetectado = fs.transform;
                }
            }
        }
        // Como el placeholder se crea siempre que clicamos, aunque sea rápido, hay que eliminarlo siempre
        GameObject existingPlaceholder = GameObject.FindGameObjectWithTag("Placeholder");
        if (existingPlaceholder != null)
        {
            int indexPlaceholder = existingPlaceholder.transform.GetSiblingIndex();
            DestroyImmediate(existingPlaceholder);
            // Colocar la carta en el PlayerHand y asignar el índice del placeholder
            if (isOnFusionator && socketDetectado != null)
            {
                FusionSocketController socketController = socketDetectado.GetComponent<FusionSocketController>();
                if (socketController.HasCard())
                {
                    socketController.QuitCard();
                }
                transform.SetParent(socketDetectado, true);
                fs.CardPositioner(transform);
            }
            else if (isOnFusionator)
            {
                transform.SetParent(fusionatorHandTransform, true);
                if (lastSocket != null)
                {
                    lastSocket.RemoveCard();
                }

            }
            else
            {
                transform.SetParent(playerHandTransform, true);
            }

            transform.SetSiblingIndex(indexPlaceholder);
        }
        placeHolderCreated = false;
        // Sobretodo actualizar la referencia cuando lo destruimos
        InitializePlaceholderReference();

        // Y actualizar las posiciones siempre que se suelta o se selecciona una carta
        if (isOnFusionator)
        {
            fusionatorController.UpdateFusionatorCardsPositions();
        }
        else if(!isOnDestroyPanel)
        {
            handController.UpdatePlayerHandPositions();
        }
        // Al soltar o seleccionar una carta, vuelven todas las flags a false
        readyToDrag = false;
        isDraggingThisCard = false;
        CurrentDraggingCard = null;
        // Esto las vuelve a posicionar por si se ha bugueado alguna
        // Alomejor habría que hacer un método que verificara las posiciones según el índice rápidamente, y recolocara las que estan mal.
        // Sería lo más elegante...
        if (isOnFusionator)
        {
            foreach (FusionSocketController socket in allFusionSockets)
            {
                socket.DeActiveGlow();
            }
            fusionatorController.Invoke("UpdateFusionatorCardsPositions", 0.2f);
        }
        else if (!isOnDestroyPanel)
        {
            handController.Invoke("UpdatePlayerHandPositions", 0.2f);
        }
        canvasGroup.blocksRaycasts = true;
        // Importante, como en Android el cursor no se queda en pantalla, sinó que son taps en pantalla
        // Hacemos que se dehovereen las cartas al soltar clic.
        if (Application.platform == RuntimePlatform.Android)
        {
            StartScaleAnimation(ref dehoverTweenId, ref hoverTweenId, originalScale, LeanTweenType.easeOutElastic);
        }          
    }

    private void ToggleSelection()
    {
        // Comprueba si la carta está en el panel de destruir cartas o no
        if (transform.parent.gameObject.CompareTag("DestroyPanel"))
        {
            // Solo se puede destruir una, así que solo se puede seleccionar una
            // Por lo tanto se desseleccionan las demás, es más elegante
            if(!IsSelected)
            {
                IsSelected = true;
                DeSelectAllOtherCards();
                ApplyOutline();
                Debug.Log("Carta seleccionada para destruir");
            }
            else if(IsSelected)
            {            
                IsSelected = false;
                ApplyOutline();
            }
            destroyCardsController.CheckIfSelected();
        }
        else if(transform.parent.gameObject.CompareTag("ChoosePanel"))
        {
            // Solo se puede seleccionar una
            // Por lo tanto se desseleccionan las demás, es más elegante
            if(!IsSelected)
            {
                IsSelected = true;
                DeSelectAllOtherCards();
                ApplyOutline();
                Debug.Log("Carta seleccionada para agregarla al mazo");
            }
            else if(IsSelected)
            {            
                IsSelected = false;
                ApplyOutline();
            }
            chooseCardsController.CheckIfSelected();
        }
        // Si no está en el panel de destruir cartas o en el de escoger significa que está en la playerHand
        else
        {
            // Siempre que no se supere el límite de cartas seleccionadas, selecciona una carta que no lo estaba y viceversa.
            if(!IsSelected && CountSelectedCards() < maxCardsSelected)
            {
                soundsFXManager.PlayCardSound();
                IsSelected = true;
                frameTextGlowToggle();
                Debug.Log("Carta seleccionada");
            }
            else if(IsSelected)
            {            
                soundsFXManager.PlayCardSound();
                IsSelected = false;
                frameTextGlowToggle();
            }
            SendCheckToConsumable();
        }
    }
    private void SendCheckToConsumable()
    {
        ConsumableCardController currentSelectedConsumable = ConsumablesController.currentSelectedConsumable;
        if (currentSelectedConsumable == null) return;
        currentSelectedConsumable.CkeckSelectionValidity(playerHandController.GetAllSelectedCardsList());
    }
    private void ApplyOutline()
    {
        
        if(IsSelected)
        {
            cardImage.material = outlineCardMaterial;
            outlineCardMaterial.SetFloat("_PixelOutlineFade", 1f);
        }
        else
        {
            outlineCardMaterial.SetFloat("_PixelOutlineFade", 0f);
        }
    }
    private void SetAllTextsToWhite()
    {
        foreach (Text text in gameObject.GetComponentsInChildren<Text>(true))
        {
            text.color = Color.white;
        }
        foreach (TextMeshProUGUI tmp in gameObject.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.color = Color.white;
        }
    }    

    private int CountSelectedCards()
    {
        // Devuelve la cantidad de cartas seleccionadas
        int count = 0;
        foreach (Transform card in playerHandTransform)
        {
            CardController effect = card.GetComponent<CardController>();
            if (effect != null && effect.IsSelected) // Verifica si está seleccionada
            {
                count++;
            }
        }
        return count;
    }
    private void DeSelectAllOtherCards()
    {
        foreach (Transform card in transform.parent)
        {
            if (card.gameObject.CompareTag("Button")) continue;
            CardController cardController = card.GetComponent<CardController>();
            if (cardController != null && card != transform && cardController.IsSelected )
            {
                cardController.ToggleSelection();
            }
        }
    }

    public void frameTextGlowToggle()
    {
        if (IsSelected && !isOnFusionator)
        {
            if (cardType == CardType.Gem)
            {
                bool textType = playerHandController.GetCardEffectType(playerHandController.GetSelectedCardsList(transform));
                if (!textType)
                {
                    deffenseTextGO.SetActive(false);
                    attackTextGO.SetActive(true);
                }
                else
                {
                    attackTextGO.SetActive(false);
                    deffenseTextGO.SetActive(true);
                }
            }            
        }
        else if (!IsSelected)
        {
            attackTextGO.SetActive(false);
            deffenseTextGO.SetActive(false);
        }
    }
    private int GetNextCardIndex(float mouseXPosition, int totalCards, float panelWidth, float cardWidth)
    {
        // Convertimos la posición del ratón a coordenadas locales dentro del panel
        Vector3 localMousePosition = playerHandTransform.InverseTransformPoint(new Vector3(mouseXPosition, 0, 0));

        // Calculamos el mismo espaciado usado en `CalculateCardPositionX`
        float availableWidth = panelWidth - cardWidth;
        float minSpacing = availableWidth / totalCards;
        float idealSpacing = Mathf.Min(cardWidth + spacing, minSpacing);
        float centerOffset = (totalCards - 1) / 2f;

        // Buscar el índice correcto de la carta según la posición del ratón
        for (int i = 0; i < totalCards; i++)
        {
            float cardCenterX = (i - centerOffset) * idealSpacing;
            if (localMousePosition.x < -panelWidth / 2)
            {
                return 0;
            }
            
            if (localMousePosition.x < cardCenterX + idealSpacing / 2f)
            {
                if (idealSpacing < cardWidth / 2)
                {
                    return i + 1;
                }
                return i;
            }
        }

        return totalCards - 1; // Si el mouse está más allá de la última carta, devolver la última
    }

    private Canvas GetMainCanvas()
    {
        // Itera para buscar el canvas mas arriba en la Jerarquía.
        Transform current = transform;
        while (current != null)
        {
            Canvas canvas = current.GetComponent<Canvas>();
            if (canvas != null && canvas.isRootCanvas)
            {
                return canvas;
            }
            current = current.parent;
        }
        return null;
    }  

     // Métodos que usará el PlayedCards
    public void FadeOutAndDestroy(bool textType)
    {
        // Iniciar la corrutina para realizar la animación de desvanecimiento      
        StartCoroutine(FadeOut(textType));
    }
    // Sobrecarga para el mismo metodo sin el efecto de texto de la carta
    public void FadeOutAndDestroy()
    {
        // Iniciar la corrutina para realizar la animación de desvanecimiento
        StartCoroutine(FadeOut());
    }
    private IEnumerator FadeOut(bool textType)
    {
        yield return new WaitUntil(() => isMoving == false);
        cardClickGraphic.enabled = false;
        Graphic[] graphics = GetComponentsInChildren<Graphic>();
        bool effectTriggered = false;

        foreach (Graphic g in graphics)
        {
            Color originalColor = g.color;

            LeanTween.value(gameObject, 1f, 0f, fadeOutSpeed)
                .setOnUpdate((float alpha) =>
                {
                    g.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                    if (!effectTriggered && alpha <= 0.5f)
                    {
                        //CardEffectText(textType);
                        effectTriggered = true;
                    }
                })
                .setEase(LeanTweenType.easeInSine);
        }
        yield return new WaitForSeconds(fadeOutSpeed);
        Destroy(gameObject);
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitUntil(() => isMoving == false);
        cardClickGraphic.enabled = false;
        Graphic[] graphics = GetComponentsInChildren<Graphic>();
        
        foreach (Graphic g in graphics)
        {
            Color originalColor = g.color;
            LeanTween.value(gameObject, 1f, 0f, fadeOutSpeed)
                .setOnUpdate((float val) =>
                {
                    g.color = new Color(originalColor.r, originalColor.g, originalColor.b, val);
                })
                .setEase(LeanTweenType.easeInSine); 
        }
        // Esperar a que termine el fade
        yield return new WaitForSeconds(fadeOutSpeed);

        yield return new WaitUntil(() => IsProcesed == true);

        Destroy(gameObject);
    }

    private void CardEffectText(bool textType)
    {
        CardData cardData = GetComponent<CardController>().cardData;        
        // Si es false es Attack Text
        if (!textType)
        {
            //Debug.Log("Attack TEXT TYPE");
            Transform weaponSocketTransform = GameObject.Find("Canvas/Middle Panel/Sockets/Canvas/Panel/HUD/Weapon Socket").transform;
            Addressables.InstantiateAsync("FloatingTextPrefab", Vector3.zero, Quaternion.identity, weaponSocketTransform).Completed += (AsyncOperationHandle<GameObject> handle) =>
            {   // Representa que si la operación se realiza con exito y encuentra el preFab y lo asigna correctamente procede a:
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject floatingText = handle.Result;
                    FloatingText textComponent = floatingText.GetComponent<FloatingText>();    
                    floatingText.transform.localPosition = Vector3.zero;                
                    textComponent.CardEffect(cardData.elementalDamagesList[0].element, cardData.attackText);                    
                    Destroy(floatingText, 2f);
                }
            };
        }
        // Si es true es Deffense Text
        else
        {
            Transform playerDwarfTransform = GameObject.FindFirstObjectByType<PlayerCharacterController>().transform;
            Addressables.InstantiateAsync("FloatingTextPrefab", Vector3.zero, Quaternion.identity, playerDwarfTransform).Completed += (AsyncOperationHandle<GameObject> handle) =>
            {   // Representa que si la operación se realiza con exito y encuentra el preFab y lo asigna correctamente procede a:
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject floatingText = handle.Result;
                    FloatingText textComponent = floatingText.GetComponent<FloatingText>();
                    floatingText.transform.localPosition = new Vector3(-40f, 150, 0);
                    textComponent.CardEffect(cardData.elementalDamagesList[0].element, cardData.deffenseText);
                    Destroy(floatingText, 2f);
                }
            };
        }        
    }

    public void ConvertCard(CardData dataToConvert)
    {
        cardData = dataToConvert;
        nameText.text = cardData.cardName.ToUpper();
        cardImage.sprite = cardData.cardImage;
        attackText.text = cardData.attackText;
        deffenseText.text = cardData.deffenseText;
        cardType = cardData.cardType;
        upgradedCard = cardData.upgradedCard;
        ActivateStars();
    }

    public void BlockInteractions()
    {
        GraphicRaycaster graphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        graphicRaycaster.enabled = false;
    }

    // Acceso público para variables
    public bool IsPlayed
    {
        get => isPlayed; // Proporciona acceso de lectura
        set => isPlayed = value; // Proporciona acceso de escritura
    }
    public bool IsSelected
    {
        get => isSelected; // Proporciona acceso de lectura
        set => isSelected = value; // Proporciona acceso de escritura
    }
    public float Spacing
    {
        get => spacing; // Proporciona acceso de lectura
        set => spacing = value; // Proporciona acceso de escritura
    }
}
