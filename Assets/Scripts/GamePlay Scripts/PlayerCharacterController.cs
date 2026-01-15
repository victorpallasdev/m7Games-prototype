using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using TMPro;
using UnityEngine.Rendering.Universal;



public class PlayerCharacterController : MonoBehaviour
{
    public static PlayerCharacterController Instance;
    [Header("Dwarf Stats")]
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int armor;
    [SerializeField] private int lifeSteal;   // Porcentage del daño que recibe en curación al atacar.
    // [SerializeField] public int LifeSteal { get; private set; } 
    [SerializeField] public int resFire { get; private set; }
    [SerializeField] public int resIce { get; private set; }
    [SerializeField] public int resElectric { get; private set; }
    [SerializeField] public int resWater { get; private set; }
    [SerializeField] public int resNature { get; private set; }
    [SerializeField] public int resEarth { get; private set; }
    [SerializeField] public int weaponFirePower { get; private set; }
    [SerializeField] public int weaponIcePower { get; private set; }
    [SerializeField] public int weaponElectricPower { get; private set; }
    [SerializeField] public int weaponWaterPower { get; private set; }
    [SerializeField] public int weaponNaturePower { get; private set; }
    [SerializeField] public int weaponEarthPower { get; private set; }
    public Dictionary<Element, int> dwarfResistances { get; private set; }
    public Dictionary<Element, int> WeaponElementalPowers { get; private set; }
    [SerializeField] public int criticalRate { get; private set; } = 0;
    [SerializeField] public int criticalDmg { get; private set; } = 75;  // Es en porcentaje
    [SerializeField] public int factorKres = 45;
    [SerializeField] public int factorKarmor = 45;

    [Header("Dwarf Info")]
    [SerializeField] public string dwarfName;
    [SerializeField] public string description;
    [SerializeField] public string id;    
    [SerializeField] Vector3 dmgTextPosition;
    [SerializeField] private float currentHealth;
    [SerializeField] public bool HaveWeapon { get; private set; }
    [SerializeField] public int weaponTurnsRemaining;
    [SerializeField] public int weaponTotalDuration;
    [SerializeField] public int PhysicalPowerBase { get; private set; }
    [SerializeField] public float PhysicalPowerModifier;
    [SerializeField] public bool HaveShield { get; private set; }
    [SerializeField] public int shieldDuration;
    [SerializeField] public int shieldTotalDuration;
    [SerializeField] public int shieldTotalAbsortion;
    [SerializeField] public int shieldAbsorb;
    [SerializeField] private int lifeStealDuration;
    [SerializeField] private int gold;
    [SerializeField] private int runes = 0;
    [SerializeField] public bool gotFusionatorAcces { get; private set; } = false;
    [SerializeField] public bool IsMoving { get; private set; } = false;
    [SerializeField] public bool isRooted = false;
    public const float kLifeSteal = 0.03067f; // Mathf.Log(2)/22.6
    private List<PlayerBuff> activeBuffs;
    private List<StatusEffect> activeEffects;
    private CoinsController coinsController;
    public int Gold => gold;
    public int Runes => runes;
    private TextMeshProUGUI goldText;
    public TextMeshProUGUI shieldDurationText;
    public string WeaponTitle { get; private set; } = "";
    public Image shieldBarImage;
    public Dictionary<Element, int> ShieldElementalPowers { get; private set; }
    public Animator DwarfAnimator { get; private set; }
    public Animator WeaponAnimator { get; private set; }
    public RuntimeAnimatorController animatorController;
    public RuntimeAnimatorController weaponAnimatorController;
    private Transform weapon;
    private Transform shield;
    private Transform healthBarFill;
    private Image healthFill;
    public Image healthBackground;
    private Transform healthBarCanvas;
    private Transform healthBarText;    
    private SocketController socketController;
    private TextMeshProUGUI healthBarTextComponent;
    private Vector3 dmgTextWorldPosition;
    private Vector3 dmgTextWorldPositionRandomized;
    [SerializeField] public GearData helmetGear { get; private set; }
    [SerializeField] public GearData chestGear { get; private set; }
    [SerializeField] public GearData glovesGear { get; private set; }
    [SerializeField] public GearData leggingsGear { get; private set; }
    [SerializeField] public GearData bootsGear { get; private set; }
    private CameraController cameraController; 
    private ShopManager shopManager;
    public DwarfStatsPanelController statsPanelController; // Vinculado desde el inspector  
    private PickaxeStatsPanelController weaponStatsPanelController;
    private GearSocketsController gearSocketsController;
    private PlayerBuffsController playerBuffsController;
    private CombatManager combatManager;
    public GearSocketTooltip helmetSocket;  
    public GearSocketTooltip chestSocket; 
    public GearSocketTooltip glovesSocket; 
    public GearSocketTooltip leggingsSocket; 
    public GearSocketTooltip bootsSocket; 
    public Transform statusEffectsBar; // Vinculado desde el inspector
    public Light2D weaponLight; // Vinculado desde el inspector
    public SpriteRenderer spriteRenderer; // Vinculado desde el inspector
    public bool unitsProcessed;
    private Material weaponMaterial;
    public GameObject statusIconPrefab;
    public static GameObject target;
    public Transform modifiersPanel;
    private Coroutine flickerWeaponLightCorutine;
    private List<SummonableUnitController> summonableUnits = new List<SummonableUnitController>();
    private Sprite characterSprite;
    private Color shieldBarImageColor;
    public bool areModifiersAbsorbed;
    public bool isAnimatingModifier;
    private int delayedTweenID = -1;
    
