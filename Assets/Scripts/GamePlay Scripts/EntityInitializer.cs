using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class EntityInitializer : MonoBehaviour
{
    private DeckManager deckManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject dwarfPrefab; // Prefab del DwarfController
    [SerializeField] private GolemData golemData;
    [SerializeField] private CharacterData characterData;
    public static Vector3 newScale;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 dwarfSpawnPosition = new Vector3(-300, -170, 0); // Posición inicial del Dwarf
    [SerializeField] private Vector3 golemSpawnPosition = new Vector3(365, -170, 0); // Posición inicial del Golem
    [SerializeField] private Transform middlePanelTransform;

    [Header("Screen Settings")]
    public Transform backgroundCanvasTransform;
    public Transform mainCanvasTransform;
    public RectTransform fusionatorCanvasRect;
    public RectTransform fusionatorHandRect;
    public Transform inspectorCatcher;
    public List<Transform> transformsToMoveEdges;
    public List<Transform> transformsToMove;
    public List<Light2D> lights;
    public Camera mainCamera;
    public Image backPackImage;
    public Image playHandButtonImage;
    public Image settingsButtonImage;
    public GameSession gameSession;
    public CharacterData directChar;
    public Transform backgroundChooseACard;
    private float heightDiference = 0f;
    private float widthFactor = 1f;
    private float widthDiference = 0f;
    private float heigthFactor = 1f;

    void Awake()
    {        
        ResizeScreen();
        StartCoroutine(FindGameSession());
        gameSession = GameSession.Instance;
        if (gameSession != null)
        {
            if (gameSession.SelectedChar != null)
            {
                characterData = gameSession.SelectedChar;
            }
            else
            {
                characterData = directChar;
            }  
        }
        else
        {
            characterData = directChar;
        }  
           
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        GameObject middlePanel = GameObject.Find("Middle Panel");
        middlePanelTransform = middlePanel.transform;

        // Instanciar el Dwarf
        if (dwarfPrefab != null)
        {
            dwarfSpawnPosition = new Vector3(dwarfSpawnPosition.x * widthFactor, dwarfSpawnPosition.y * heigthFactor, 0f);
            GameObject characterGO = Instantiate(dwarfPrefab, dwarfSpawnPosition, Quaternion.identity, middlePanelTransform);
            characterGO.transform.localScale = newScale;
            PlayerCharacterController characterController = characterGO.GetComponent<PlayerCharacterController>();
            characterController.InitializeDwarf(characterData);
            StartCoroutine(SetCharacterColorOnHUD(characterData));
            StartCoroutine(LoadDeckToManager(characterData.deck));
        }
        else
        {
            Debug.LogError("Prefab de DwarfController no asignado.");
        }
        golemSpawnPosition = new Vector3(golemSpawnPosition.x * widthFactor, golemSpawnPosition.y * heigthFactor, 0f);
        // Instanciar el Golem
        Addressables.InstantiateAsync("GolemPreFab", golemSpawnPosition, Quaternion.identity, middlePanelTransform).Completed += (AsyncOperationHandle<GameObject> handle) =>
        {
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject golemInstance = handle.Result;
                golemInstance.transform.localScale = newScale;
                // Inicializar el Golem
                GolemController golemController = golemInstance.GetComponent<GolemController>();
                if (golemData != null)
                {
                    golemController.InitializeGolem(golemData);
                    golemInstance.name = golemData.golemName;
                }
                else
                {
                    Debug.LogError("GolemData no asignada.");
                }

            }
            else
            {
                Debug.LogError("Prefab del Golem no encontrado.");
            }
        };
    }
    IEnumerator SetCharacterColorOnHUD(CharacterData characterData)
    {
        yield return new WaitUntil(() => SocketController.Instance != null);
        SocketController.Instance.SetSpriteColor(characterData.characterColor);
        playHandButtonImage.color = characterData.characterColor;
        settingsButtonImage.color = characterData.characterColor;
        backPackImage.sprite = characterData.backPackSprite;
    }
    private IEnumerator FindGameSession()
    {
        yield return new WaitUntil(() => (gameSession = GameSession.Instance) != null);
    }
    IEnumerator LoadDeckToManager(List<CardData> deck)
    {
        yield return new WaitUntil(() => (deckManager = DeckManager.Instance) != null);
        deckManager.LoadFullDeck(deck);
    }
    private void ResizeScreen()
    {
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;

        if (screenWidth / screenHeight == 16f / 9f && screenWidth != 1920f)
        {
            float factor = screenWidth / 1920f;
            mainCanvasTransform.localScale = new Vector3(factor, factor, 1);
            backgroundCanvasTransform.localScale = new Vector3(factor, factor, 1);
            inspectorCatcher.localScale = new Vector3(factor, factor, 1);
            //backgroundChooseACard.localScale = new(factor, factor, 1);
        }
        else if (screenWidth / screenHeight != 16f / 9f)
        {
            widthFactor = screenWidth / 1920f;
            widthDiference = screenWidth - 1920f;
            heigthFactor = screenHeight / 1080f;
            heightDiference = screenHeight - 1080f;

            mainCamera.orthographicSize = screenHeight / 2f;
            CameraController cameraController = mainCamera.transform.GetComponent<CameraController>();
            cameraController.SetOriginalValors();
            backgroundCanvasTransform.localScale = new Vector3(widthFactor, heigthFactor, 1);
            //backgroundChooseACard.localScale = new(widthFactor, heigthFactor, 1);
            inspectorCatcher.localScale = new Vector3(widthFactor, heigthFactor, 1);
            fusionatorCanvasRect.sizeDelta = new Vector2(fusionatorCanvasRect.rect.width * widthFactor, fusionatorCanvasRect.rect.height);
            fusionatorHandRect.sizeDelta = new Vector2(fusionatorHandRect.rect.width * widthFactor, fusionatorHandRect.rect.height);

            foreach (Transform element in transformsToMove)
            {
                Vector3 originalPos = element.localPosition;
                element.localPosition = new Vector3(originalPos.x, originalPos.y * heigthFactor, 0f);

                if (heightDiference >= widthDiference)
                    element.localScale = new Vector3(widthFactor, widthFactor, 1f);
                else
                    element.localScale = new Vector3(heigthFactor, heigthFactor, 1f);
            }

            foreach (Transform element in transformsToMoveEdges)
            {
                RectTransform rt = element.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Obtener tamaño original
                Vector2 originalSize = rt.rect.size;

                // Escalado nuevo
                newScale = (heightDiference >= widthDiference)
                    ? new Vector3(widthFactor, widthFactor, 1f)
                    : new Vector3(heigthFactor, heigthFactor, 1f);

                element.localScale = newScale;

                // Tamaño nuevo escalado
                float newWidth = originalSize.x * newScale.x;
                float newHeight = originalSize.y * newScale.y;

                float widthDiffLocal = newWidth - originalSize.x;
                float heightDiffLocal = newHeight - originalSize.y;

                Vector3 offset = Vector3.zero;

                Vector3 originalPos = element.localPosition;

                // Determinar dirección más cercana (comparando con centro canvas)
                Vector2 canvasCenter = Vector2.zero;
                Vector2 elementPos = new Vector2(originalPos.x, originalPos.y);

                bool isLeft = elementPos.x < canvasCenter.x;
                bool isRight = elementPos.x > canvasCenter.x;
                bool isTop = elementPos.y > canvasCenter.y;
                bool isBottom = elementPos.y < canvasCenter.y;
                bool isCenterX = Mathf.Approximately(elementPos.x, canvasCenter.x);
                bool isCenterY = Mathf.Approximately(elementPos.y, canvasCenter.y);

                if (isTop && isLeft)         // Top-Left
                    offset = new Vector3(-widthDiference / 2f + widthDiffLocal / 2f, heightDiference / 2f - heightDiffLocal / 2f, 0f);
                else if (isTop && isCenterX) // Top-Center
                    offset = new Vector3(0f, heightDiference / 2f - heightDiffLocal / 2f, 0f);
                else if (isTop && isRight)   // Top-Right
                    offset = new Vector3(widthDiference / 2f - widthDiffLocal / 2f, heightDiference / 2f - heightDiffLocal / 2f, 0f);
                else if (isCenterY && isLeft) // Middle-Left
                    offset = new Vector3(-widthDiference / 2f + widthDiffLocal / 2f, 0f, 0f);
                else if (isCenterY && isRight) // Middle-Right
                    offset = new Vector3(widthDiference / 2f - widthDiffLocal / 2f, 0f, 0f);
                else if (isBottom && isLeft)  // Bottom-Left
                    offset = new Vector3(-widthDiference / 2f + widthDiffLocal / 2f, -heightDiference / 2f + heightDiffLocal / 2f, 0f);
                else if (isBottom && isCenterX) // Bottom-Center
                    offset = new Vector3(0f, -heightDiference / 2f + heightDiffLocal / 2f, 0f);
                else if (isBottom && isRight) // Bottom-Right
                    offset = new Vector3(widthDiference / 2f - widthDiffLocal / 2f, -heightDiference / 2f + heightDiffLocal / 2f, 0f);

                element.localPosition += offset;
            }
        }
        foreach (Light2D light in lights)
        {
            light.pointLightOuterRadius *= newScale.x;
        }
    }


    public void SpawnFieldEntity(FieldEntityData entityData)
    {
        Addressables.LoadAssetAsync<GameObject>("FieldEntityPrefab").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject fieldEntityGO = Instantiate(handle.Result, GetRandomSpawnPosition(), Quaternion.identity, middlePanelTransform);

                FieldEntityController fieldEntity = fieldEntityGO.GetComponent<FieldEntityController>();
                fieldEntity.Initialize(entityData);
                fieldEntity.SpawnFall();
            }
        };
    }
    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(120f, 560f);
        float spawnY = 540f;
        return new Vector3(randomX, spawnY, 0f);
    }
}
