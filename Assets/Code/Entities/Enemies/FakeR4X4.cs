using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FakeR4X4 : MonoBehaviour
{
    [SerializeField] Transform _healthBar, _health;
    [SerializeField] Image _winnerImage;
    Vector3 _healthBarTargetPos;
    bool _doTheThingWithHealthBar = false;
    Rigidbody2D _rigidbody;
    int pointWorth = 10;

    SpeedRunTimer _timer;
    AudioManager _audioManager;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _healthBarTargetPos = _healthBar.localPosition;
        _healthBar.localPosition += Vector3.left * 50f; 
        _timer = GameObject.FindObjectOfType<SpeedRunTimer>();
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_doTheThingWithHealthBar)
        {
            _healthBar.localPosition = Vector3.MoveTowards(_healthBar.localPosition, _healthBarTargetPos, 30 * Time.deltaTime);
            if (_healthBar.localPosition == _healthBarTargetPos)
            {
                _health.localPosition = Vector3.MoveTowards(_health.localPosition, Vector3.zero, Time.deltaTime);
            }
        }
    }

    public void EnterHealthBar()
    {
        _doTheThingWithHealthBar = true;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Bye bye!");
        _audioManager?.PlaySFX(_audioManager.r4x4Death);
        GameManager.AddScore(pointWorth, transform.position);
        _health.localPosition = Vector2.left * 500f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.AddForce(Vector2.one * 20f, ForceMode2D.Impulse);
        _rigidbody.AddTorque(-15f, ForceMode2D.Impulse);
        _timer.StopTimer();
        collider.enabled = false;
        StartCoroutine(Winner());
    }

    IEnumerator Winner()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("WinScene", LoadSceneMode.Single);
        /*float opacity = 0f;
        while (opacity < 1f)
        {
            opacity += Time.deltaTime / 2;
            if (opacity > 1f)
                opacity = 1f;
            _winnerImage.color = new Color(1, 1, 1, opacity);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(20f);
        GameManager.ReturnToMainMenu();*/
    }
}
