using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGenericSwitch : InteractiveItem
{
    [Header("Game State Management")]
    [SerializeField] protected List<GameState> requiredStates = new List<GameState>();
    [SerializeField] protected List<GameState> activateStates = new List<GameState>();
    [SerializeField] protected List<GameState> deactivateStates = new List<GameState>();

    [Header("Message")]
    [TextArea(3, 10)]
    [SerializeField] protected string stateNotSetText = string.Empty;
    [TextArea(3, 10)]
    [SerializeField] protected string stateSetText = string.Empty;
    [TextArea(3, 10)]
    [SerializeField] protected string objectActiveText = string.Empty;

    // Activation Sound
    [Header("Activation Parameters")]
    [SerializeField] protected float activationDelay = 1.0f;
    [SerializeField] protected float deactivationDelay = 1.0f;
    [SerializeField] protected AudioCollection activationSounds = null;
    [SerializeField] protected AudioSource audioSource = null;

    // Operation
    [Header("Operating Mode")]
    [SerializeField] protected bool startActivated = false;
    [SerializeField] protected bool canToggle = false;

    [Header("Configurable Entities")]
    // Inspector Assigned Animators that need to be affected by the trigger
    [SerializeField] protected List<AnimatorConfigurator> animations = new List<AnimatorConfigurator>();

    // Materials that need to have their emissive properties effected by the trigger
    [SerializeField] protected List<MaterialController> materialControllers = new List<MaterialController>();

    // GameObjects that need to be activated or deactivated by this trigger	 
    [SerializeField] protected List<GameObject> objectActivators = new List<GameObject>();
    [SerializeField] protected List<GameObject> objectDeactivators = new List<GameObject>();

    protected IEnumerator coroutine = null;
    protected bool activated = false;
    protected bool firstUse = false;

    /// <summary>
    /// Return different hint text depending on whether the object is currently able to be activated
    /// </summary>
    public override string GetText()
    {
        // If we have no application database or this switch is disabled then return null
        if (!enabled)
            return string.Empty;

        // If its already activated then just return the activated text
        if (activated)
        {
            return objectActiveText;
        }

        // We need to test all the states that need to be set to see if this item can be activated
        // as that determines the text we send back
        bool requiredStates = AreRequiredStatesSet();

        // Return the desired text to reflect whether ot not we can use the object yet
        if (!requiredStates)
        {
            return stateNotSetText;
        }
        else
        {
            return stateSetText;
        }
    }

    /// <summary>
    /// Activates the object
    /// </summary>
    public override void Activate(CharacterManager characterManager)
    {
        ApplicationManager appManager = ApplicationManager.Instance;

        if (appManager == null) return;

        // If we are already in a different state to the starting state and
        // we are not in toggle mode then this item has been switched on/ switched off
        // and can not longer be altered
        if (firstUse && !canToggle) return;

        if (!activated)
        {
            bool requiredStates = AreRequiredStatesSet();
            if (!requiredStates) return;
        }

        // Object state has been switched
        activated = !activated;
        firstUse = true;

        // PLay the activation Sound Effect
        if (activationSounds != null && activated)
        {
            AudioClip clipToPlay = activationSounds[0];

            if (clipToPlay == null) return;

            // If an audio source has been specified then use it
            if (audioSource != null && AudioManager.Instance)
            {
                audioSource.clip = clipToPlay;
                audioSource.volume = activationSounds.Volume;
                audioSource.spatialBlend = activationSounds.SpatialBlend;
                audioSource.priority = activationSounds.Priority;
                audioSource.outputAudioMixerGroup = AudioManager.Instance.GetAudioGroupFromTrackName(activationSounds.AudioGroup);
                audioSource.Play();
            }
        }

        // Get the coroutine to perform the delayed activation and if its playing stop it
        if (coroutine != null)
            StopCoroutine(coroutine);

        // Start a new corotuine to do the activation
        coroutine = DoDelayedActivation();
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Registers this objects collider with the Activation Database
    /// </summary>
    protected override void Start()
    {
        // Call the base class to register the scene with the app database
        base.Start();

        // Activate Material Controller
        for (int i = 0; i < materialControllers.Count; i++)
        {

            if (materialControllers[i] != null)
            {
                materialControllers[i].OnStart();
            }
        }

        // Turn off all objects that should be activated
        for (int i = 0; i < objectActivators.Count; i++)
        {
            if (objectActivators[i] != null)
                objectActivators[i].SetActive(false);
        }

        for (int i = 0; i < objectDeactivators.Count; i++)
        {
            if (objectDeactivators[i] != null)
                objectDeactivators[i].SetActive(true);
        }

        if (startActivated)
        {
            Activate(null);
            firstUse = false;
        }
    }

    protected void SetActivationStates()
    {
        ApplicationManager appManager = ApplicationManager.Instance;
        if (appManager == null) return;

        if (activated)
        {
            foreach (GameState state in activateStates)
            {
                appManager.SetGameState(state.Key, state.Value);
            }
        }
        else
        {
            foreach (GameState state in deactivateStates)
            {
                appManager.SetGameState(state.Key, state.Value);
            }
        }
    }

    protected bool AreRequiredStatesSet()
    {
        ApplicationManager appManager = ApplicationManager.Instance;

        if (appManager == null)
            return false;

        // Assume the states are all set and then loop to find a state to disprove this
        for (int i = 0; i < requiredStates.Count; i++)
        {
            GameState state = requiredStates[i];

            // Does the current state exist in the app dictionary?
            string result = appManager.GetGameState(state.Key);

            if (string.IsNullOrEmpty(result) || !result.Equals(state.Value))
                return false;
        }

        return true;
    }

    protected virtual IEnumerator DoDelayedActivation()
    {
        // Now perform the derived class animator stuff
        foreach (AnimatorConfigurator configurator in animations)
        {
            if (configurator != null)
            {
                foreach (AnimatorParameter param in configurator.animatorParams)
                {
                    switch (param.type)
                    {
                        case AnimatorParameterType.Bool:
                            bool boolean = bool.Parse(param.value);
                            configurator.animator.SetBool(param.name, activated ? boolean : !boolean);
                            break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(activated ? activationDelay : deactivationDelay);

        // Set the states that should be set when activating / deactivating
        SetActivationStates();

        if (activationSounds != null && !activated)
        {
            AudioClip clipToPlay = activationSounds[1];

            // If an audio source has been specified then use it. This is good for playing looping sounds
            // or sounds that need to happen nowhere near the tigger source
            if (audioSource != null && clipToPlay && AudioManager.Instance)
            {
                audioSource.clip = clipToPlay;
                audioSource.volume = activationSounds.Volume;
                audioSource.spatialBlend = activationSounds.SpatialBlend;

                audioSource.outputAudioMixerGroup = AudioManager.Instance.GetAudioGroupFromTrackName(activationSounds.AudioGroup);
                audioSource.Play();
            }
        }

        // If we get here then we are allow to enable this object
        // so first turn on any game objects that should be made active by this 
        // action
        if (objectActivators.Count > 0)
        {
            for (int i = 0; i < objectActivators.Count; i++)
            {
                if (objectActivators[i]) objectActivators[i].SetActive(activated);
            }
        }

        // Turn off any game objects that should be disabled by this action
        if (objectDeactivators.Count > 0)
        {
            for (int i = 0; i < objectDeactivators.Count; i++)
            {
                if (objectDeactivators[i]) objectDeactivators[i].SetActive(!activated);
            }
        }

        // Activate Material Controller
        for (int i = 0; i < materialControllers.Count; i++)
        {

            if (materialControllers[i] != null)
            {
                materialControllers[i].Activate(activated);
            }
        }
    }
}
