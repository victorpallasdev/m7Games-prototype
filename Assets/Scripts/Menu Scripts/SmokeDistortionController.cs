using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]  // O MeshRenderer si usas Quad
public class SmokeDistortionController : MonoBehaviour
{
    [Header("References")]
    public Material targetMaterial; // Arrastra aquí tu material con el shader SSU

    [Header("Toggles")]
    public bool useUVDistort         = true;
    public bool useDirectionalDistort = true;

    [Header("UV Noise Distortion")]
    public float baseNoiseScale = 3f;
    public float baseNoiseSpeed = 0.5f;
    public float baseAmplitude  = 0.02f;

    [Header("Directional Distortion")]
    [Range(0,1)]  public float directionalFadeSpeed = 0.2f;
    public float   rotationSpeed           = 30f;
    public float   directionalWidth        = 0.5f;
    public Vector2 directionalNoiseScale   = new Vector2(0.4f,0.4f);
    [Range(0,1)]  public float directionalNoiseFactor = 0.2f;
    public Vector2 directionalDistortion   = new Vector2(0f, 0.1f);
    public Vector2 directionalDistortScale = new Vector2(1f, 1f);

    // Keywords (tal cual están en tu shader)
    const string KW_UV_DISTORT   = "_ENABLEUVDISTORT_ON";
    const string KW_DIR_DISTORT  = "_ENABLEDIRECTIONALDISTORTION_ON";

    void Awake()
    {
        if (targetMaterial == null)
        {
            var sr = GetComponent<SpriteRenderer>();
            targetMaterial = sr ? sr.material : null;
        }

        // Hacemos instancia para no modificar el material original
        targetMaterial = Instantiate(targetMaterial);
        if (TryGetComponent<SpriteRenderer>(out var sprite))
            sprite.material = targetMaterial;
    }

    void Start()
    {
        // Activamos o desactivamos keywords según los toggles
        if (useUVDistort)    targetMaterial.EnableKeyword(KW_UV_DISTORT);
        else                  targetMaterial.DisableKeyword(KW_UV_DISTORT);

        if (useDirectionalDistort)   targetMaterial.EnableKeyword(KW_DIR_DISTORT);
        else                          targetMaterial.DisableKeyword(KW_DIR_DISTORT);
    }

    void Update()
    {
        float t = Time.time;

        // 1) UV Noise (humo base)
        targetMaterial.SetFloat("_NoiseScale", baseNoiseScale);
        targetMaterial.SetFloat("_NoiseSpeed", baseNoiseSpeed);
        targetMaterial.SetFloat("_Amplitude",  baseAmplitude);

        if (useDirectionalDistort)
        {
            // 2) Rotación de la distorsión direccional
            float rot = Mathf.Repeat(t * rotationSpeed, 360f);
            targetMaterial.SetFloat("_DirectionalDistortionRotation", rot);

            // 3) Fade pulsante de la distorsión direccional
            float fade = Mathf.PingPong(t * directionalFadeSpeed, 1f);
            targetMaterial.SetFloat("_DirectionalDistortionFade", fade);

            // 4) Resto de parámetros direccionales
            targetMaterial.SetFloat("_DirectionalDistortionWidth", directionalWidth);
            targetMaterial.SetVector("_DirectionalDistortionNoiseScale",
                                     new Vector4(directionalNoiseScale.x, directionalNoiseScale.y, 0, 0));
            targetMaterial.SetFloat("_DirectionalDistortionNoiseFactor", directionalNoiseFactor);
            targetMaterial.SetVector("_DirectionalDistortionDistortion",
                                     new Vector4(directionalDistortion.x, directionalDistortion.y, 0, 0));
            targetMaterial.SetVector("_DirectionalDistortionDistortionScale",
                                     new Vector4(directionalDistortScale.x, directionalDistortScale.y, 0, 0));
        }
    }
}
