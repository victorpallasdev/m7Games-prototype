using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinsController : MonoBehaviour
{
    public static CoinsController Instance;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI runesText;
    public RectTransform runesRect;
    public RectTransform goldRect;
    public RectTransform coinsContainerRect;
    public Button coinsButton;
    private Vector3 originalPosition;
    private Vector3 originalRunesPosition;
    private float goldWidth;
    private bool isOpen = false;
    private int moveID = -1;


    void Start()
    {
        Instance = this;
        originalPosition = coinsContainerRect.anchoredPosition;
        originalRunesPosition = runesRect.localPosition;

        
        coinsButton.onClick.RemoveAllListeners();
        coinsButton.onClick.AddListener(() => ToggleShowAllCoins());
        runesRect.gameObject.SetActive(false);
    }


    public void UpdateCoinsTexts()
    {
        PlayerCharacterController playerDwarfController = FindAnyObjectByType<PlayerCharacterController>();
        goldText.text = playerDwarfController.Gold.ToString();
        runesText.text = playerDwarfController.Runes.ToString();
    }

    private void ToggleShowAllCoins(bool inverse = false)
    {
        goldWidth = goldRect.rect.width;
        LeanTween.cancel(moveID);
        if (!inverse && !isOpen)
        {
            runesRect.gameObject.SetActive(true);
            isOpen = true;
            moveID = LeanTween.moveLocalX(runesRect.gameObject, originalRunesPosition.x + 0.8f * goldWidth, 0.2f)
                        .setEaseInOutSine()
                        .id;
        }
        else if (!inverse && isOpen)
        {
            isOpen = false;
            coinsButton.interactable = false;
            moveID = LeanTween.moveLocalX(runesRect.gameObject, originalRunesPosition.x, 0.2f)
                        .setEaseInOutSine()
                        .setOnComplete(() =>
                            {
                                runesRect.gameObject.SetActive(false);
                                coinsButton.interactable = true;
                            })
                        .id;            
        }
    }
}
