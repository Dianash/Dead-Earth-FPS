using UnityEngine;

public class Controller : MonoBehaviour
{
    private Animator animator = null;

    private int horizontalHash = 0;
    private int vericalHash = 0;
    private int atackHash = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        horizontalHash = Animator.StringToHash("Horizontal");
        vericalHash = Animator.StringToHash("Vertical");
        atackHash = Animator.StringToHash("Atack");
    }

    // Update is called once per frame
    void Update()
    {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.6f;

        if (Input.GetMouseButton(0))
            animator.SetTrigger(atackHash);
        animator.SetFloat(horizontalHash, xAxis, 0.1f, Time.deltaTime);
        animator.SetFloat(vericalHash, yAxis, 1.0f, Time.deltaTime);
    }
}