    private void Awake()
    {
        Instance = this;
        // Buscar automáticamente el transform del Pickaxe dentro de la jerarquía del prefab
        weapon = transform.Find("Weapon");
        weaponMaterial = weapon.GetComponent<SpriteRenderer>().material;
        shield = transform.Find("Shield");
        WeaponAnimator = weapon.GetComponent<Animator>();
        DwarfAnimator = gameObject.GetComponent<Animator>();
        shieldBarImageColor = shieldBarImage.color;      
        // Buscar automáticamente el Fill dentro de la jerarquía del prefab
        healthBarFill = transform.Find("HealthBar/HealthBarCanvas/Fill"); 
        healthBarCanvas = transform.Find("HealthBar/HealthBarCanvas");
        Canvas healthBarCanvasComponent = healthBarCanvas.GetComponent<Canvas>();
        healthBarCanvasComponent.overrideSorting = true;
        healthBarCanvasComponent.sortingOrder = 3;
        healthBarText = transform.Find("HealthBar/HealthBarCanvas/Text");     
        GameObject gearSocketsGameObject = GameObject.Find("Canvas/Middle Panel/Sockets/Canvas/Gear Sockets");
        helmetSocket = gearSocketsGameObject.transform.Find("Helmet")?.GetComponent<GearSocketTooltip>();
        chestSocket = gearSocketsGameObject.transform.Find("Chest")?.GetComponent<GearSocketTooltip>();
        glovesSocket = gearSocketsGameObject.transform.Find("Gloves")?.GetComponent<GearSocketTooltip>();
        leggingsSocket = gearSocketsGameObject.transform.Find("Legs")?.GetComponent<GearSocketTooltip>();
        bootsSocket = gearSocketsGameObject.transform.Find("Boots")?.GetComponent<GearSocketTooltip>();
        healthFill = healthBarFill.GetComponent<Image>();
        healthBarTextComponent = healthBarText.GetComponent<TextMeshProUGUI>();
        dmgTextWorldPosition = healthBarCanvas.TransformPoint(dmgTextPosition);
        WeaponElementalPowers = new Dictionary<Element, int>
        {
            { Element.Fire, 0 },
            { Element.Electric, 0 },
            { Element.Ice, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };        
        dwarfResistances = new Dictionary<Element, int> 
        {
            { Element.Fire, resFire },
            { Element.Electric, resIce },
            { Element.Ice, resElectric },
            { Element.Water, resWater },
            { Element.Nature, resNature },
            { Element.Earth, resEarth }
        };
    }

    private void Start()
    {
        shieldAbsorb = 0;
        LifeSteal = 0;
        UpdateHealthBar();
        weapon.gameObject.SetActive(HaveWeapon);
        shield.gameObject.SetActive(HaveShield);
        activeEffects = new List<StatusEffect>();
        activeBuffs = new List<PlayerBuff>();
        coinsController = CoinsController.Instance;
        cameraController = CameraController.Instance;
        shopManager = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
        combatManager = CombatManager.Instance;
        socketController = SocketController.Instance;
        weaponStatsPanelController = FindFirstObjectByType<PickaxeStatsPanelController>();
        gearSocketsController = GearSocketsController.Instance;
        playerBuffsController = PlayerBuffsController.Instance;
        StartCoroutine(GetFirstTarget());
    }

    public void InitializeDwarf(CharacterData characterData)
    {
        dwarfName = characterData.characterName;
        gameObject.name = dwarfName;
        description = characterData.description;
        id = characterData.id;
        characterSprite = characterData.characterSprite;

        maxHealth = characterData.maxHealth;
        currentHealth = maxHealth;
        healthBackground.color = characterData.characterColor;

        animatorController = characterData.animatorController;
        DwarfAnimator.runtimeAnimatorController = animatorController;
        weaponAnimatorController = characterData.weaponAnimatorController;
        WeaponAnimator.runtimeAnimatorController = weaponAnimatorController;
    }     

    public IEnumerator TakeDamage(int physicalDamage, Dictionary<Element, int> elementalPowers)
    {
        Vector3 spawnPos = dmgTextWorldPosition + new Vector3(UnityEngine.Random.Range(-60f, 60f), 0f, 0f);
        List<(Element element, int dmg)> finalElements = new List<(Element element, int dmg)>();
        int totalDmg = 0;
        // APARTADO DEL DAÑO ELEMENTAL
        // // Texto Flotante para los daños elementales
        foreach (var kv in elementalPowers)
        {
            var elementName = kv.Key;
            var elementPower = kv.Value;
            if (elementPower == 0) continue;

           float resistance = 0f;
            if (dwarfResistances.ContainsKey(elementName))
                resistance = dwarfResistances[elementName] / ((float)dwarfResistances[elementName] + factorKres);
            Debug.Log($"Resistance = {resistance}");
            int realElementalDmg = Mathf.Max(0, (int)(elementPower - elementPower * resistance));
            if (realElementalDmg > 0)
            {
                finalElements.Add((elementName, realElementalDmg));
                totalDmg += realElementalDmg;
            }
            Debug.Log($"realElementalDmg = {realElementalDmg}");
        }
        // APARTADO DEL DAÑO FÍSICO
        // Texto Flotante para el daño Fisico   
        // CALCULO DE LA ABSORCIÓN FISICA POR EL ESCUDO
        // Fácil, simplemente se resta la absorción.
        float dmgAfterShield = Mathf.Max(0, physicalDamage - shieldAbsorb);
        if (dmgAfterShield == 0f)
        {
            shieldAbsorb = shieldAbsorb - physicalDamage;
        }
        else
        {
            RemoveShield();
        }
        FillAndFlashShield();
        int absorbed = (int)(physicalDamage - dmgAfterShield);
        if (absorbed > 0)
        {
            FloatingTextSpawner.Instance?.SpawnAbsorbOne(
            spawnPos,
            healthBarCanvas,         
            absorbed
            );
        }               
        // CALCULO DE LA MITIGACIÓN FÍSICA POR ARMADURA //
        // MITIGATION = ARMOR / (ARMOR + K)
        // DMG = DMG - DMG * MITIGATION
        float mitigation = (float)armor / (armor + factorKarmor);
        //Debug.Log($"mitigationFromArmor = {mitigation}");
        // Es importante que la mitigación de la armadura se haga con el dmg despues de la absorcion del escudo.
        int realPhysicalDmg = Mathf.Max(0, (int)(dmgAfterShield - dmgAfterShield * mitigation));
        //Debug.Log($"realPhysicalDmg = {realPhysicalDmg}");
        //Debug.Log($"totalDmgBeforePhysyc = {totalDmg}");
        FloatingTextSpawner.Instance?.SpawnDamageBatch(
            spawnPos,
            healthBarCanvas,         // el mismo parent que usabas
            realPhysicalDmg,
            finalElements
        );
        SoundsFXManager.Instance.PlayPlayerTakeDmgSound();
        totalDmg += realPhysicalDmg;       
        currentHealth -= totalDmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
        

        // ANIMACIÓN DEL DAÑO
        spriteRenderer.color = Color.red;
        yield return null;
        spriteRenderer.color = Color.white;
        // Actualizar la barra de vida
        UpdateHealthBar();
    }

    public IEnumerator TakeHeal(int healPower, Action onComplete = null)
    {
        if (healPower > 0)
        {            
            dmgTextWorldPositionRandomized = dmgTextWorldPosition + new Vector3(UnityEngine.Random.Range(-150, 150), 0, 0);
            currentHealth += healPower;
            FloatingTextSpawner.Instance.SpawnHealOne(dmgTextWorldPositionRandomized, healthBarCanvas, healPower);
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
            UpdateHealthBar();
        }
        onComplete?.Invoke();        
        yield return null;
    }
    private void UpdateHealthBar()
    {
        // Actualizar la barra de vida en función de la salud actual
        // Aasegurarnos que nunca supere la maxHealth
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }
        healthBarTextComponent.text = $"{currentHealth}  /  {maxHealth}";
    }
    public void UpdateResistancesText()
    {
        resFire = dwarfResistances[Element.Fire];
        resIce = dwarfResistances[Element.Ice];
        resElectric = dwarfResistances[Element.Electric];
        resWater = dwarfResistances[Element.Water];
        resNature = dwarfResistances[Element.Nature];
        resEarth = dwarfResistances[Element.Earth];
    }
    public void UpdateWeaponPowersText()
    {
        weaponFirePower = WeaponElementalPowers[Element.Fire];
        weaponIcePower = WeaponElementalPowers[Element.Ice];
        weaponElectricPower = WeaponElementalPowers[Element.Electric];
        weaponWaterPower = WeaponElementalPowers[Element.Water];
        weaponNaturePower = WeaponElementalPowers[Element.Nature];
        weaponEarthPower = WeaponElementalPowers[Element.Earth];
    }
    public void UpdateWeaponMaterialEffect()
    {
        if (WeaponElementalPowers[Element.Fire] != 0)
        {
            if (!weaponLight.enabled)
            {
                weaponLight.enabled = true;
                flickerWeaponLightCorutine = StartCoroutine(WeaponLightFlicker());
            }
            weaponMaterial.SetFloat("_MetalFade", 1f);
        }
        else
        {                 
            weaponMaterial.SetFloat("_MetalFade", 0f);
        }
        if (WeaponElementalPowers[Element.Electric] != 0)
        {
            if (!weaponLight.enabled)
            {
                weaponLight.enabled = true;
                flickerWeaponLightCorutine = StartCoroutine(WeaponLightFlicker());
            }
            weaponMaterial.SetFloat("_TextureLayer1Fade", 1f);
        }
        else
        {   
            if (WeaponElementalPowers[Element.Fire] == 0)
            {
                weaponLight.enabled = false;
                if (flickerWeaponLightCorutine != null) StopCoroutine(flickerWeaponLightCorutine);
            }                    
            weaponMaterial.SetFloat("_TextureLayer1Fade", 0f);
            
        }       
    }
    private void Die()
    {
        Debug.Log("PlayerDwarf ha muerto");
        // Aquí puedo añadir efectos o lógica de muerte
    }
    public void GiveWeapon(int power, int turns, Dictionary<Element, int> elementalPowers, string title, Sprite icon)
    {
        // Solo hace la animación si no tenia el pico antes.
        if (!HaveWeapon) StartCoroutine(AnimateGiveWeapon());
        else  StartCoroutine(LiberateFlagCombat());
        HaveWeapon = true;
        PhysicalPowerBase = power;
        WeaponTitle = title;
        weaponTotalDuration = turns;
        weaponTurnsRemaining = turns;
        socketController.UpdateSockets(icon);
        AddElementalPowers(elementalPowers, new Dictionary<Element, int> { });
    }
    private IEnumerator LiberateFlagCombat()
    {
        yield return new WaitUntil(() => socketController.isAbsorbed);
        PlayedCardsController.Instance.combatReadyFlag = true;
    }
    public IEnumerator AnimateGiveWeapon()
    {
        // Pero no empieza la animación hasta que no haya pasado la animación del socket de absorción de elementos.
        yield return new WaitUntil(() => socketController.isAbsorbed);        
        DwarfAnimator.SetBool("GivePickaxeAnimation", true);
    }

