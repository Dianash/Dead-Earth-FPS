using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Collection")]
public class AudioCollection : ScriptableObject
{
    [SerializeField] string audioGroup = string.Empty;
    [SerializeField] [Range(0.0f, 1.0f)] float volume = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float spatialBlend = 1.0f;
    [SerializeField] [Range(0, 256)] int priority = 128;
    [SerializeField] List<ClipBank> audioClipBanks = new List<ClipBank>();

    public string AudioGroup { get => audioGroup; }
    public float Volume { get => volume; }
    public float SpatialBlend { get => spatialBlend; }
    public int Priority { get => priority; }
    public int BankCount { get => audioClipBanks.Count; }

    /// <summary>
    /// Fetches a random audio clip from the bank
    /// </summary>
    public AudioClip this[int i]
    {
        get
        {
            if (audioClipBanks == null || audioClipBanks.Count <= i)
                return null;

            if (audioClipBanks[i].clips.Count == 0)
                return null;

            // Fetch the ClipBank to sample from
            List<AudioClip> clipList = audioClipBanks[i].clips;

            // Select random clip from the bank
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];

            return clip;
        }
    }

    /// <summary>
    /// Returns a random audio clip from the 1st clip bank;
    /// </summary>
    public AudioClip AudioClip
    {
        get
        {
            if (audioClipBanks == null || audioClipBanks.Count == 0) return null;
            if (audioClipBanks[0].clips.Count == 0) return null;

            List<AudioClip> clipList = audioClipBanks[0].clips;
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];
            return clip;
        }
    }
}
