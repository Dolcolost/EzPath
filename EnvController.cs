using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public PlayerAgent Agent; 
        [HideInInspector] public Vector3 StartingPos;
        [HideInInspector] public Rigidbody Rb;
    }

    [System.Serializable]
    public class WallInfo //class to store inside wall informations
    {
        public InsideWall Wall;
        [HideInInspector] public Vector3 wall_rot; //wall rotation
        [HideInInspector] public Vector3 wall_scale; //wall scaling 
    }


    public float rewardValue = 1; //reward multiplier

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000; //number of action before reset

    public GameObject Victory_zone;
    [HideInInspector] Vector3 victory_zone_Pos;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>(); //List of all agent

    public List<WallInfo> WallList = new List<WallInfo>(); // List of all inside wall

    public int radiusSpawnCube; //space between agents and walls


    private SimpleMultiAgentGroup BlueAgentGroup;
    private SimpleMultiAgentGroup RedAgentGroup;

    private int resetTimer;
    private int numberBlueAgent;
    private int numberBlueAgentAlive;

    void Start()
    {
        // Initialize TeamManager
        BlueAgentGroup = new SimpleMultiAgentGroup();
        RedAgentGroup = new SimpleMultiAgentGroup();
        victory_zone_Pos = new Vector3(Victory_zone.transform.position.x, Victory_zone.transform.position.y,
            Victory_zone.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue) //put agents in their teams
            {
                numberBlueAgent += 1;
                BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                RedAgentGroup.RegisterAgent(item.Agent);
            }
        }
        
        ResetScene();
    }

    void FixedUpdate()
    {
        resetTimer += 1;

        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0) // max time reached
        {
            PointScored(1); // Red team win by timer
            BlueAgentGroup.GroupEpisodeInterrupted();
            RedAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void ResetVictoryZone()
    {
        var randomGoalX = Random.Range(-28f, 28f);
        var randomGoalZ = Random.Range(-28f, 28f);
        var randomGoalY = Random.Range(0f, 48f);

        Victory_zone.transform.position = victory_zone_Pos + new Vector3(randomGoalX, randomGoalY, randomGoalZ);
    }

    public void ResetInsideWall()
    {
        foreach (var item in WallList)
        {
            var randomRot = Random.Range(0f, 180f); //random rotation for a wall
            var randomScale = Random.Range(2f, 10f); // random scaling for a wall

            var newRot = new Vector3(0f, randomRot, 0f);
            var newScale = new Vector3(randomScale, 3f, 1f);

            item.Wall.transform.Rotate(newRot);
            item.Wall.transform.localScale = newScale;
        }
    }

    public void PointScored(int result)
    {
        if (numberBlueAgentAlive == 1)
        {
            if (result == 0) //Blue win by touching victory zone
            {
                foreach (var item in AgentsList)
                {
                    if (item.Agent.team == Team.Blue) //give reward to blue agent
                    {
                        item.Agent.giveRewardBlue(rewardValue * 5, resetTimer, MaxEnvironmentSteps);
                    }
                }

                RedAgentGroup.AddGroupReward(-(rewardValue - (float) resetTimer / MaxEnvironmentSteps)); 
                //give malus to red agent
            }
            else if (result == 1) //red win by timer or there is no blue left
            {
                RedAgentGroup.AddGroupReward(rewardValue); //give
                foreach (var item in AgentsList)
                {
                    if (item.Agent.team == Team.Blue)
                    {
                        item.Agent.malusRedTouchBlue(rewardValue * 3); //give malus to blue agent
                    }
                }
            }
            else if (result == 2) //blue touch wall
            {
                foreach (var item in AgentsList)
                {
                    if (item.Agent.team == Team.Blue)
                    {
                        item.Agent.malusWallTouch(rewardValue, resetTimer, MaxEnvironmentSteps); //give malus to blue agent
                    }
                }
            }
            //give group reward
            RedAgentGroup.EndGroupEpisode(); 
            BlueAgentGroup.EndGroupEpisode();
            
            ResetScene();
        }
        else
        {
            numberBlueAgentAlive -= 1;
        }
    }

    public Vector3 setRandomPosition()
    {
        var randomPosX = Random.Range(-28f, 28f);
        var randomPosZ = Random.Range(-28f, 28f);
        var randomPosY = Random.Range(0f, 48f);
        return new Vector3(randomPosX, randomPosY, randomPosZ);
    }
    
    public void ResetScene()
    {
        resetTimer = 0;
        numberBlueAgentAlive = numberBlueAgent;

        ResetInsideWall();

        //Reset Agents

        foreach (var item in AgentsList)
        {
            bool haveCollision = true;
            var newStartPos = new Vector3(0f, 0f, 0f);

            while (haveCollision)
            {
                newStartPos = item.Agent.initialPos + setRandomPosition();
                if (!Physics.CheckSphere(newStartPos, radiusSpawnCube + 0.5f)) //check if agent touches something
                {
                    haveCollision = false;
                }
            }

            var newRot = Quaternion.Euler(0, 0, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        ResetVictoryZone();
    }
}