using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeR4X4 : MonoBehaviour
{
    [SerializeField] Transform _healthBar, _health;
    [SerializeField] Image _winnerImage;
    Vector3 _healthBarTargetPos;
    bool _doTheThingWithHealthBar = false;
    Rigidbody2D _rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _healthBarTargetPos = _healthBar.localPosition;
        _healthBar.localPosition += Vector3.left * 50f; 
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
        _health.localPosition = Vector2.left * 500f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.AddForce(Vector2.one * 20f, ForceMode2D.Impulse);
        _rigidbody.AddTorque(-15f, ForceMode2D.Impulse);
        StartCoroutine(Winner());
    }

    IEnumerator Winner()
    {
        yield return new WaitForSeconds(1.5f);
        float opacity = 0f;
        while (opacity < 1f)
        {
            opacity += Time.deltaTime / 2;
            if (opacity > 1f)
                opacity = 1f;
            _winnerImage.color = new Color(1, 1, 1, opacity);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(20f);
        GameManager.ReturnToMainMenu();
    }
}
