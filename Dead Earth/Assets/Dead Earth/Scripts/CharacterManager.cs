using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera cam = null;
    [SerializeField] private float health = 100.0f;

    private Collider coll = null;
    private FPSController fPSController = null;
    private CharacterController characterController = null;
    private GameSceneManager gameSceneManager = null;

    private void Start()
    {
        coll = GetComponent<Collider>();
        fPSController = GetComponent<FPSController>();
        characterController = GetComponent<CharacterController>();
        gameSceneManager = GameSceneManager.Instance;

        if (gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = cam;
            info.characterManager = this;
            info.collider = coll;
            info.meleeTrigger = meleeTrigger;

            gameSceneManager.RegisterPlayerInfo(coll.GetInstanceID(), info);
        }
    }

    internal void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0.0f);

        if (cameraBloodEffect != null)
        {
            cameraBloodEffect.MinBloodAmount = 1.0f - ((health / 100.0f) / 3.0f);
            cameraBloodEffect.BloodAmount = Mathf.Min(cameraBloodEffect.MinBloodAmount + 0.3f, 1.0f);
        }
    }
}
