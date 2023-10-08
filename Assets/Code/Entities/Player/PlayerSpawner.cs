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
            GameObject player = Instantiate(_playerPrefab, transform.position + new Vector3(i % 4, 0, 0), Quaternion.identity);
            //player.GetComponent<SpriteRenderer>().color = GameManager.GetPlayerColor(i);
            SpriteRenderer[] sprites = player.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.color = GameManager.GetPlayerColor(i);
            }
            player.GetComponent<PlayerMover>().Device = GameManager.PlayerDevices[i];
            player.GetComponent<PlayerMover>().Id = i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
