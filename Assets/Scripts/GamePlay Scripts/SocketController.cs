using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;





public class SocketController : MonoBehaviour
{
    public static SocketController Instance;
    [SerializeField] public Sprite fireIcon;
    [SerializeField] public Sprite iceIcon;
    [SerializeField] public Sprite lightningIcon;
    [SerializeField] public Sprite waterIcon;
    [SerializeField] public Sprite natureIcon;
    [SerializeField] public Sprite earthIcon;
    public GameObject modifierPrefab;
    public Transform modifiersPanel;
    int delayedTweenID = -1;
    

    public static Dictionary<Element, Color> ElementColorMap = new Dictionary<Element, Color> // Diccionario con los colores de los elementos
    {
        { Element.Fire, Color.red },
        { Element.Ice, new Color(0.7f, 0.9f, 1.0f)}, // Azul muy claro
        { Element.Electric, new Color(0.1f, 0.6f, 1.0f)}, // Azul eléctrico
        { Element.Water, new Color(0.0f, 0.1f, 0.4f)}, // Azul marino
        { Element.Nature, new Color(0.1f, 0.4f, 0.1f)}, // Verde mas oscuro
        { Element.Earth, new Color(0.5f, 0.25f, 0.1f)} // Marrón personalizado
    };


    Dictionary<string, Sprite> spriteIconsMapper;
    private PlayerCharacterController dwarfController;
    private PlayedCardsController playedCardsController;
    public Transform weaponIconTransform;
    public Transform weaponTurnsRemainingTransform;
    public Transform weaponPowerTextTransform;
    public TextMeshProUGUI weaponPhysicalPowerText;
    public TextMeshProUGUI weaponElementalPowerText;
    public GameObject turnSegmentPrefab;
    public GameObject lifeStealIcon;
    public Image gearSocketsImage;
    public Image weaponSocketsImage;
    public Image backgroundImage;
    Image weaponIconImage;
    GameObject weaponIconGameObject;
    private bool isAnimating = false;
    public bool isAbsorbed = false;
    void Awake()
    {
        Instance = this;
        weaponIconGameObject = weaponIconTransform.gameObject;
        weaponIconImage = weaponIconTransform.GetComponent<Image>();
        spriteIconsMapper = new Dictionary<string, Sprite>
        {
            { "Fire", fireIcon },
            { "Ice", iceIcon },
            { "Electric", lightningIcon },
            { "Water", waterIcon },
            { "Nature", natureIcon },
            { "Earth", earthIcon }
        };
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<PlayerCharacterController>() != null);
        dwarfController = PlayerCharacterController.Instance;
        playedCardsController = PlayedCardsController.Instance;   
        UpdateSockets();
    }
    
    void Update()
    {
        
    }

    public void UpdateSockets()
    {
        UpdateWeaponSegments();
        // Activa o desactiva el gameObject si hay o no un arma
        weaponIconGameObject.SetActive(dwarfController.HaveWeapon);
        // Si hay un arma modificamos los textos.        
        if (dwarfController.HaveWeapon)
        {            
            if (isAbsorbed)
            {

                if (dwarfController.LifeSteal > 0)
                {
                    lifeStealIcon.GetComponent<RectTransform>().localScale = Vector3.zero;
                    lifeStealIcon.SetActive(true);
                    LeanTween.scale(lifeStealIcon.gameObject, Vector3.one * 1.5f, 0.2f)
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(lifeStealIcon.gameObject, Vector3.one, 0.1f);
                        });  
                }
                else
                {
                    lifeStealIcon.SetActive(false);
                }
                int elementalWeaponPower;
                (elementalWeaponPower, weaponElementalPowerText.color) = SumOfPowers(dwarfController.WeaponElementalPowers);
                if (elementalWeaponPower > 0)
                {
                    weaponElementalPowerText.GetComponent<RectTransform>().localScale = Vector3.zero;
                    weaponElementalPowerText.text = "+ " + elementalWeaponPower.ToString();
                    LeanTween.scale(weaponElementalPowerText.gameObject, Vector3.one * 1.5f, 0.2f)
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(weaponElementalPowerText.gameObject, Vector3.one, 0.1f);
                        }); 
                }                
            }            
        }
        else
        {
            ResetWeaponSegments();
            lifeStealIcon.SetActive(false);
            weaponPhysicalPowerText.text = "";
            weaponElementalPowerText.text = "";
        }
    }
    public void UpdateSockets(Sprite icon)
    {        
        UpdateWeaponSegments();
        // Activa o desactiva el gameObject si hay o no un arma
        weaponIconGameObject.SetActive(dwarfController.HaveWeapon);

        // Si hay un arma modificamos los textos.
        if (dwarfController.HaveWeapon)
        {
            weaponIconImage.sprite = icon;
            weaponPhysicalPowerText.GetComponent<RectTransform>().localScale = Vector3.zero;
            weaponPhysicalPowerText.text = dwarfController.ModifiedPhysicalPower().ToString();
            LeanTween.scale(weaponPhysicalPowerText.gameObject, Vector3.one * 1.5f, 0.3f)
                .setOnComplete(() =>
                        {
                            LeanTween.scale(weaponPhysicalPowerText.gameObject, Vector3.one, 0.2f);
                        });           
        }
        else
        {
            weaponPhysicalPowerText.text = "";
            weaponElementalPowerText.text = "";
        }     
    }


    public Sprite ElementActive(Dictionary<string, int> elementalPowers)
    {
        foreach (var element in elementalPowers)
        {
            if (element.Value != 0)
            {
                return spriteIconsMapper[element.Key];
            }
        }
        return null;

    }
    public void SetSpriteColor(Color spriteColor)
    {
        gearSocketsImage.color = spriteColor;
        weaponSocketsImage.color = spriteColor;
        // Parece que se tiene que ahcer asçi para mantener el alfa. Sinó la pone al máximo.
        // Ya que el spriteColor viene con el alfa al máximo.
        float originalAlfa = backgroundImage.color.a;
        backgroundImage.color = spriteColor;
        Color spriteColorWithAlfa = spriteColor;
        spriteColorWithAlfa.a = originalAlfa;
        backgroundImage.color = spriteColorWithAlfa;
    }

    public (int, Color) SumOfPowers(Dictionary<Element, int> elementalPowers)
    {
        int total = 0;
        List<Color> ourColors = new List<Color>();
        foreach (var element in elementalPowers)
        {
            total += element.Value;
            if (element.Value != 0)
            {
                ourColors.Add(ElementColorMap[element.Key]);
            }
        }
        if (ourColors.Count > 1)
        {
            return (total, MixColors(ourColors, 2f));
        }
        return (total, MixColors(ourColors));
    }

    // Método que devuelve la mezcla de los colores incluidos en la lista
    public static Color MixColors(List<Color> colors, float brightnessFactor = 1f)
    {
        if (colors == null || colors.Count == 0)
            return Color.white; // Si la lista está vacía, retorna blanco.

        float r = 0, g = 0, b = 0, a = 0;
        int count = colors.Count;

        foreach (Color color in colors)
        {
            r += color.r;
            g += color.g;
            b += color.b;
            a += color.a;
        }

        // Promedio normal de cada componente
        r /= count;
        g /= count;
        b /= count;
        a /= count;

        // 🔹 Aplicar un factor de brillo para evitar que las mezclas sean demasiado oscuras
        r = Mathf.Clamp01(r * brightnessFactor);
        g = Mathf.Clamp01(g * brightnessFactor);
        b = Mathf.Clamp01(b * brightnessFactor);

        return new Color(r, g, b, a);
    }


    public void UpdateWeaponSegments()
    {
        if (CheckWeaponSegments())
        {
            return;
        }
        else
        {
            ResetWeaponSegments();
        }
        float spacingGroup = weaponTurnsRemainingTransform.GetComponent<HorizontalLayoutGroup>().spacing;
        int weaponTurns = dwarfController.weaponTurnsRemaining;
        RectTransform weaponTurnsRemainingRect = weaponTurnsRemainingTransform.GetComponent<RectTransform>();
        float s = spacingGroup;  // spacing de los Layout, los dos deben de ser iguales
        int n = dwarfController.weaponTotalDuration;
        float C = weaponTurnsRemainingRect.rect.width;
        float segmentWidth = (C - s * (n - 1)) / n;   // mismo valor para todos los segmentos de la weapon

        for (int i = 0; i < weaponTurns; i++)
        {
            GameObject newSegment = Instantiate(turnSegmentPrefab, weaponTurnsRemainingTransform);
            RectTransform segmentRect = newSegment.GetComponent<RectTransform>();
            segmentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, segmentWidth);
        }
        // Añade medio hueco a izquierda y derecha con el propio LayoutGroup, así no hay ilusiones ópticas
        // ya que si lops segmentos de los extremos miden diferente de los centrales, se aprecian cosas raras.
        HorizontalLayoutGroup hlg = weaponTurnsRemainingTransform.GetComponent<HorizontalLayoutGroup>();
        int half = Mathf.RoundToInt(s * 0.5f);
        hlg.padding = new RectOffset(half, half, 0, 0);
    }

    private void ResetWeaponSegments()
    {
        foreach (Transform segment in weaponTurnsRemainingTransform)
        {
            Destroy(segment.gameObject);
        }
    }
    private bool CheckWeaponSegments()
    {
        bool check = true;
        if (dwarfController.HaveWeapon)
        {
            if (weaponTurnsRemainingTransform.childCount != dwarfController.weaponTurnsRemaining)
            {
                return false;
            }
        }
        return check;
    }
    public void AddWeaponModifier(TemporaryModifier modifier, Vector3 screenPos)
    {
        isAbsorbed = false;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            modifiersPanel.GetComponent<RectTransform>(),
            screenPos,
            Camera.main,
            out Vector2 anchoredPos
        );
        Transform duplicate = null;
        foreach (Transform tmpTransform in modifiersPanel)
        {
            TextMeshProUGUI textComponent = tmpTransform.GetComponent<TextMeshProUGUI>();
            Debug.Log("Color: " + textComponent.color);
            if (textComponent.color == Color.green && modifier.lifeSteal > 0)
            {
                duplicate = tmpTransform;
            }
            else if (modifier.lifeSteal > 0)
            {
                continue;
            }
            else if (textComponent.color == FloatingText.ElementColorMap[modifier.attackElement])
            {
                duplicate = tmpTransform;
            }
        }
        GameObject modifierGO = Instantiate(modifierPrefab, anchoredPos, Quaternion.identity, modifiersPanel);
        modifierGO.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        modifierGO.GetComponent<RectTransform>().localScale = Vector3.one * 2.5f;
        TextMeshProUGUI tmp = modifierGO.GetComponent<TextMeshProUGUI>();
        tmp.fontMaterial = Instantiate(tmp.fontMaterial);        
        Color c = Color.white;
        if (modifier.lifeSteal > 0)
        {
            tmp.color = Color.green;
            tmp.text = $"+ {modifier.lifeSteal} LifeSteal";
            c = Color.crimson;
        }
        else
        {
            tmp.color = FloatingText.ElementColorMap[modifier.attackElement];
            c = FloatingText.ElementOutlineColorMap[modifier.attackElement];      
            tmp.text = $"+ {modifier.damage} {modifier.attackElement}";
        }
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, c); 
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);              
        AnimateModifierFromCard(modifierGO.transform, duplicate);        
    }

    private float CalculateYPosition(Transform modifierTransform)
    {
        float modifierHeight = modifierTransform.GetComponent<RectTransform>().rect.height * 2;
        float y = 87.5f - modifierHeight * modifierTransform.GetSiblingIndex();
        return y;
    }
    private void AnimateModifierFromCard(Transform modifierTransform, Transform duplicate)
    {
        isAnimating = true;
        LeanTween.cancel(delayedTweenID);
        Vector3 targetPosition = new Vector3(0f, CalculateYPosition(modifierTransform), 0f);

        LeanTween.moveLocal(modifierTransform.gameObject, targetPosition, 0.4f)
            .setEaseOutSine();

        LeanTween.scale(modifierTransform.gameObject, Vector3.one, 0.4f)
            .setEaseOutSine();
        if (duplicate != null)
        {            
            LeanTween.delayedCall(0.4f, () =>
                        {
                            TMP_Text tmp = modifierTransform.GetComponent<TMP_Text>();
                            Color startColor = tmp.color;
                            string statA = duplicate.GetComponent<TextMeshProUGUI>().text;
                            string statB = modifierTransform.GetComponent<TextMeshProUGUI>().text;
                            string combinedTexts = FloatingText.CombineStats(statA, statB);
                            LeanTween.moveLocal(modifierTransform.gameObject, duplicate.localPosition, 0.1f)
                                .setOnComplete(() =>
                                {
                                    LeanTween.value(modifierTransform.gameObject, 0f, 1f, 0.1f)
                                        .setOnUpdate((float t) =>
                                        {
                                            Color c = startColor;
                                            c.a = t; // interpolamos el alpha
                                            tmp.color = c;
                                            
                                        })
                                        .setOnComplete(() =>
                                        {
                                            Destroy(modifierTransform.gameObject);
                                            duplicate.GetComponent<TextMeshProUGUI>().text = combinedTexts;
                                            PlayedCardsController.processingCard.IsProcesed = true;
                                            delayedTweenID = LeanTween.delayedCall(0.05f, () =>
                                                {
                                                    isAnimating = false;                                                   
                                                })
                                                .id;
                                        });                             
                                });                            
                        });
        }
        else
        {
            delayedTweenID = LeanTween.delayedCall(0.4f, () =>
                        {
                            isAnimating = false;
                            PlayedCardsController.processingCard.IsProcesed = true;
                        })
                        .id;
        }
    }
    public IEnumerator AbsorbingElementsAnimation()
    {
        isAbsorbed = false;
        // Éste bucle de espera es prácticamente igual que el WaitUntil, pero más estable
        // Ya que en la carrera de las flags para ser false, en cada frame pasan los checks a la vez
        // Por lo tanto es más estable
        while (isAnimating || playedCardsController.isProcessingCards)
        {
            yield return null; // esperar un frame
        }
        //Debug.Log($"Coinciden las 2 en false, {isAnimating} y {playedCardsController.isProcessingCards}");
        //yield return new WaitUntil(() => isAnimating == false && playedCardsController.isProcessingCards == false);
        // Recolectar hijos con TMP + sus datos iniciales
        var childRTs = new List<RectTransform>();
        var tmps     = new List<TextMeshProUGUI>();
        var startPos = new List<Vector2>();
        var baseColors = new List<Color>(); // guardamos RGB original

        foreach (Transform child in modifiersPanel)
        {
            var rt  = child as RectTransform;
            if (rt == null) continue;

            var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null) continue;

            childRTs.Add(rt);
            tmps.Add(tmp);
            startPos.Add(rt.anchoredPosition);
            if (tmp.color != Color.green)
            {
                baseColors.Add(tmp.color); // guardamos color original (RGB)
            }            
        }

        if (childRTs.Count == 0)
        {
            isAbsorbed = true;
            yield break;
        }

        // Convertir el centro del target a coordenadas locales del mismo espacio que usan los hijos (el padre 'source')
        // Usamos el centro del target (pivot) como destino.
        Vector3 targetWorldCenter = weaponIconTransform.TransformPoint(Vector3.zero);
        Vector2 targetLocalInSourceParent = modifiersPanel.InverseTransformPoint(targetWorldCenter);

        // Para cada hijo, el target local para su anchoredPosition debe estar en el espacio de su propio padre (source)
        // Si el padre de cada hijo es 'source', esto ya vale directamente:
        var endPos = new List<Vector2>(childRTs.Count);
        for (int i = 0; i < childRTs.Count; i++)
            endPos.Add(targetLocalInSourceParent);
        float duration = 0.8f;
        float fadeStartT = 0.9f;                  // 90% del tiempo

        // Lanzamos un tween maestro 0..1 y actualizamos todos en OnUpdate
        // (así se mueven exactamente sincronizados).
        LeanTween.value(modifiersPanel.gameObject, 0f, 1f, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                // Mover cada hijo
                for (int i = 0; i < childRTs.Count; i++)
                {
                    Vector2 p = Vector2.Lerp(startPos[i], endPos[i], t);
                    childRTs[i].anchoredPosition = p;
                }

                // Fade-out cuando se supera el 90%
                if (t >= fadeStartT)
                {
                    float k = Mathf.InverseLerp(fadeStartT, 1f, t); // 0..1 en el tramo final
                    for (int i = 0; i < tmps.Count; i++)
                    {
                        tmps[i].color = new Color(tmps[i].color.r, tmps[i].color.g, tmps[i].color.b, 1f - k);
                    }
                    Color mixedColor = MixColors(baseColors);
                    weaponIconImage.material.SetColor("_InkSpreadColor", mixedColor);
                    LeanTween.value(weaponIconImage.gameObject, 0f, 1f, 0.1f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnUpdate((float t) =>
                        {
                            weaponIconImage.material.SetFloat("_InkSpreadFade", t);
                        })
                        .setOnComplete(() =>
                        {
                            StartCoroutine(SpriteBlinkOutline(mixedColor));
                            LeanTween.value(weaponIconImage.gameObject, 1f, 0f, 0.1f)
                            .setEase(LeanTweenType.easeInOutQuad)
                            .setOnUpdate((float t) =>
                            {
                                weaponIconImage.material.SetFloat("_InkSpreadFade", t);
                            });
                        });
                }
            })
            .setOnComplete(() =>
            {
                // Destruir hijos al final
                for (int i = 0; i < childRTs.Count; i++)
                {
                    if (childRTs[i] != null)
                        Destroy(childRTs[i].gameObject);
                }
                isAbsorbed = true;
                UpdateSockets();                
            });
    }

    private IEnumerator SpriteBlinkOutline(Color c)
    {
        weaponIconImage.material.SetColor("_PixelOutlineColor", c);
        weaponIconImage.material.SetFloat("_PixelOutlineFade", 1f);
        yield return new WaitForSeconds(0.1f);
        weaponIconImage.material.SetFloat("_PixelOutlineFade", 0f);
        yield return new WaitForSeconds(0.2f);
        weaponIconImage.material.SetFloat("_PixelOutlineFade", 1f);
        yield return new WaitForSeconds(0.1f);
        weaponIconImage.material.SetFloat("_PixelOutlineFade", 0f);
    }

}
