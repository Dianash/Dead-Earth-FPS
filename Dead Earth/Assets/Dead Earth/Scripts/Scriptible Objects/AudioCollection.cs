using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Collection")]
public class AudioCollection : ScriptableObject
{
    [SerializeField] string audioGroup = string.Empty;
    [SerializeField] [Range(0.0f, 1.0f)] float volume = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float spatialBlend = 1.0f;
    [SerializeField] [Range(0, 256)] int priority = 128;
}
