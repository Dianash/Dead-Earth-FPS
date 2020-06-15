using UnityEngine;

public class AudioManagerTest : MonoBehaviour
{
    public AudioClip clip;
    // Start is called before the first frame update
    private void Start()
    {
        if (AudioManager.Instance)
            AudioManager.Instance.SetTrackVolume("Zombies", 10, 5);

        InvokeRepeating("PlayTest", 1, 1);
    }

    private void PlayTest()
    {
        AudioManager.Instance.PlayOneShotSound("Player", clip, transform.position, 0.5f, 0.0f, 128);
    }
}
