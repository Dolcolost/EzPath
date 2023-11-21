using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Red = 1
}

public class PlayerAgent : Agent
{
    public enum Position
    {
        DeliveryMan,
        Catcher
    }
    
    //from unity inspector
    [HideInInspector] public Team team;
    public Position position;

    float lateralSpeed; //cube lateral speed
    float forwardSpeed; //cube forward speed

    [HideInInspector] public Rigidbody agentRb;
    PlayerSettings m_PlayerSettings; 
    BehaviorParameters behaviorParameters;
    public Vector3 initialPos;

    public Transform victory_zone;

    private EnvController gameController;

    public override void Initialize()
    {
        gameController = GetComponentInParent<EnvController>();

        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        if (behaviorParameters.TeamId == (int) Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
        else
        {
            team = Team.Red;
            initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }

        if (position == Position.DeliveryMan)
        {
            lateralSpeed = 1.0f;
            forwardSpeed = 1.0f;
        }
        else
        {
            lateralSpeed = 1.0f;
            forwardSpeed = 1.0f;
        }

        m_PlayerSettings = FindObjectOfType<PlayerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500; // max rotation speed
    }


    public void MoveAgent(ActionSegment<int> act) //take actuator array which contains agent's actions
    {
        var dirToGo = Vector3.zero;

        var forwardAxis = act[0];
        var lateralAxis = act[1];
        var heigthAxis = act[2];

        dirToGo += moveForwardAxis(forwardAxis);
        dirToGo += moveLateralAxis(lateralAxis);
        dirToGo += moveHeightAxis(heigthAxis);
        
        if (dirToGo.z == 0f || dirToGo.x == 0f) //no diagonal movement
        {
            agentRb.AddForce(dirToGo, ForceMode.VelocityChange);
        }
        else
        {
            agentRb.AddForce(dirToGo * 0.71f, ForceMode.VelocityChange); //multiply by 0.71 to equalize with simple forward or lateral action
        }
    }

    private Vector3 moveForwardAxis(int forwardAxis)
    {
        var dirToGo = Vector3.zero;
        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * forwardSpeed * m_PlayerSettings.agentRunSpeed;
                break;
            case 2:
                dirToGo = transform.forward * -forwardSpeed * m_PlayerSettings.agentRunSpeed;
                break;
        }

        return dirToGo;
    }
    

    private Vector3 moveLateralAxis(int LateralAxis)
    {
        var dirToGo = Vector3.zero;
        switch (LateralAxis)
        {
            case 1:
                dirToGo = transform.right * lateralSpeed * forwardSpeed * m_PlayerSettings.agentRunSpeed;
                break;
            case 2:
                dirToGo = transform.right * -lateralSpeed * m_PlayerSettings.agentRunSpeed;
                break;
        }

        return dirToGo;
    }
    
    private Vector3 moveHeightAxis(int heightAxis)
    {
        var dirToGo = Vector3.zero;
        switch (heightAxis)
        {
            case 1:
                dirToGo = transform.up * forwardSpeed * m_PlayerSettings.agentRunSpeed;
                break;
            case 2:
                dirToGo = transform.up * -forwardSpeed * m_PlayerSettings.agentRunSpeed;
                break;
        }

        return dirToGo;
    }

    public override void CollectObservations(VectorSensor sensor) //declaration of agent's sensor - raycasts are in inspector
    {
        if (behaviorParameters.TeamId == (int) Team.Blue)
        {
            sensor.AddObservation(this.transform.localPosition - victory_zone.localPosition);
        }
    }

    public void giveRewardBlue(float rewardValue, float m_ResetTimer, int MaxEnvironmentSteps)
    {
        AddReward(rewardValue);
    }

    public void malusRedTouchBlue(float rewardValue)
    {
        AddReward(-rewardValue);
    }

    public void malusWallTouch(float rewardValue, float m_ResetTimer, int MaxEnvironmentSteps)
    {
        AddReward(-rewardValue - m_ResetTimer / MaxEnvironmentSteps);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) //actions listener
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) //inputs for heuristic mode
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        //forward
        if (Input.GetKey(KeyCode.Z))
        {
            discreteActionsOut[0] = 1;
        }
        
        //backwward
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }

        //right
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 1;
        }
    
        //left
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
        
        //up
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        
        //down
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[2] = 2;
        }
    }
}