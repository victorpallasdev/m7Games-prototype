using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ChooseCardsController : MonoBehaviour
{
    public static ChooseCardsController Instance;
    public Image backgroundImage; // Vinculado desde el Inspector
    public DeckManager deckManager; // Vinculado desde el Inspector
    public Transform panelTransform; // Vinculado desde el Inspector
    public Button selectButton; // Vinculado desde el Inspector
    public Button skipButton; // Vinculado desde el Inspector
    public Image selectButtonImage; // Vinculado desde el Inspector
    public ShopManager shopManager; // Vinculado desde el Inspector
    public Material destroyCardEffectMaterial; // Vinculado desde el Inspector
    public static CardController selectedCard;
    private Material buttonMaterial;
    public GameObject cardPrefab; // Vinculado desde el Inspector
    public Image packImage;

    private float startPositionX = -304f;

    void Awake()
    {
        Instance = this;
    }

    public void StartScene(List<CardData> cards, Sprite packSprite)
    {        
        backgroundImage.enabled = true;
        packImage.sprite = packSprite;
        OpenPackAnimation(() =>
        {
            GenerateAndPositioning(cards);
            selectButton.gameObject.SetActive(true);
            selectButton.interactable = false;
            selectButton.onClick.AddListener(SelectAndMoveCard);
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.AddListener(CloseScene);
            buttonMaterial = selectButtonImage.material;  
        });         
    }

    private void OpenPackAnimation(Action onComplete = null)
    {
        //Instancio nuevo material para no tener que resetear los valores siempre
        Material packImageMaterial = new Material(packImage.material);
        packImage.material = packImageMaterial;
        packImageMaterial.SetFloat("_GaussianBlurFade", 0f);
        packImageMaterial.SetFloat("_WiggleFade", 0f);    
        //Seteo los valores de posicion y escala que tendrá al principio, están sacados
        //de los valores de la image del inspector, para que parezca que viene de ahí.
        Vector2 center = Vector2.zero;
        Vector3 targetScale = Vector3.one * 3f;
        Vector3 startPosition = Vector3.zero;
        startPosition.x = startPositionX;        
        RectTransform packImageRect = packImage.rectTransform;
        packImageRect.localPosition = startPosition;
        packImageRect.localScale = Vector3.one * 2f;
        //Le tengo que volver a dar el alfa al color de la imagen ya que se lo quitamos en el fadeOut
        //por lo tanto el primero iba bien pero los que le preceden no.
        Color imageColor = packImage.color;
        imageColor.a = 255;
        packImage.color = imageColor;
        //CUando la imagen tiene todos los valores correctos, la activo.        
        packImage.gameObject.SetActive(true);
        
        // Animacion de moverse al centro de la pantalla y aumentar su tamaño paralelamente, tienen que durar lo mismo.
        LeanTween.moveLocal(packImage.gameObject, center, 0.4f)
            .setEase(LeanTweenType.easeInOutQuad);

        LeanTween.scale(packImage.gameObject, targetScale, 0.4f)
           .setEase(LeanTweenType.easeInOutQuad)
           .setOnComplete(() =>
            {
                //Cuando termina la primera parte de la animación empieza la segunda.
                //En esta parte hacemos un wiggle que aumenta de manera exponencial inversa su speed
                //SoundsFXManager.Instance.PlayOpenPackSound();
                packImageMaterial.SetFloat("_WiggleFade", 1f);                 
                LeanTween.value(packImage.gameObject, 1f, 100f, 0.5f)
                    .setEase(LeanTweenType.easeOutExpo)
                    .setOnUpdate((float val) =>
                    {
                        packImageMaterial.SetFloat("_WiggleSpeed", val);
                    })
                    .setOnComplete(() =>
                    {
                        //Cuando termina la segunda parte empieza la tercera
                        //En esta tercera hacemos un emborronadoGausiano rápido que se paralela con su fadeOut
                        packImageMaterial.SetFloat("_GaussianBlurFade", 0.6f);
                        Color startColor = packImage.color;
                        LeanTween.value(packImage.gameObject, 1f, 0f, 0.15f)
                            .setEase(LeanTweenType.linear)
                            .setOnUpdate((float alpha) =>
                            {
                                Color c = startColor;
                                c.a = alpha;
                                packImage.color = c;
                            });
                        LeanTween.value(packImage.gameObject, 0f, 100, 0.15f)
                            .setEase(LeanTweenType.linear)
                            .setOnUpdate((float blur) =>
                            {
                                packImageMaterial.SetFloat("_GaussianBlurOffset", blur);
                            })
                            .setOnComplete(() =>
                            {
                                //Cuando termina se desactiva la imagen y se llama al callback para que continue el código.
                                packImage.gameObject.SetActive(false);
                                onComplete?.Invoke();
                            });
                        
                    });
            });
    }

    private void GenerateAndPositioning(List<CardData> cards)
    {
        List<Vector2> positions = GetGridPositions(cards.Count);
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, panelTransform);
            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            cardRect.anchoredPosition = positions[i];
            cardRect.localScale = Vector3.zero;
            CardController cardComponent = newCard.GetComponent<CardController>();
            if (cardComponent != null)
            {
                cardComponent.Initialize(cards[i]);
                newCard.name = $"{cards[i].cardName}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
            }
            LeanTween.scale(newCard, Vector3.one, 1f)
                .setEase(LeanTweenType.easeOutElastic);
        }
    }

    public List<Vector2> GetGridPositions(int numberOfCards)
    {
        List<Vector2> points = new List<Vector2>();
        RectTransform panelRect = panelTransform.GetComponent<RectTransform>();

        int rows = 2;
        int cols = numberOfCards + 1;
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
        return points;
    }
    public void AnimatePosition(Transform cardTransform, Vector2 targetPosition)
    {
        Vector3 target = new Vector3(targetPosition.x, targetPosition.y, cardTransform.localPosition.z);

        LeanTween.moveLocal(cardTransform.gameObject, target, 1f)
            .setEase(LeanTweenType.easeOutElastic);
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
        if(isSelected)
        {
            selectButton.interactable = true;            
            buttonMaterial.SetFloat("_EnableSaturation", 0f);
            buttonMaterial.DisableKeyword("_ENABLESATURATION_ON");            
        }
        else
        {
            selectButton.interactable = false;
            buttonMaterial.EnableKeyword("_ENABLESATURATION_ON");
            buttonMaterial.SetFloat("_EnableSaturation", 1f);
        }
    }
    private void SelectAndMoveCard()
    {        
        RectTransform panelRectTransform = panelTransform.GetComponent<RectTransform>();
        Vector2 panelSize = panelRectTransform.rect.size;
        Vector2 offScreenPos = new Vector2(panelSize.x / 2, -panelSize.y / 2);
        RectTransform cardRect = selectedCard.GetComponent<RectTransform>();
        Vector3 target = new Vector3(offScreenPos.x + cardRect.rect.width / 2, offScreenPos.y - cardRect.rect.height / 2, cardRect.localPosition.z);
        LeanTween.moveLocal(selectedCard.gameObject, target, 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => 
            {
                Destroy(selectedCard.gameObject);
                deckManager.AddCardToDeck(selectedCard.cardData);
                foreach (Transform card in panelTransform)
                {
                    if (card.gameObject.CompareTag("Button")) continue;
                    CardController cardController = card.GetComponent<CardController>();
                    if (card != selectedCard) DestroyEffect(cardController, () => CloseScene());
                }                             
            });        
    }
    private void CloseScene()
    {
        foreach (Transform card in panelTransform)
        {
            if (card.gameObject.CompareTag("Button")) continue;
            CardController cardController = card.GetComponent<CardController>();
            DestroyEffect(cardController);
        }   
        backgroundImage.enabled = false;       
        deckManager.ShuffleDeck();
        selectButton.onClick.RemoveAllListeners();
        selectButton.gameObject.SetActive(false);
        skipButton.onClick.RemoveAllListeners();
        skipButton.gameObject.SetActive(false);
        shopManager.ReturnDestroyCardsTransition();
    }

    private void DestroyEffect(CardController cardToDestroy, Action onComplete = null)
    {
        Image cardImage = cardToDestroy.cardImage;
        // Instanciar el material para evitar modificar el sharedMaterial
        Material mat = new Material(destroyCardEffectMaterial);
        cardImage.material = mat;
        bool textsHidden = false;

        // TODAS LAS ANIMACIONES EMPIEZAN A LA VEZ
        // ANIMACIÓN FADE DEL BURN
        LeanTween.value(cardToDestroy.gameObject, 0f, 1f, 0.1f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_BurnFade", val);
            });
        // ANIMACIÓN DE LAS LLAMAS
        LeanTween.value(cardToDestroy.gameObject, 1f, 3f, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_BurnSwirldFactor", val);
            });

        // ANIMACIÓN DISSOLVER
        LeanTween.value(cardToDestroy.gameObject, 1f, 0.3f, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_FullGlowDissolveFade", val);
                if (!textsHidden && val <= 0.90f) // 5% del progreso
                {
                    HideTexts(cardToDestroy.gameObject);
                    textsHidden = true;
                }
            })
            .setOnComplete(() =>
            {
                Destroy(cardToDestroy.gameObject);
                onComplete?.Invoke();
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


}
