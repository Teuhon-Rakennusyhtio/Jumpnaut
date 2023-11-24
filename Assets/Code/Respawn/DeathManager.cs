using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    private int playerDead = 0;
    private List<PlayerMover> _obituary;
    private GameObject spawnPoint;
    private AudioManager _audioManager;

    void Awake()
    {
        _obituary = new List<PlayerMover>();
        spawnPoint = GameObject.Find("Spawnpoint");
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
        _audioManager?.PlayStageMusic();
    }
    
    public void DeathCount(PlayerMover player)
    {
        playerDead += 1;
        _obituary.Add(player);
    }

    public void DeathReducer(PlayerMover player)
    {
        playerDead -= 1;
        if (playerDead < 0)
        {
            playerDead = 0;
        }
        _obituary.Remove(player);
    }

    public bool DeathToll(int deathLimit)
    {
        if (playerDead == deathLimit)
        {
            Debug.Log(playerDead);
            return true;
        }
        else
        {
            Debug.Log(playerDead);
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
