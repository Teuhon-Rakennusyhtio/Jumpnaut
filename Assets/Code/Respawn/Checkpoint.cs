using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameObject spawnPoint;
    GameObject[] enemies;
    GameObject[] platforms;
    GameObject[] previousAreaLock;
    AudioManager _audioManager;
    private bool checkpointActivated = false;

    void Start()
    {
        spawnPoint = GameObject.Find("Spawnpoint");
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        platforms = GameObject.FindGameObjectsWithTag("MovingPlatform");
        previousAreaLock = GameObject.FindGameObjectsWithTag("Lock");
        for (int i = 0; i < previousAreaLock.Length; i++)
        {
            previousAreaLock[i].SetActive(false);
        }

        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    void Update()
    {
        LockArea();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!checkpointActivated && col.gameObject.tag == "Player")
        {
            _audioManager?.PlaySFX(_audioManager.checkpoint);
            if (gameObject.name != "Checkpoint 3.5" || gameObject.name != "Checkpoint 4")
            {
                DespawnEnemies();
                DespawnMovingPlatforms();
            }
            checkpointActivated = true;
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

    void LockArea()
    {
        for (int i = 0; i < previousAreaLock.Length; i++)
        {
            if (spawnPoint.transform.position.y > previousAreaLock[i].transform.position.y)
            {
                previousAreaLock[i].SetActive(true);
            }
        }
    }
}
