using UnityEngine;

public class CameraBloodEffect : MonoBehaviour
{
    [SerializeField] private Shader shader = null;

    private Material material = null;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null) return;

        if (material == null)
        {
            material = new Material(shader);
        }
        if (material == null) return;

        Graphics.Blit(source, destination, material);
    }
}
