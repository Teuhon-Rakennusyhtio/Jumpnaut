using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] CutsceneMovement[] _spawnCutscene;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameManager.PlayerDevices.Count; i++)
        {
            GameObject player = Instantiate(_playerPrefab, transform.position + new Vector3(i % 4, 0, 0), Quaternion.identity);
            player.GetComponent<PlayerMover>().AssignPlayer(GameManager.PlayerDevices[i], i);
            if (transform.position.y < 10f)
            {
                player.GetComponent<PlayerMover>().PlayCutscene(_spawnCutscene);
                Invoke(nameof(EnablePipeCollider), 1.1f);
            }
        }
    }

    void EnablePipeCollider()
    {
        Collider2D pipeCollider = GameObject.Find("StartCutscenePipeCollider").GetComponent<Collider2D>();
        if (pipeCollider != null)
            pipeCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
