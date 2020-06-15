using UnityEngine;

public class AILayeredAudioSourcePlayer : AIStateMachineLink
{
	[SerializeField] private AudioCollection collection = null;
	[SerializeField] private int bank = 0;
	[SerializeField] private bool looping = true;
	[SerializeField] private bool stopOnExit = false;

	private float prevLayerWeight = 0.0f;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
	{
		if (stateMachine == null) return;

		// Get the layer weight and only play for weighted layer
		float layerWeight = animator.GetLayerWeight(layerIndex);

		if (collection != null)
		{
			if (layerIndex == 0 || layerWeight > 0.5f)
            {
				stateMachine.PlayAudio(collection, bank, layerIndex, looping);
			}
			else
            {
				stateMachine.StopAudio(layerIndex);
			}
		}

		// Store layer weight to detect changes mid animation
		prevLayerWeight = layerWeight;
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
	{
		if (stateMachine == null) return;

		// Get the current layer weight
		float layerWeight = animator.GetLayerWeight(layerIndex);

		if (layerWeight != prevLayerWeight && collection != null)
		{
			if (layerWeight > 0.5f)
            {
				stateMachine.PlayAudio(collection, bank, layerIndex, true);
			}
			else
            {
				stateMachine.StopAudio(layerIndex);
			}
		}

		prevLayerWeight = layerWeight;
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
	{
		if (stateMachine && stopOnExit)
			stateMachine.StopAudio(layerIndex);
	}
}
