﻿using UnityEngine;

public class AIZombieStateFeeding : AIZombieState
{
    [SerializeField] float slerpSpeed = 5.0f;

    private int eatingStateHash = Animator.StringToHash("Feeding State");
    private int eatingLayerIndex = -1;

    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Feeding state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Feeding State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        if (eatingLayerIndex == -1)
            eatingLayerIndex = zombieStateMachine.Animator.GetLayerIndex("Cinematic Layer");

        // Configure State Machine`s Animator
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = true;
        zombieStateMachine.AttackType = 0;

        zombieStateMachine.NavAgentControl(true, false);
    }

    public override AIStateType OnUpdate()
    {
        if (zombieStateMachine.Satisfaction > 0.9f)
        {
            zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.visualThreat.Type != AITargetType.None && zombieStateMachine.visualThreat.Type != AITargetType.VisualFood)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.audioThreat.Type == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.Animator.GetCurrentAnimatorStateInfo(eatingLayerIndex).shortNameHash == eatingStateHash)
        {
            zombieStateMachine.Satisfaction = Mathf.Min(zombieStateMachine.Satisfaction + (Time.deltaTime * zombieStateMachine.ReplenishRate), 1.0f);
        }

        if (!zombieStateMachine.UseRootRotation)
        {
            Vector3 targetPos = zombieStateMachine.TargetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
        }

        return AIStateType.Feeding;
    }

    public override void OnExitState()
    {
        if (zombieStateMachine != null)
            zombieStateMachine.Feeding = false;
    }
}
