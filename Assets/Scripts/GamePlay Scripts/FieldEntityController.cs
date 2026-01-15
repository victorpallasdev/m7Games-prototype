using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System;
using TMPro;



public class FieldEntityController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material materialInstance;
    public SpriteRenderer targetSprite;
    private Sprite missHitPointSprite;
    private BoxCollider2D boxCollider;
    private Transform hitPointsCanvasTransform;
    private string entityName;
    private int hitPoints;
    private int actualHitpoints;
    public bool imTarget { get; private set; } = false;
    public bool isDropping { get; private set; } = false;
    private LTDescr fadeTween; // Variable para almacenar el FadeOut Tween
    private LTDescr delayTween; // Variable para almacenar el delay del fade-out
    private GolemController golemController;
    private DeckManager deckManager;
    public Transform dropPanelTransform; // Vinculado desde el Inspector.
    public GameObject cardPrefab;// Vinculado desde el Inspector.
    public GameObject consumablePrefab;// Vinculado desde el Inspector.
    public GameObject coinDropPrefab;// Vinculado desde el Inspector.
    private List<ConsumableData> availableConsumablesToDrop;
    private List<CardData> availableCardsToDrop;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        hitPointsCanvasTransform = transform.Find("HitPointsCanvas");  
        golemController = GolemController.Instance;
        deckManager =DeckManager.Instance;  
        materialInstance = new Material(spriteRenderer.material);
        spriteRenderer.material = materialInstance;
        // PrintMaterialProperties(materialInstance); Para Imprimir por consola todos las properties del Material
        // StartCoroutine(GetOutlinerMaterial());
        Addressables.LoadAssetAsync<Sprite>("MissHitPointIcon").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                missHitPointSprite = handle.Result;
            }
        };
    }
    public void Initialize(FieldEntityData entityData)
    {
        entityName = entityData.entityName;
        spriteRenderer.sprite = entityData.entitySprite;
        hitPoints = entityData.hitPoints;
        actualHitpoints = hitPoints;
        StartCoroutine(generateHitPoints(hitPoints));
        availableCardsToDrop = entityData.CardsToDrop;
        availableConsumablesToDrop = entityData.consumablesToDrop;
        boxCollider.size = spriteRenderer.sprite.bounds.size;
        boxCollider.offset = spriteRenderer.sprite.bounds.center;

        gameObject.name = $"{entityName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
    }
    public void SpawnFall()
    {
        float targetY = -190f + UnityEngine.Random.Range(-20f, 0f); // Y con margen aleatorio
        float randomRotation = UnityEngine.Random.Range(-10f, 10f); // Rotación aleatoria en Z
        StartCoroutine(FallAnimation(targetY, randomRotation));
    }
    private IEnumerator FallAnimation(float targetY, float rotationAmount)
    {
        float duration = 1.2f; // Duración total de la caída
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotationAmount);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = t * t; // Hace que la caída sea más rápida al principio y desacelere al final

            // Interpolación de posición (simulando gravedad)
            transform.position = new Vector3(startPos.x, Mathf.Lerp(startPos.y, targetY, t), startPos.z);

            // Interpolación de rotación
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Asegurar que termina exactamente en la posición final
        transform.position = new Vector3(startPos.x, targetY, startPos.z);
        transform.rotation = targetRotation;
    }

    private IEnumerator generateHitPoints(int amount)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("HitPointPrefab");
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject hitPointPrefab = handle.Result;

            for (int i = 0; i < amount; i++)
            {
                Instantiate(hitPointPrefab, hitPointsCanvasTransform);
            }
        }
    }

    public void ApplyOutline()
    {        
        materialInstance.SetFloat("_PixelOutlineFade", 1f);
    }

    public void RestoreMaterial()
    {
        materialInstance.SetFloat("_PixelOutlineFade", 0f); 
    }

    public void TakeHit()
    {
        
        int indexToChange = actualHitpoints - 1;
        if (indexToChange < 0) return;
        Transform hitPoint = hitPointsCanvasTransform.GetChild(indexToChange);
        Image hitPointImage = hitPoint.GetComponent<Image>();

        hitPointImage.sprite = missHitPointSprite;
        actualHitpoints--;        
        if (actualHitpoints <= 0)
        {
            DestroyEntity();
        }
    }
    public void SetTarget()
    {
        GameObject previousTarget = PlayerCharacterController.target;
        if (previousTarget.CompareTag("Golem"))
        {
            GolemController golemController = previousTarget.GetComponent<GolemController>();
            golemController.QuitTarget();
            imTarget = true;
        }
        else if (previousTarget.CompareTag("FieldEntity"))
        {
            FieldEntityController fieldEntity = previousTarget.GetComponent<FieldEntityController>();
            fieldEntity.QuitTarget();
            imTarget = true;
        }     
        PlayerCharacterController.target = gameObject;   
        ShowTargetIndicator();
    }

    public void QuitTarget()
    {
        imTarget = false;

        // Cancelar cualquier animación en curso para evitar errores en el fade
        LeanTween.cancel(targetSprite.gameObject);

        if (fadeTween != null) LeanTween.cancel(fadeTween.uniqueId);
        if (delayTween != null) LeanTween.cancel(delayTween.uniqueId);

        // Reiniciar el Alpha antes de desactivar
        targetSprite.color = new Color(targetSprite.color.r, targetSprite.color.g, targetSprite.color.b, 1f);
        targetSprite.enabled = false;
    }

    public void ShowTargetIndicator()
    {
        // Cancelar cualquier animación previa para evitar acumulaciones
        LeanTween.cancel(targetSprite.gameObject);

        if (fadeTween != null) LeanTween.cancel(fadeTween.uniqueId);
        if (delayTween != null) LeanTween.cancel(delayTween.uniqueId);

        targetSprite.enabled = true;
        targetSprite.color = new Color(targetSprite.color.r, targetSprite.color.g, targetSprite.color.b, 1f); // 🔹 Reiniciar Alpha a 1
        targetSprite.transform.localScale = Vector3.one * 0.3f;

        // Fase 1: Reducir la escala de 0.3 a 0.1 rápidamente (0.2s)
        LeanTween.scale(targetSprite.gameObject, Vector3.one * 0.1f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Fase 2: Aumentar la escala de 0.1 a 0.18 más lentamente (0.3s)
                LeanTween.scale(targetSprite.gameObject, Vector3.one * 0.18f, 0.3f)
                    .setEase(LeanTweenType.easeOutElastic)
                    .setOnComplete(() =>
                    {
                        // Fase 3: Esperar 3 segundos antes del fade out
                        delayTween = LeanTween.delayedCall(3f, () =>
                        {
                            // Cancelar cualquier fade previo antes de iniciar uno nuevo
                            if (fadeTween != null) LeanTween.cancel(fadeTween.uniqueId);
                            
                            // Fase 4: Fade Out en 0.5s y desactivar el objeto al finalizar
                            fadeTween = LeanTween.alpha(targetSprite.gameObject, 0f, 0.5f)
                                .setEase(LeanTweenType.easeOutQuad)
                                .setOnComplete(() =>
                                {
                                    targetSprite.enabled = false;
                                });
                        });
                    });
            });
    }

    public void DestroyEntity()
    {
        EntityDrop(() => Destroy(gameObject));
    }

    public void EntityDrop(Action onComplete = null)
    {
        isDropping = true;
        spriteRenderer.enabled = false;
        float roll = UnityEngine.Random.value;
        // DROP ORO CON UN 50%
        if (roll <= 0.5f)
        {
            // Hago una potencia de 2 para que los valores sean más pequeños, así se acercan más al cero
            // y hace que sea más difícil sacar un roll de oro más alto            
            float biasedValue = Mathf.Pow(UnityEngine.Random.value, 2f);
            // Una interpolación lineal simple con el valor de antes para sacar el valor entre 10-50
            // Como digo antes, es más probable los pequeños y empieza a ser más difícil pero no imposible el 50
            int gold = Mathf.RoundToInt(Mathf.Lerp(10f, 50f, biasedValue));
            PlayerCharacterController playerDwarfController = FindFirstObjectByType<PlayerCharacterController>();
            // Instancio el prefab de la moneda en el panel de drops de la entity
            GameObject newCoinDrop = Instantiate(coinDropPrefab, dropPanelTransform);
            // Hay que setear su rotacion global a 1 porque el entity tiene una ligera rotación aleatoria
            newCoinDrop.transform.rotation = Quaternion.identity;
            TextMeshProUGUI coinText = newCoinDrop.GetComponentInChildren<TextMeshProUGUI>();
            // Seteo el texto del oro que va encima de la moneda
            coinText.text = "+ " + gold.ToString();
            // Empieza la fiesta
            // Cuando la DropAnimation termina del todo, envía el callback, y es entonces cuando añado el oro
            // Y entonces es cuando devuelvo el callback de que este método ha terminado para que se destruya
            StartCoroutine(DropAnimation(newCoinDrop, () =>
            {
                playerDwarfController.AddGold(gold);
                onComplete?.Invoke();
            }));
        }
        // DROP CONSUMIBLE DE UN (85-50) = 35%
        else if (roll <= 0.85f)
        {
            // COge un consumible random de la lista
            int i = UnityEngine.Random.Range(0, availableConsumablesToDrop.Count);
            ConsumableData consumableToDrop = availableConsumablesToDrop[i];
            // Instancio el prefab del Consumible en el panel de drops de la entity
            GameObject newConsumable = Instantiate(consumablePrefab, dropPanelTransform);
            ConsumableCardController consumableCardController = newConsumable.GetComponent<ConsumableCardController>();
            // Hay que setear su rotacion global a 1 porque el entity tiene una ligera rotación aleatoria
            newConsumable.transform.rotation = Quaternion.identity;
            newConsumable.name = $"{consumableToDrop.consumableName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
            consumableCardController.Initialize(consumableToDrop);
            // Empieza la fiesta
            // Primero hace la rutina dropAnimation, cuand está envía su callback de que ha terminado, entonces se ejecuta:
            // MoveToConsumables, y cuando este devuelve su callback de que ha terminado entonces devolvemos nuestro callback
            // de que este mismo método ha terminado para que se destruya el gameObject.
            StartCoroutine(DropAnimation(newConsumable, () => MoveToConsumables(newConsumable, () => onComplete?.Invoke())));
        }
        // DROP CARTA DE UN 100-85 = 15%
        else
        {
            // Funciona igual que el de arriba
            int i = UnityEngine.Random.Range(0, availableCardsToDrop.Count);
            CardData cardToDrop = availableCardsToDrop[i];
            GameObject newCard = Instantiate(cardPrefab, dropPanelTransform);
            newCard.transform.rotation = Quaternion.identity;
            newCard.name = $"{cardToDrop.cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
            CardController cardController = newCard.GetComponent<CardController>();
            cardController.Initialize(cardToDrop);
            newCard.transform.localScale = Vector3.one * 0.5f;
            // Excepto por que tenemos que meter la carta en el mazo
            deckManager.AddCardToDeck(cardToDrop);
            StartCoroutine(DropAnimation(newCard, () => MoveToHand(newCard, () => onComplete?.Invoke())));
        }

        PlayerCharacterController.Instance.SetGolemTarget();
    }
    public IEnumerator DropAnimation(GameObject drop, Action onComplete = null)
    {  
        // Ha sido importantísimo hacer que esto sea una rutina y esperar ese frame
        // Ya que cuando Instancio los prefabs en los métodos anteriores, Unity algunas veces,
        // necesita un frame para que la posición y jerarquía de los GameObject estén actualizadas
        // completamente. Al esperar el frame todo se soluciona.
        yield return null;
        //Debug.Log("Entra en la dropAnimation");     
        Vector3 startPos = drop.transform.position;
        Vector3 targetPos = startPos + new Vector3(0f, 100f, 0f);        
        LeanTween.moveY(drop, targetPos.y, 1f)
            .setEase(LeanTweenType.easeOutSine)
            .setOnComplete(() =>
            {
                // Envía el callback de que ha terminado justo cuando acaba esta animación.
                //Debug.Log("Termina la dropAnimation");
                onComplete?.Invoke();                
            });
    }
    private void MoveToHand(GameObject card, Action onComplete = null)
    {
        PlayerHandController playerHandController = FindFirstObjectByType<PlayerHandController>();
        // No recuerdo porque hago un delayedCall de todo esto.
        // La cuestión es que había algún error hasta que lo puse
        // Probablemente cuando cambié DropAnimation a rutina es posible que se solucionara
        // Y por lo tanto puede que no haga falta el delayedCall
    
        LeanTween.cancel(card);
        // Lo pasamos a la mano
        card.transform.SetParent(playerHandController.transform, true);
        // Ponemos la escala que queremos que tenga en la mano
        card.transform.localScale = Vector3.one;
        // Hay que rotar otra vez el objeto ya que resta la rotación del entity al salir del parent.
        card.transform.localRotation = Quaternion.identity;
        // La primera para que la mueva a la mano
        playerHandController.UpdatePlayerHandPositions();
        // La seguna con delay hace que todas se recoloquen bien en su sitio sin errores.
        playerHandController.DelayedUpdatePlayerHandPositions(0.5f);
        onComplete?.Invoke();         
    }
    private void MoveToConsumables(GameObject consumable, Action onComplete = null)
    {        
        ConsumablesController consumablesController = FindFirstObjectByType<ConsumablesController>();
        // Funciona igual que el de encima       
        LeanTween.cancel(consumable);
        consumable.transform.SetParent(consumablesController.transform, true);
        consumable.transform.localRotation = Quaternion.identity;
        consumablesController.UpdateConsumablesPositions();
        consumablesController.DelayedUpdateConsumablesPositions(0.5f);
        onComplete?.Invoke();            
    }
}
