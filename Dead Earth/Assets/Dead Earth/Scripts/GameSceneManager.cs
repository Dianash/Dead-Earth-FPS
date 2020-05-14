using UnityEngine;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] ParticleSystem bloodParticle = null;

    private static GameSceneManager instance = null;

    public ParticleSystem BloodParticles
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
    private Dictionary<int, PlayerInfo> playerInfos = new Dictionary<int, PlayerInfo>();

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

    /// <summary>
    /// Stores the passed PlayerInfo in the dictionary with the supplied key
    /// </summary>
    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)
    {
        if (!playerInfos.ContainsKey(key))
        {
            playerInfos[key] = playerInfo;
        }
    }

    /// <summary>
    /// Returns a PlayerInfo reference searched on by the instance ID of an object
    /// </summary>
    /// <param name="key">AI State Machine instance ID</param>
    public PlayerInfo GetPlayerInfo(int key)
    {
        PlayerInfo info = null;

        if (playerInfos.TryGetValue(key, out info))
        {
            return info;
        }

        return null;
    }
}
