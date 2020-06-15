using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        if (ApplicationManager.Instance)
            ApplicationManager.Instance.LoadMainMenu();
    }
}
