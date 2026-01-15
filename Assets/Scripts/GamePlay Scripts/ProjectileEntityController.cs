using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ProjectileEntityController : MonoBehaviour
{
    private RectTransform rectTransform;
    public Light2D projectileLight;
 

    public void Initialize(Sprite projectileSprite)
    {
        rectTransform = transform.GetComponent<RectTransform>();
        SpriteRenderer projectileImage = GetComponent<SpriteRenderer>();
        projectileImage.sprite = projectileSprite;
    }
    public void ParabolicLaunch(Vector2 from, Vector2 to, float duration, float jumpHeight, float rotation, Action onComplete)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = from;

        LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate((float t) =>
            {
                Vector2 pos = Vector2.Lerp(from, to, t);
                float height = 4 * jumpHeight * t * (1 - t); // parábola tipo física
                pos.y += height;
                rt.anchoredPosition = pos;

                float rotZ = Mathf.Lerp(0f, rotation, t);
                rt.rotation = Quaternion.Euler(0f, 0f, rotZ);
            })
            .setEase(LeanTweenType.linear) // 👈 importante
            .setOnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
    public void StartTNTFlicker()
    {
        StartCoroutine(TNTFlicker());
    }
    private IEnumerator TNTFlicker()
    {
        while (true)
        {
            float targetIntensity = UnityEngine.Random.Range(2f, 50f);
            float duration = UnityEngine.Random.Range(0.05f, 0.1f); // Ciclo rápido, parece nerviosa
            float startIntensity = projectileLight.intensity;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                projectileLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t / duration);
                yield return null;
            }
        }
    }
    
}
