using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayHandButtonController : MonoBehaviour
{
    public static PlayHandButtonController Instance;
    GameObject playedCards;
    GameObject playerHand;
    PlayedCardsController playedCardsController;
    private CardController cardController;
    private Button playHandButton;
    PlayerHandController playerHandController;
    public float cardScalerModifier = 1f;

    
    void Start()
    {  
        playedCards = GameObject.Find("PlayedCards");
        playerHand = GameObject.Find("PlayerHand");
        playerHandController = playerHand.GetComponent<PlayerHandController>();
        playedCardsController = playedCards.GetComponent<PlayedCardsController>();
        playHandButton = GetComponent<Button>();
        // Listener del boton para activarse
        playHandButton.onClick.AddListener(() => moveCardstoPlayed(GetSelectedCards()));       
    }
    
    void Awake()
    {
        Instance = this;
    }

    private List<GameObject> GetSelectedCards()
    {
        // Devuelve la lista con los GameObject que son las cartas seleccionadas
        List<GameObject> selectedCards = new List<GameObject>();

        foreach (Transform card in playerHand.transform)
        {
            CardController cardEffect = card.GetComponent<CardController>();
            if (cardEffect.IsSelected)
            {
                cardController = card.gameObject.GetComponent<CardController>();
                cardController.BlockInteractions();
                cardController.IsPlayed = true;
                cardEffect.IsSelected = false;
                cardEffect.Spacing = playedCardsController.spacing;
                //Debug.Log($"spacing = {cardEffect.Spacing}");
                selectedCards.Add(card.gameObject);                
            }            
        }
        return selectedCards;
    }

    private void moveCardstoPlayed(List<GameObject> selectedCards)
    {   
        // Coge todos los gameObjects de la lista y los mete en el canvas de PlayedCards
        if (selectedCards.Count == 0) return;
        playerHandController.ToggleBlockPlayerHand();
        playerHandController.LowerPlayerHand();
        for (int i = 0; i < selectedCards.Count; i++)
        {
            GameObject cardToMove = selectedCards[i];
            LeanTween.cancel(cardToMove);
            Vector3 globalPosition = cardToMove.transform.position;
            Vector3 scaleTarget = new Vector3(cardScalerModifier, cardScalerModifier, 0);
            Canvas cardToMoveCanvas = cardToMove.GetComponentInChildren<Canvas>();
            CardController controller = cardToMove.GetComponent<CardController>();
            Image cardImage = controller.cardImage;
            cardImage.transform.localRotation = Quaternion.identity; 
            cardToMoveCanvas.GetComponent<GraphicRaycaster>().enabled = false;
            cardToMoveCanvas.sortingOrder = 15;
            cardToMove.transform.SetParent(playedCards.transform, true);             
            cardToMove.transform.position = globalPosition;
            cardToMove.transform.localScale = scaleTarget;
        }
        // Justo despues de pasar de canvas las cartas jugadas se updatean las posiciones en los dos canvas
        playedCardsController.UpdatePlayedCardsPositions();
        playerHandController.UpdatePlayerHandPositions();
        // Entonces marcamos la flag de que se ha jugado la mano
        playedCardsController.HandPlayed = true;
    }

}
