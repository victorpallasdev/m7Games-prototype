using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System;
using UnityEngine.InputSystem;


public class GolemController : MonoBehaviour
{
    public static GolemController Instance;
    public GolemData golemData;

    [Header("Golem Data Info")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] public int golemPower { get; private set; }  
    [SerializeField] public string specialAbilityName; // Nombre de la habilidad especial
    private string specialAbilityMethodName;
    [SerializeField] public string specialAbilityDescription; // Descripción de la habilidad especial
    [SerializeField] public float specialAttackMultiplier; // Multiplicador del ataque especial
    [SerializeField] private int specialAttackCooldown; // Turnos necesarios para el ataque especial 
    [SerializeField] public int CurrentSpecialCD { get; private set; }  
    public TextMeshProUGUI specialAttackCooldownText; // Vinculado en el inspector
    public SpriteRenderer targetSprite; // Vinculado en el inspector
    [SerializeField] private int currentBarIndex = 0; // Índice de la barra activa
    [SerializeField] private int numberOfLifes;
    [SerializeField] public bool IsKnockedOut {get; set;} = false;
    [SerializeField] public bool IsAtacking = false;
    [SerializeField] public float factorKweaknes = 50;
    private List<int> healthbarvalues;


    [Header("Other Settings")]
    [SerializeField] Vector3 dmgTextPosition;
    public AssetReference floatingTextPrefab;
    private Image healthFill; // Referencia al Fill de la barra de vida
    public Dictionary<Element, int> golemElementalPowers { get; private set; } 
    public Dictionary<Element, int> golemElementalResistances { get; private set; } 
    public Dictionary<Element, int> golemElementalWeaknesses { get; private set; }
    public List<GolemStatusEffect> activeStatusEffects;
    private List<Color> BarColors;
    private Animator golemAnimator;
    private Material materialInstance;
    private SpriteRenderer spriteRenderer;
    public Transform healthBarFill;
    public Transform healthBarCanvas;
    public Transform healthBarText;
    public Transform healthBarBackground;
    public Transform statusBar;
    private Image healthBarBackgroundImage;
    private Image healthBarFillImage;
    private Vector3 dmgTextWorldPosition;
    private Vector3 dmgTextWorldPositionRandomized;
    private TextMeshProUGUI healthBarTextComponent;
    public bool imTarget { get; private set; } = false;
    public event Action OnHealthBarDepleted;
    public GolemStatsPanelController golemStatsPanelController; // Vinculado con el inspector
    public Canvas golemStatsPanelCanvas; // Vinculado con el inspector
    private CameraController cameraController;
    private LTDescr fadeTween; // Variable para almacenar el FadeOut Tween
    private LTDescr delayTween; // Variable para almacenar el delay del fade-out
    private Canvas healthCanvas;
    public Transform specialCDTransform;
    public Transform statusEffectsBar;
    public GameObject statusIconPrefab;
    public Canvas specialCDCanvas;
 

