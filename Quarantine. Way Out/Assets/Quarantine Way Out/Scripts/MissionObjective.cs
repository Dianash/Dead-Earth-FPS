﻿using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (GameSceneManager.Instance)
        {
            PlayerInfo playerInfo = GameSceneManager.Instance.GetPlayerInfo(col.GetInstanceID());

            if (playerInfo != null)
                playerInfo.characterManager.DoLevelComplete();
        }
    }
}
