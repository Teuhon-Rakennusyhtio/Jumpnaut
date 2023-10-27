using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StarSkyGenerator : MonoBehaviour
{
    public void ArrangeStars()
    {
        Transform stars = transform.Find("Stars");
        Transform darkness = transform.Find("Darkness");
        if (stars == null || darkness == null) return;
        float spaceX = darkness.localScale.x / 2;
        float spaceY = darkness.localScale.y / 2;
        foreach (Transform child in stars)
        {
            child.localScale = Vector3.one * Random.Range(0.4f, 2f);
            child.localPosition = new Vector3(Random.Range(-spaceX, spaceX), Random.Range(-spaceY, spaceY), 0);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StarSkyGenerator))]
public class StarSkyGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        StarSkyGenerator generator = (StarSkyGenerator)target;
        if(GUILayout.Button("Arrange Stars"))
        {
            generator.ArrangeStars();
        }
    }
}
#endif