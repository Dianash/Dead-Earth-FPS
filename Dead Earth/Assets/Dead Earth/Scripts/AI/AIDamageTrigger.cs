using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string parameter = null;

    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player being damaged");
        }
    }
}
