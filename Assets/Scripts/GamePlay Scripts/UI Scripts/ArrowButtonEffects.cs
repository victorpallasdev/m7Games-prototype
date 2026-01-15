using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ArrowButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Effect")]
    [Tooltip("Scale factor on hover")] public float hoverScale = 1.2f;
    [Tooltip("Duration of hover tween")] public float hoverDuration = 0.2f;

    [Header("Idle Loop Motion")]
    [Tooltip("Horizontal offset from initial position")] public float idleOffset = 10f;
    [Tooltip("Duration to move out and back")] public float idleDuration = 0.5f;

    private RectTransform rectTransform;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private LTDescr idleTween;
    private int hoverTweenId;
    private int dehoverTweenId;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        initialScale = rectTransform.localScale;

        StartIdleLoop();
    }

    void OnDestroy()
    {
        // Clean up tweens
        if (idleTween != null) LeanTween.cancel(idleTween.uniqueId);
        LeanTween.cancel(hoverTweenId);
        LeanTween.cancel(dehoverTweenId);
    }

    private void StartIdleLoop()
    {
        // Ping-pong horizontal move
        idleTween = LeanTween.moveLocalX(gameObject, initialPosition.x - idleOffset, idleDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cancel any dehover in progress
        LeanTween.cancel(dehoverTweenId);
        // Scale up
        hoverTweenId = LeanTween.scale(gameObject, initialScale * hoverScale, hoverDuration)
            .setEase(LeanTweenType.easeOutBack)
            .uniqueId;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancel any hover in progress
        LeanTween.cancel(hoverTweenId);
        // Scale back to original
        dehoverTweenId = LeanTween.scale(gameObject, initialScale, hoverDuration)
            .setEase(LeanTweenType.easeOutBack)
            .uniqueId;
    }
}
