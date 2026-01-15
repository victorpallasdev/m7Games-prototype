using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniCardDisplayController : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI nameText;

    public void Initialize(CardData data)
    {
        cardImage.sprite = data.cardImage;
        nameText.text = data.cardName;
    }

}
