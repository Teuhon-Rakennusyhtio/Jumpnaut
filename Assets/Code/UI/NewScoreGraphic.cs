using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewScoreGraphic : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeField] RectTransform _rectTransform;
    /*void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _text = GetComponent<TMP_Text>();
    }*/

    public void GotScore(int score)
    {
        SetScoreNumber(score);
        StartCoroutine(IEGotScore(score));
    }

    IEnumerator IEGotScore(int score)
    {
        float scale = 0f;
        float fadeOut = 1f;
        while (fadeOut > 0f)
        {
            fadeOut -= Time.deltaTime;
            if (fadeOut < 0f)
                fadeOut = 0f;
            _rectTransform.localPosition += Vector3.up * Time.deltaTime;
            scale += Time.deltaTime * 2;
            _rectTransform.localScale = Vector3.one * scale;
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, fadeOut);
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    void SetScoreNumber(int score)
    {
        string scoreToText = score.ToString();
        string scoreText = "";
        int numbersSinceSpace = 0;
        for (int i = scoreToText.Length - 1; i >= 0; i--)
        {
            scoreText = scoreText.Insert(0, $"{scoreToText[i]}");
            numbersSinceSpace++;
            if (numbersSinceSpace >= 3)
            {
                numbersSinceSpace = 0;
                scoreText = scoreText.Insert(0, " ");
            }
        }
        _text.text = scoreText;
    }
}
