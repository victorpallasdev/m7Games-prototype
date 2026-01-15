using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FusionSocketController : MonoBehaviour
{
    public FusionatorController fusionatorController; // Vinculado desde el Inspector
    public Image highlightImage; // Vinculado desde el Inspector
    private CardData cardDataOfThisSocket;
    private Transform cardInThisSocket;
    private CardController cardControllerInSocket;

   
    public bool HasCard()
    {
        // Busca en todos los hijos (y nietos) un componente CardController
        return GetComponentInChildren<CardController>() != null;
    }
    public Transform GetCardTransform()
    {
        return cardInThisSocket;
    }
    public CardData GetCardData()
    {
        return cardDataOfThisSocket;
    }
    public void QuitCard()
    {
        cardInThisSocket.SetParent(fusionatorController.playerGemsHand, true);
        cardInThisSocket = null;
        cardDataOfThisSocket = null;
        fusionatorController.UpdateFusionButton();
    }
    public void RemoveCard()
    {
        cardInThisSocket = null;
        cardDataOfThisSocket = null;
        fusionatorController.UpdateFusionButton();
    }

    public void CardPositioner(Transform card)
    {
        StartCoroutine(CardPositionerRutine(card));
    }
    private IEnumerator CardPositionerRutine(Transform card)
    {
        cardInThisSocket = card;
        card.localPosition = Vector3.zero;
        CardController cardController = card.GetComponent<CardController>();
        cardController.cardImage.transform.localRotation = Quaternion.identity;        
        cardControllerInSocket = card.GetComponent<CardController>();
        cardDataOfThisSocket = cardControllerInSocket.cardData;

        yield return new WaitUntil(() => cardDataOfThisSocket != null);

        fusionatorController.UpdateFusionButton();
    }

    public void ActiveGlow()
    {
        highlightImage.gameObject.SetActive(true);
    }
    public void DeActiveGlow()
    {
        if (highlightImage.gameObject.activeSelf)
        {
            highlightImage.gameObject.SetActive(false);
        }        
    }   



}
