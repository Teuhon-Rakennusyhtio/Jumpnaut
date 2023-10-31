using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakageDebris : MonoBehaviour
{
    [SerializeField] GameObject _particleEffect;
    [SerializeField] float _force = 10f;
    bool _alreadyHitTheGround;
    SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Break(float angle)
    {
        transform.parent = null;
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Rigidbody2D>().simulated = true;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _force, ForceMode2D.Impulse);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_alreadyHitTheGround && collision.gameObject.layer == 6)
        {
            _alreadyHitTheGround = true;
            StartCoroutine(HitTheGround());
        }
    }

    IEnumerator HitTheGround()
    {
        GameObject effect = Instantiate(_particleEffect, transform.position, Quaternion.identity);
        ParticleSystem effectParticle = effect.GetComponent<ParticleSystem>();
        Color startColour = _spriteRenderer.color;
        Color newColour = new Color(startColour.r, startColour.g, startColour.b, 0f);
        float fadeDuration = 1f;
        float particleDuration = 0.5f;
        if (effectParticle != null)
        {
            if (!effectParticle.isPlaying) effectParticle.Play();
            particleDuration = effectParticle.main.duration;
        }
        particleDuration -= fadeDuration;
        if (particleDuration < 0f) particleDuration = 0f;

        while(fadeDuration > 0f)
        {
            fadeDuration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            if (fadeDuration < 0f) fadeDuration = 0f;
            _spriteRenderer.color = Color.Lerp(newColour, startColour, fadeDuration);
        }

        yield return new WaitForSeconds(particleDuration);
        Destroy(effect);
        Destroy(gameObject);
    }
}
