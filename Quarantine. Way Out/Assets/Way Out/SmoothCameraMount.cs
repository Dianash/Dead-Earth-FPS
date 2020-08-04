using UnityEngine;

public class SmoothCameraMount : MonoBehaviour
{
    public Transform mount = null;
    public float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, mount.position, Time.deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, mount.rotation, Time.deltaTime * speed);
    }
}
