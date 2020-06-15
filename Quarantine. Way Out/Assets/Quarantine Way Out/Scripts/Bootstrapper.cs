using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    private void Start()
    {
        if (ApplicationManager.Instance)
            ApplicationManager.Instance.LoadMainMenu();
    }
}
