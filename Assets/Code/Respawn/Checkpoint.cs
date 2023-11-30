using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameObject[] enemies;
    GameObject[] platforms;
    AudioManager _audioManager;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        platforms = GameObject.FindGameObjectsWithTag("MovingPlatform");
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            _audioManager?.PlaySFX(_audioManager.checkpoint);
            if (gameObject.name != "Checkpoint 3.5")
            {
                DespawnEnemies();
                DespawnMovingPlatforms();
            }
            GetComponent<Collider2D>().enabled = false;
        }
    }

    void DespawnEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].activeSelf && this.transform.position.y > enemies[i].transform.position.y + 15f)
            {
                enemies[i].SetActive(false);
            }
        }
    }

    void DespawnMovingPlatforms()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null && platforms[i].activeSelf && this.transform.position.y > platforms[i].transform.position.y + 15f)
            {
                platforms[i].SetActive(false);
            }
        }
    }
}
