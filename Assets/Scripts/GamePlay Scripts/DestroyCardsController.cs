using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DestroyCardsController : MonoBehaviour
{
    public static DestroyCardsController Instance;
    public Image backgroundImage; // Vinculado desde el Inspector
    public DeckManager deckManager; // Vinculado desde el Inspector
    public Transform panelTransform; // Vinculado desde el Inspector
    public Transform runesTransform;
    public CameraController cameraController;
    public TextMeshProUGUI runesText;
    public TextMeshProUGUI runesToEarn;
    public Button destroyButton; // Vinculado desde el Inspector
    public Button passButton;
    public Image destroyButtonImage; // Vinculado desde el Inspector
    public ShopManager shopManager; // Vinculado desde el Inspector
    public Material destroyCardEffectMaterial; // Vinculado desde el Inspector
    public static CardController selectedCard;
    private Material buttonMaterial;
    private PlayerCharacterController playerDwarfController;
    public SoundsFXManager soundsFXManager;

    void Awake()
    {
        Instance = this;
    }

    public void StartScene()
    {
        backgroundImage.enabled = true;
        deckManager.ShuffleDeck();
        DrawAndPositioning(GetGridPositions(8));
        playerDwarfController = PlayerCharacterController.Instance;
        panelTransform.gameObject.SetActive(true);
        runesTransform.gameObject.SetActive(true);
        destroyButton.onClick.RemoveAllListeners();
        destroyButton.onClick.AddListener(ExecuteDestroy);
        passButton.onClick.RemoveAllListeners();
        passButton.onClick.AddListener(CloseScene);
        buttonMaterial = destroyButtonImage.material;
        CheckIfSelected();
        UpdateRunesText();
    }
    private void DrawAndPositioning(List<Vector2> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Transform card = deckManager.DrawCardToDestroy();
            AnimatePosition(card, positions[i]);
        }   
    }
    public void AnimatePosition(Transform cardTransform, Vector2 targetPosition)
    {
        Vector3 target = new Vector3(targetPosition.x, targetPosition.y, cardTransform.localPosition.z);

        LeanTween.moveLocal(cardTransform.gameObject, target, 1f)
            .setEase(LeanTweenType.easeOutElastic);
    }

    public List<Vector2> GetGridPositions(int numberOfCards)
    {
        List<Vector2> points = new List<Vector2>();
        RectTransform panelRect = panelTransform.GetComponent<RectTransform>();

        bool isOdd = numberOfCards % 2 != 0;
        int adjustedCards = isOdd ? numberOfCards + 1 : numberOfCards;

        int rows = 3;
        int cols = (adjustedCards / 2) + 1;
        float cellWidth = panelRect.rect.width / cols;
        float cellHeight = panelRect.rect.height / rows;

        // Filas internas: 1 y 2 (sin 0 ni 3)
        for (int row = 1; row < rows; row++)
        {
            // Columnas internas: 1 hasta cols-1 (sin 0 ni cols)
            for (int col = 1; col < cols; col++)
            {
                float x = panelRect.rect.xMin + col * cellWidth;
                float y = panelRect.rect.yMin + row * cellHeight;
                points.Add(new Vector2(x, y));
            }
        }
        // Si el número original era impar, quitamos la última posición
        if (isOdd && points.Count > 0)
        {
            points.RemoveAt(points.Count - 1);
        }

        return points;
    }
    public void CheckIfSelected()
    {
        bool isSelected = false;
        foreach (Transform card in panelTransform)
        {
            CardController cardController = card.GetComponent<CardController>();
            if (cardController == null) continue;
            if (cardController.IsSelected)
            {
                isSelected = true;
                selectedCard = cardController;
                break;
            }
            selectedCard = null;
        }

        if (isSelected)
        {
            destroyButton.interactable = true;
            buttonMaterial.SetFloat("_EnableSaturation", 0f);
            buttonMaterial.DisableKeyword("_ENABLESATURATION_ON");
            runesToEarn.text = (selectedCard.cardData.cardLevel * 2).ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(destroyButton.GetComponent<RectTransform>());
        }
        else
        {
            destroyButton.interactable = false;
            buttonMaterial.EnableKeyword("_ENABLESATURATION_ON");
            buttonMaterial.SetFloat("_EnableSaturation", 1f);
            runesToEarn.text = "0";
        }
    }
    public void ExecuteDestroy()
    {
        destroyButton.interactable = false;
        CardData cardToDestroy = selectedCard.cardData;
        Image cardImage = selectedCard.cardImage;
        // Añadimos las runas correspondientes
        playerDwarfController.AddRunes(cardToDestroy.cardLevel * 2);
        UpdateRunesText();
        // Instanciar el material para evitar modificar el sharedMaterial
        Material mat = new Material(destroyCardEffectMaterial);
        cardImage.material = mat;
        bool textsHidden = false;
        deckManager.RemoveCardFromDeck(cardToDestroy);

        // TODAS LAS ANIMACIONES EMPIEZAN A LA VEZ
        // ANIMACIÓN FADE DEL BURN
        soundsFXManager.PlayBurningCardSound();
        LeanTween.value(selectedCard.gameObject, 0f, 1f, 0.1f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_BurnFade", val);
            });
        // ANIMACIÓN DE LAS LLAMAS
        LeanTween.value(selectedCard.gameObject, 1f, 3f, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_BurnSwirldFactor", val);
            });

        // ANIMACIÓN DISSOLVER
        LeanTween.value(selectedCard.gameObject, 1f, 0.3f, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_FullGlowDissolveFade", val);
                if (!textsHidden && val <= 0.90f) // 5% del progreso
                {
                    HideTexts(selectedCard.gameObject);
                    textsHidden = true;
                }
            })
            .setOnComplete(() =>
            {
                Destroy(selectedCard.gameObject);
                CloseScene();
            });
    }
    private void HideTexts(GameObject card)
    {
        foreach (Text t in card.GetComponentsInChildren<Text>(true))
        {
            t.gameObject.SetActive(false);
        }

        foreach (TMPro.TextMeshProUGUI tmp in card.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            tmp.gameObject.SetActive(false);
        }
    }
    private void UpdateRunesText()
    {
        runesText.text = playerDwarfController.Runes.ToString();
    }
    public void CloseScene()
    {
        runesTransform.gameObject.SetActive(false);
        MoveAndDestroyAllCards(() =>
        {
            backgroundImage.enabled = false;
            deckManager.ShuffleDeck();
            panelTransform.gameObject.SetActive(false);
            shopManager.ReturnDestroyCardsTransition();
        });
    }
    private void MoveAndDestroyAllCards(Action onComplete)
    {
        RectTransform panelRectTransform = panelTransform.GetComponent<RectTransform>();
        Vector2 panelSize = panelRectTransform.rect.size;
        Vector2 offScreenPos = new Vector2(panelSize.x / 2, -panelSize.y / 2);
        
        foreach (Transform card in panelTransform)
        {
            if (card.gameObject.CompareTag("Button")) continue;
            RectTransform cardRect = card.GetComponent<RectTransform>();
            Vector3 target = new Vector3(offScreenPos.x + cardRect.rect.width / 2, offScreenPos.y - cardRect.rect.height / 2, card.localPosition.z);
            LeanTween.moveLocal(card.gameObject, target, 1f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    Destroy(card.gameObject);
                    onComplete?.Invoke();
                });                
        }
    }
}
