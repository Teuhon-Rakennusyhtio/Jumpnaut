using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryBreakableRock : MonoBehaviour, IBatteryDamageable
{
    [SerializeField] SpriteRenderer[] _forceFieldSprites;
    [SerializeField] ParticleSystem[] _particles;
    Color[] _forceFieldSpriteColours;
    bool _broken = false;
    BreakageDebris[] _debris;
    Color _startColour;
    AudioManager _audioManager;

    void Start()
    {
        //_forceFieldSprite = GetComponent<SpriteRenderer>();
        _debris = GetComponentsInChildren<BreakageDebris>();
        //_startColour = _forceFieldSprite.color;
        _forceFieldSpriteColours = new Color[_forceFieldSprites.Length];
        for (int i = 0; i < _forceFieldSpriteColours.Length; i++)
        {
            _forceFieldSpriteColours[i] = _forceFieldSprites[i].color;
        }
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    public void HitByBattery()
    {
        if (!_broken)
        {
            _broken = true;
            StartCoroutine(Break());
        }
    }

    IEnumerator Break()
    {
        float blinkingTimer = 1f;
        _audioManager?.PlaySFX(_audioManager.pilarBreak);
        while (blinkingTimer > 0f)
        {
            blinkingTimer -= Time.deltaTime;
            if (blinkingTimer < 0f)
                blinkingTimer = 0f;
            float colourMultiplier = (Mathf.Sin(blinkingTimer * 30f) / 2 + 0.5f) * blinkingTimer;
            for (int i = 0; i < _forceFieldSprites.Length; i++)
            {
                _forceFieldSprites[i].color = _forceFieldSpriteColours[i] * colourMultiplier;
            }
            for (int i = 0; i < _particles.Length; i++)
            {
                if (colourMultiplier < 0.5f)
                    _particles[i].Stop();
                else
                    _particles[i].Play();
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.2f);
        GetComponent<Collider2D>().enabled = false;
        foreach (BreakageDebris pieceOfDebris in _debris)
        {
            pieceOfDebris.Break(0f);
            yield return new WaitForSeconds(Random.Range(0.01f, 0.02f));
        }
    }
}
