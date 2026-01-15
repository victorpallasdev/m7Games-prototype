using UnityEngine;
using TMPro;
using static UnityEngine.ColorUtility;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

[DisallowMultipleComponent]
public class FloatingText : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float riseDuration = 0.9f;
    [SerializeField] private float riseDistance = 120f;
    [SerializeField] private float arcLift = 24f;
    [SerializeField] private AnimationCurve arcCurve = null; // si es null uso EaseInOut

    [Header("Fade")]
    [SerializeField] private float fadeIn = 0.08f;
    [SerializeField] private float fadeOut = 0.22f;
    [SerializeField] private float hold = 0.25f;

    [Header("Scale Pop")]
    [SerializeField] private float popIn = 0.12f;
    [SerializeField] private float settle = 0.10f;
    [SerializeField] private float minScale = 0.85f;
    [SerializeField] private float maxScale = 1.10f;

    [Header("Offsets")]
    [SerializeField] private Vector2 baseOffset = new Vector2(0f, 200f);
    [SerializeField] private Vector2 randomNudge = new Vector2(6f, 2f);

    [Header("Typography")]
    [SerializeField] private float fontSizePhysical = 64f;
    [SerializeField] private float fontSizeElemental = 54f;
    [SerializeField] private float fontSizeHeal = 58f;

    // Colores (tus mapas)
    public static Dictionary<Element, Color> ElementColorMap = new Dictionary<Element, Color>
    {
        { Element.Fire, Color.red},
        { Element.Ice,  new Color(0.7f, 0.9f, 1.0f)},
        { Element.Electric, new Color(0.1f, 0.6f, 1.0f)},
        { Element.Water, new Color(0.0f, 0.1f, 0.4f)},
        { Element.Nature, new Color(0.1f, 0.4f, 0.1f)},
        { Element.Earth, new Color(0.5f, 0.25f, 0.1f)}
    };
    public static Dictionary<Element, string> ElementcolorMapHex = new Dictionary<Element, string>
    {
        { Element.Fire, ToHtmlStringRGB(Color.red)},
        { Element.Ice,  ToHtmlStringRGB(new Color(0.7f, 0.9f, 1.0f))},
        { Element.Electric, ToHtmlStringRGB(new Color(0.1f, 0.6f, 1.0f))},
        { Element.Water, ToHtmlStringRGB(new Color(0.0f, 0.1f, 0.4f))},
        { Element.Nature, ToHtmlStringRGB(new Color(0.1f, 0.4f, 0.1f))},
        { Element.Earth, ToHtmlStringRGB(new Color(0.5f, 0.25f, 0.1f))}
    };

    public static Dictionary<Element, Color> ElementOutlineColorMap = new Dictionary<Element, Color>
    {
        { Element.Fire, Color.crimson},
        { Element.Ice, new Color(0.55f, 0.75f, 0.90f) },
        { Element.Electric, new Color(0.7f, 0.9f, 1.0f) },
        { Element.Water, new Color(0.7f, 0.9f, 1.0f) },
        { Element.Nature, Color.greenYellow },
        { Element.Earth, new Color(0.36f, 0.17f, 0.07f) }
    };

    // refs a tu jerarquía actual
    private RectTransform canvasRT;     // moveremos este (el hijo Canvas)
    private TextMeshProUGUI tmp;        // el hijo Text
    private CanvasGroup group;          // lo añadimos al Canvas hijo

    // slots anti-solape
    private int slotIndex = -1;
    private int slotCount = 1;
    public float slotSpacingX = 200f;
    

    void Awake()
    {
        // Buscar "Canvas" y "Text" según tu prefab
        Transform canvasT = transform.Find("Canvas");        
        canvasT.GetComponent<Canvas>().overrideSorting = true;
        canvasRT = canvasT.GetComponent<RectTransform>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();       

        if (tmp == null)
        {
            Debug.LogWarning("FloatingText: No se encontró TextMeshProUGUI en 'Canvas/Text'.");
        }           

        // CanvasGroup en el Canvas hijo (se crea si no existe)
        group = canvasRT.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = canvasRT.gameObject.AddComponent<CanvasGroup>();
        } 
        if (arcCurve == null)
        {
            arcCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }            
    }

    void Start()
    {
        // Situación inicial: offset + pequeño nudge
        if (canvasRT != null)
        {
            Vector2 start = canvasRT.anchoredPosition + baseOffset +
                            new Vector2(UnityEngine.Random.Range(-randomNudge.x, randomNudge.x),
                                        UnityEngine.Random.Range(-randomNudge.y, randomNudge.y));
            canvasRT.anchoredPosition = start;
        }
    }

    // ——— API para asignar slots anti-solape (opcional) ———
    public void SetSlot(int index, int count)
    {
        slotIndex = Mathf.Max(0, index);
        slotCount = Mathf.Max(1, count);
    }
    private Vector2 GetLateralBySlot()
    {
        if (slotCount <= 1) return Vector2.zero;

        // centro geométrico (p. ej. 4 slots: centro = 1.5)
        float center = (slotCount - 1) * 0.5f;

        // desplazamiento lineal por slot: …, -2, -1, 0, +1, +2, …
        float x = (slotIndex - center) * slotSpacingX;

        return new Vector2(x, 0f);
    }

    // ——— Métodos públicos que ya usas ———
    public void TakeDamage(Element elementName, int power)
    {
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);
        tmp.text = power.ToString();
        tmp.fontSize = fontSizeElemental;
        tmp.color = ElementColorMap.TryGetValue(elementName, out var c) ? c : Color.white;
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.white);
        PlaySequence();
    }

    public void TakeDamage(int power)
    {
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);
        tmp.text = power.ToString();
        tmp.fontSize = fontSizePhysical;
        tmp.color = Color.white;
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        PlaySequence();
    }

    public void TakeHeal(int healPower)
    {
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);
        tmp.text = "+" + healPower.ToString();
        tmp.fontSize = fontSizeHeal;
        tmp.color = new Color(0.2f, 1f, 0.2f);
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.greenYellow);
        PlaySequence();
    }

    public void AbsorbPhysical(int physicalAbsorbed)
    {
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);
        tmp.text = $"<size=70%>{physicalAbsorbed}</size> <size=30%>Absorbed</size>";
        tmp.fontSize = fontSizeHeal;
        tmp.color = new Color(1f, 1f, 0f);
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        PlaySequence(true);
    }

    public void CardEffect(Element elementName, string textToShow)
    {
        tmp.text = textToShow;
        tmp.fontSize = 75;
        tmp.color = ElementColorMap.TryGetValue(elementName, out var c) ? c : Color.white;
        ApplyTMPOutline();
        PlaySequence();
    }

    // ——— Core anim con LeanTween sobre el Canvas hijo ———
    private void PlaySequence(bool isAbsorbed = false)
    {
        if (canvasRT == null) { Destroy(gameObject, 1f); return; }

        // Reset
        if (group != null) group.alpha = 0f;
        canvasRT.localScale = Vector3.one * minScale;



        Vector2 lateral = GetLateralBySlot();
        Vector2 start = canvasRT.anchoredPosition + lateral;
        Vector2 end = start + lateral + new Vector2(0f, riseDistance);

        // POP de escala
        LeanTween.scale(canvasRT, Vector3.one * maxScale, popIn).setEaseOutBack()
            .setOnComplete(() =>
            {
                LeanTween.scale(canvasRT, Vector3.one, settle).setEaseOutQuad();
            });

        // FADE IN -> HOLD -> FADE OUT (si hay CanvasGroup)
        LeanTween.value(canvasRT.gameObject, 0f, 1f, fadeIn)
            .setOnUpdate(a => group.alpha = a)
            .setOnComplete(() =>
            {
                LeanTween.delayedCall(canvasRT.gameObject, hold, () =>
                {
                    LeanTween.value(canvasRT.gameObject, 1f, 0f, fadeOut)
                        .setOnUpdate(a => group.alpha = a);
                });
            });

        if (!isAbsorbed)
        {
            // Movimiento con arco (anchoredPosition)
            LeanTween.value(canvasRT.gameObject, 0f, 1f, riseDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate(t =>
            {
                Vector2 p = Vector2.Lerp(start, end, t);
                float lift = (arcCurve != null ? arcCurve.Evaluate(t) : t) * arcLift;
                p.y += lift;
                canvasRT.anchoredPosition = p;
            })
            .setOnComplete(() => Destroy(gameObject));
        }
        else
        {
            // Movimiento vertical hacia ABAJO, más lento y más suave
            float absorbedDuration = riseDuration * 1.3f;
            float downDistance = riseDistance * 0.6f; // puedes ajustar para menos desplazamiento
            Vector2 endDown = start - new Vector2(0f, downDistance);

            LeanTween.value(canvasRT.gameObject, 0f, 1f, absorbedDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate(t =>
            {
                // Movimiento recto hacia abajo
                Vector2 p = Vector2.Lerp(start, endDown, t);
                canvasRT.anchoredPosition = p;
            })
            .setOnComplete(() => Destroy(gameObject));
        }

    }
    
    private static readonly Regex pattern = new Regex(@"([+-]?\d+)\s*([\w\s]+?)(?:\s*Resist\.?)?$",RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static string CombineStats(string statA, string statB)
    {
        statA = Regex.Replace(statA, "<.*?>", string.Empty);
        statB = Regex.Replace(statB, "<.*?>", string.Empty);
        var matchA = pattern.Match(statA);
        var matchB = pattern.Match(statB);

        if (!matchA.Success || !matchB.Success)
            throw new ArgumentException("Formato inválido. Debe ser como '+10 Fire' o '+10 Fire Resist.'");

        // Extraer valores numéricos
        int valueA = int.Parse(matchA.Groups[1].Value);
        int valueB = int.Parse(matchB.Groups[1].Value);

        // Extraer nombres
        string nameA = matchA.Groups[2].Value.Trim();
        string nameB = matchB.Groups[2].Value.Trim();

        // Detectar si incluyen "Resist."
        bool isResistA = statA.Contains("Resist", StringComparison.OrdinalIgnoreCase);
        bool isResistB = statB.Contains("Resist", StringComparison.OrdinalIgnoreCase);

        // Verificar coincidencia de base (sin Resist.)
        if (!string.Equals(nameA, nameB, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Los nombres base no coinciden.");

        // Sumar valores
        int total = valueA + valueB;

        // Construir el resultado
        string suffix = (isResistA || isResistB) ? " Resist" : "";
        return $"+ {total} {nameA}<size=50%>{suffix}</size>";
    }

    private void ApplyTMPOutline()
    {
        
    }
    private void ApplyTMPOutlineAbsorb()
    {
        
    }
}