    private void Update()
    {

    }
    private void Awake()
    {
        Instance = this;       
        spriteRenderer = GetComponent<SpriteRenderer>();
        materialInstance = new Material(spriteRenderer.material);
        spriteRenderer.material = materialInstance;
        healthBarTextComponent = healthBarText.GetComponent<TextMeshProUGUI>();
        healthBarFillImage = healthBarFill.GetComponent<Image>();
        healthBarBackgroundImage = healthBarBackground.GetComponent<Image>();
        dmgTextWorldPosition = healthBarCanvas.TransformPoint(dmgTextPosition);
        healthCanvas = healthBarCanvas.GetComponent<Canvas>();
        healthCanvas.overrideSorting = true;
        specialCDCanvas = specialCDTransform.GetComponent<Canvas>();
        specialCDCanvas.overrideSorting = true;
        healthFill = healthBarFill.GetComponent<Image>();
        golemStatsPanelCanvas.overrideSorting = true;
        golemElementalPowers = new Dictionary<Element, int>()
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };
        golemElementalResistances = new Dictionary<Element, int>()
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };
        golemElementalWeaknesses = new Dictionary<Element, int>()
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };


    }
    private void Start()
    {
        cameraController = CameraController.Instance;
    }

    public void InitializeGolem(GolemData data)
    {
        golemData = data;
        activeStatusEffects = new List<GolemStatusEffect>();
        // Establecer estadísticas básicas
        golemPower = golemData.golemPower;
        healthbarvalues = golemData.maxHealthbarvalues;
        numberOfLifes = healthbarvalues.Count;
        BarColors = new List<Color>(GolemData.BarColors);
        maxHealth = healthbarvalues[0];
        currentHealth = maxHealth;
        healthBarFillImage.color = BarColors[0];
        healthBarBackgroundImage.color = BarColors[1];
        UpdateHealthBar();
        // Configurar poderes elementales
        foreach (var elementalDamage in golemData.elementalDamagesList)
        {
            golemElementalPowers[elementalDamage.element] += elementalDamage.value;
        }
        foreach (var elementalDamage in golemData.elementalResistances)
        {
            golemElementalResistances[elementalDamage.element] += elementalDamage.value;
        }
        foreach (var elementalDamage in golemData.elementalWeaknesses)
        {
            golemElementalWeaknesses[elementalDamage.element] += elementalDamage.value;
        }
        specialAbilityName = golemData.specialAbilityName;
        specialAbilityDescription = golemData.specialAbilityDescription;
        specialAttackMultiplier = golemData.specialAttackMultiplier;
        specialAttackCooldown = golemData.specialAttackCooldown;
        specialAbilityMethodName = golemData.specialAbilityMethodName;
        CurrentSpecialCD = specialAttackCooldown;
        specialAttackCooldownText.text = CurrentSpecialCD.ToString();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = golemData.golemSprite;
        golemAnimator = GetComponent<Animator>();
        golemAnimator.runtimeAnimatorController = golemData.animatorController;
        
        Debug.Log($"{golemData.golemName} inicializado con {currentHealth} de salud.");       
    }

    public IEnumerator Attack(PlayerCharacterController characterController)
    {
        if(CurrentSpecialCD <= 0)
        {
            SpecialAttack(characterController);
            yield break;
        }    
        Debug.Log("El Golem está atacando al Dwarf...");
        ////////////// ANIMACIÓN DE ATACAR ///////////////////////
        Vector3 startPosition = transform.position;
        golemAnimator.SetTrigger("Attack");
        Vector3 attackPosition = startPosition + new Vector3(-380f, 0, 0);
        float elapsedTime = 0f;
        float durationForward = 1f;
        float durationJumpBack = 1f;
        float jumpHeight = 50f;
        while (elapsedTime < durationForward)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, attackPosition, elapsedTime / durationForward);
            yield return null;
        }
        transform.position = attackPosition; // Asegurar que llegue a la posición exacta
        ///////////////////////////////////////////////////////////
        StartCoroutine(characterController.TakeDamage(golemPower, golemElementalPowers));
        yield return new WaitForSeconds(0.4f);
        ///////////// PARÁBOLA HACIA ATRAS ////////////////////////
        elapsedTime = 0f;
        PlayStepSound();
        while (elapsedTime < durationJumpBack)
        {
            elapsedTime += Time.deltaTime;

            // Movimiento en X
            float t = elapsedTime / durationJumpBack;
            float newX = Mathf.Lerp(attackPosition.x, startPosition.x, t);

            // Movimiento en Y (trayectoria parabólica)
            float newY = Mathf.Lerp(attackPosition.y, startPosition.y, t) + jumpHeight * Mathf.Sin(t * Mathf.PI);

            transform.position = new Vector3(newX, newY, transform.position.z);
            yield return null;
        }
        transform.position = startPosition;
        PlayStepSound();
        //////////////////////////////////////////////////////////
        IsAtacking = false;
    }
    public void SpecialAttack(PlayerCharacterController playerDwarfController)
    {
        if (specialAbilityMethodName != "")
        {
            var method = typeof(GolemAbilities).GetMethod(specialAbilityMethodName);
            if(method != null)
            {
                GolemAbilities golemAbilitiesInstance = new GolemAbilities();
                StartCoroutine((IEnumerator)method.Invoke(golemAbilitiesInstance, new object[] { this, playerDwarfController }));
            }
            else
            {
                Debug.LogWarning($"La habilidad especial '{specialAbilityMethodName}' no existe en GolemAbilities.");
            }
        }
    }
    public IEnumerator TakeDamage(int physicalDamage, Dictionary<Element, int> weaponElementalPowers, PlayerCharacterController playerCharacterController, Action onComplete = null)
    {
        // ——— 1) Prepara daños elementales finales ———
        List<(Element element, int dmg)> finalElements = new List<(Element element, int dmg)>();
        int totalDmg = 0;
        foreach (var kv in weaponElementalPowers)
        {
            var elementName = kv.Key;
            var elementPower = kv.Value;
            if (elementPower == 0) continue;

            float weakness = 0f, resistance = 0f;
            if (golemElementalWeaknesses.ContainsKey(elementName))
                weakness = golemElementalWeaknesses[elementName] / (golemElementalWeaknesses[elementName] + factorKweaknes);
            if (golemElementalResistances.ContainsKey(elementName))
                resistance = golemElementalResistances[elementName] / (golemElementalResistances[elementName] + factorKweaknes);

            int realElementalDmg = Mathf.Max(0, (int)(elementPower + elementPower * weakness - elementPower * resistance));
            if (realElementalDmg > 0)
            {
                finalElements.Add((elementName, realElementalDmg));
                totalDmg += realElementalDmg;
            }
        }

        // ——— 2) Suma físico ———
        int hpBefore = currentHealth;
        if (physicalDamage > 0)
            totalDmg += physicalDamage;


        // ——— 3) Spawn TODOS los textos a la vez (abanico sin solaparse) ———
        // Pequeño desplazamiento horizontal aleatorio “de golpe” si te gusta mantener un toque orgánico
        Vector3 spawnPos = dmgTextWorldPosition + new Vector3(UnityEngine.Random.Range(-60f, 60f), 0f, 0f);
        FloatingTextSpawner.Instance?.SpawnDamageBatch(
            spawnPos,
            healthBarCanvas,         // el mismo parent que usabas
            physicalDamage,
            finalElements
        );
        SoundsFXManager.Instance.PlayGolemTakeDmgSound();

        // ——— 4) Aplicar daño real ———
        currentHealth = Mathf.Clamp(currentHealth - totalDmg, 0, maxHealth);

        // ——— 5) Robar vida del FÍSICO ———
        if (playerCharacterController != null) // Hay que poner esta protección, por si es un proyectil lo que hace daño
        {
            Debug.Log($"Envía {physicalDamage}");
            playerCharacterController.LifeStealHeal(physicalDamage);
        }

        // ——— 6) Feedback rojo sin bloquear ———
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr.color;
        sr.color = Color.red;
        yield return null;
        sr.color = original;

        // ——— 7) Barra de vida + callbacks ———
        UpdateHealthBar();
        SoundsFXManager.Instance.PlayGolemSound();
        onComplete?.Invoke();
    }
    public void PlayStepSound()
    {
        SoundsFXManager.Instance.PlayGolemStep();
    }
    public void killGolem()
    {
        Dictionary<Element, int> pickaxeElementalPowers = new Dictionary<Element, int>();
        PlayerCharacterController playerCharacterController = FindFirstObjectByType<PlayerCharacterController>();
        StartCoroutine(TakeDamage(1000000, pickaxeElementalPowers, playerCharacterController));
    }

    private void UpdateHealthBar()
    {       
        // Actualizar la barra de vida en función de la salud actual
        // Asegurarnos que nunca supere la maxHealth con las curaciones
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentHealth <= 0)
        {
            IsKnockedOut = true;
            OnHealthBarDepleted?.Invoke();         
        }
        healthFill.fillAmount = (float)currentHealth / (float)maxHealth;        
        healthBarTextComponent.text = $"{currentHealth}/{maxHealth}";
    }
    public void NextRound()
    {
        currentBarIndex++;
        maxHealth = healthbarvalues[currentBarIndex];            
        currentHealth = maxHealth;
        healthBarFillImage.color = BarColors[currentBarIndex];
        healthBarBackgroundImage.color = BarColors[currentBarIndex + 1];
        UpdateHealthBar();
    }
    public void UpdateTurnsOfCdAndBuffs()
    {
        CurrentSpecialCD--;
        CurrentSpecialCD = CurrentSpecialCD < 0 ? specialAttackCooldown : CurrentSpecialCD;
        specialAttackCooldownText.text = CurrentSpecialCD.ToString();      
    }
    public void ResolveStatusEffects()
    {
        List<GolemStatusEffect> expiredEffects = new List<GolemStatusEffect>();
        foreach (var effect in activeStatusEffects)
        {
            Debug.Log($"Processing status: {effect.StatusName}");
            effect.OnTurn(this);
            effect.TurnsRemaining--;
            if (effect.IsExpired())
            {
                expiredEffects.Add(effect);
            }
        }

        foreach (var effect in expiredEffects)
        {
            RemoveStatusEffect(effect);
        }
    }
    public void RemoveStatusEffect(GolemStatusEffect effect)
    {
        effect.OnQuit?.Invoke(this);
        activeStatusEffects.Remove(effect);
        UpdateStatusBarIcons();        
    }

    public void ResetSpecialCD()
    {
        CurrentSpecialCD = specialAttackCooldown;
        specialAttackCooldownText.text = CurrentSpecialCD.ToString();
    }
    public void OnRightClick()
    {
        cameraController.ZoomToGolem();
        golemStatsPanelController.ToggleStatsPanel();

    }
    public void ModifyResistances(List<Element> elements, float value)
    {
        foreach (Element element in elements)
        {
            golemElementalResistances[element] += (int)value;
        }
    }
    public void ModifyWeaknesses(List<Element> elements, float value)
    {
        foreach (Element element in elements)
        {
            golemElementalWeaknesses[element] += (int)value;
        }
    }
    public void ApplyStatusEffect(GolemStatusEffect effect)
    {
        GolemStatusEffect existing = activeStatusEffects.Find(e => e.StatusName == effect.StatusName);
        if (existing != null)
        {
            existing.TurnsRemaining = effect.TurnsRemaining; // refresca duración
            Debug.Log($"Efecto {effect.StatusName} refrescado en {gameObject.name}");
        }
        else
        {
            activeStatusEffects.Add(effect);
            effect.OnGet?.Invoke(this);
            Debug.Log($"Efecto {effect.StatusName} aplicado a {gameObject.name}");
        }
        UpdateStatusBarIcons();
    }

    private void UpdateStatusBarIcons()
    {
        foreach (Transform child in statusEffectsBar)
        {
            Destroy(child.gameObject);
        }
        foreach (var effect in activeStatusEffects)
        {
            if (effect.EffectIcon != null) // Asegurarse de que el icono existe
            {
                GameObject iconGO = Instantiate(statusIconPrefab, statusEffectsBar); // Crear el icono en el canvas
                Image iconImage = iconGO.transform.GetChild(0).GetComponent<Image>();
                iconImage.sprite = effect.EffectIcon; // Asignar el sprite del estado
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
        targetSprite.transform.localScale = Vector3.one;

        // Fase 1: Reducir la escala de 1 a 0.16 rápidamente (0.2s)
        LeanTween.scale(targetSprite.gameObject, Vector3.one * 0.16f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Fase 2: Aumentar la escala de 0.16 a 0.25 más lentamente (0.3s)
                LeanTween.scale(targetSprite.gameObject, Vector3.one * 0.25f, 0.3f)
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

    private void Die()
    {
        // Lógica para cuando el golem muere
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}
