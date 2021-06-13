﻿using System;
using System.Diagnostics;
using Consolation;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField] [Range(10,80)] float randomSpeed = 40;
    [SerializeField] float initialForce = 400;
    [SerializeField] float hitMultiplier = 50;
    [SerializeField] private float _maxAngluarVelocity = 6.0f;
    [SerializeField] private float _maxVelocity = 60.0f;
    private bool isTouchedGround = false;
    
    Rigidbody _rb;
    Transform _transform;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _transform = this.transform;
        isTouchedGround = false;
        _rb.maxAngularVelocity = _maxAngluarVelocity;
        _rb.maxDepenetrationVelocity = _maxVelocity;
        
    }
    
    void Update()
    {
        //TODO: move inputs to the InputController
        if (Input.GetKeyDown(KeyCode.T))
            ShootInRandomDirection(randomSpeed);
        
        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();
        
        if (Input.GetButtonDown("Select"))
            ResetShot(new Vector3(7.76f, 2.98f, 0f));
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxVelocity);
    }

    private void FixedUpdate()
    {
        if (_rb.velocity.magnitude > _maxVelocity)
        {
            _rb.velocity = _rb.velocity.normalized * _maxVelocity;
        }
    }

    private void ResetShot(Vector3 pos)
    {
        _transform.position = pos;
        _rb.velocity = new Vector3(30, 10, 0);
        _rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ResetBall")]
    private void ResetBall()
    {
        var desired = new Vector3(0, 12.23f, 0f);
        _transform.SetPositionAndRotation(desired, Quaternion.identity);
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ShootInRandomDirection")]
    private void ShootInRandomDirection(float speed)
    {
        float speedRange = Random.Range(speed - 10, speed + 10);
        var randomDirection = Random.insideUnitCircle.normalized;
        var direction = new Vector3(randomDirection.x, Random.Range(-0.5f, 0.5f), randomDirection.y).normalized;
        _rb.velocity = direction * speedRange;
    }

    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.CompareTag("Player"))
        {
            float force = initialForce + col.rigidbody.velocity.magnitude * hitMultiplier;
            //Vector3 dir = transform.position - col.contacts[0].point;
            var dir = transform.position - col.transform.position;
            _rb.AddForce(dir.normalized * force);
        }


        if (col.gameObject.CompareTag("Ground"))
            isTouchedGround = true;

        //if (col.gameObject.tag == "Ground")
        //    if (rb.velocity.y > 3)
        //    {
        //    //rb.AddForce(Vector3.up * -downForce);
        //        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - SlowVelocityGround, rb.velocity.z);
        //    }
    }
}
