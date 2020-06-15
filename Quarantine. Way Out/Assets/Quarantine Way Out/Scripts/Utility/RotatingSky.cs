using UnityEngine;

public class RotatingSky : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField] protected Material skyMaterial = null;
    [SerializeField] protected float speed = 1.0f;

    // Internals
    protected float angle = 0;
    protected float originalAngle = 0;

    private void OnEnable()
    {
        if (skyMaterial)
            originalAngle = angle = skyMaterial.GetFloat("_Rotation");
    }


    private void OnDisable()
    {
        if (skyMaterial)
            skyMaterial.SetFloat("_Rotation", originalAngle);
    }

    private void Update()
    {
        if (skyMaterial == null)
            return;

        angle += speed * Time.deltaTime;

        if (angle > 360.0f)
        {
            angle -= 360.0f;
        }
        else if (angle < 0.0f)
        {
            angle += 360.0f;
        }

        skyMaterial.SetFloat("_Rotation", angle);
    }
}
