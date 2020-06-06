using UnityEngine;

public class AudioLayer : MonoBehaviour
{
	public AudioClip clip = null;
	public AudioCollection collection = null;
	public int bank = 0;
	public bool looping = true;
	public float time = 0.0f;
	public float duration = 0.0f;
	public bool muted = false;
}
