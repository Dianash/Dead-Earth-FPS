using UnityEngine;

[ExecuteInEditMode]
public class CameraBloodEffect : MonoBehaviour
{
    [SerializeField] private Texture2D bloodTexture = null;
    [SerializeField] private Texture2D bloodNormalMap = null;

    [SerializeField] private float bloodAmount = 0.0f;
    [SerializeField] private float minBloodAmount = 0.0f;
    [SerializeField] private float distortion = 1.0f;
    [SerializeField] private float fadeSpeed = 0.05f;

    [SerializeField] private bool autoFade = true;

    [SerializeField] private Shader shader = null;

    private Material material = null;

    public float BloodAmount { get => bloodAmount; set => bloodAmount = value; }
    public float MinBloodAmount { get => minBloodAmount; set => minBloodAmount = value; }
    public float FadeSpeed { get => fadeSpeed; set => fadeSpeed = value; }
    public bool AutoFade { get => autoFade; set => autoFade = value; }

    private void Update()
    {
        if (autoFade)
        {
            bloodAmount -= fadeSpeed * Time.deltaTime;
            bloodAmount = Mathf.Max(bloodAmount, minBloodAmount);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null) return;
        if (material == null)
        {
            material = new Material(shader);
        }
        if (material == null) return;

        if (bloodTexture != null)
            material.SetTexture("_BloodTex", bloodTexture);

        if (bloodNormalMap != null)
            material.SetTexture("_BloodBump", bloodNormalMap);

        material.SetFloat("Distortion", distortion);
        material.SetFloat("_BloodAmount", bloodAmount);

        Graphics.Blit(source, destination, material);
    }
}
