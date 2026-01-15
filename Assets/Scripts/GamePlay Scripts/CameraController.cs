using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public Camera mainCamera;
    private Transform dwarfTransform; // Referencia al Dwarf en la escena.
    private Transform golemTransform;
    private PlayerCharacterController playerCharacterController; 
    private GolemController golemController;   
    private float zoomSize; // Tamaño ortográfico deseado.
    public float zoomSpeed = 2f; // Velocidad del zoom.
    public Vector3 offset; // Ajuste de posición.
    private float originalSize;
    private Vector3 originalPosition;
    public Vector3 targetPosition { get; private set; }
    private float spriteDwarfHeight;
    private float spriteDwarfWidth;
    public float spriteGolemHeight;
    private float spriteGolemWidth;
    private float borderPercent = 20f;
    public bool IsZoomed { get; private set; } = false;
    private int zoomAnimationID;
    private int moveAnimationID;

    void Awake()
    {
        Instance = this;
    }
    public void ZoomToDwarf()
    {
        if (!IsZoomed)
        {
            StartCoroutine(ZoomInDwarf());
        }
        else
        {
            ResetCamera();
        }

    }

    public void ZoomToGolem()
    {
        if (!IsZoomed)
        {
            StartCoroutine(ZoomInGolem());
        }
        else
        {
            ResetCamera();
        }             
        
    }

    private IEnumerator ZoomInDwarf()
    {
        SetCameraDwarfSpriteHeight();
        yield return new WaitUntil(() => spriteDwarfHeight > 0);
        IsZoomed = true;

        playerCharacterController = PlayerCharacterController.Instance;
        dwarfTransform = playerCharacterController.transform;

        // Obtener la escala ajustada por resolución
        Vector3 resolutionScale = EntityInitializer.newScale;

        // Calcular tamaño deseado con escala incluida
        zoomSize = spriteDwarfHeight * resolutionScale.y * (1 + borderPercent / 100f) / 2f;

        float targetX = dwarfTransform.localPosition.x + spriteDwarfWidth * resolutionScale.x * (1 + borderPercent / 100f);
        float targetY = dwarfTransform.localPosition.y + spriteDwarfHeight * resolutionScale.y * (1 + borderPercent / 2f / 100f) / 2f;
        float targetZ = mainCamera.transform.position.z;

        Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

        float duration = 0.8f;

        LeanTween.cancel(moveAnimationID);
        LeanTween.cancel(zoomAnimationID);

        // Animar posición
        moveAnimationID = LeanTween.move(mainCamera.gameObject, targetPosition, duration)
                .setEase(LeanTweenType.easeOutBack)
                .id;

        // Animar zoom
        zoomAnimationID = LeanTween.value(gameObject, mainCamera.orthographicSize, zoomSize, duration)
                .setEase(LeanTweenType.easeOutBack)
                .setOnUpdate((float val) => mainCamera.orthographicSize = val)
                .setOnComplete(() =>
                {
                    IsZoomed = true;
                    GearTooltipManager.Instance.ChangeScale(1 / cameraZoomFactor());
                })
                .id;
    }

    private IEnumerator ZoomInGolem()
    {
        IsZoomed = true;
        SetCameraGolemSpriteHeight();
        yield return new WaitUntil(() => spriteGolemHeight > 0);

        golemController = FindFirstObjectByType<GolemController>();
        golemTransform = golemController.transform;

        Vector3 resolutionScale = EntityInitializer.newScale;

        // Calcular tamaño de zoom ajustado a resolución
        zoomSize = spriteGolemHeight * resolutionScale.y * (1 + borderPercent / 100f) / 2f;

        float targetX = golemTransform.localPosition.x - spriteGolemWidth * resolutionScale.x * (0.5f + borderPercent / 100f);
        float targetY = golemTransform.localPosition.y + spriteGolemHeight * resolutionScale.y * (1 + borderPercent / 2f / 100f) / 2f;
        float targetZ = mainCamera.transform.position.z;

        Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);        

        float duration = 0.8f;

        LeanTween.cancel(moveAnimationID);
        LeanTween.cancel(zoomAnimationID);

        moveAnimationID = LeanTween.move(mainCamera.gameObject, targetPosition, duration)
                .setEase(LeanTweenType.easeOutBack)
                .id;

        zoomAnimationID = LeanTween.value(gameObject, mainCamera.orthographicSize, zoomSize, duration)
                .setEase(LeanTweenType.easeOutBack)
                .setOnUpdate((float val) => mainCamera.orthographicSize = val)
                .id;
    }

    public void ResetCamera()
    {
        IsZoomed = false;
        float duration = 0.8f;

        LeanTween.cancel(moveAnimationID);
        LeanTween.cancel(zoomAnimationID);

        // Movimiento de la cámara
        moveAnimationID = LeanTween.move(mainCamera.gameObject, originalPosition, duration)
                .setEase(LeanTweenType.easeInOutQuad)
                .id; // Puedes cambiar a easeOutBack si quieres efecto elástico

        // Zoom (ortographic size)
        zoomAnimationID = LeanTween.value(gameObject, mainCamera.orthographicSize, originalSize, duration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((float val) => mainCamera.orthographicSize = val)
                .setOnComplete(() => GearTooltipManager.Instance.ChangeScale(1 / cameraZoomFactor()))
                .id;
    }
    public void SetCameraDwarfSpriteHeight()
    {
        Sprite sprite;
        AssetReference spriteReference = new AssetReference("Dwarf_1_Sprite");
        spriteReference.LoadAssetAsync<Sprite>().Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                sprite = handle.Result;
                spriteDwarfHeight = sprite.bounds.size.y;
                // Debug.Log($"spriteDwarfHeight = {spriteDwarfHeight}");
                spriteDwarfWidth = sprite.bounds.size.x;
                // Debug.Log($"spriteDwarfWidth = {spriteDwarfWidth}");
            }            
        };
        
    }
    public void SetCameraGolemSpriteHeight()
    {
        Sprite sprite;
        AssetReference spriteReference = new AssetReference("Wood_Golem_Sprite");
        spriteReference.LoadAssetAsync<Sprite>().Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                sprite = handle.Result;
                spriteGolemHeight = sprite.bounds.size.y;
                // Debug.Log($"spriteDwarfHeight = {spriteDwarfHeight}");
                spriteGolemWidth = sprite.bounds.size.x;
                // Debug.Log($"spriteDwarfWidth = {spriteDwarfWidth}");
            }            
        };        
    }
    public void SetOriginalValors()
    {
        originalSize = mainCamera.orthographicSize;
        originalPosition = mainCamera.transform.position;
        targetPosition = originalPosition;
    }

    public float cameraZoomFactor()
    {
        if (IsZoomed)
        {
            return originalSize / zoomSize;
        }
        else
        {
            return 1;
        }
    }
}
