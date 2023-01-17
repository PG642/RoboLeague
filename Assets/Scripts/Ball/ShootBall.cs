using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    public Transform ShootAt;
    private Vector2 speed = new Vector2(20, 40);
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void ShootTarget()
    {
        Vector3 dir = ShootAt.position - transform.position;
        _rb.velocity = dir.normalized  * UnityEngine.Random.Range(speed.x, speed.y);
    }
}
