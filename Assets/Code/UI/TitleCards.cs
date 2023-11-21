using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleCards : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(4f);
        float logoOpacity = 1f;
        Image[] images = GetComponentsInChildren<Image>();
        while (logoOpacity > 0f)
        {
            logoOpacity -= Time.unscaledDeltaTime;
            if (logoOpacity < 0f)
                logoOpacity = 0f;
            foreach (Image image in images)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, logoOpacity);
            }
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
