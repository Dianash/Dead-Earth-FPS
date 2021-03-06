﻿using UnityEngine;

public class NPCStickyDetector : MonoBehaviour
{
    FPSController controller = null;

    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<FPSController>();
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider col)
    {
        AIStateMachine machine = GameSceneManager.Instance.GetAIStateMachine(col.GetInstanceID());

        if (machine != null && controller != null)
        {
            controller.DoStickiness();

            machine.visualThreat.Set(AITargetType.VisualPlayer, controller.CharacterController, controller.transform.position,
                Vector3.Distance(machine.transform.position, controller.transform.position));

            machine.SetStateOverride(AIStateType.Attack);
        }
    }
}
