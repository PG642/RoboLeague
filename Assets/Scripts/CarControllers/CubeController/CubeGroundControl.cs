﻿using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeGroundControl : MonoBehaviour
{
    private const float NaiveRotationForce = 5;
    private const float NaiveRotationDampeningForce = -10;


    private Rigidbody _rb;
    private CubeController _controller;
    private InputManager _inputManager;
    private CubeWheel[] _wheelArray;
    private CarCollision _carCollision;

    public bool disableGroundStabilization;
    public bool disableWallStabilization;
    public bool disableDrift;
    [Header("Steering")] [Range(0, 100)] public float turnRadiusCoefficient = 50;
    public float currentSteerAngle;
    public float driftFactor = 0.5f;


    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _controller = GetComponent<CubeController>();
        _wheelArray = GetComponentsInChildren<CubeWheel>();
        _inputManager = GetComponentInParent<InputManager>();
        _carCollision = GetComponentInParent<CarCollision>();
    }


    private void FixedUpdate()
    {
        ApplyStabilizationFloor();
        ApplyStabilizationWall();
        var forwardAcceleration = CalcForwardForce(_inputManager.throttleInput);
        ApplyWheelForwardForce(forwardAcceleration);

        currentSteerAngle = CalculateSteerAngle();
        ApplyWheelRotation(currentSteerAngle);
    }

    private void ApplyStabilizationWall()
    {
        if (disableWallStabilization)
        {
            return;
        }

        if (Mathf.Abs(Vector3.Dot(Vector3.up, transform.up)) > 0.95f ||
            _controller.carState == CubeController.CarStates.Air || _controller.numWheelsSurface < 2)
        {
            return;
        }

        _rb.AddForce(-transform.up * 5f, ForceMode.Acceleration);
    }

    private void ApplyStabilizationFloor()
    {
        if (disableGroundStabilization)
        {
            return;
        }

        if (Mathf.Abs(_inputManager.throttleInput) <= 0.0001f)
        {
            return;
        }

        if (_controller.carState == CubeController.CarStates.Air
            || _controller.numWheelsSurface >= 3)
        {
            return;
        }

        if (_carCollision == null || _carCollision.surfaceNormal == null)
        {
            return;
        }

        var torqueDirection = -Mathf.Sign(Vector3.SignedAngle(_carCollision.surfaceNormal, _rb.transform.up,
            _controller.cogLow.transform.forward));
        var torqueForceMode = ForceMode.Acceleration;
        var factor = 50.0f;
        if (_controller.carState == CubeController.CarStates.BodyGroundDead)
        {
            torqueForceMode = ForceMode.VelocityChange;
            factor = 0.4f;
        }

        _rb.AddTorque(_controller.cogLow.transform.forward * factor * torqueDirection, torqueForceMode);
        if (_controller.carState == CubeController.CarStates.SomeWheelsSurface)
        {
            _rb.AddForce(-_carCollision.surfaceNormal * 3.25f, ForceMode.Acceleration);
        }
    }

    private void ApplyWheelForwardForce(float forwardAcceleration)
    {
        // Apply forces to each wheel
        foreach (var wheel in _wheelArray)
        {
            //TODO: Func. call like this below OR Wheel class fetches data from this class?
            // Also probably should be an interface to a concrete implementation. Same for the NaiveGroundControl below.
            if (_controller.isCanDrive && Mathf.Abs(_inputManager.throttleInput) >= 0.0001f)
                wheel.ApplyForwardForce(forwardAcceleration / 4);
        }
    }

    private void ApplyWheelRotation(float steerAngle)
    {
        // Apply steer angle to each wheel
        foreach (var wheel in _wheelArray)
        {
            //TODO: Func. call like this below OR Wheel class fetches data from this class?
            // Also probably should be an interface to a concrete implementation. Same for the NaiveGroundControl below.
            wheel.RotateWheels(steerAngle);
        }
    }

    private float CalcForwardForce(float throttleInput)
    {
        // Throttle
        float forwardAcceleration = 0;

        if (_inputManager.isBoost)
            forwardAcceleration = GetForwardAcceleration(_controller.forwardSpeedAbs);
        else
            forwardAcceleration = throttleInput * GetForwardAcceleration(_controller.forwardSpeedAbs);

        if (_inputManager.isDrift && !disableDrift)
            forwardAcceleration *= driftFactor;
        else if (_controller.forwardSpeedSign != Mathf.Sign(throttleInput) && throttleInput != 0)
            forwardAcceleration += -1 * _controller.forwardSpeedSign * 35; // Braking
        return forwardAcceleration;
    }


    private float CalculateForwardForce(float input, float speed)
    {
        return input * GetForwardAcceleration(_controller.forwardSpeedAbs);
    }

    private float CalculateSteerAngle()
    {
        var curvature = 1 / GetTurnRadius(_controller.forwardSpeed);
        return _inputManager.steerInput * curvature * turnRadiusCoefficient;
    }

    static float GetForwardAcceleration(float speed)
    {
        // Replicates acceleration curve from RL, depends on current car forward velocity
        speed = Mathf.Abs(speed);
        float throttle = 0;

        if (speed > (1410 / 100))
            throttle = 0;
        else if (speed > (1400 / 100))
            throttle = RoboUtils.Scale(14, 14.1f, 1.6f, 0, speed);
        else if (speed <= (1400 / 100))
            throttle = RoboUtils.Scale(0, 14, 16, 1.6f, speed);

        return throttle;
    }

    static float GetTurnRadius(float speed)
    {
        var forwardSpeed = Mathf.Abs(speed);

        var curvature = RoboUtils.Scale(0, 5, 0.0069f, 0.00398f, forwardSpeed);

        if (forwardSpeed >= 500 / 100)
            curvature = RoboUtils.Scale(5, 10, 0.00398f, 0.00235f, forwardSpeed);

        if (forwardSpeed >= 1000 / 100)
            curvature = RoboUtils.Scale(10, 15, 0.00235f, 0.001375f, forwardSpeed);

        if (forwardSpeed >= 1500 / 100)
            curvature = RoboUtils.Scale(15, 17.5f, 0.001375f, 0.0011f, forwardSpeed);

        if (forwardSpeed >= 1750 / 100)
            curvature = RoboUtils.Scale(17.5f, 23, 0.0011f, 0.00088f, forwardSpeed);

        float turnRadius = 1 / (curvature * 100);
        return turnRadius;
    }

    private void NaiveGroundControl()
    {
        if (_controller.carState != CubeController.CarStates.AllWheelsSurface &&
            _controller.carState != CubeController.CarStates.AllWheelsGround) return;

        // Throttle
        var throttleInput = Input.GetAxis("Vertical");
        float Fx = throttleInput * GetForwardAcceleration(_controller.forwardSpeedAbs);
        var forward = transform.forward;
        _rb.AddForceAtPosition(Fx * forward, _rb.transform.TransformPoint(_rb.centerOfMass),
            ForceMode.Acceleration);

        // Auto dampening
        _rb.AddForce(
            forward * (5.25f * -Mathf.Sign(_controller.forwardSpeed) * (1 - Mathf.Abs(throttleInput))),
            ForceMode.Acceleration);
        // alternative auto dampening
        //if (throttleInput == 0) _rb.AddForce(transform.forward * (5.25f * -Mathf.Sign(forwardSpeed)), ForceMode.Acceleration); 

        // Steering
        var up = transform.up;
        _rb.AddTorque(up * (Input.GetAxis("Horizontal") * NaiveRotationForce), ForceMode.Acceleration);
        _rb.AddTorque(up * (NaiveRotationDampeningForce * (1 - Mathf.Abs(Input.GetAxis("Horizontal"))) *
                            transform.InverseTransformDirection(_rb.angularVelocity).y),
            ForceMode.Acceleration);
    }
}