using UnityEngine;

public class AIZombieStateAtack : AIZombieState
{
    [SerializeField] float stoppingDistance = 1.0f;
    [SerializeField] [Range(0, 10)] float speed = 0.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float lookAtAngleThreshold = 15.0f;
    [SerializeField] float slerpSpeed = 5.0f;

    private float currentLookAtWeight = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Attack State");

        base.OnEnterState();
        if (zombieStateMachine == null)
            return;

        // Configure State Machine
        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = Random.Range(1, 100); ;
        zombieStateMachine.Speed = speed;
        currentLookAtWeight = 0.0f;
    }

    public override void OnExitState()
    {
       // zombieStateMachine.AttackType = 0;
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        if(Vector3.Distance(zombieStateMachine.transform.position, zombieStateMachine.TargetPosition) < stoppingDistance)
        {
            zombieStateMachine.Speed = 0;
        }
        else
            zombieStateMachine.Speed = speed;

        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualPlayer)
        {
            zombieStateMachine.SetTarget(stateMachine.visualThreat);

            if (!zombieStateMachine.InMeleeRange)
                return AIStateType.Pursuit;

            if (!zombieStateMachine.UseRootRotation)
            {
                // Keep the zombie facing the player at all times
                targetPos = zombieStateMachine.TargetPosition;
                targetPos.y = zombieStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
                zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
            }

            zombieStateMachine.AttackType = Random.Range(1, 100);

            return AIStateType.Attack;
        }

        // PLayer has stepped outside out FOV or hidden, so face in his/her direction and then
        // drop back to Alerted mode to give the AI a chance to re-aquire target
        if (!zombieStateMachine.UseRootRotation)
        {
            targetPos = zombieStateMachine.TargetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = newRot;
        }

        return AIStateType.Alerted;
    }

    public override void OnAnimatorIKUpdated()
    {
        if (zombieStateMachine == null)
            return;

        if (Vector3.Angle(zombieStateMachine.transform.forward, zombieStateMachine.TargetPosition - zombieStateMachine.transform.position) < lookAtAngleThreshold)
        {
            zombieStateMachine.Animator.SetLookAtPosition(zombieStateMachine.TargetPosition + Vector3.up);
            currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, lookAtWeight, Time.deltaTime);
            zombieStateMachine.Animator.SetLookAtWeight(currentLookAtWeight);
        }
        else
        {
            currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, 0.0f, Time.deltaTime);
            zombieStateMachine.Animator.SetLookAtWeight(currentLookAtWeight);
        }
    }
}
