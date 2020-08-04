using System.Collections;
using UnityEngine;

public class InteractiveSound : InteractiveItem
{
    [TextArea(3, 10)] [SerializeField] private string infoText = null;
    [TextArea(3, 10)] [SerializeField] private string activatedText = null;

    [SerializeField] private float activatedTextDuration = 3.0f;
    [SerializeField] private AudioCollection audioCollection = null;
    [SerializeField] private int bank = 0;

    private IEnumerator coroutine = null;
    private float hideActivatedTextTime = 0.0f;

    public override string GetText()
    {
        if (coroutine != null || Time.time < hideActivatedTextTime)
            return activatedText;
        else
            return infoText;
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (coroutine == null)
        {
            hideActivatedTextTime = Time.time + activatedTextDuration;
            coroutine = DoActivation();
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator DoActivation()
    {
        if (audioCollection == null || AudioManager.Instance == null) 
            yield break;

        // Fetch Clip from Collection
        AudioClip clip = audioCollection[bank];

        if (clip == null) yield break;

        // Play it as one shot sound
        AudioManager.Instance.PlayOneShotSound(audioCollection.AudioGroup, clip, transform.position, audioCollection.Volume,
            audioCollection.SpatialBlend, audioCollection.Priority);

        // Run while clip is playing
        yield return new WaitForSeconds(clip.length);

        // Unblock coroutine instantiation
        coroutine = null;
    }
}
