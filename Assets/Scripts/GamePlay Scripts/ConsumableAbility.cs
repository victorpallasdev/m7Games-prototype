using System.Collections.Generic;
using UnityEngine;

public class ConsumableAbility : MonoBehaviour
{
    public void AnvilEffect(List<CardController> selectedCardsList)
    {
        // El Anvil solo va a upradear una carta.
        CardController card = selectedCardsList[0];
        // Obtenemos el deckManager
        DeckManager deckManager = DeckManager.Instance;
        // Reemplazamos la carta en el deck        
        deckManager.ReplaceCardInDeck(card.cardData, card.upgradedCard);
        // Convertimos la carta de la mano
        card.ConvertCard(card.upgradedCard);
    }

    public void WheelEffect(List<CardController> selectedCardsList)
    {
        // La Wheel solo va a upradear una carta.
        CardController card = selectedCardsList[0];
        // Obtenemos el deckManager
        DeckManager deckManager = DeckManager.Instance;;
        // Reemplazamos la carta en el deck
        deckManager.ReplaceCardInDeck(card.cardData, card.upgradedCard);
        // Convertimos la carta de la mano
        card.ConvertCard(card.upgradedCard);
    }

    public void BlueprintEffect(List<CardController> selectedCardsList)
    {
        // La Wheel solo va a upradear una carta.
        CardController card = selectedCardsList[0];
        // Obtenemos el deckManager
        DeckManager deckManager = DeckManager.Instance;;

        deckManager.AddCardToDeck(card.cardData);

        deckManager.InstantiateACardOnPlayerHand(card.cardData);        
    }
}
