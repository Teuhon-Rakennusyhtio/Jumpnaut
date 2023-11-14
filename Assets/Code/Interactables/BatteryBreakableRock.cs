using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryBreakableRock : MonoBehaviour, IBatteryDamageable
{
    SpriteRenderer _forceFieldSprite;
    bool _broken = false;
    BreakageDebris[] _debris;
    Color _startColour;
    void Start()
    {
        _forceFieldSprite = GetComponent<SpriteRenderer>();
        _debris = GetComponentsInChildren<BreakageDebris>();
        _startColour = _forceFieldSprite.color;
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
        while (blinkingTimer > 0f)
        {
            blinkingTimer -= Time.deltaTime;
            if (blinkingTimer < 0f)
                blinkingTimer = 0f;
            _forceFieldSprite.color = _startColour * (Mathf.Sin(blinkingTimer * 30f) / 2 + 0.5f) * blinkingTimer; // new Color(_startColour.r, _startColour.g, _startColour.b, (Mathf.Sin(blinkingTimer * 30f) / 2 + 0.5f) * blinkingTimer);
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
