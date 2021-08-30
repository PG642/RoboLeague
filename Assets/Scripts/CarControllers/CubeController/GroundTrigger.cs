﻿using UnityEditor;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    public bool isTouchingSurface = false;

    public bool isCollisionStay;
    //Raycast options
    float _rayLen, _rayOffset = 0f;
    Vector3 _rayContactPoint, _rayContactNormal;
    WheelSuspension _ws;

    Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _ws = GetComponentInParent<WheelSuspension>();
        _rayLen = _ws.radius + _rayOffset;
    }

    private void FixedUpdate()
    {
        isTouchingSurface =  _isColliderContact; // || IsRayContact();

        //TODO: this class should only do raycasts and sphere collider ground detection. Move to CubeWheel or CubeController
        if (isTouchingSurface)
            ApplyStickyForces(StickyForceConstant * 5, _rayContactPoint, -_rayContactNormal);
    }

    const int StickyForceConstant = 0 / 100;

    private void ApplyStickyForces(float stickyForce, Vector3 position, Vector3 dir)
    {
        var force = stickyForce / 4 * dir;

        //_rb.AddForceAtPosition(stickyForce, _contactPoint, ForceMode.Acceleration);
        _rb.AddForceAtPosition(force, position, ForceMode.Acceleration);
        //Debug.DrawRay(position, force, Color.blue, 0, true);
    }

    // Does a wheel touches the ground? Using raycasts, not sphere collider contact point, since no suspension
    bool IsRayContact()
    {
        var isHit = Physics.Raycast(transform.position, -_rb.transform.up, out var hit, _rayLen);
        _rayContactPoint = hit.point;
        _rayContactNormal = hit.normal;
        return isHit;
    }

    bool _isColliderContact;

    public void CollisionEnter(Collision collision, int index)
    {
        ContactPoint contactPoint = collision.contacts[index];
        _isColliderContact = true;
        isCollisionStay = true;

        Debug.Log("Enter" + this.transform.parent.parent.name + Time.frameCount);

    }

    public void CollisionStay(Collision collision)
    {
        isCollisionStay = true;
        _isColliderContact = true;
        Debug.Log("Stay" + this.transform.parent.parent.name + Time.frameCount);
    }

    public void CollisionExit(Collision collision, int index)
    {
        ContactPoint contactPoint = collision.contacts[index];
        _isColliderContact = false;
        Debug.Log("Exit" + index);
    }

    public void CollisionExit()
    {
        _isColliderContact = false;
        Debug.Log("No Contact"+ this.transform.parent.parent.name + Time.frameCount);


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