using System.Collections;
using UnityEngine;

public class InteractiveKeypad : InteractiveItem
{
    [SerializeField] protected Transform elevator = null;
    [SerializeField] protected AudioCollection collection = null;
    [SerializeField] protected int bank = 0;
    [SerializeField] protected float activationDelay = 0.0f;

    bool isActivated = false;

    public override string GetText()
    {
        ApplicationManager appDatabase = ApplicationManager.Instance;

        if (!appDatabase)
            return string.Empty;

        string powerState = appDatabase.GetGameState("POWER");
        string lockdownState = appDatabase.GetGameState("LOCKDOWN");
        string accessCodeState = appDatabase.GetGameState("ACCESSCODE");

        // If we have not turned on the power
        if (string.IsNullOrEmpty(powerState) || !powerState.Equals("TRUE"))
        {
            return "Keypad : No Power";
        }
        else
        // Or we have not deactivated lockdown
        if (string.IsNullOrEmpty(lockdownState) || !lockdownState.Equals("FALSE"))
        {
            return "Keypad : Under Lockdown";
        }
        else
        // or we don't have the access code yet
        if (string.IsNullOrEmpty(accessCodeState) || !accessCodeState.Equals("TRUE"))
        {
            return "Keypad : Access Code Required";
        }

        // We have everything we need
        return "Keypad";
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (isActivated)
            return;

        ApplicationManager appDatabase = ApplicationManager.Instance;

        if (!appDatabase)
            return;

        string powerState = appDatabase.GetGameState("POWER");
        string lockdownState = appDatabase.GetGameState("LOCKDOWN");
        string accessCodeState = appDatabase.GetGameState("ACCESSCODE");

        if (string.IsNullOrEmpty(powerState) || !powerState.Equals("TRUE"))
            return;

        if (string.IsNullOrEmpty(lockdownState) || !lockdownState.Equals("FALSE"))
            return;

        if (string.IsNullOrEmpty(accessCodeState) || !accessCodeState.Equals("TRUE"))
            return;

        //Delay the actual animation for the desired number of seconds
        StartCoroutine(DoDelayedActivation(characterManager));

        isActivated = true;
    }

    protected IEnumerator DoDelayedActivation(CharacterManager characterManager)
    {
        if (!elevator) yield break;

        // Play the sound
        if (collection != null)
        {
            AudioClip clip = collection[bank];
            if (clip)
            {
                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlayOneShotSound(collection.AudioGroup, clip, elevator.position, collection.Volume,
                        collection.SpatialBlend, collection.Priority);
                }
            }
        }

        // Wait for the desired delay
        yield return new WaitForSeconds(activationDelay);

        if (characterManager != null)
        {
            // Make it a child of the elevator
            characterManager.transform.parent = elevator;

            // Get the animator of the elevator and activate it animation
            Animator animator = elevator.GetComponent<Animator>();

            if (animator) 
                animator.SetTrigger("Activate");

            // Freeze the FPS motor so it can rotate/jump/croach but can
            // not move off of the elevator.
            if (characterManager.FPSController)
            {
                characterManager.FPSController.FreezeMovement = true;
            }
        }
    }
}
