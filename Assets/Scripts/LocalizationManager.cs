using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    private Dictionary<string, string> localizedTexts;
    public string currentLanguage = "en";
    private JObject fullJsonData;
    private static List<LocalizedText> trackedTexts = new();
    public static event Action OnLanguageChanged;

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadJsonData();
            LoadLanguage(currentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadJsonData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Localization/localization");
        if (jsonFile == null)
        {
            Debug.LogError("Localization JSON not found in Resources/Localization!");
            return;
        }

        try
        {
            fullJsonData = JObject.Parse(jsonFile.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse localization JSON: " + ex.Message);
        }
       
    }

    public void LoadLanguage(string languageCode)
    {
        currentLanguage = languageCode;

        if (fullJsonData == null)
        {
            Debug.LogError("Localization data not loaded.");
            return;
        }

        var langData = fullJsonData[languageCode] as JObject;
        if (langData != null)
        {
            localizedTexts = langData.ToObject<Dictionary<string, string>>();
        }
        else
        {
            Debug.LogWarning("Language not found: " + languageCode);
        }

        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (localizedTexts != null && localizedTexts.TryGetValue(key, out var value))
        {
            return value;
        }
        return "[[" + key + "]]";
    }
    public string GetText(Element elementKey)
    {
        string key = "";
        switch (elementKey)
        {
            case Element.Fire:
                key = "Fire";
                break;
            case Element.Electric:
                key = "Electric";
                break;
            case Element.Ice:
                key = "Ice";
                break;
            case Element.Water:
                key = "Water";
                break;
            case Element.Nature:
                key = "Nature";
                break;
            case Element.Earth:
                key = "Earth";
                break;
        }
        if (localizedTexts != null && localizedTexts.TryGetValue(key, out var value))
        {
            return value;
        }
        return "[[" + key + "]]";
    }
    public static void Register(LocalizedText text)
    {
        if (!trackedTexts.Contains(text))
            trackedTexts.Add(text);
    }

    public static void Unregister(LocalizedText text)
    {
        trackedTexts.Remove(text);
    }

    public void UpdateAllLocalizedTexts()
    {
        foreach (var lt in trackedTexts)
        {
            lt.UpdateText();
        }
    }
}
