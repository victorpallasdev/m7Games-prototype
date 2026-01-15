using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    private PlayerCharacterController dwarfController;
    private GolemController golemController; 
    private PlayedCardsController playedCardsController;
    public ShopManager shopManager; // Vinculado desde el inspector
    public EntityInitializer entityInitializer; // Vinculado desde el inspector 
    public FieldEntityData entityData; // Vinculado desde el inspector
    private int turnCounter = 1;
    private int entitySpawnTurn = 1;
    PlayerHandController playerHandController;
    CameraController cameraController;
    private bool isUIhide;
    GameObject topPanel;
    GameObject sockets;
    public GameObject weaponShieldSockets;
    public GameObject playHandButton;
    GameObject bottomPanel;
    DeckManager deckManager;


    void Awake()
    {
        Instance = this;
    }
    IEnumerator Start()
    {
        // Obtén referencias a los controladores        
        dwarfController = PlayerCharacterController.Instance;
        yield return new WaitUntil(() => (golemController = GolemController.Instance) != null);        
        cameraController = CameraController.Instance;
        playerHandController = PlayerHandController.Instance;
        topPanel = GameObject.Find("Top Panel");
        sockets = GameObject.Find("Middle Panel/Sockets");
        bottomPanel = GameObject.Find("Bottom Panel");
        playedCardsController = PlayedCardsController.Instance;
     
        yield return new WaitUntil(() => ((deckManager = DeckManager.Instance) != null) && deckManager.isDeckLoaded);        
        deckManager.ShuffleDeck();
        playerHandController.turnDrawing();

        golemController.OnHealthBarDepleted += StartShopTransition;
        PlayedCardsController.CombatReady += StartCombat;
    }  
    public void StartCombat()
    {
        StartCoroutine(ExecuteCombat());
    }

    private IEnumerator ExecuteCombat()
    {
        // 1. Dwarf ataca al Golem
        
        yield return StartCoroutine(dwarfController.Attack());
        // Pausa entre ataques
        if(golemController.IsKnockedOut) yield break;
        yield return new WaitForSeconds(1f);
        // 4. Atacan las summoned Units, si las hay.
         yield return StartCoroutine(dwarfController.AttackOfAllSummonedUnits());
        
        if (golemController.IsKnockedOut) yield break;
        yield return new WaitForSeconds(1f);
        // 3. Golem ataca al Dwarf
        if (!golemController.IsKnockedOut)
        {
            golemController.IsAtacking = true;
            StartCoroutine(golemController.Attack(dwarfController));
            // yield return StartCoroutine(dwarfController.ShieldRetaliation(golemController));
            // 3. Resolver efectos de estado
            yield return new WaitUntil(() => !golemController.IsAtacking);
            yield return StartCoroutine(dwarfController.ResolveStatusEffects());
            // yield return StartCoroutine(golemController.ResolveStatusEffects());
            // 4. Finalizar turno
            EndTurn();
        }        
    }
    private void EndTurn()
    {
        playerHandController.ToggleBlockPlayerHand();
        // Creo que esto no hace falta por que se hace desde la misma playedCardsController
        //playedCardsController.ClearTemporaryModifiers();
        dwarfController.UpdateTurnsOfGearAndBuffs();
        dwarfController.ReduceDurationOfAllSummonedUnit();
        golemController.UpdateTurnsOfCdAndBuffs();
        
        Debug.Log("Turno finalizado. Listo para el siguiente turno.");
        turnCounter++;
        entitySpawnTurn++;
        dwarfController.ResolveBuffsOnTurnStart();
        playerHandController.turnDrawing();
        CheckAndSpawnFieldEntity();
    }
    public void CheckAndSpawnFieldEntity()
    {
        // La posibilidad de spawneo empieza en el turno 2        
        if (entitySpawnTurn >= 2)
        {
            float spawnChance = 0f;
            if (entitySpawnTurn == 2) // En el turno 2 hay un 50%
                spawnChance = 0.50f;
            else if (entitySpawnTurn == 3) // En el turno 3 hay un 75%
                spawnChance = 0.75f;
            else if (entitySpawnTurn >= 4) // En el 4 un 100%
                spawnChance = 1.0f;

            if (Random.value <= spawnChance)
            {
                if (GetFieldEntitiesAmount() < 3) // Si hay 3 entities ya, no spawnean mas
                {
                    entityInitializer.SpawnFieldEntity(entityData);
                }
                // Pero la variable la vamos reiniciando siempre que iba a spawnearse                
                entitySpawnTurn = 0;
            }
        }
    }
    // Devuelve la cantidad de fieldEntities que hay spawneadas en el Middle Panel
    private int GetFieldEntitiesAmount()
    {
        int amount = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("FieldEntity"))
            {
                amount++;
            }
        }
        return amount;
    }
    public List<Transform> GetFieldEntities()
    {
        List<Transform> entitites = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("FieldEntity"))
            {
                FieldEntityController entityController = child.GetComponent<FieldEntityController>();
                if (!entityController.isDropping)
                {
                    entitites.Add(child);
                }                
            }
        }
        return entitites;
    }

    private void StartShopTransition()
    {
        shopManager.gameObject.SetActive(true);
        StartCoroutine(ShopTransitionSequence());
    }
    private IEnumerator ShopTransitionSequence()
    {
        //golemController.PlayKnockedOutAnimation();
        // Esto espera hasta que el Dwarf esté en su sitio quieto. 
        yield return new WaitUntil(() => !dwarfController.IsMoving);
        shopManager.UpdateGoldTextOfShop();
        dwarfController.ResetDwarfForRound();
        golemController.ResetSpecialCD();
        dwarfController.GainGoldFromRound();

        //toggleUI();
        cameraController.ZoomToDwarf();
        shopManager.ShopOpen();

        yield return null;        
    }
    
    public void toggleUI()
    {
        if (isUIhide)
        {
            sockets.SetActive(true);
            topPanel.SetActive(true);
            bottomPanel.SetActive(true);
            isUIhide = false;
        }
        else
        {
            sockets.SetActive(false);
            topPanel.SetActive(false);
            bottomPanel.SetActive(false);
            isUIhide = true;           
        }
    }

    public void showUIInspectorItem()
    {
        playHandButton.SetActive(true);
        weaponShieldSockets.SetActive(true);
        bottomPanel.SetActive(true);
        isUIhide = false;                  
    }
    public void hideUIInspectorItem()
    {
        playHandButton.SetActive(false);
        weaponShieldSockets.SetActive(false);
        bottomPanel.SetActive(false);
        isUIhide = true; 
    }

    void OnDestroy()
    {
        PlayedCardsController.CombatReady -= StartCombat;
        golemController.OnHealthBarDepleted -= StartShopTransition;
        
    }
}
