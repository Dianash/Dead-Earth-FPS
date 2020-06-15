using UnityEngine;

public class AudioOnEnter : StateMachineBehaviour
{
    [SerializeField] AudioCollection audioCollection = null;
    [SerializeField] int bank = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.Instance == null || audioCollection == null) 
            return;

        AudioManager.Instance.PlayOneShotSound(audioCollection.AudioGroup, audioCollection[bank], animator.transform.position,
            audioCollection.Volume, audioCollection.SpatialBlend, audioCollection.Priority);
    }
}