    public void AddElementalPowers(Dictionary<Element, int> elementalPowers, Dictionary<Element, int> elementalResistances)
    {
        // Comprueba que al menos hay un valor mayor que 0
        if(!elementalPowers.Values.Any(value => value > 0) && !elementalResistances.Values.Any(value => value > 0))
        {
            Debug.Log("No hay modificadores elementales, no se añade ni poder ni resistencia");
            return;
        }
        if (HaveWeapon)
        {
            Debug.Log("Le agregamos los poderes al pico");
            foreach (var element in elementalPowers)
            {
                if (WeaponElementalPowers.ContainsKey(element.Key))
                {
                    WeaponElementalPowers[element.Key] += elementalPowers[element.Key];
                }
            }
            UpdateWeaponPowersText();
            UpdateWeaponMaterialEffect();

            //weaponStatsPanelController.UpdatePickaxeStatsPanel();
        }
        AddResistances(elementalResistances);
        UpdateResistancesText();       
    }
    public void AddResistances(Dictionary<Element, int> elementalResistances)
    {
        foreach (var element in elementalResistances)
        {
            if (dwarfResistances.ContainsKey(element.Key))
            {
                dwarfResistances[element.Key] += element.Value;
            }
            else
            {
                dwarfResistances[element.Key] = element.Value;
            }            
        }
        UpdateResistancesText();
    }
    public void RemoveResistances(Dictionary<Element, int> elementalResistances)
    {
        foreach (var element in elementalResistances)
        {
            if (dwarfResistances.ContainsKey(element.Key))
            {
                dwarfResistances[element.Key] -= element.Value;
            }
        }
        UpdateResistancesText();
    }
    
