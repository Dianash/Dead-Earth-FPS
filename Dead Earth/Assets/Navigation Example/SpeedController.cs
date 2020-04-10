using UnityEngine;

public class SpeedController : MonoBehaviour
{
    public float speed = 0.0f;

    private Animator controller = null;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        controller.SetFloat("speed", speed);
    }
}
