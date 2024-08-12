using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.LlmSwarmNav;
using RosMessageTypes.Geometry;
using System;

public class Initializator : MonoBehaviour
{
    ROSConnection ros;
    public string topicNameSub = "vector3array";
    public string topicNamePub = "vector3posearray";
    public uint numberOfDrones = 0;

    public GameObject Prefab;

    public List<GameObject> swarm = new();

    public Vector3StampedArrayMsg temp_msg;

    void Spawn(uint numberOfDrones)
    {
        for (int i = 0; i < Math.Sqrt(numberOfDrones); i++)
        {
            for (int j = 0; j < Math.Sqrt(numberOfDrones); j++)
            {

                Vector3 spawnPoint = Vector3.zero + Vector3.up * 0.1f + Vector3.right * 0.5f * i + Vector3.forward * 0.5f * j;
                GameObject spawnDrone = Instantiate(Prefab, spawnPoint, Quaternion.identity);
                spawnDrone.name = "drone_" + i.ToString();
                spawnDrone.GetComponent<Communicator>().setNumber((uint)i);
                swarm.Add(spawnDrone);
            }
        }
    }
        

    void SendVel(Vector3[] vector3s)
    {
       int i = 0;
       foreach (var swarm in swarm)
        {
            swarm.GetComponent<Communicator>().setVel(vector3s[i]);
            //Debug.Log(vector3s[i]);
            i++;
        }
    }

    Vector3StampedArrayMsg GetPose()
    {
        var msg = new Vector3StampedArrayMsg();

        Vector3Msg[] msg1 = new Vector3Msg[numberOfDrones];

        int i = 0;
        foreach(var swarm in swarm)
        {
            Vector3Msg msg2 = swarm.GetComponent<Communicator>().Pose.To<FLU>();
            msg1[i] = msg2;
            i++;
        }


        msg.vector = msg1;
        return msg;
    }

    private uint GetLength()
    {
        return numberOfDrones;
    }

    private void setVelCallback(Vector3StampedArrayMsg msg)
    {
        
        uint num = 0;
        List<Vector3> vels = new List<Vector3>();
        foreach (Vector3Msg vector in msg.vector)
        {
            num++;
            var vel = vector.From<FLU>();
            //Vector3 vel = new Vector3((float)vector.x, (float)vector.y, (float)vector.z) ;
            vels.Add(vel);
        }

        if (num != numberOfDrones)
        {
            numberOfDrones = num;
            Spawn(num);
        }
        //var res=AverageSpeed(vels.ToArray());
        //Debug.Log (vels.ToString());
        SendVel(vels.ToArray());
    }

    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Vector3StampedArrayMsg>(topicNameSub, setVelCallback);
        ros.RegisterPublisher<Vector3StampedArrayMsg>(topicNamePub);
        accumulatedSpeed = new Vector3[GetLength()];

    }
     void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn(numberOfDrones);
        }
        ros.Publish(topicNamePub, GetPose());


    }
    

    
    public Vector3[] speeds; // Array of Vector3 speeds to be averaged
    public Vector3[] averageSpeed; // Variable to store the average speed

    private int currentStep = 0;
    

    static Vector3[] Average(Vector3[] a, Vector3[] b)
    {
        Vector3[] c = new Vector3[b.Length];

        for (int i = 0; i < b.Length; i++)
            if (i < a.Length)
                c[i] = 0.9f*(a[i] + b[i])/2;
            else
                c[i] = b[i];
        return c;
    }

    private Vector3[] accumulatedSpeed;
    private Vector3[] AverageSpeed(Vector3[] speeds)
    {
        
        accumulatedSpeed = Average(speeds,accumulatedSpeed);
        return accumulatedSpeed;

    }






}

/*
//private Drone[] drones;

//private List<Drone> dronesList = new List<Drone>(); 
public class Drone
{

    public string name;

    public Vector3 targetVel;
    public GameObject gameObject;

    public Drone(int number, GameObject gameObject)
    {
        this.name = "drone_" + number.ToString();
        this.gameObject = gameObject;
    }
}
          */