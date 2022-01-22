﻿using UnityEditor;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    public bool isTouchingSurface = false;

    //Raycast options
    float _rayLen, _rayOffset = 0f;
    Vector3 _rayContactPoint, _rayContactNormal;

    bool _isColliderContact;
    WheelSuspension _ws;

    Rigidbody _rb;

    SuspensionCollider _sc;

    public int groundedTriggers = 0;

    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _ws = GetComponentInParent<WheelSuspension>();
        _sc = _ws.suspensionCollider.GetComponent<SuspensionCollider>();
        groundedTriggers = 0;
        _rayLen = _ws.radius + _rayOffset;
    }

    private void FixedUpdate()
    {
        isTouchingSurface = _isColliderContact;

        //TODO: this class should only do raycasts and sphere collider ground detection. Move to CubeWheel or CubeController
        if (isTouchingSurface) {
            ApplyStickyForces(StickyForceConstant * 5, _rayContactPoint, -_rayContactNormal);
        }
        
    }

    const int StickyForceConstant = 0 / 100;

    private void ApplyStickyForces(float stickyForce, Vector3 position, Vector3 dir)
    {
        var force = stickyForce / 4 * dir;
        _rb.AddForceAtPosition(force, position, ForceMode.Acceleration);
    }

    public void TriggerEnter(Collider other)
    {
        groundedTriggers++;
        _isColliderContact = true;
        _sc.CalculateContactDepth(other);
    }

    public void TriggerStay(Collider other)
    {
        _isColliderContact = true;
        _sc.CalculateContactDepth(other);
    }

    public void TriggerExit()
    {
        groundedTriggers--;
        if (groundedTriggers <= 0)
        {
            _isColliderContact = false;
            _sc.CalculateContactDepth(null);
        }
    }

    public bool isDrawContactLines = false;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (isDrawContactLines)
            DrawContactLines();
#endif
        // Sticky forces
        //Debug.DrawRay(_contactPoint, _contactNormal);
        //Gizmos.DrawSphere(_rayContactPoint, 0.02f);
    }

    public void DrawContactLines() // Draw vertical lines for ground contact for visual feedback
    {
        _rayLen = transform.localScale.x / 2 + _rayOffset;
        var rayEndPoint = transform.position - (transform.up * _rayLen);
        Gizmos.color = Color.red;
#if UNITY_EDITOR
        Handles.color = Color.red;
#endif

        Vector3 sphereContactPoint;
        if (isTouchingSurface)
        {
            Gizmos.color = Color.green;
#if UNITY_EDITOR
            Handles.color = Color.green;
#endif
            sphereContactPoint = _rayContactPoint;
        }
        else sphereContactPoint = rayEndPoint;

        // Draw Raycast ray
        Gizmos.DrawLine(transform.position, rayEndPoint);
        Gizmos.DrawSphere(sphereContactPoint, 0.03f);
        // Draw vertical line as ground hit indicators         
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
    }
}
