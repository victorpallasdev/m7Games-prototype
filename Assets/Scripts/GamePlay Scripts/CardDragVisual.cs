using UnityEngine;
using UnityEngine.InputSystem;

public class CardDragVisual : MonoBehaviour
{
    public Transform cardTransform;
    public Transform cardImageTransform;
    public float maxVelocity = 5000f; // Umbral máximo de aceleración
    public float rotationSpeed = 90f;
    [SerializeField] public float instantVelocity;
    [SerializeField] public float targetZRotation;
    private float lastX = 0f;
    public bool isDragging = false;
    private CardController cardController;
    private float currentZ = 0f;
    private float zVelocity = 0f;

void Update()
{
    if (!isDragging) return;

    float currentX = transform.localPosition.x;
    float deltaX = currentX - lastX;
    float t = Time.deltaTime;

    instantVelocity = deltaX / t;
    lastX = currentX;

    if (instantVelocity >= maxVelocity)
        targetZRotation = -90f;
    else if (instantVelocity <= -maxVelocity)
        targetZRotation = 90f;
    else
        targetZRotation = Mathf.Lerp(90f, -90f, (instantVelocity + maxVelocity) / (2f * maxVelocity));

    // Suaviza la rotación con inercia natural
    currentZ = Mathf.SmoothDampAngle(currentZ, targetZRotation, ref zVelocity, 0.1f);
    cardImageTransform.localRotation = Quaternion.Euler(0, 0, currentZ);
}

    public void StartDragging()
    {
        cardController = GetComponent<CardController>();
        isDragging = true;
        cardController.ResetRotation();
        lastX = transform.localPosition.x;
    }

    public void StopDragging()
    {
        
        isDragging = false;        
    }

    public void UpdateDraggedPosition(Vector2 screenPosition, RectTransform canvasRectTransform)
    {
        CameraController camController = FindFirstObjectByType<CameraController>();
        Camera mainCamera = camController.transform.GetComponent<Camera>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            mainCamera,
            out Vector2 localPoint
        );
        RectTransform cardRectTransform = GetComponent<RectTransform>();
        cardRectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 70f);
    }

}
