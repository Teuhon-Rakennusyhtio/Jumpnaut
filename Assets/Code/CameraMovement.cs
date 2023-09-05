using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float _lowestPoint = 0f, _highestPoint = 1000f;
    List<PlayerMover> _players;
    Vector3 _startPosition;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        _startPosition = transform.position;
        _players = new List<PlayerMover>();
    }

    void LateUpdate()
    {
        if (_players.Count == 0) return;

        float cameraPosition = 0;
        foreach (PlayerMover player in _players)
        {
            cameraPosition += player.GetPlayerCameraPosition().y;
        }
        cameraPosition /= _players.Count;
        cameraPosition += 2f;
        if (cameraPosition < _lowestPoint) cameraPosition = _lowestPoint;
        else if (cameraPosition > _highestPoint) cameraPosition = _highestPoint;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(_startPosition.x, cameraPosition, _startPosition.z), Mathf.Abs(cameraPosition - transform.position.y) * 2f * Time.deltaTime);
    }

    public void AddPlayer(PlayerMover player)
    {
        _players.Add(player);
    }

    public void RemovePlayer(PlayerMover player)
    {
        _players.Add(player);
    }

    public void ReturnToMainMenu()
    {
        transform.position =_startPosition;
        _players.Clear();
    }
}
