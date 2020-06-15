using UnityEngine;
using UnityEngine.UI;

public class LockdownTrigger : MonoBehaviour
{
	[SerializeField] protected float downloadTime = 10.0f;
	[SerializeField] protected Slider downloadBar = null;
	[SerializeField] protected Text hintText = null;
	[SerializeField] protected MaterialController materialController = null;
	[SerializeField] protected GameObject lockedLight = null;
	[SerializeField] protected GameObject unlockedLight = null;

	private ApplicationManager applicationManager = null;
	private GameSceneManager gameSceneManager = null;
	private bool inTrigger = false;
	private float downloadProgress = 0.0f;
	private AudioSource audioSource = null;
	private bool downloadComplete = false;

	void OnEnable()
	{
		// Fetch application Database, Game Scene Manager and the AudioSource Component
		applicationManager = ApplicationManager.Instance;
		audioSource = GetComponent<AudioSource>();

		// Reset Download Progress
		downloadProgress = 0.0f;

		// Instruct the material controller to register itself with the gamescene
		// manager so that it can be restored on exit.
		if (materialController != null)
			materialController.OnStart();

		// If we have an app database
		if (applicationManager != null)
		{
			// Get the state of Lockdown if it exists
			string lockedDown = applicationManager.GetGameState("LOCKDOWN");

			if (string.IsNullOrEmpty(lockedDown) || lockedDown.Equals("TRUE"))
			{
				if (materialController != null) 
					materialController.Activate(false);

				if (unlockedLight) 
					unlockedLight.SetActive(false);

				if (lockedLight) 
					lockedLight.SetActive(true);

				downloadComplete = false;
			}
			else
			if (lockedDown.Equals("FALSE"))
			{
				if (materialController != null) 
					materialController.Activate(true);

				if (unlockedLight) 
					unlockedLight.SetActive(true);

				if (lockedLight) 
					lockedLight.SetActive(false);

				downloadComplete = true;
			}
		}

		// Set all UI Elements to starting condition
		ResetSoundAndUI();
	}

	void Update()
	{
		// This computer has already been emptied of information
		if (downloadComplete) return;

		// Are we in the trigger
		if (inTrigger)
		{
			// and are we holding down the activation key
			if (Input.GetButton("Use"))
			{
				// Play the downloading sound
				if (audioSource && !audioSource.isPlaying)
					audioSource.Play();

				// Increase with delta time clamping to max time
				downloadProgress = Mathf.Clamp(downloadProgress + Time.deltaTime, 0.0f, downloadTime);

				// If download is not fully complete then update UI to reflect download progress
				if (downloadProgress != downloadTime)
				{
					if (downloadBar)
					{
						downloadBar.gameObject.SetActive(true);
						downloadBar.value = downloadProgress / downloadTime;
					}
					return;
				}
				else
				{
					// Otherwise download is complete
					downloadComplete = true;

					// Turn off UI elements
					ResetSoundAndUI();

					// Change Hint Text to show success
					if (hintText) hintText.text = "Successful Deactivation";

					// Shutdown lockdown
					applicationManager.SetGameState("LOCKDOWN", "FALSE");

					// Swap texture Over
					if (materialController != null) 
						materialController.Activate(true);

					if (unlockedLight) 
						unlockedLight.SetActive(true);

					if (lockedLight) 
						lockedLight.SetActive(false);

					return;
				}
			}
		}

		// Reset UI and Sound
		downloadProgress = 0.0f;
		ResetSoundAndUI();
	}

	/// <summary>
	/// Stops any audio playing, resets download progress and hide UI elements
	/// </summary>
	void ResetSoundAndUI()
	{
		if (audioSource && audioSource.isPlaying) 
			audioSource.Stop();

		if (downloadBar)
		{
			downloadBar.value = downloadProgress;
			downloadBar.gameObject.SetActive(false);
		}

		if (hintText)
			hintText.text = "Hold 'Use' Button to Deactivate";
	}

	void OnTriggerEnter(Collider other)
	{
		if (inTrigger || downloadComplete) 
			return;

		if (other.CompareTag("Player")) 
			inTrigger = true;
	}

	void OnTriggerExit(Collider other)
	{
		if (downloadComplete) 
			return;

		if (other.CompareTag("Player")) 
			inTrigger = false;
	}
}
