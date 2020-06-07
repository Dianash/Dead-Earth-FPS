using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    [SerializeField] private float decayRate = 1.0f;

    private SphereCollider sphereCollider = null;
    private float sourceRadius = 0.0f;
    private float targetRadius = 0.0f;
    private float interpolator = 0.0f;
    private float interpolatorSpeed = 0.0f;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (!sphereCollider) return;

        sourceRadius = targetRadius = sphereCollider.radius;

        interpolator = 0.0f;

        if (decayRate > 0.02f)
            interpolatorSpeed = 1.0f / decayRate;
        else
            interpolatorSpeed = 0.0f;
    }

    private void FixedUpdate()
    {
        if (!sphereCollider) return;

        interpolator = Mathf.Clamp01(interpolator + Time.deltaTime * interpolatorSpeed);
        sphereCollider.radius = Mathf.Lerp(sourceRadius, targetRadius, interpolator);

        if (sphereCollider.radius < Mathf.Epsilon) sphereCollider.enabled = false;
        else sphereCollider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (!sphereCollider || newRadius == targetRadius) return;

        sourceRadius = (instantResize || newRadius > sphereCollider.radius) ? newRadius : sphereCollider.radius;
        targetRadius = newRadius;
        interpolator = 0.0f;
    }
}
