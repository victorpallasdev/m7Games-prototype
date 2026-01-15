using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class DisplayDeckManager : MonoBehaviour
{
    public Transform gemsPanel;
    public TextMeshProUGUI gemsNumberText;
    public Transform weaponsPanel;
    public TextMeshProUGUI weaponsNumberText;
    public Transform gadgetsPanel;
    public TextMeshProUGUI gadgetsNumberText;
    public Transform supportPanel;
    public TextMeshProUGUI supportNumberText;
    public GameObject miniCardPrefab;
    public float spacing = 0;
    private List<CardData> deckData;

    public void Initialize(List<CardData> deck)
    {
        deckData = new List<CardData>(deck);
        DisplayDeck();
    }

    private void DisplayDeck()
    {
        ResetDeck(() =>
        {
            LoadGems();
            LoadWeapons();
            LoadSupports();
            LoadGadgets();
            PositionCards(); 
        });        
    }
    private void ResetDeck(Action onComplete)
    {
        StartCoroutine(ResetDeckCoroutine(onComplete));
    }

    private IEnumerator ResetDeckCoroutine(Action onComplete)
    {
        foreach (Transform card in weaponsPanel) Destroy(card.gameObject);
        foreach (Transform card in gadgetsPanel) Destroy(card.gameObject);
        foreach (Transform card in supportPanel) Destroy(card.gameObject);
        foreach (Transform card in gemsPanel) Destroy(card.gameObject);

        yield return new WaitForEndOfFrame(); // Espera a que Destroy surta efecto

        onComplete?.Invoke();
    }

    private void LoadGems()
    {
        List<CardData> onlyGems = new List<CardData>();
        foreach (CardData card in deckData)
        {
            if (card.cardType == CardType.Gem || card.cardType == CardType.DefensiveGem)
            {
                onlyGems.Add(card);
            }
        }

        foreach (CardData card in onlyGems)
        {
            GameObject newMiniCard = Instantiate(miniCardPrefab, gemsPanel);
            MiniCardDisplayController displayController = newMiniCard.GetComponent<MiniCardDisplayController>();
            displayController.Initialize(card);
        }
        gemsNumberText.text = onlyGems.Count.ToString();
    }

    private void LoadWeapons()
    {
        List<CardData> onlyWeps = new List<CardData>();
        foreach (CardData card in deckData)
        {
            if (card.cardType == CardType.Weapon)
            {
                onlyWeps.Add(card);
                GameObject newMiniCard = Instantiate(miniCardPrefab, weaponsPanel);
                MiniCardDisplayController displayController = newMiniCard.GetComponent<MiniCardDisplayController>();
                displayController.Initialize(card);
            }
        }
        weaponsNumberText.text = onlyWeps.Count.ToString();
    }

    private void LoadSupports()
    {
        List<CardData> onlySupps = new List<CardData>();
        foreach (CardData card in deckData)
        {
            if (card.cardType == CardType.Support)
            {
                onlySupps.Add(card);
                GameObject newMiniCard = Instantiate(miniCardPrefab, supportPanel);
                MiniCardDisplayController displayController = newMiniCard.GetComponent<MiniCardDisplayController>();
                displayController.Initialize(card);
            }
        }
        supportNumberText.text = onlySupps.Count.ToString();
    }

    private void LoadGadgets()
    {
        List<CardData> onlyGadgets = new List<CardData>();
        foreach (CardData card in deckData)
        {
            if (card.cardType == CardType.Gadget)
            {
                onlyGadgets.Add(card);
                GameObject newMiniCard = Instantiate(miniCardPrefab, gadgetsPanel);
                MiniCardDisplayController displayController = newMiniCard.GetComponent<MiniCardDisplayController>();
                displayController.Initialize(card);
            }
        }
        gadgetsNumberText.text = onlyGadgets.Count.ToString();
    }

    public void PositionCards()
    {
        PositionCardsInPanel(gemsPanel);
        PositionCardsInPanel(weaponsPanel);
        PositionCardsInPanel(supportPanel);
        PositionCardsInPanel(gadgetsPanel);
    }

    private void PositionCardsInPanel(Transform panel)
    {
        
        int totalCards = panel.childCount;
        if (totalCards == 0) return;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        float panelWidth = panelRect.rect.width;

        RectTransform cardRect = panel.GetChild(0).GetComponent<RectTransform>();
        float cardWidth = cardRect.rect.width;

        for (int i = 0; i < totalCards; i++)
        {
            Vector3 position = CalculateCardPositionX(i, totalCards, panelWidth, cardWidth);
            panel.GetChild(i).localPosition = position;
        }
    }

    private Vector3 CalculateCardPositionX(int index, int totalCards, float panelWidth, float cardWidth)
    {
        float availableWidth = panelWidth - cardWidth;
        float minSpacing = availableWidth / totalCards;
        float idealSpacing = Mathf.Min(cardWidth + spacing, minSpacing);
        // Alineado a la izquierda, sin centrado
        float startX = cardWidth / 2;
        return new Vector3(startX + (index) * idealSpacing, 0, 0);
    }
}
