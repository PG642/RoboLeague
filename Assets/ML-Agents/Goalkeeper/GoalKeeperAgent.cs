using System.Collections;
using System;
using System.Collections.Generic;
using ML_Agents.Goalkeeper;
using ML_Agents.Handler;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public class GoalKeeperAgent : PGBaseAgent
{
    [SerializeField] public GoalkeeperEnvironmentParameters defaultParameter;
    [SerializeField] private Text _seedText;

    private GoalkeeperEvironmentHandler _handler;
    private Rigidbody _rbBall;

    private Transform _ball, _shootAt;
    private Vector3 _startPosition;

    protected override void Start()
    {
        base.Start();
        _handler = new GoalkeeperEvironmentHandler(GameObject.Find("Environment"), defaultParameter);
        _ball = transform.parent.Find("Ball");
        _rbBall = _ball.GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _shootAt = transform.parent.Find("ShootAt");
    }

    public override void OnEpisodeBegin()
    {
        _handler.ResetParameter();
        _seedText.text = "Seed: " + _handler.activeSeed.ToString();
        // Reset Car
        controller.ResetCar(_startPosition, Quaternion.Euler(0f, 90f, 0f));
        // Reset Ball
        _rbBall.velocity = Vector3.zero;
        _rbBall.angularVelocity = Vector3.zero;
        _ball.localPosition = new Vector3(UnityEngine.Random.Range(-10f, 0f), UnityEngine.Random.Range(1.0f, 12f), UnityEngine.Random.Range(-30f, 30f));
        _ball.rotation = Quaternion.Euler(0f, 0f, 0f);

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(-53f, UnityEngine.Random.Range(3f, 3f), UnityEngine.Random.Range(-7f, 7f));

        // Shoot Ball
        _ball.GetComponent<ShootBall>().ShootTarget();

        mapData.ResetIsScored();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car position
        // TODO: Welche Normalisierung?? (alle /120f?)
        var carXNormalized = (transform.localPosition.x + 60f) / 120f;
        var carYNormalized = transform.localPosition.y / 20f;
        var carZNormalized = (transform.localPosition.z + 41f) / 82f;
        if (float.IsNaN(carXNormalized))
        {
            Debug.Log("Car: carXNormalized == NaN");
            carXNormalized = -1f;
        }
        if (float.IsNaN(carYNormalized))
        {
            Debug.Log("Car: carYNormalized == NaN");
            carYNormalized = -1f;
        }
        if (float.IsNaN(carZNormalized))
        {
            Debug.Log("Car: carZNormalized == NaN");
            carZNormalized = -1f;
        }
        sensor.AddObservation(new Vector3(carXNormalized, carYNormalized, carZNormalized));

        //Car rotation, already normalized
        float car_rotation_x = transform.rotation.x;
        float car_rotation_y = transform.rotation.y;
        float car_rotation_z = transform.rotation.z;
        float car_rotation_w = transform.rotation.w;
        if (float.IsNaN(car_rotation_x))
        {
            Debug.Log("Car: car_rotation_x == NaN");
            car_rotation_x = -1f;
        }
        if (float.IsNaN(car_rotation_y))
        {
            Debug.Log("Car: car_rotation_y == NaN");
            car_rotation_y = -1f;
        }
        if (float.IsNaN(car_rotation_z))
        {
            Debug.Log("Car: car_rotation_z == NaN");
            car_rotation_z = -1f;
        }
        if (float.IsNaN(car_rotation_w))
        {
            Debug.Log("Car: car_rotation_w == NaN");
            car_rotation_w = -1f;
        }
        sensor.AddObservation(new Quaternion(car_rotation_x, car_rotation_y, car_rotation_z, car_rotation_w));
        //Car velocity
        Vector3 car_velocity = rb.velocity.normalized * (rb.velocity.magnitude / 23f);
        if (float.IsNaN(car_velocity.x))
        {
            Debug.Log("Car: car_velocity.x == NaN");
            car_velocity.x = -1f;
        }
        if (float.IsNaN(car_velocity.y))
        {
            Debug.Log("Car: car_velocity.y == NaN");
            car_velocity.y = -1f;
        }
        if (float.IsNaN(car_velocity.z))
        {
            Debug.Log("Car: car_velocity.z == NaN");
            car_velocity.z = -1f;
        }
        sensor.AddObservation(car_velocity);

        //Car angular velocity
        Vector3 car_angularVelocity = rb.angularVelocity.normalized * (rb.angularVelocity.magnitude / 5.5f);
        if (float.IsNaN(car_angularVelocity.x))
        {
            Debug.Log("Car: rb.angularVelocity.x == NaN");
            car_angularVelocity.x = -1f;
        }
        if (float.IsNaN(car_angularVelocity.y))
        {
            Debug.Log("Car: rb.angularVelocity.y == NaN");
            car_angularVelocity.y = -1f;
        }
        if (float.IsNaN(car_angularVelocity.z))
        {
            Debug.Log("Car: rb.angularVelocity.z == NaN");
            car_angularVelocity.z = -1f;
        }
        sensor.AddObservation(car_angularVelocity);

        //Ball position
        // TODO: Welche Normalisierung?? (alle /120f?)
        var ballXNormalized = (_ball.localPosition.x + 60f) / 120f;
        var ballYNormalized = _ball.localPosition.y / 20f;
        var ballZNormalized = (_ball.localPosition.z + 41f) / 82f;
        if (float.IsNaN(ballXNormalized))
        {
            Debug.Log("Ball: ballXNormalized == NaN");
            ballXNormalized = -1f;
        }
        if (float.IsNaN(ballYNormalized))
        {
            Debug.Log("Ball: ballYNormalized == NaN");
            ballYNormalized = -1f;
        }
        if (float.IsNaN(ballZNormalized))
        {
            Debug.Log("Ball: ballZNormalized == NaN");
            ballZNormalized = -1f;
        }
        sensor.AddObservation(new Vector3(ballXNormalized, ballYNormalized, ballZNormalized));

        //Ball velocity
        Vector3 ball_velocity = _rbBall.velocity / 60f;
        if (float.IsNaN(ball_velocity.x))
        {
            Debug.Log("Ball: ball_velocity.x == NaN");
            ball_velocity.x = -1f;
        }
        if (float.IsNaN(ball_velocity.y))
        {
            Debug.Log("Ball: ball_velocity.y == NaN");
            ball_velocity.y = -1f;
        }
        if (float.IsNaN(ball_velocity.z))
        {
            Debug.Log("Ball: ball_velocity.z == NaN");
            ball_velocity.z = -1f;
        }
        sensor.AddObservation(ball_velocity);

        // Boost amount
        float boostAmount = boostControl.boostAmount / 100f;
        if (float.IsNaN(boostAmount))
        {
            Debug.Log("Car: boostAmount == NaN");
            boostAmount = -1f;
        }
        sensor.AddObservation(boostAmount);

        // Relative position car to ball
        float relativeXPos = ((_ball.localPosition.x + 60f) - (transform.localPosition.x + 60f)) / 120f;
        float relativeYPos = (_ball.localPosition.y - transform.localPosition.y) / 20f;
        float relativeZPos = ((_ball.localPosition.z + 41f) - (transform.localPosition.z + 41f)) / 82f;
        sensor.AddObservation(new Vector3(relativeXPos, relativeYPos, relativeZPos));
    }

    /// <summary>
    /// Assigns a reward if the agent saved.
    /// </summary>
    protected override void AssignReward()
    {
        if (_rbBall.velocity.x > 0f)
        {
            // Agent saved
            SetReward(1f);
            EndEpisode();
        }
        if (mapData.isScoredOrange)
        {
            // Agent got scored on
            SetReward(0f);
            EndEpisode();
        }
    }
}


