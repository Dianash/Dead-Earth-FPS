using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCollectionPlayer : AIStateMachineLink
{
    [SerializeField] ComChannelName commandChannel = ComChannelName.ComChannel1;
    [SerializeField] AudioCollection collection = null;
    [SerializeField] CustomCurve customCurve = null;
    [SerializeField] StringList layerExclusions = null;


    private int previousCommand = 0;
    private AudioManager audioManager = null;
    private int commandChannelHash = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        audioManager = AudioManager.Instance;
        previousCommand = 0;

        // TODO: Store hashes in state machine lookup
        if (commandChannelHash == 0)
            commandChannelHash = Animator.StringToHash(commandChannel.ToString());
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Don't make these sounds if our layer weight is zero
        if (layerIndex != 0 && animator.GetLayerWeight(layerIndex).Equals(0.0f))
            return;

        if (stateMachine == null)
            return;

        int customCommand = (customCurve == null) ? 0 : Mathf.FloorToInt(customCurve.Evaluate(animStateInfo.normalizedTime - (long)animStateInfo.normalizedTime));

        int command;
        if (customCommand != 0)
        {
            command = customCommand;
        }
        else
        {
            command = Mathf.FloorToInt(animator.GetFloat(commandChannelHash));
        }

        if (previousCommand != command && command > 0 && audioManager != null && collection != null && stateMachine != null)
        {
            int bank = Mathf.Max(0, Mathf.Min(command - 1, collection.BankCount - 1));

            audioManager.PlayOneShotSound(collection.AudioGroup, collection[bank], stateMachine.transform.position,
                collection.Volume, collection.SpatialBlend, collection.Priority);
        }

        previousCommand = command;
    }



}
