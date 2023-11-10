using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThingsAtCameraEdges : MonoBehaviour
{
    [SerializeField] bool _leftEdge;
    [SerializeField] float _offset;
    [SerializeField] float _clamp = float.NegativeInfinity;
    // Start is called before the first frame update
    void Start()
    {
        ScreenSizeChanged();
    }

    public void ScreenSizeChanged()
    {
        float screenEdge = Mathf.Max((Camera.main.aspect * Camera.main.orthographicSize + _offset), _clamp) * (_leftEdge ? -1 : 1);
        transform.position = new Vector3(screenEdge, transform.position.y, transform.position.z);
    }
}
