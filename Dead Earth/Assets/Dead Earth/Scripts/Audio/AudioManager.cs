using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

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
    /// Sets the volume of the AudioMixergroup assigned to the passed track.
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

    public void StopOneShotSound(ulong id)
    {
        AudioPoolItem activeSound;

        if (activePool.TryGetValue(id, out activeSound))
        {
            StopCoroutine(activeSound.coroutine);

            activeSound.audioSource.Stop();
            activeSound.audioSource.clip = null;
            activeSound.gameObject.SetActive(false);
            activePool.Remove(id);

            activeSound.playing = false;
        }
    }


    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)
    {
        if (!tracks.ContainsKey(track) || clip == null || volume.Equals(0.0f))
            return 0;

        float unimportance = (listenerPosition.position - position).sqrMagnitude / Mathf.Max(1, priority);

        int leastImportantIndex = -1;
        float leastImportanceValue = float.MaxValue;

        // Find an available audio source
        for (int i = 0; i < pool.Count; i++)
        {
            AudioPoolItem poolItem = pool[i];

            if (!poolItem.playing)
            {
                return ConfigurePoolObject(i, track, clip, position, volume, spatialBlend, unimportance);
            }
            else if (poolItem.unimportance > leastImportanceValue)
            {
                leastImportanceValue = poolItem.unimportance;
                leastImportantIndex = i;
            }
        }

        if (leastImportanceValue > unimportance)
        {
            return ConfigurePoolObject(leastImportantIndex, track, clip, position, volume, spatialBlend, unimportance);
        }

        return 0;
    }

    /// <summary>
    /// Queue a one shot sound to be played after a number of seconds
    /// </summary>
    public IEnumerator PlayOneShotSoundDelayed(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128)
    {
        yield return new WaitForSeconds(duration);
        PlayOneShotSound(track, clip, position, volume, spatialBlend, priority);
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

    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float unimportance)
    {
        if (poolIndex < 0 || poolIndex >= pool.Count)
            return 0;

        AudioPoolItem poolItem = pool[poolIndex];

        idGiver++;

        AudioSource source = poolItem.audioSource;
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;

        // Asiign to requested audio group
        source.outputAudioMixerGroup = tracks[track].group;

        // Position source at requested position
        source.transform.position = position;

        // Enable gameobject and record that it is being played
        poolItem.playing = true;
        poolItem.unimportance = unimportance;
        poolItem.id = idGiver;
        poolItem.gameObject.SetActive(true);
        source.Play();
        poolItem.coroutine = StopSoundDelayed(idGiver, source.clip.length);
        StartCoroutine(poolItem.coroutine);

        // Add this sound to our active pool with its unique id
        activePool[idGiver] = poolItem;

        return idGiver;
    }

    /// <summary>
    /// Stops a one shot sound from playing after a given time duration.
    /// </summary>
    protected IEnumerator StopSoundDelayed(ulong id, float duration)
    {
        yield return new WaitForSeconds(duration);

        AudioPoolItem activeSound;

        if (activePool.TryGetValue(id, out activeSound))
        {
            activeSound.audioSource.Stop();
            activeSound.audioSource.clip = null;
            activeSound.gameObject.SetActive(false);
            activePool.Remove(id);

            activeSound.playing = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        listenerPosition = FindObjectOfType<AudioListener>().transform;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!mixer) return;

        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup group in groups)
        {
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.name = group.name;
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
            poolItem.transform = gameObject.transform;
            poolItem.playing = false;
            gameObject.SetActive(false);
            pool.Add(poolItem);
        }
    }
}
