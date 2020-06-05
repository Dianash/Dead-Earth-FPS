using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer = null;
    [SerializeField] int maxSounds = 10;

    private static AudioManager instance = null;
    private Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();
    private List<AudioPoolItem> pool = new List<AudioPoolItem>();
    private Dictionary<ulong, AudioPoolItem> activePool = new Dictionary<ulong, AudioPoolItem>();
    private ulong idGiver = 0;
    private Transform listenerPosition = null;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = (AudioManager)FindObjectOfType(typeof(AudioManager));

            return instance;
        }
    }

    public AudioMixerGroup GetAudioGroupFromTrackName(string name)
    {
        TrackInfo trackInfo;
        if (tracks.TryGetValue(name, out trackInfo))
        {
            return trackInfo.group;
        }

        return null;
    }

    public float GetTrackVolume(string track)
    {
        TrackInfo trackInfo;

        if (tracks.TryGetValue(track, out trackInfo))
        {
            float volume;
            mixer.GetFloat(track, out volume);
            return volume;
        }

        return float.MinValue;
    }

    /// <summary>
    /// Sets the volume of the AudioMixergroup assigned to the passed track
    /// </summary>
    public void SetTrackVolume(string track, float volume, float fadeTime = 0.0f)
    {
        if (!mixer) return;

        TrackInfo trackInfo;

        if (tracks.TryGetValue(track, out trackInfo))
        {
            // Stop any coroutine that might be in the middle of fading this track
            if (trackInfo.trackFader != null)
                StopCoroutine(trackInfo.trackFader);

            if (fadeTime == 0.0f)
            {
                mixer.SetFloat(track, volume);
            }
            else
            {
                trackInfo.trackFader = SetTrackVolumeInternal(track, volume, fadeTime);
                StartCoroutine(trackInfo.trackFader);
            }
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

        // Generate pool
        for (int i = 0; i < maxSounds; i++)
        {
            GameObject gameObject = new GameObject("Pool Item");
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            gameObject.transform.parent = transform;

            AudioPoolItem poolItem = new AudioPoolItem();
            poolItem.gameObject = gameObject;
            poolItem.audioSource = audioSource;
            poolItem.transform = transform;
            poolItem.playing = false;
            gameObject.SetActive(false);
            pool.Add(poolItem);
        }
    }
}
