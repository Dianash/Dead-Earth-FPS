using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player being damaged");
        }
    }
}
