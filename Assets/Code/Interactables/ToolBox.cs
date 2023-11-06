using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolBox : MonoBehaviour
{
    [SerializeField] GameObject _holdable;
    void Start()
    {
        SpawnNewHoldable();
    }

    public void SpawnNewHoldable()
    {
        GameObject newHoldableObject = Instantiate(_holdable, transform.position, Quaternion.identity);
        Holdable newHoldable = newHoldableObject.GetComponent<Holdable>();
        if (newHoldable != null)
        {
            newHoldable.ToolBox = this;
        }
        else
        {
            Debug.LogError($"{newHoldableObject.name} is not a holdable item and should not be used with a tool box!");
        }
    }
}
