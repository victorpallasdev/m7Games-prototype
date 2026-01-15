using System.Collections;
using TMPro;
using UnityEngine;

public class ConsumablesController : MonoBehaviour
{
    public static ConsumablesController Instance;
    [Header("Settings to modify")] 
    [SerializeField] private int maxConsumableCards = 4;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private float selectedHeightPercent = 0.1f;

    [Header("Only monitoring")] 
    [SerializeField] public float cardWidth;
    [SerializeField] public float cardHeight;
    [SerializeField] public TextMeshProUGUI maxConsumablesText; // Vinculado desde el Inspector 
    public static ConsumableCardController currentSelectedConsumable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DelayedUpdateConsumablesPositions(float delay)
    {
        StartCoroutine(DelayUpdatePositions(delay));
    }
    private IEnumerator DelayUpdatePositions(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateConsumablesPositions();
    }
    public void UpdateConsumablesPositions()
    {            
        Transform consumables = transform;
        RectTransform panelRectTransform = consumables.GetComponent<RectTransform>();

        int totalCards = consumables.childCount;        
        
        for (int i = 0; i < totalCards; i++)
        {                
            Transform card = consumables.GetChild(i);
            ConsumableCardController consumableCardController = card.GetComponent<ConsumableCardController>();
            
            if(consumableCardController.isMoving) continue;

            Canvas cardCanvas = card.GetComponentInChildren<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = true; // Asegurar que se pueda sobrescribir el orden               
                if (ConsumableCardController.currentConsumableSelected == consumableCardController)
                {
                    cardCanvas.sortingOrder = 4 + 2*totalCards; // Si la carta está seleccionada tiene el sorting mas alto para ponerse al frente
                }
                else
                {
                    cardCanvas.sortingOrder = 4 + 2*i; // La carta a la DERECHA tiene el orden más alto
                }
            }
            Vector3 originalPosition = card.localPosition;
            Vector3 newPosition = CalculateCardPositionX(i, totalCards, panelRectTransform.rect.width, cardWidth);
            if (ConsumableCardController.currentConsumableSelected == consumableCardController)
            {
                newPosition.y += cardHeight * selectedHeightPercent;
                consumableCardController.HoverCard();
            }
            else
            {
                consumableCardController.DeHoverCard();
            }
            if (card.gameObject.CompareTag("Placeholder"))
            {
                card.localPosition = newPosition;
            }
            else
            {
                if (originalPosition != newPosition)
                {
                    consumableCardController.isMoving = true;
                    AnimatePosition(card, newPosition);                    
                }                
            }                      
        }
        SetMaxConsumablesText();
    }
    private Vector3 CalculateCardPositionX(int index, int totalCards, float panelWidth, float cardWidth)
    {
        float minSpacing = (panelWidth - cardWidth) / totalCards;
        float spacing = Mathf.Min(cardWidth, minSpacing);
        float centerOffset = (totalCards - 1) / 2f;
        return new Vector3((index - centerOffset) * spacing, 0, 0);
    }
    private void AnimatePosition(Transform card, Vector3 targetPosition)
    {
        ConsumableCardController cardController = card.GetComponent<ConsumableCardController>();

        // Cancelar cualquier animación previa para evitar conflictos
        LeanTween.cancel(card.gameObject);

        // Calcular la distancia entre la posición actual y la de destino
        float distance = Vector3.Distance(card.localPosition, targetPosition);
        float adjustedSpeed = Mathf.Clamp(distance / 100f, 0.1f, animationSpeed); 
        // Asegurar que no sea demasiado rápido ni demasiado lento

        // Iniciar la animación con LeanTween
        LeanTween.moveLocal(card.gameObject, targetPosition, adjustedSpeed)
            .setEase(LeanTweenType.easeOutQuad) // Suavizado natural sin rebote
            .setOnComplete(() =>
            {
                if (card != null)
                {
                    card.localPosition = targetPosition; // Asegurar que termine exactamente en la posición
                    // cardController.currentPosition = targetPosition;
                    cardController.isMoving = false; // Marcar que la carta terminó su animación
                    // cardController.StartFloatingEffect();
                }
            });        
    }
    private void SetMaxConsumablesText()
    {
        maxConsumablesText.text = transform.childCount + "/" + maxConsumableCards;
    }

}
