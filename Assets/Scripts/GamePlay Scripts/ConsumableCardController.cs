using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System.Linq;

public class ConsumableCardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Effects Settings")]  
    [SerializeField] private float fadeOutSpeed = 0.4f;
    [SerializeField] public float animationSpeed = 0.5f;
    [SerializeField] public float expandedScale = 1.1f;

    [Header("Only monitoring")] 
    private float spacing;
    [SerializeField] public float cardWidth;
    public string consumableName;
    public int consumableCost;
    private string methodName;
    private ConsumableData consumableData;
    private Transform cardTransform;
    public GameObject useLabel;
    private ConsumablesController consumablesController;
    private PlayerHandController playerHandController;    
    public bool isMoving = false;
    // [SerializeField] public static bool ConsumableSelected { get; private set; } = false;
    public static ConsumableCardController currentConsumableSelected;
    [SerializeField] private bool isPlayed = false;
    [SerializeField] public Canvas cardCanvas; // Vinculado desde el Inspector 
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public TextMeshProUGUI labelText; // Vinculado desde el Inspector
    [SerializeField] public Image useButtonImage; // Vinculado desde el Inspector
    [SerializeField] public Button useButton; // Vinculado desde el Inspector
    [SerializeField] public Image cardImage;
    private AssetReference placeholderPrefabReference;
    private Transform consumablesTransform;
    private static GameObject placeholder;
    private Canvas mainCanvas;
    private RectTransform canvasRectTransform;
    private RectTransform consumablesRectTransform;
    public bool isDraggingThisCard = false;
    public static ConsumableCardController CurrentDraggingCard { get; private set; }
    private bool readyToDrag = false;
    private Vector2 dragOffset;
    private bool placeHolderCreated = false;
    public Vector3 originalScale;
    private List<CardTypeGroup> requiredGroups;  
    private Material useLabelMaterialInstance;
    private int hoverTweenId = -1; // Guardamos el ID de la animación de hover
    private int dehoverTweenId = -1; // Guardamos el ID de la animación de dehover



    private void Start()
    {
        originalScale = transform.localScale;
        cardTransform = transform;
        
        if (!gameObject.CompareTag("Placeholder"))
        {
            useLabelMaterialInstance = new Material(useButtonImage.material);
            useButtonImage.material = useLabelMaterialInstance;
        }  
        consumablesController = FindFirstObjectByType<ConsumablesController>();
        consumablesTransform = consumablesController.transform;
        consumablesRectTransform = consumablesTransform.GetComponent<RectTransform>();

        playerHandController = FindFirstObjectByType<PlayerHandController>();
        mainCanvas = GetMainCanvas();
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();         

        StartCoroutine(InitializeCardInfoAndPosition());
        
        InitializePlaceholderReference();
    }
    public void Initialize(ConsumableData data)
    {
        consumableData = data;
        consumableName = consumableData.consumableName;
        nameText.text = consumableName.ToUpper();
        // consumableCost = consumableData.cost;
        cardImage.sprite = consumableData.cardImage;
        methodName = consumableData.methodName;
        requiredGroups = consumableData.requiredGroups;
        

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(UseConsumable);
        //Debug.Log($"✅ Listener agregado a {consumableName}");
        useLabel.SetActive(false);
    }

    private void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ConsumablesController.currentSelectedConsumable != this)
        {
            DeHoverCard();
        }        
    }
    public void HoverCard()
    {
        AnimateScale(ref hoverTweenId, ref dehoverTweenId, originalScale * expandedScale, LeanTweenType.easeOutElastic);
    }
    public void DeHoverCard()
    {
        AnimateScale(ref dehoverTweenId, ref hoverTweenId, originalScale, LeanTweenType.easeOutElastic);
    }

    private void AnimateScale(ref int tweenToStart, ref int tweenToCancel, Vector3 targetScale, LeanTweenType easing)
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
    
    
    private IEnumerator InitializeCardInfoAndPosition()
    {
        // Esperar un frame
        yield return null;
        cardWidth = consumablesController.cardWidth;
        consumablesController.UpdateConsumablesPositions();
    }
    private void InitializePlaceholderReference()
    {
        // Inicializa el prefab gracias al Addressables
        string placeholderKey = "ConsumableCardPlaceholder";
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
        // Bloquear clics si una animación está en curso, sinó hace efectos raros
        if (isMoving)
        {
            //Debug.Log("Clicks bloqueados!!");
            return;
        }
        // Si los clics no estan bloqueados entonces está lista para hacer el arrastre
        readyToDrag = true;
        // Obtenemos la posición del ratón en coordenadas world, (son las mismas que las coordenadas en el mainCanvas ya que está centrado en el world)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
            out Vector2 pointerPosition
        );
        // Obtenemos la posición de la carta en coordenadas world
        Vector2 cardWorldPosition = cardTransform.position;
        // Calculamos el offset
        dragOffset = cardWorldPosition - pointerPosition;
        // Debug.Log($"OFFSET = " + dragOffset);
        // En móviles, bloquear múltiples toques para evitar errores
        if (eventData.pointerId > 0) return;

         
        // Lo siguiente no hace falta.      
        //transform.localPosition = mainCanvas.transform.InverseTransformPoint(globalPosition);
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Si no estaba lista para hacer el arrastre vuelven las variables a false y no se arrastra.
        if (isMoving || !readyToDrag) 
        {
            readyToDrag = false;
            isDraggingThisCard = false;
            CurrentDraggingCard = null;
            return;
        }
        if (!placeHolderCreated && placeholderPrefabReference != null)
        {   
            // Instanciamos el placeholder según la referencia al prefab
            GameObject newPlaceholder = Instantiate(placeholder, consumablesTransform, false);
            newPlaceholder.name = "ConsumableCardPlaceholder";
            newPlaceholder.tag = "Placeholder";
            // Lo posiciona en el mismo sitio que estaba la carta
            newPlaceholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
            newPlaceholder.transform.localPosition = transform.localPosition;
            // Actualiza la referencia al nuevo placeholder
            placeholder = newPlaceholder;
            // Cogemos su posicion global antes de nada
            Vector3 globalPosition = transform.position;
            // Pasamos la carta al Canvas Principal
            transform.SetParent(mainCanvas.transform, true);
            // Una vez en el canvas principal la dejamos en la misma posicion global
            transform.position = globalPosition;   
            placeHolderCreated = true; 
        }             
        // Como se empieza a arrastrar establecemos la bandera de que sí está arrastrando esta carta.
        isDraggingThisCard = true;
        // Tambien se establece el objeto de esta carta como la que se está arrastrando en la variable static para futuros usos.
        CurrentDraggingCard = this;
        // Aquí seteamos el sorting order para que renderice la carta que estamos arrastrando encima de las demas
        // Obtenemos el sorting order más alto de las cartas de la mano utilizando el mismo calculo que para organizarlas.
        int totalConsumables = consumablesController.transform.childCount;
        int maxSortingOrder = 4 + totalConsumables;
        // Obtenemos el canvas de esta carta arrastrada
        Canvas cardCanvas = GetComponentInChildren<Canvas>();
        // Y simplemente hacemos que sea 1 nivel mas alto en el sorting order.
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = maxSortingOrder + 1;        

        // Cogemos la posición del ratón
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
            out Vector2 localPointerPosition
        );
        // se la pasamos a la carta para que siga al ratón
        cardTransform.localPosition = localPointerPosition + dragOffset;       
        // newIndex para monitorear el indice del placeholder según donde está el ratón
        int newIndex = GetNextCardIndex(localPointerPosition.x, consumablesTransform.childCount, consumablesRectTransform.rect.width, cardWidth);
        // Buscamos la referencia al gameobject placeholder por su tag
        placeholder = GameObject.FindWithTag("Placeholder");
        // Movemos el placeholder a su posicion según el índice que le toca
        if (placeholder != null && placeholder.transform.GetSiblingIndex() != newIndex)
        { 
            
            placeholder.transform.SetSiblingIndex(newIndex);            
        }
        // Actualizamos la posicion de las cartas en cada tick del arrastre.
        consumablesController.UpdateConsumablesPositions();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Si esta carta no se estaba arrastrando, representa que ha sido un click de selección
        if(!isDraggingThisCard)
        {
            ToggleSelection();
            consumablesController.UpdateConsumablesPositions();
        }       
        // Como el placeholder se crea siempre que clicamos, aunque sea rápido, hay que eliminarlo siempre
        GameObject existingPlaceholder = GameObject.FindGameObjectWithTag("Placeholder");
        if (existingPlaceholder != null)
        {
            int indexPlaceholder = existingPlaceholder.transform.GetSiblingIndex();
            DestroyImmediate(existingPlaceholder);
            // Colocar la carta en el PlayerHand y asignar el índice del placeholder
            transform.SetParent(consumablesTransform, true);
            transform.SetSiblingIndex(indexPlaceholder);
        }
        placeHolderCreated = false;
        // Sobretodo actualizar la referencia cuando lo destruimos
        InitializePlaceholderReference();
        // Y actualizar las posiciones siempre que se suelta o se selecciona una carta
        consumablesController.UpdateConsumablesPositions();
        // Al soltar o seleccionar una carta, vuelven todas las flags a false
        readyToDrag = false;
        isDraggingThisCard = false;
        CurrentDraggingCard = null;        
    }

    private void ToggleSelection()
    {
       
        if(currentConsumableSelected != this)
        {
            if (currentConsumableSelected != null)
            {
                currentConsumableSelected.HideLabels();
            }            
            currentConsumableSelected = this;
            ConsumablesController.currentSelectedConsumable = this;
            ShowLabels();
           
            Debug.Log("Consumible seleccionado");
        }
        else
        {  
            HideLabels();
            currentConsumableSelected = null;
            ConsumablesController.currentSelectedConsumable = null;
        }
    }
    private void ShowLabels()
    { 
        // Activamos el gameObject de la label       
        useLabel.SetActive(true);
        CkeckSelectionValidity(playerHandController.GetAllSelectedCardsList());
        // Obtenemos su canvas
        Canvas labelCanvas = useLabel.GetComponent<Canvas>();
        int totalCards = consumablesTransform.childCount;
        // Utilizamos la misma fórmula que en UpdateConsumablesPositions() para setear los sortingOrder de la seleccionada con el num total de cartas.
        // ya que esto lo ejecuta antes y por lo tanto lo tenemos que calcular 2 veces paralelamente.
        int canvasOrderInLayer = 4 + 2*totalCards;
        // Pero realmente la label está 1 order por debajo para que parezca que sale de detrás de la carta
        labelCanvas.sortingOrder = canvasOrderInLayer - 1;

        RectTransform rectTransform = useLabel.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        // Nueva posición en coordenadas locales dentro del Canvas (anchoredPosition)
        Vector2 newPosition = rectTransform.anchoredPosition - new Vector2(0, height);
        // Animación
        LeanTween.value(useLabel, rectTransform.anchoredPosition, newPosition, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((Vector2 val) => rectTransform.anchoredPosition = val);
    }
    public void HideLabels()
    {
        // Obtenemos el canvas de la label
        Canvas labelCanvas = useLabel.GetComponent<Canvas>();
        // Utilizamos la misma fórmula que en UpdateConsumablesPositions() para setear los sortingOrder de una NO seleccionada con el sibling index.
        // En este caso como la label está un order por debajo, sumamos 3 en vez de 4.
        labelCanvas.sortingOrder = 3 + 2*transform.GetSiblingIndex();
        RectTransform rectTransform = useLabel.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        // Nueva posición en coordenadas locales dentro del Canvas (anchoredPosition)
        Vector2 originalPosition = rectTransform.anchoredPosition + new Vector2(0, height);

        LeanTween.value(useLabel, rectTransform.anchoredPosition, originalPosition, 0.2f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((Vector2 val) => rectTransform.anchoredPosition = val)
            .setOnComplete(() => useLabel.SetActive(false));
    }

    private int GetNextCardIndex(float mouseXPosition, int totalCards, float panelWidth, float cardWidth)
    {
        // Convertimos la posición del ratón a coordenadas locales dentro del panel
        Vector3 localMousePosition = consumablesTransform.InverseTransformPoint(new Vector3(mouseXPosition, 0, 0));

        // Calculamos el mismo espaciado usado en `CalculateCardPositionX`
        float minSpacing = (panelWidth - cardWidth) / totalCards;
        float spacing = Mathf.Min(cardWidth, minSpacing);
        float centerOffset = (totalCards - 1) / 2f;

        // Buscar el índice correcto de la carta según la posición del ratón
        for (int i = 0; i < totalCards; i++)
        {
            float cardCenterX = (i - centerOffset) * spacing;
            if (localMousePosition.x < -panelWidth / 2)
            {
                return 0;
            }
            
            if (localMousePosition.x < cardCenterX + spacing / 2f)
            {
                if (spacing < cardWidth / 2)
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
    private IEnumerator FadeOutAndDestroy()
    {
        Color originalColor = cardImage.color;
        Color originalLabelColor = useButtonImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutSpeed)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutSpeed);
            cardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            useButtonImage.color = new Color(originalLabelColor.r, originalLabelColor.g, originalLabelColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurar que la transparencia llega a 0 al final
        cardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        // Esperar un frame antes de destruir
        yield return null;
        // Destruir el objeto de manera segura
        Destroy(gameObject);
        // Tengo que ejecutarlo con el invoke y un retraso, ya que sinó se ejecuta antes de que lo destruya
        // un WaitUntil no funcionaría ya que si el gameObject es destruido la rutina se para por completo y tampoco llega a ejecutarse
        consumablesController.Invoke("UpdateConsumablesPositions", 0.1f);
    }


    public void UseConsumable()
    {
        Debug.Log($"Su utiliza el consumible {consumableName}");
        PlayerHandController playerHand = FindFirstObjectByType<PlayerHandController>();
        HideLabels();
        CallAbilityByName(gameObject, methodName, playerHand.GetAllSelectedCardsList());
        StartCoroutine(FadeOutAndDestroy());
        
    }
    void CallAbilityByName(GameObject consumableObject, string methodName, List<CardController> selectedCards)
    {
        
        // Buscar la instancia donde están los métodos de efecto
        var effectInstance = consumableObject.GetComponent<ConsumableAbility>();
        if (effectInstance == null)
        {
            Debug.LogWarning($"❌ ConsumablesAbility no encontrado en {consumableObject.name}.");
            return;
        }

        // Buscar el método en la clase ConsumableEffects
        var method = effectInstance.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            // Invocar el método con los parámetros necesarios
            method.Invoke(effectInstance, new object[] { selectedCards });
        }
        else
        {
            Debug.LogWarning($"❌ Método '{methodName}' no encontrado en {consumableObject.name}.");
        }
    }
   public void CkeckSelectionValidity(List<CardController> listSelectedCards)
    {
    bool valid = false;
    var groups = requiredGroups
                   .Select(g => g.allowedTypes)
                   .ToList();   

    // Tienes que seleccionar exactamente tantos como grupos haya
        if (listSelectedCards.Count == requiredGroups.Count)
        {
            // Marcar qué grupos ya has satisfecho
            bool[] groupSatisfied = new bool[requiredGroups.Count];

            // Intentamos asignar cada carta a un único grupo que acepte su tipo
            foreach (var card in listSelectedCards)
            {
                var ct = card.cardData.cardType;
                bool assigned = false;

                // Busca un grupo donde no esté aún asignado y acepte este tipo
                for (int g = 0; g < requiredGroups.Count; g++)
                {
                    if (!groupSatisfied[g] && groups[g].Contains(ct))
                    {
                        groupSatisfied[g] = true;
                        assigned = true;
                        break;
                    }
                }

                // Si alguna carta no encaja en ningún grupo → invalido
                if (!assigned)
                {
                    valid = false;
                    ToggleInteractableUseButton(valid);
                    return;
                }
            }

            // Si todos los grupos quedaron satisfechos, es válido
            valid = groupSatisfied.All(x => x);
        }

    ToggleInteractableUseButton(valid);
}

    private void ToggleInteractableUseButton(bool validity)
    {
        if (validity)
        {   
            Color activedTextColor;
            ColorUtility.TryParseHtmlString("#6CDAFF", out activedTextColor);
            labelText.color = activedTextColor;            
            useLabelMaterialInstance.SetFloat("_EnableSaturation", 0f);
            useLabelMaterialInstance.DisableKeyword("_ENABLESATURATION_ON");
            useButton.interactable = true;

        }
        else
        {
            useButton.interactable = false;
            Color deactivedTextColor;
            ColorUtility.TryParseHtmlString("#878787", out deactivedTextColor);
            labelText.color = deactivedTextColor;
            useLabelMaterialInstance.EnableKeyword("_ENABLESATURATION_ON");
            useLabelMaterialInstance.SetFloat("_EnableSaturation", 1f);
        }
    }



    // Acceso público para variables
    public bool IsPlayed
    {
        get => isPlayed; // Proporciona acceso de lectura
        set => isPlayed = value; // Proporciona acceso de escritura
    }
    public float Spacing
    {
        get => spacing; // Proporciona acceso de lectura
        set => spacing = value; // Proporciona acceso de escritura
    }
}