    public void CreateShield(int duration, int physicalMitigation, Dictionary<Element, int> elementalResistances)
    {
        if (HaveShield)
        {
            // Si el escudo ya existe y no ha alcanzado el nivel máximo, lo incrementa
            shieldAbsorb += physicalMitigation;
            shieldTotalAbsortion += physicalMitigation;
        }
        else
        {
            // Si no hay escudo, se crea en Nivel 1
            shieldAbsorb = physicalMitigation;
            shieldTotalAbsortion = physicalMitigation;
            HaveShield = true;
            shield.gameObject.SetActive(true);
        }
        shieldTotalDuration = duration;
        shieldDuration = duration;
        FillAndFlashShield();        
        // Esto era para que el escudo haga daño al tocarlo
        //ShieldElementalPowers = new Dictionary<string, int>(elementalResistances);
        AddResistances(elementalResistances);
        //socketController.UpdateSockets();
        // Debug.Log($"Dwarf recibió un escudo con mitigación física de {physicalMitigation} por {duration} turnos, con poderes elementales: {string.Join(", ", elementalResistances.Select(element => $"{element.Key}: {element.Value}"))}.");   
    }
    private void FillAndFlashShield()
    {

        if (shieldAbsorb == 0)
        {
            LeanTween.value(shieldBarImage.gameObject, shieldBarImage.fillAmount, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                shieldBarImage.fillAmount = val;
            });
        }
        else if ((float)shieldAbsorb / (float)shieldTotalAbsortion == shieldBarImage.fillAmount)
        {
            return;
        }
        else if ((float)shieldAbsorb / (float)shieldTotalAbsortion < shieldBarImage.fillAmount)
        {
            SoundsFXManager.Instance.PlayLoadShieldSound(false);
            LeanTween.value(shieldBarImage.gameObject, shieldBarImage.fillAmount, (float)shieldAbsorb / (float)shieldTotalAbsortion, 0.76f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                shieldBarImage.fillAmount = val;
            })
            .setOnComplete(() =>
            {
                StartCoroutine(FlashWhiteRoutine());
            });
        }
        else if ((float)shieldAbsorb / (float)shieldTotalAbsortion > shieldBarImage.fillAmount)
        {
            SoundsFXManager.Instance.PlayLoadShieldSound(true);
            LeanTween.value(shieldBarImage.gameObject, shieldBarImage.fillAmount, (float)shieldAbsorb / (float)shieldTotalAbsortion, 0.76f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                shieldBarImage.fillAmount = val;
            })
            .setOnComplete(() =>
            {
                StartCoroutine(FlashWhiteRoutine());
            });
        }
        UpdateShieldDurationText();
    }
    private void UpdateShieldDurationText()
    {
        if (shieldDuration > 0)
        {
            shieldDurationText.text = shieldDuration.ToString();
        }
        else
        {
            shieldDurationText.text = "";
        }       
    }
    private IEnumerator FlashWhiteRoutine()
    {
        Color originalColor = shieldBarImageColor;
        for (int i = 0; i < 2; i++)
        {
            Color startColor = shieldBarImage.color;

            // Subir a blanco (aumentar alpha o brillo, según lo que quieras)
            LeanTween.value(shieldBarImage.gameObject, 0f, 1f, 0.1f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnUpdate((float t) =>
                {
                    Color c = startColor;
                    c.a = Mathf.Lerp(0f, 1f, t); // si quieres parpadeo de transparencia
                    shieldBarImage.color = c;
                });

            yield return new WaitForSeconds(0.1f);

            // Volver al color original
            LeanTween.value(shieldBarImage.gameObject, 0f, 1f, 0.1f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnUpdate((float t) =>
                {
                    Color c = startColor;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    shieldBarImage.color = c;
                });

            yield return new WaitForSeconds(0.1f);
        }
        // Restaurar el color final original
        
        shieldBarImage.color = shieldBarImageColor;
    }

    public void ModifyAttack(float modifier)
    {
        PhysicalPowerModifier = modifier;
    }
    public int ModifiedPhysicalPower()
    {
        float physical = PhysicalPowerBase;
        float modified = physical * (1 + PhysicalPowerModifier / 100);
        // Esto trunca el decimal y ya esta
        return (int)modified;
        // se podria utilizar para redondear al entero mas cercano (int)Math.Round(modified);
    }
    public IEnumerator Attack()
    {
        FieldEntityController fieldEntityController;
        GolemController golemController = GolemController.Instance;
        if (!HaveWeapon)
        {
            yield break;
        }
        if (isRooted)
        {
            Debug.Log("Rooted, can't move");
            yield break;
        }
        IsMoving = true;
        if (target.CompareTag("FieldEntity"))
        {
            fieldEntityController = target.GetComponent<FieldEntityController>();
            Debug.Log("El Dwarf está atacando al FieldEntity...");
            // Esperamos que acabe la animación de recibir el pico
            yield return new WaitUntil(() => DwarfAnimator.GetBool("GivePickaxeAnimation") == false);
            ////////////// ANIMACIÓN DE ATACAR //////////////////////
            Vector3 startPosition = transform.position;
            WeaponAnimator.SetTrigger("Attack");
            DwarfAnimator.SetTrigger("Attack");
            Vector3 attackPosition = new Vector3(target.transform.position.x - 10f, startPosition.y, 0f);
            float elapsedTime = 0f;
            float durationForward = 1.2f;
            float durationJumpBack = 1f;
            float jumpHeight = 50f;
            while (elapsedTime < durationForward)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, attackPosition, elapsedTime / durationForward);
                yield return null;
            }
            transform.position = attackPosition; // Asegurar que llegue a la posición exacta
            // Hit al FieldEntity targeteado
            fieldEntityController.TakeHit();
            foreach (GearEffect effect in GearEffectsToActivateWhenMakeDmg())
            {
                effect.OnMakeDamage?.Invoke(this);
            }
            yield return new WaitForSeconds(0.3f);
            ///////////// PARÁBOLA HACIA ATRAS ////////////////////////
            elapsedTime = 0f;
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
            //////////////////////////////////////////////////////////
            IsMoving = false;
            SyncIdleAnimations();
        }
        else if (target.CompareTag("Golem"))
        {
            Debug.Log("El Dwarf está atacando al Golem...");
            // Esperamos que acabe la animación de recibir el pico
            yield return new WaitUntil(() => DwarfAnimator.GetBool("GivePickaxeAnimation") == false);
            ////////////// ANIMACIÓN DE ATACAR //////////////////////
            Vector3 startPosition = transform.position;
            WeaponAnimator.SetTrigger("Attack");
            DwarfAnimator.SetTrigger("Attack");
            Vector3 attackPosition = startPosition + new Vector3(440f, 0, 0);
            float elapsedTime = 0f;
            float durationForward = 1.2f;
            float durationJumpBack = 1f;
            float jumpHeight = 50f;
            while (elapsedTime < durationForward)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, attackPosition, elapsedTime / durationForward);
                yield return null;
            }
            transform.position = attackPosition; // Asegurar que llegue a la posición exacta
            //////////////////////////////////////////////////////////
            if (RollCritical())
            {
                // El ultimo argumento es el controller del atacante, en este caso esta misma clase.
                Debug.Log("CRITIQUIIIN");
                StartCoroutine(golemController.TakeDamage(ModifiedPhysicalPower() * (1 + criticalDmg / 100), WeaponElementalPowers, this));
            }
            else
            {
                // El ultimo argumento es el controller del atacante, en este caso esta misma clase.
                StartCoroutine(golemController.TakeDamage(ModifiedPhysicalPower(), WeaponElementalPowers, this));
            }
            foreach (GearEffect effect in GearEffectsToActivateWhenMakeDmg())
            {
                effect.OnMakeDamage?.Invoke(this);
            }
            yield return new WaitForSeconds(0.3f);
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
            IsMoving = false;
            SyncIdleAnimations();
        }
        golemController.SetTarget();
    }
    public void PlayStepSound()
    {
        SoundsFXManager.Instance.PlayPlayerStep();
    }
    public void PlayWeaponSpawnSound()
    {
        SoundsFXManager.Instance.PlayWeaponSpawnSound();
    }
    private bool RollCritical()
    {
        float rnd = UnityEngine.Random.Range(0f, 100f);
        return rnd < criticalRate;
    }
    public void BounceDmg(float bounceDmg, int objectives)
    {
        List<Transform> targets = GetOtherTargets();
        int counter = 0;
        foreach (Transform target in targets)
        {
            if (counter >= objectives)
            {
                return;
            }
            if (target.CompareTag("Golem"))
            {
                GolemController golemController = target.GetComponent<GolemController>();
                StartCoroutine(golemController.TakeDamage((int)(ModifiedPhysicalPower() * (bounceDmg / 100)), new Dictionary<Element, int>(), this));
            }
            else if (target.CompareTag("FieldEntity"))
            {
                FieldEntityController entityController = target.GetComponent<FieldEntityController>();
                entityController.TakeHit();
            }
            counter++;            
        }        
    }
    private List<Transform> GetOtherTargets()
    {
        List<Transform> entities = new List<Transform>();

        if (target.CompareTag("Golem"))
        {
            // Si el target es el golem, es fácil, los demás target son los del campo
            Debug.Log("Devuelve las entidades del campo");
            entities = combatManager.GetFieldEntities();
        }
        else if (target.CompareTag("FieldEntity"))
        {
            // Si el target es un fieldEntity restamos ese entity de la lista y metemos el Golem
            entities = combatManager.GetFieldEntities();
            Debug.Log("Le quitamos la misma entity");
            entities.Remove(target.transform);
            GolemController golemController = FindFirstObjectByType<GolemController>();
            entities.Add(golemController.transform);
            Debug.Log($"Le agregamos el {golemController.golemData.golemName} ");
        }

        return entities;
    }
    public IEnumerator ShieldRetaliation(GolemController golemController)
    {
        // Si tiene escudo y además tiene daño elemental entonces hace las represalias
        if (HaveShield && ShieldElementalPowers.Values.Any(value => value > 0))
        {
            Debug.Log("El escudo hace daño al Golem...");
            yield return new WaitForSeconds(1f);
            // El ultimo argumento es el controller del atacante, en este caso esta misma clase.
            StartCoroutine(golemController.TakeDamage(0, ShieldElementalPowers, this));
        }
    }

    public void LifeStealHeal(int physicalDmg)
    {
        float lifeStealHeal = physicalDmg * (0.98f * (1 - Mathf.Exp(-kLifeSteal * LifeSteal)));
        Debug.Log($"LifeStealHeal = {(int)lifeStealHeal}");
        StartCoroutine(TakeHeal((int)lifeStealHeal));
    }
    
    // Esta función la llamo desde un Event en el último frame de la animación GivePickaxe y siempre que queramos sincronizar el Idle del pico con el Idle del dwarf
    public void SyncIdleAnimations()
    {
        DwarfAnimator.SetBool("GivePickaxeAnimation", false);
        weapon.gameObject.SetActive(HaveWeapon);
        AnimatorStateInfo dwarfStateInfo = DwarfAnimator.GetCurrentAnimatorStateInfo(0);  
        float normalizedTime = dwarfStateInfo.normalizedTime % 1;
        WeaponAnimator.enabled = true;
        WeaponAnimator.Play("Basic_Pickaxe_Idle", 0, normalizedTime);
        PlayedCardsController.Instance.combatReadyFlag = true;
        
    }
    private void RemoveWeapon()
    {
        HaveWeapon = false;
        weapon.gameObject.SetActive(HaveWeapon);
        ClearElementalPowers();
        socketController.UpdateSockets();
        PhysicalPowerBase = 0;
        weaponTurnsRemaining = 0;
        //weaponStatsPanelController.UpdatePickaxeStatsPanel();
        Debug.Log("El pico ha expirado.");
    }
    private void ClearElementalPowers()
    {
        // Hay que meter los valores de las keys en una lista para luego recorrer el diccionario segun las keys, ya que con element.Value no se modifica el diccionario.
        foreach (var key in WeaponElementalPowers.Keys.ToList())
        {
            WeaponElementalPowers[key] = 0;
        }
        UpdateWeaponPowersText();
        UpdateWeaponMaterialEffect();
    }
    private void RemoveShield()
    {
        HaveShield = false;
        shield.gameObject.SetActive(HaveShield);        
        shieldDuration = 0;
        shieldAbsorb = 0;
        shieldTotalAbsortion = 0;
        FillAndFlashShield();
        Debug.Log("El escudo ha expirado.");
    }
    public void IncreaseLifeSteal(int lifesteal, int duration)
    {
        LifeSteal += lifesteal;
        lifeStealDuration = duration;
    }
    public void ModifyCriticalRate(int value)
    {
        Debug.Log($"Critical Rate modified {value}");
        criticalRate += value;
    }
    
    public void ModifyArmor(int value)
    {
        armor += value;
    }
    public void AddPlayerBuff(PlayerBuff buff)
    {
        activeBuffs.Add(buff);
        playerBuffsController.UpdatePlayerBuffs();

    }
    public void RemovePlayerBuff(PlayerBuff buff)
    {
        activeBuffs.Remove(buff);
        playerBuffsController.UpdatePlayerBuffs();
    }
    public void AddStatusEffect(StatusEffect effect)
    {        
        activeEffects.Add(effect);

        List<GearEffect> gearEffects = GearEffectsToActivateWhenStatusAdded(effect);
        foreach (GearEffect ef in gearEffects)
        {
            ef.OnStatusEffect?.Invoke(this);
        }
        UpdateStatusBarIcons();
    }
    public void RemoveStatusEffect(StatusEffect effect)
    {
        activeEffects.Remove(effect);

        if (effect.EffectType == StatusEffectType.Immobilize)
        {
            isRooted = false;
        }
        UpdateStatusBarIcons();
    }
    private void UpdateStatusBarIcons()
    {
        foreach (Transform child in statusEffectsBar)
        {
            Destroy(child.gameObject);
        }
        foreach (var effect in activeEffects)
        {
            if (effect.EffectIcon != null) // Asegurarse de que el icono existe
            {
                GameObject iconGO = Instantiate(statusIconPrefab, statusEffectsBar); // Crear el icono en el canvas
                Image iconImage = iconGO.transform.GetChild(0).GetComponent<Image>();
                iconImage.sprite = effect.EffectIcon; // Asignar el sprite del estado
            }
        }       
    }
    public void ResolveBuffsOnTurnStart()
    {
        foreach (PlayerBuff buff in activeBuffs)
        {
            Debug.Log($"Resolviendo buff {buff.BuffName}");
            buff.OnTurnStart?.Invoke(this);
        }
    }
    public IEnumerator ResolveStatusEffects()
    {
        Debug.Log("Resolviendo efectos de estado del Dwarf...");
        List<StatusEffect> expiredEffects = new List<StatusEffect>();
        foreach (var effect in activeEffects)
        {
            Debug.Log($"Processing status: {effect.StatusName}");
            effect.ApplyEffect(this);
            effect.TurnsRemaining--;
            if (effect.IsExpired())
            {
                expiredEffects.Add(effect);
            }
        }
        // Eliminamos los efectos expirados después del loop anterior para evitar bugs utilizando una lista auxiliar
        foreach (var effect in expiredEffects)
        {
            RemoveStatusEffect(effect);
        }
        yield return new WaitForSeconds(0.5f);
    }
    
    public void Immobilize()
    {
        Debug.Log("IMMOVILIZADO");
        isRooted = true;
    }
    public void ModifyStatusEffectTurns(string statusName, int value)
    {
        foreach (StatusEffect effect in activeEffects)
        {
            if (effect.StatusName == statusName)
            {
                effect.TurnsRemaining -= value;
            }
        }
    }
    public void UpdateTurnsOfGearAndBuffs()
    {
        weaponTurnsRemaining--;
        weaponTurnsRemaining = Mathf.Clamp(weaponTurnsRemaining, 0, int.MaxValue);
        shieldDuration--;
        shieldDuration = Mathf.Clamp(shieldDuration, 0, int.MaxValue);
        UpdateShieldDurationText();
        lifeStealDuration--;
        lifeStealDuration = Mathf.Clamp(lifeStealDuration, 0, int.MaxValue);
        if (weaponTurnsRemaining == 0)
        {
            RemoveWeapon();
        }
        if (shieldDuration == 0)
        {
            RemoveShield();
        }
        if (lifeStealDuration == 0)
        {
            LifeSteal = 0;
        }
        socketController.UpdateWeaponSegments();
    }
    public void EquipGear(GearData gearData)
    {
        // Las pasivas de la ropa las sacamos del catalogo
        // Y se las agregamos a cada pieza justo antes de equiparla
        gearData.gearEffects = GearEffectCatalog.CreateEffectsForGear(gearData);
        // Equipamos la pieza en el hueco que le toca
        switch (gearData.gearType)
        {
            case GearType.Helmet:
                if (helmetGear != null) DisEquip(helmetGear);
                helmetGear = gearData;
                helmetSocket.EquipGear(gearData);

                break;
            case GearType.Chestplate:
                if (chestGear != null) DisEquip(chestGear);
                chestGear = gearData;
                chestSocket.EquipGear(gearData);
                break;
            case GearType.Gloves:
                if (glovesGear != null) DisEquip(glovesGear);
                glovesGear = gearData;
                glovesSocket.EquipGear(gearData);
                break;
            case GearType.Leggings:
                if (leggingsGear != null) DisEquip(leggingsGear);
                leggingsGear = gearData;
                leggingsSocket.EquipGear(gearData);
                break;
            case GearType.Boots:
                if (bootsGear != null) DisEquip(bootsGear);
                bootsGear = gearData;
                bootsSocket.EquipGear(gearData);
                break;
        }
        // Y si tiene efectos que se activan al equipar se hace ahora.
        foreach (GearEffect effect in gearData.gearEffects)
        {
            effect.OnEquip?.Invoke(this);
        }
        gearSocketsController.UpdateGearUI();
    }
    public void DisEquip(GearData gearData)
    {
        bool disequipped = false;
        switch (gearData.gearType)
        {
            case GearType.Helmet:
                if (helmetGear == gearData)
                {
                    disequipped = true;
                    helmetGear = null;
                }
                break;
            case GearType.Chestplate:
                if (chestGear == gearData)
                {
                    disequipped = true;
                    chestGear = null;
                }
                break;
            case GearType.Gloves:
                if (glovesGear == gearData)
                {
                    disequipped = true;
                    glovesGear = null;
                }
                break;
            case GearType.Leggings:
                if (leggingsGear == gearData)
                {
                    disequipped = true;
                    leggingsGear = null;
                }
                break;
            case GearType.Boots:
                if (bootsGear == gearData)
                {
                    disequipped = true;
                    bootsGear = null;
                }
                break;
        }
        if (disequipped)
        {
            foreach (GearEffect effect in gearData.gearEffects)
            {
                effect.OnDisequip?.Invoke(this);
            }
        }
        gearSocketsController.UpdateGearUI();
    }

    private IEnumerator WeaponLightFlicker()
    {
        while (true)
        {
            float targetIntensity = UnityEngine.Random.Range(1f, 5f);
            float duration = UnityEngine.Random.Range(0.1f, 0.2f);
            float startIntensity = weaponLight.intensity;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                weaponLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t / duration);
                yield return null;
            }
        }
    }
    public void ResetDwarfForRound()
    {
        RemoveShield();
        RemoveWeapon();
        socketController.UpdateSockets();
        LifeSteal = 0;
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            PlayerBuff buff = activeBuffs[i];
            buff.roundRemaining--;
            if (buff.roundRemaining <= 0)
            {
                buff.OnBuffExpires?.Invoke(this);
                RemovePlayerBuff(buff);
            }
        }
        // ESTO NO SE PUEDE HACER NUNCA!! SINÓ PETA! NO SE PUEDEN BORRAR ELEMENTOS DE UNA LISTA MIENTRAS SE HACE FOREACH
        /*
            foreach (PlayerBuff buff in activeBuffs)
            {
                buff.roundRemaining--;
                if (buff.roundRemaining <= 0)
                {
                    RemovePlayerBuff(buff);
                }
            }
            */
        foreach (var unit in summonableUnits.ToList()) // Copia para evitar modificar mientras itero
        {
            Destroy(unit.gameObject);
            RemoveSummonableUnit(unit);
        }
        playerBuffsController.UpdatePlayerBuffs();
    }
    public void AddSummonableUnit(SummonableUnitController unit)
    {
        Debug.Log("Spawned Unit");
        summonableUnits.Add(unit);
    }

    public void RemoveSummonableUnit(SummonableUnitController unit)
    {
        if (summonableUnits.Contains(unit))
        {
            summonableUnits.Remove(unit);
        }
    }
    public IEnumerator AttackOfAllSummonedUnits()
    {
        Debug.Log("Intenta hacer los ataques de las units");

        if (summonableUnits.Count == 0)
            yield break;

        foreach (var unit in summonableUnits.ToList()) // copia defensiva
        {
            if (GolemController.Instance.IsKnockedOut)
                yield break; // detiene la secuencia de ataques

            yield return StartCoroutine(unit.UnitAttack(null));
        }
    }
    public void AddResistanceModifier(TemporaryModifier modifier, Vector3 screenPos)
    {
        areModifiersAbsorbed = false;
        GameObject modifierPrefab = socketController.modifierPrefab;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            modifiersPanel.GetComponent<RectTransform>(),
            screenPos,
            Camera.main,
            out Vector2 anchoredPos
        );
        Transform duplicate = null;
        foreach (Transform tmpTransform in modifiersPanel)
        {
            TextMeshProUGUI textComponent = tmpTransform.GetComponent<TextMeshProUGUI>();
            Debug.Log("Element: " + modifier.resistanceElement);
            if (textComponent.color == FloatingText.ElementColorMap[modifier.resistanceElement])
            {
                duplicate = tmpTransform;
            }
        }
        GameObject modifierGO = Instantiate(modifierPrefab, anchoredPos, Quaternion.identity, modifiersPanel);
        modifierGO.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        modifierGO.GetComponent<RectTransform>().localScale = Vector3.one * 2.5f;
        TextMeshProUGUI tmp = modifierGO.GetComponent<TextMeshProUGUI>();
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.greenYellow);
        tmp.color = FloatingText.ElementColorMap[modifier.resistanceElement];
        tmp.text = $"+ {modifier.damage} {modifier.resistanceElement} <size=50%>Resist</size>";        
        AnimateModifierFromCard(modifierGO.transform, duplicate);
    }
    private float CalculateYPosition(Transform modifierTransform)
    {
        float modifierHeight = modifierTransform.GetComponent<RectTransform>().rect.height * 2;
        float y = 87.5f - modifierHeight * modifierTransform.GetSiblingIndex();
        return y;
    }
    private void AnimateModifierFromCard(Transform modifierTransform, Transform duplicate)
    {
        isAnimatingModifier = true;
        LeanTween.cancel(delayedTweenID);
        Vector3 targetPosition = new Vector3(0f, CalculateYPosition(modifierTransform), 0f);

        LeanTween.moveLocal(modifierTransform.gameObject, targetPosition, 0.4f)
            .setEaseOutSine();

        LeanTween.scale(modifierTransform.gameObject, Vector3.one, 0.4f)
            .setEaseOutSine();
        if (duplicate != null)
        {
            LeanTween.delayedCall(0.4f, () =>
                        {                            
                                TMP_Text tmp = modifierTransform.GetComponent<TMP_Text>();
                                Color startColor = tmp.color;
                                string statA = duplicate.GetComponent<TextMeshProUGUI>().text;
                                string statB = modifierTransform.GetComponent<TextMeshProUGUI>().text;
                                string combinedTexts = FloatingText.CombineStats(statA, statB);
                                LeanTween.moveLocal(modifierTransform.gameObject, duplicate.localPosition, 0.1f)
                                    .setOnComplete(() =>
                                    {
                                        LeanTween.value(modifierTransform.gameObject, 0f, 1f, 0.1f)
                                            .setOnUpdate((float t) =>
                                            {
                                                Color c = startColor;
                                                c.a = t; // interpolamos el alpha
                                                tmp.color = c;

                                            })
                                            .setOnComplete(() =>
                                            {
                                                Destroy(modifierTransform.gameObject);
                                                duplicate.GetComponent<TextMeshProUGUI>().text = combinedTexts;
                                                PlayedCardsController.processingCard.IsProcesed = true;
                                                delayedTweenID = LeanTween.delayedCall(0.05f, () =>
                                                    {
                                                        isAnimatingModifier = false;
                                                    })
                                                    .id;
                                            });
                                    });                                                       
                        });
        }
        else
        {
            delayedTweenID = LeanTween.delayedCall(0.4f, () =>
                        {
                            PlayedCardsController.processingCard.IsProcesed = true;
                            isAnimatingModifier = false;
                            //Debug.Log($"Ponemos isAnimating en {isAnimating} (tras delay {1.2f}s)");
                        })
                        .id;
        }
    }
    public IEnumerator AbsorbingElementsAnimation()
    {
        areModifiersAbsorbed = false;
        // Éste bucle de espera es prácticamente igual que el WaitUntil, pero más estable
        // Ya que en la carrera de las flags para ser false, en cada frame pasan los checks a la vez
        // Por lo tanto es más estable
        while (isAnimatingModifier || PlayedCardsController.Instance.isProcessingCards)
        {
            yield return null; // esperar un frame
        }
        //Debug.Log($"Coinciden las 2 en false, {isAnimating} y {playedCardsController.isProcessingCards}");
        //yield return new WaitUntil(() => isAnimating == false && playedCardsController.isProcessingCards == false);
        // Recolectar hijos con TMP + sus datos iniciales
        var childRTs = new List<RectTransform>();
        var tmps = new List<TextMeshProUGUI>();
        var startPos = new List<Vector2>();
        var baseColors = new List<Color>(); // guardamos RGB original

        foreach (Transform child in modifiersPanel)
        {
            var rt = child as RectTransform;
            if (rt == null) continue;

            var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null) continue;

            childRTs.Add(rt);
            tmps.Add(tmp);
            startPos.Add(rt.anchoredPosition);
            if (tmp.color != Color.green)
            {
                baseColors.Add(tmp.color); // guardamos color original (RGB)
            }
        }

        if (childRTs.Count == 0)
        {
            areModifiersAbsorbed = true;
            yield break;
        }

        // Convertir el centro del target a coordenadas locales del mismo espacio que usan los hijos (el padre 'source')
        // Usamos el centro del target (pivot) como destino.
        float spriteDwarfHeight = characterSprite.bounds.size.y;
        Vector3 targetWorldCenter = transform.TransformPoint(new Vector3(0, spriteDwarfHeight / 2f, 0));
        Vector2 targetLocalInSourceParent = modifiersPanel.InverseTransformPoint(targetWorldCenter);

        // Para cada hijo, el target local para su anchoredPosition debe estar en el espacio de su propio padre (source)
        // Si el padre de cada hijo es 'source', esto ya vale directamente:
        var endPos = new List<Vector2>(childRTs.Count);
        for (int i = 0; i < childRTs.Count; i++)
            endPos.Add(targetLocalInSourceParent);
        float duration = 0.8f;
        float fadeStartT = 0.9f;                  // 90% del tiempo

        // Lanzamos un tween maestro 0..1 y actualizamos todos en OnUpdate
        // (así se mueven exactamente sincronizados).
        LeanTween.value(modifiersPanel.gameObject, 0f, 1f, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                // Mover cada hijo
                for (int i = 0; i < childRTs.Count; i++)
                {
                    Vector2 p = Vector2.Lerp(startPos[i], endPos[i], t);
                    childRTs[i].anchoredPosition = p;
                }

                // Fade-out cuando se supera el 90%
                if (t >= fadeStartT)
                {
                    float k = Mathf.InverseLerp(fadeStartT, 1f, t); // 0..1 en el tramo final
                    for (int i = 0; i < tmps.Count; i++)
                    {
                        tmps[i].color = new Color(tmps[i].color.r, tmps[i].color.g, tmps[i].color.b, 1f - k);
                    }
                    Color mixedColor = SocketController.MixColors(baseColors);
                    spriteRenderer.material.SetColor("_InkSpreadColor", mixedColor);
                    // FADE-IN COLOR EN PLAYER SPRITE
                    LeanTween.value(spriteRenderer.gameObject, 0f, 1f, 0.2f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnUpdate((float t) =>
                        {
                            spriteRenderer.material.SetFloat("_InkSpreadFade", t);
                        })
                        .setOnComplete(() =>
                        {
                            StartCoroutine(SpriteBlinkOutline(mixedColor));
                            // FADE-OUT COLOR EN PLAYER SPRITE                           
                            LeanTween.value(spriteRenderer.gameObject, 1f, 0f, 0.2f)
                                .setEase(LeanTweenType.easeInOutQuad)
                                .setOnUpdate((float t) =>
                                {
                                    spriteRenderer.material.SetFloat("_InkSpreadFade", t);
                                });
                        });
                }
            })
            .setOnComplete(() =>
            {
                // Destruir hijos al final
                for (int i = 0; i < childRTs.Count; i++)
                {
                    if (childRTs[i] != null)
                        Destroy(childRTs[i].gameObject);
                }
                areModifiersAbsorbed = true;
            });
    }
    private IEnumerator SpriteBlinkOutline(Color c)
    {
        spriteRenderer.material.SetColor("_PixelOutlineColor", c);
        spriteRenderer.material.SetFloat("_PixelOutlineFade", 1f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material.SetFloat("_PixelOutlineFade", 0f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.material.SetFloat("_PixelOutlineFade", 1f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material.SetFloat("_PixelOutlineFade", 0f);
    }
    

    public void ReduceDurationOfAllSummonedUnit()
    {
        for (int i = summonableUnits.Count - 1; i >= 0; i--)
        {
            var unit = summonableUnits[i];
            unit.reduceTurnDurationUnit();
        }
    }
    public List<SummonableUnitController> GetSummonableUnits()
    {
        return summonableUnits;
    }

    public void AddGold(int amount)
    {
        if (amount >= 100)
        {
            SoundsFXManager.Instance.PlayLotOfMoneySound();
        }
        if (amount <= 30)
        {
            SoundsFXManager.Instance.PlayAlittleBitMoney();
        }
        else
        {
            SoundsFXManager.Instance.PlayMoneySound();
        }       
        gold += amount; 
        UpdateCoinsText();
    }
    public void SpendGold(int amount)
    {
        SoundsFXManager.Instance.PlaySpentMoneySound();
        gold -= amount;
        UpdateCoinsText();
    }
    public void AddRunes(int amount)
    {        
        if (amount >= 4)
        {
            SoundsFXManager.Instance.PlayRunesSound();
        }
        else
        {
            SoundsFXManager.Instance.PlayAlittleBitRunes();
        }
        runes += amount;
        UpdateCoinsText();
    }
    public void SpendRunes(int amount)
    {
        SoundsFXManager.Instance.PlayAlittleBitRunes();
        runes -= amount;
        UpdateCoinsText();
    }
    public void GainGoldFromRound()
    {
        int goldEarned = (int)currentHealth;
        AddGold(goldEarned);
        UpdateCoinsText();
    }
    public void UpdateCoinsText()
    {
        coinsController.UpdateCoinsTexts();
        if (shopManager != null)
        {
            shopManager.UpdateGoldTextOfShop();
        }        
    }
    public void getFusionatorAcces()
    {
        gotFusionatorAcces = true;
    }

    public void OnRightClick()
    {
        cameraController.ZoomToDwarf();
        statsPanelController.ToggleStatsPanel();
    }
    public List<GearEffect> GearEffectsToActivateWhenCardPlayed(CardData card)
    {
        List<GearEffect> gearEffects = new List<GearEffect>();
        foreach (GearData piece in GetDwarfGear())
        {
            // Si el slot está vacío, piece será null → lo saltamos
            if (piece == null) continue;                

            // Protección adicional: si gearEffects no se inicializó por alguna razón
            if (piece.gearEffects == null) continue;            
            
            foreach (GearEffect effect in piece.gearEffects)
            {
                if (effect.OnPlayedCardCondition != null && effect.OnPlayedCardCondition(card))
                {
                    gearEffects.Add(effect);
                }
            }
        }        
        return gearEffects;
    }
    public List<GearEffect> GearEffectsToActivateWhenStatusAdded(StatusEffect status)
    {
        List<GearEffect> gearEffects = new List<GearEffect>();
        foreach (GearData piece in GetDwarfGear())
        {
            // Si el slot está vacío, piece será null → lo saltamos
            if (piece == null) continue;                

            // Protección adicional: si gearEffects no se inicializó por alguna razón
            if (piece.gearEffects == null) continue;            
            
            foreach (GearEffect effect in piece.gearEffects)
            {
                if (effect.OnStatusCondition != null && effect.OnStatusCondition(status))
                {
                    gearEffects.Add(effect);
                }
            }
        }        
        return gearEffects;
    }
    public List<GearEffect> GearEffectsToActivateWhenMakeDmg()
    {
        List<GearEffect> gearEffects = new List<GearEffect>();
        foreach (GearData piece in GetDwarfGear())
        {
            // Si el slot está vacío, piece será null → lo saltamos
            if (piece == null) continue;                

            // Protección adicional: si gearEffects no se inicializó por alguna razón
            if (piece.gearEffects == null) continue;

            foreach (GearEffect effect in piece.gearEffects)
            {
                if (effect.OnMakeDamage != null)
                {
                    gearEffects.Add(effect);
                }
            }
        }        
        return gearEffects;
    }

    public List<GearData> GetDwarfGear()
    {
        List<GearData> gearList = new List<GearData>();
        gearList.Add(helmetGear);
        gearList.Add(chestGear);
        gearList.Add(glovesGear);
        gearList.Add(leggingsGear);
        gearList.Add(bootsGear);

        return gearList;
    }
    public List<PlayerBuff> GetActiveBuffs()
    {
        return activeBuffs;
    }
    private IEnumerator GetFirstTarget()
    {
        GolemController golem = null;
        yield return new WaitUntil(() => (golem = GolemController.Instance) != null);
        SetGolemTarget();        
    }
    public void SetGolemTarget()
    {
        target = GolemController.Instance.gameObject;
    }
    public int LifeSteal
    {
        get => lifeSteal;
        set => lifeSteal = value;
    }
}
