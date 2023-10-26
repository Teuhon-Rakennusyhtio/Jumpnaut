using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float _lowestPoint = 0f, _highestPoint = 1000f;
    public static CameraMovement Instance;
    List<PlayerMover> _players;
    Vector3 _startPosition, _shake;
    float _maxShakeDuration, _currentShakeDuration;
    float _shakeIntensity, _shakeMagnitude, _shakeFallOff;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        _startPosition = transform.position;
        _players = new List<PlayerMover>();
    }

    void LateUpdate()
    {
        if (_players.Count == 0) return;

        
        Vector3 cameraPosition = CalculateCameraPosition();
        Vector3 shake = CalculateShake();
        
        transform.position = cameraPosition + shake;
    }

    Vector3 CalculateCameraPosition()
    {
        float cameraPosition = 0;
        foreach (PlayerMover player in _players)
        {
            cameraPosition += player.GetPlayerCameraPosition().y;
        }
        cameraPosition /= _players.Count;
        cameraPosition += 2f;
        if (cameraPosition < _lowestPoint) cameraPosition = _lowestPoint;
        else if (cameraPosition > _highestPoint) cameraPosition = _highestPoint;

        return Vector3.MoveTowards(transform.position, new Vector3(_startPosition.x, cameraPosition, _startPosition.z), Mathf.Abs(cameraPosition - transform.position.y) * 2f * Time.deltaTime);
    }

    Vector3 CalculateShake()
    {
        _currentShakeDuration -= Time.unscaledDeltaTime;
        if (_currentShakeDuration < 0f) _currentShakeDuration = 0f;
        float fallOff = 1; 
        

        if (_currentShakeDuration == 0f)
        {
            fallOff = 0f;
        }
        else
        {
            float fallOffStart = _maxShakeDuration * _shakeFallOff;
            if (fallOffStart > _currentShakeDuration)
                fallOff = _currentShakeDuration / (_maxShakeDuration + fallOffStart);
        }
        _shake = new Vector3(
                            Mathf.Sin(Time.unscaledTime * 16 * _shakeIntensity),
                            Mathf.Sin(Time.unscaledTime * 32 * _shakeIntensity) * 2,
                            0
                            ) * 0.02f * _shakeMagnitude * fallOff * GameManager.ShakeIntensity;

        return _shake;
    }

    public static void SetCameraShake(float intensity, float magnitude, float duration, float fallOff)
    {
        if (duration < 0f) duration = 0f;
        if (fallOff < 0f) fallOff = 1f;
        Instance._shakeIntensity = intensity;
        Instance._shakeMagnitude = magnitude;
        Instance._maxShakeDuration = duration;
        Instance._currentShakeDuration = duration;
        Instance._shakeFallOff = fallOff;
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
