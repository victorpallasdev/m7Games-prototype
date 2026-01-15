using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightningController : MonoBehaviour
{
    public Light2D fruitLight;
    public Light2D mushroomLight;
    public Light2D torch;

    void Start()
    {
        // Arrancamos cada animación por separado
        StartCoroutine(FruitLightRoutine());
        StartCoroutine(MushroomLightRoutine());
        StartCoroutine(TorchLightRoutine());
    }

    // --- FRUIT LIGHT ---
    IEnumerator FruitLightRoutine()
    {
        while (true)
        {
            float target = Random.Range(0.3f, 2f);
            float time = Random.Range(1.5f, 3f);
            yield return AnimateLight(fruitLight, target, time);

            // Simulamos apagón ocasional
            if (Random.value < 0.2f) // 20% de las veces
            {
                yield return AnimateLight(fruitLight, 0.1f, 0.3f);
                yield return AnimateLight(fruitLight, target, 0.4f);
            }
        }
    }

    // --- MUSHROOM LIGHT ---
    IEnumerator MushroomLightRoutine()
    {
        while (true)
        {
            float target = Random.Range(0.6f, 5f);
            float time = Random.Range(2f, 4f);
            yield return AnimateLight(mushroomLight, target, time);
        }
    }

    // --- TORCH LIGHT ---
    IEnumerator TorchLightRoutine()
    {
        while (true)
        {
            float target = Random.Range(1.2f, 2f);
            float time = Random.Range(0.1f, 0.3f); // Corto para que tiemble mucho
            yield return AnimateLight(torch, target, time);
        }
    }

    // --- COMMON ANIMATION ---
    IEnumerator AnimateLight(Light2D light, float targetIntensity, float duration)
    {
        float start = light.intensity;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / duration);
            light.intensity = Mathf.Lerp(start, targetIntensity, progress);
            yield return null;
        }

        light.intensity = targetIntensity;
    }
}
