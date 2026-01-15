using UnityEngine;
using UnityEngine.UI;


public class IntroManager : MonoBehaviour
{
    public Transform backgroundStudio;
    public Camera mainCamera;
    
    [Header("Intro Studio")]
    public Image studioLogo;
    public MusicManager musicManager;
    

    [Header("Title Game Panel")]
    public GameObject startGamePanel;          // El panel que contiene todo
    public Image logo;      // Fondo con logo
    public GameObject startButtonsContainer;   // Botones: Play, Exit, etc.
    public Image background;
    public GameObject blackBackground;


    [Header("Config Intro Studio")]
    public float fadeInStudioTime = 8f;
    public float fadeOutStudioTime = 3f;
    public float fadeInStartPanelTime = 2f;    // Tiempo controlable

    [Header("Halftone & Glow Animations")]
    public float halftoneFadeDuration = 4f;
    public float glowFadeDuration = 4f;

    [Header("Animación Loop Background")]
    public float uvDistortMin = 0.1f;
    public float uvDistortMax = 0.55f;

    public float wiggleMin = 0.2f;
    public float wiggleMax = 0.85f;

    public float sharpenMin = 0f;
    public float sharpenMax = 1f;

    public float transitionTime = 4f; // duración entre cambios
    public float variance = 1f;       // aleatoriedad de tiempo

    private Material backgroundMaterial;
    
    public Transform charsPanel;
    public CharacterSelectionManager characterSelectionManager;

    private void Start()
    {
        CheckAndResize();
        if (GameSession.firstTimeMenu)
        {
            StartIntro();
        }
        else
        {
            GoToCharSelection();
        }
    }
    private void CheckAndResize()
    {
        float widthFactor = Screen.width / 1920f;
        float heightFactor = Screen.height / 1080f;
        mainCamera.orthographicSize = Screen.height / 2f;

        backgroundStudio.localScale = new Vector3(widthFactor, heightFactor, 1f);
        blackBackground.transform.localScale = new Vector3(widthFactor, heightFactor, 1f);
        background.transform.localScale = new Vector3(widthFactor, heightFactor, 1f);
               
    }

    public void SkipIntro()
    {
        StopAllCoroutines();
        LeanTween.cancelAll();
        studioLogo.gameObject.SetActive(false);
        blackBackground.SetActive(true);
        background.gameObject.SetActive(true);
        startGamePanel.SetActive(true);
        startButtonsContainer.SetActive(true);
        musicManager.StopIntro();
        musicManager.PlayMenuLoop();        
    }

    void StartIntro()
    {
        // Empieza música de intro
        musicManager.PlayIntro();        
        
        Material studioMaterial = studioLogo.material;   

        LeanTween.value(1f, 0f, fadeInStudioTime)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
                studioMaterial.SetFloat("_PixelateFade", val);
            })
            .setOnComplete(() =>
            {
                studioMaterial.DisableKeyword("_EnablePixelate");
                StartFadeOut();
            });
    }

    void StartFadeOut()
    {
        LeanTween.value(studioLogo.gameObject, 1f, 0f, fadeOutStudioTime)
                .setOnUpdate((float val) => SetAlpha(studioLogo, val))
                .setOnComplete(() =>
                {
                    studioLogo.gameObject.SetActive(false);
                    musicManager.PlayMenuLoop();
                    ShowStartGamePanel(); // Aquí comienza lo nuevo
                });
    }

    void ShowStartGamePanel()
    {
        blackBackground.SetActive(true);
        background.gameObject.SetActive(true); 
        backgroundMaterial = background.material;       

        AnimateHalftoneThenGlow();
        AnimateFloat("_UVDistortFade", uvDistortMin, uvDistortMax);
        AnimateFloat("_WiggleFade", wiggleMin, wiggleMax);
        AnimateFloat("_SharpenFade", sharpenMin, sharpenMax);

        startGamePanel.SetActive(true);
             
        
        startButtonsContainer.SetActive(false); // Oculta los botones       
    }
    private void GoToCharSelection()
    {
        studioLogo.gameObject.SetActive(false);
        blackBackground.SetActive(true);
        background.gameObject.SetActive(true); 
        backgroundMaterial = background.material;
        AnimateFloat("_UVDistortFade", uvDistortMin, uvDistortMax);
        AnimateFloat("_WiggleFade", wiggleMin, wiggleMax);
        AnimateFloat("_SharpenFade", sharpenMin, sharpenMax);
        musicManager.PlayMenuLoop();
        charsPanel.gameObject.SetActive(true);
        SaveSystem.Load();
        characterSelectionManager.StartSelection();
        startGamePanel.gameObject.SetActive(false);
    }

    void AnimateFloat(string property, float min, float max)
    {
        float startValue = backgroundMaterial.GetFloat(property);
        float endValue = Random.Range(min, max);
        float duration = transitionTime + Random.Range(-variance, variance);

        LeanTween.value(gameObject, startValue, endValue, duration)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnUpdate((float val) =>
                {
                backgroundMaterial.SetFloat(property, val);
                })
                .setOnComplete(() =>
                {
                AnimateFloat(property, min, max); // 🔁 vuelve a animar hacia un nuevo valor
                });
    }

    void AnimateHalftoneThenGlow()
    {
        Material logoMat = logo.material;
        backgroundMaterial.SetFloat("_HalftoneFade", 0f);
        logoMat.SetFloat("_FullGlowDissolveFade", 0f);

        // Halftone (0 → 40, easeInExpo)
        LeanTween.value(background.gameObject, 0f, 40f, halftoneFadeDuration)
                 .setEase(LeanTweenType.easeInExpo)
                 .setOnUpdate((float val) =>
                 {
                    backgroundMaterial.SetFloat("_HalftoneFade", val);
                 })
                 .setOnComplete(() =>
                 {
                     // Glow (0 → 1, linear)
                     LeanTween.value(logo.gameObject, 0f, 1f, glowFadeDuration)
                            .setEase(LeanTweenType.linear)
                            .setOnUpdate((float val) =>
                            {
                                logoMat.SetFloat("_FullGlowDissolveFade", val);
                            })
                            .setOnComplete(() =>
                            {
                                startButtonsContainer.SetActive(true); // Muestra 
                            });
                 });
    }

    void SetAlpha(Graphic g, float alpha)
    {
        Color c = g.color;
        c.a = alpha;
        g.color = c;
    }
}
