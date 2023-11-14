using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BatteryCollidedWithHurtbox : MonoBehaviour
{
    [SerializeField] UnityEvent _collided;
    public WeaponAlignment Alignment { get; set; }
    void Start()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!this.enabled) return;
        GenericHealth targetHealth = collider.GetComponent<GenericHealth>();
        if (targetHealth != null && targetHealth.Alignment != Alignment && !targetHealth.InvincibleToCatchable)
        {
            _collided.Invoke();
        }
    }
}
