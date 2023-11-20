using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public int playerDead = 0;
    List<PlayerMover> _obituary;
    GameObject spawnPoint;
    public AudioManager _AudioManager;

    void Awake()
    {
        _obituary = new List<PlayerMover>();
        spawnPoint = GameObject.Find("Spawnpoint");
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _AudioManager = audioManagerGameObject.GetComponent<AudioManager>();
        _AudioManager?.PlayStageMusic();
    }
    
    public void DeathCount(PlayerMover player)
    {
        playerDead += 1;
        _obituary.Add(player);
    }

    public void DeathReducer(PlayerMover player)
    {
        playerDead -= 1;
        _obituary.Remove(player);
    }

    public bool DeathToll(int deathLimit)
    {
        if (playerDead == deathLimit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HearseService()
    {
        foreach (PlayerMover player in _obituary)
        {
            player.transform.position = spawnPoint.transform.position;
        }
    }


}
