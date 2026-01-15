using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardInspectionManager : MonoBehaviour, IPointerDownHandler
{
    public static CardInspectionManager Instance;
    public CameraController cameraController;
    public Transform descriptionPanel;
    public Transform textsContainerTransform;
    public Transform cardPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI powersText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI defensiveModeText;
    public TextMeshProUGUI offensiveModeText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI inCardNameText;
    public Transform cardImageTransform;
    public CombatManager combatManager;
    private CardData cardData;
    public Transform backgroundClickCather;
    public GameObject attackGlowGO;
    public GameObject defenseGlowGO;
    private int moveAnimationID = -1;
    private int scaleAnimationID = -1;
    private int floatTweenID = -1;
    private Vector3 targetPosition;
    public float rotationAmountX = 20f;
    public float rotationAmountY = 20f;
    public float rotationAmountZ = 2.5f;
    public float baseDuration = 3f;
    public float durationVariation = 0.5f;
    private Vector3 originalRotation;
    public List<Image> starsList;

    void Awake()
    {
        targetPosition = cardPanel.localPosition;
        Instance = this;
        descriptionPanel.gameObject.SetActive(false);
        cardPanel.gameObject.SetActive(false);
        backgroundClickCather.gameObject.SetActive(false);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        HideDetails();
    }

    public void ShowDetails(CardData data, Vector3 originalCardPos)
    {
        combatManager.toggleUI();
        cardData = data;
        transform.localScale = Vector3.one / cameraController.cameraZoomFactor();
        transform.localPosition = new Vector3(cameraController.transform.localPosition.x, cameraController.transform.localPosition.y, 0f);
        descriptionPanel.gameObject.SetActive(true);
        cardPanel.gameObject.SetActive(true);
        backgroundClickCather.gameObject.SetActive(true);
        nameText.transform.GetComponent<LocalizedText>().key = cardData.cardName;
        inCardNameText.transform.GetComponent<LocalizedText>().key = cardData.cardName;
        attackText.text = cardData.attackText;
        defenseText.text = cardData.deffenseText;
        levelText.text = LocalizationManager.Instance.GetText("level") + " " + cardData.cardLevel.ToString();

        durationText.gameObject.SetActive(false);
        powersText.gameObject.SetActive(false);
        defensiveModeText.gameObject.SetActive(false);
        offensiveModeText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        attackGlowGO.SetActive(false);
        defenseGlowGO.SetActive(false);
        ActivateStars();
        switch (cardData.cardType)
        {
            case CardType.Gem:
                typeText.transform.GetComponent<LocalizedText>().key = "gem_type";
                if (cardData.defaultTextType)
                {
                    modeText.transform.GetComponent<LocalizedText>().key = "defensive_mode";
                    defenseGlowGO.SetActive(true);
                }
                else
                {
                    modeText.transform.GetComponent<LocalizedText>().key = "offensive_mode";
                    attackGlowGO.SetActive(true);
                }
                defensiveModeText.gameObject.SetActive(true);
                offensiveModeText.gameObject.SetActive(true);
                defensiveModeText.transform.GetComponent<LocalizedText>().key = cardData.defensiveMode;
                offensiveModeText.transform.GetComponent<LocalizedText>().key = cardData.offensiveMode;
                break;

            case CardType.DefensiveGem:
                typeText.transform.GetComponent<LocalizedText>().key = "defensiveGem_type";
                durationText.gameObject.SetActive(true);
                durationText.text = LocalizationManager.Instance.GetText("duration") + ": " + cardData.duration.ToString() + " " + LocalizationManager.Instance.GetText("turns");
                modeText.transform.GetComponent<LocalizedText>().key = "unique_mode";
                descriptionText.gameObject.SetActive(true);
                descriptionText.transform.GetComponent<LocalizedText>().key = cardData.cardDescription;
                break;

            case CardType.Weapon:
                typeText.transform.GetComponent<LocalizedText>().key = "weapon_type";
                powersText.gameObject.SetActive(true);
                powersText.text = LocalizationManager.Instance.GetText("powers");
                powersText.text += " " + cardData.power.ToString() + " " + LocalizationManager.Instance.GetText("physical");
                bool elem = false;
                foreach (ElementalPower power in cardData.elementalDamagesList)
                {
                    elem = true;
                    // Obtenemos el Color del elemento segun nuestro mapeado
                    Color elementColor = FloatingText.ElementColorMap[power.element];
                    // Lo convertimos de RGB a HEX para utilizarlo en el texto de formato y así tener varios colores en la misma cadena
                    string colorHex = ColorUtility.ToHtmlStringRGB(elementColor);
                    powersText.text += $" <color=#{colorHex}>{power.value} {LocalizationManager.Instance.GetText(power.element)}</color> |";
                }
                if (elem)
                {
                    powersText.text.Remove(powersText.text.Length - 1);
                }
                durationText.gameObject.SetActive(true);
                durationText.text = LocalizationManager.Instance.GetText("duration") + ": " + cardData.duration.ToString() + " " + LocalizationManager.Instance.GetText("turns");
                modeText.transform.GetComponent<LocalizedText>().key = "unique_mode";
                descriptionText.gameObject.SetActive(true);
                descriptionText.transform.GetComponent<LocalizedText>().key = cardData.cardDescription;
                break;

            case CardType.Support:
                typeText.transform.GetComponent<LocalizedText>().key = "support_type";
                modeText.transform.GetComponent<LocalizedText>().key = "unique_mode";
                descriptionText.gameObject.SetActive(true);
                descriptionText.transform.GetComponent<LocalizedText>().key = cardData.cardDescription;
                break;

            case CardType.Gadget:
                typeText.transform.GetComponent<LocalizedText>().key = "gadget_type";
                durationText.gameObject.SetActive(true);
                durationText.text = LocalizationManager.Instance.GetText("duration") + ": " + cardData.duration.ToString() + " " + LocalizationManager.Instance.GetText("turns");
                modeText.transform.GetComponent<LocalizedText>().key = "unique_mode";
                descriptionText.gameObject.SetActive(true);
                descriptionText.transform.GetComponent<LocalizedText>().key = cardData.cardDescription;
                break;
        }             
        SetAllTextsToWhite();
        Image cardImage = cardImageTransform.GetComponent<Image>();
        cardImage.sprite = cardData.cardImage;
        cardImage.enabled = false;        

        AnimateCardSpawn(originalCardPos);

        LocalizationManager.Instance.UpdateAllLocalizedTexts();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(textsContainerTransform.GetComponent<RectTransform>());
    }
    private void SetAllTextsToWhite()
    {
        foreach (Text text in cardPanel.gameObject.GetComponentsInChildren<Text>(true))
        {
            if (cardData.cardType == CardType.Gadget)
            {
                text.color = Color.white;
            }
            else
            { 
                text.color = Color.black;
            }                 
        }
        foreach (TextMeshProUGUI tmp in cardPanel.gameObject.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (cardData.cardType == CardType.Gadget)
            {
                tmp.color = Color.white;
            }
            else
            { 
                tmp.color = Color.black;
            } 
        }
    }

    private void ActivateStars()
    {
        foreach (Image star in starsList)
        {
            star.gameObject.SetActive(false);
        }

        for (int i = 0; i < cardData.cardLevel; i++)
        {
            starsList[i].gameObject.SetActive(true);
        }
    }
    private void HideDetails()
    {
        LeanTween.cancel(scaleAnimationID);
        LeanTween.cancel(moveAnimationID);
        LeanTween.cancel(floatTweenID);
        cardPanel.localPosition = targetPosition;
        cardPanel.localRotation = Quaternion.identity;
        cardPanel.localScale = Vector3.one;
        descriptionPanel.gameObject.SetActive(false);
        cardPanel.gameObject.SetActive(false);
        backgroundClickCather.gameObject.SetActive(false);
        combatManager.toggleUI();
    }

    private void AnimateCardSpawn(Vector3 originalCardPos)
    {
        RectTransform cardRect = cardPanel.GetComponent<RectTransform>();
        // Establecer posición inicial y escala inicial
        Vector3 localPos = transform.InverseTransformPoint(originalCardPos);
        cardRect.anchoredPosition = originalCardPos;
        cardPanel.localScale = Vector3.one;
        Image cardImage = cardImageTransform.GetComponent<Image>();
        cardImage.enabled = true;

        // Duración de la animación
        float duration = 2f;

        // Animar posición
        moveAnimationID = LeanTween.moveLocal(cardPanel.gameObject, targetPosition, duration).setEaseOutExpo()
            .setOnComplete(() => cardPanel.localPosition = targetPosition)
            .id;

        // Animar escala
        scaleAnimationID = LeanTween.scale(cardPanel.gameObject, Vector3.one * 9f, duration).setEaseOutExpo()
            .setOnComplete(() =>
                {
                    cardPanel.localScale = Vector3.one * 9f;
                    StartFloatingEffect();
                })
            .id;
    }
    public void StartFloatingEffect()
    {
        float duration = baseDuration + Random.Range(-durationVariation, durationVariation);

        Vector3 targetRotation = new Vector3(
            originalRotation.x + Random.Range(-rotationAmountX, rotationAmountX),
            originalRotation.y + Random.Range(-rotationAmountY, rotationAmountY),
            originalRotation.z + Random.Range(-rotationAmountZ, rotationAmountZ)
        );

        floatTweenID = LeanTween.rotateLocal(cardPanel.gameObject, targetRotation, duration)
        .setEaseInOutSine()
        .setOnComplete(() => StartFloatingEffect())
        .id;
    }





}
