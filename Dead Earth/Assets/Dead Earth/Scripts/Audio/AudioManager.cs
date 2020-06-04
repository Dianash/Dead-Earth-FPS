using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer = null;

    private static AudioManager instance = null;
    private Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = (AudioManager)FindObjectOfType(typeof(AudioManager));

            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!mixer) return;

        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup group in groups)
        {
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.group = group;
            trackInfo.trackFader = null;
            tracks[group.name] = trackInfo;
        }
    }

    protected IEnumerator SetTrackVolumeInternal(string track, float volume, float fadeTime)
    {
        float startVolume = 0.0f;
        float timer = 0.0f;

        mixer.GetFloat(track, out startVolume);

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer / fadeTime));
            yield return null;
        }

        mixer.SetFloat(track, volume);
    }


}
