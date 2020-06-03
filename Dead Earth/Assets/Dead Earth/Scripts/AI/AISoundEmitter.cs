using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    [SerializeField] private float decayRate = 1.0f;

    private SphereCollider collider = null;
    private float sourceRadius = 0.0f;
    private float targetRadius = 0.0f;
    private float interpolator = 0.0f;
    private float interpolatorSpeed = 0.0f;

    void Start()
    {
        collider = GetComponent<SphereCollider>();
        if (!collider) return;

        sourceRadius = targetRadius = collider.radius;

        interpolator = 0.0f;

        if (decayRate > 0.02f)
            interpolatorSpeed = 1.0f / decayRate;
        else
            interpolatorSpeed = 0.0f;
    }

    private void FixedUpdate()
    {
        if (!collider) return;

        interpolator = Mathf.Clamp01(interpolator + Time.deltaTime * interpolatorSpeed);
        collider.radius = Mathf.Lerp(sourceRadius, targetRadius, interpolator);

        if (collider.radius < Mathf.Epsilon) collider.enabled = false;
        else collider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (!collider || newRadius == targetRadius) return;

        sourceRadius = (instantResize || newRadius > collider.radius) ? newRadius : collider.radius;
        targetRadius = newRadius;
        interpolator = 0.0f;
    }

    private void Update()
    {
        SetRadius(2.0f);
        if (Input.GetMouseButtonDown(0)) SetRadius(15.0f);
    }
}
