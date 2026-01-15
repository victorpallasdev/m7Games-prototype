using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public string key;
    public bool CAPS;
    private TextMeshProUGUI uiText;

    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    public void UpdateText()
    {
        if (LocalizationManager.Instance != null)
        {
            if (CAPS)
            {
                uiText.text = LocalizationManager.Instance.GetText(key).ToUpper();
            }
            else
            {
                uiText.text = LocalizationManager.Instance.GetText(key);
            }
        }
        else
        {
            StartCoroutine(UpdateTextRutine());
        }
    }
    private IEnumerator UpdateTextRutine()
    {

        yield return new WaitUntil(() => LocalizationManager.Instance != null);

        if (CAPS)
        {
            uiText.text = LocalizationManager.Instance.GetText(key).ToUpper();
        }
        else
        {
            uiText.text = LocalizationManager.Instance.GetText(key);
        }
    }
    void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Register(this);
            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }        
    }

  

    void OnDisable()
    {
        LocalizationManager.Unregister(this);
    }
}
