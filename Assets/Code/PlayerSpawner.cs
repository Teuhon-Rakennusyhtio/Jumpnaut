using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameManager.PlayerDevices.Count; i++)
        {
            GameObject player = Instantiate(_playerPrefab, new Vector3(i, 0, 0), Quaternion.identity);
            player.GetComponent<SpriteRenderer>().color = GameManager.GetPlayerColor(i);
            player.GetComponent<PlayerMover>().SetPlayerDevice(GameManager.PlayerDevices[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
