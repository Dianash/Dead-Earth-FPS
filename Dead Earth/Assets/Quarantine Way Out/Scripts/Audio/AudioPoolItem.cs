using System.Collections;
using UnityEngine;

public class AudioPoolItem
{
    public GameObject gameObject = null;
    public Transform transform = null;
    public AudioSource audioSource = null;
    public IEnumerator coroutine = null;

    public float unimportance = float.MaxValue;
    public bool playing = false;
    public ulong id = 0;
}
