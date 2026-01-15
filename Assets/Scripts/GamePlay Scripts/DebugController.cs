using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugController : MonoBehaviour
{  
    public Button passRoundButton; // Vinculado desde el Inspector
    public Button addTNTButton; // Vinculado desde el Inspector
    public Button spawnEntity; // Vinculado desde el Inspector
    public CardData cardData; // Vinculado desde el Inspector
    public FieldEntityData logData; // Vinculado desde el Inspector
    private GolemController golemController;
    private DeckManager deckManager;
    private EntityInitializer entityInitializer;
    

    IEnumerator Start()
    {
        yield return new WaitUntil(() => (golemController = FindFirstObjectByType<GolemController>()) != null);
        passRoundButton.onClick.AddListener(() => golemController.killGolem());

        yield return new WaitUntil(() => (deckManager = FindFirstObjectByType<DeckManager>()) != null);
        addTNTButton.onClick.AddListener(() => deckManager.PutCardOnHand(cardData));

        yield return new WaitUntil(() => (entityInitializer = FindFirstObjectByType<EntityInitializer>()) != null);
        spawnEntity.onClick.AddListener(() => entityInitializer.SpawnFieldEntity(logData));
    }

}
