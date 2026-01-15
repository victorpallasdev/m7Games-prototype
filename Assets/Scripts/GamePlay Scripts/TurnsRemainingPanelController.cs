using UnityEngine;

public class TurnsRemainingPanelController : MonoBehaviour
{
    public GameObject statsPanel;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = statsPanel.transform.localScale;
        statsPanel.transform.localScale = Vector3.zero; 
    }

    public void ShowPickaxeStatsPanel()
    {
        statsPanel.SetActive(true);
        statsPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(statsPanel, originalScale, 0.3f).setEaseOutBack(); // Animación de crecimiento
    }

    public void HidePickaxeStatsPanel()
    {
        LeanTween.scale(statsPanel, Vector3.zero, 0.2f).setEaseInBack().setOnComplete(() =>
        {
            statsPanel.SetActive(false);
        });
    }

    public bool IsPanelActive()
    {
        return statsPanel.activeSelf;
    }


}
