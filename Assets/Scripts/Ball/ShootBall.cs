using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    public Transform ShootAt;
    public Vector2 speed = new Vector2(50, 100);
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void ShootTarget()
    {
        Vector3 dir = ShootAt.position - transform.position;
        _rb.AddForce(dir.normalized  * UnityEngine.Random.Range(speed.x, speed.y), ForceMode.VelocityChange);
    }
}
