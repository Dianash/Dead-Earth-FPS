using UnityEngine;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] ParticleSystem bloodParticle = null;

    private static GameSceneManager instance = null;

    public ParticleSystem BloodParticle
    {
        get => bloodParticle;
    }

    public static GameSceneManager Instance
    {
        get
        {
            if (instance == null)
                instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            return instance;
        }
    }

    private Dictionary<int, AIStateMachine> stateMachine = new Dictionary<int, AIStateMachine>();

    /// <summary>
    /// Stores the passed state machine in the dictionary with the supplied key
    /// </summary>
    public void RegisterAIStateMachine(int key, AIStateMachine machine)
    {
        if (!stateMachine.ContainsKey(key))
            stateMachine[key] = machine;
    }

    /// <summary>
    /// Returns an AI State Machine reference searched on by the instance ID of an object
    /// </summary>
    /// <param name="key">AI State Machine instance ID</param>
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;

        if (stateMachine.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }
}
