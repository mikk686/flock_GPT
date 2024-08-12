using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using RosMessageTypes.LlmSwarmNav;

public class Communicator : MonoBehaviour
{
    ROSConnection ros;

    // Start is called before the first frame update
    public float publishMessageFrequency = 0.01f;
    private float timeElapsed;

    private uint t = 0;

    public Vector3 Pose;
    
    private DateTime startTime;

    private QuadrotorController quadrotorController;

    private string topicNameSub;
    private string topicNamePub = "drone";

    public uint numberOfDrone;

    public void setNumber(uint num)
    {
       numberOfDrone = num;
       topicNamePub += numberOfDrone.ToString();
    }


    //Vector3StampedArrayMsg temp_msg;

    private void Awake()
    {
        quadrotorController = GetComponent<QuadrotorController>();
        

    }
    void Start()
    {

        /*
          ros = ROSConnection.GetOrCreateInstance();

        topicNamePub = this.gameObject.name + "/pose";
        topicNameSub = this.gameObject.name + "/vel";
        ros.RegisterPublisher<TransformStampedMsg>(topicNamePub);
        ros.Subscribe<TwistStampedMsg>(topicNameSub, setVelCallback);
        Debug.Log(topicNamePub);  
        startTime = DateTime.Now;   */
    }

    private void setVelCallback(TwistStampedMsg msg)
    {
        Vector3 speed = new Vector3 ((float)msg.twist.linear.x, (float)msg.twist.linear.y, (float)msg.twist.linear.z);
        quadrotorController.applyControl(speed, 1f) ;
    }

    public void setVel(Vector3 vector3)
    {
        Vector3 speed = vector3; 
        quadrotorController.applyControl(speed, 1f);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Pose = transform.position;
        //UpdatePoseMessage();
    }

    private void UpdatePoseMessage()
    {
        ros.Publish(topicNamePub, UpdateTransform());
    }

    private TransformStampedMsg UpdateTransform()
    {
        TransformStampedMsg messege =
          new TransformStampedMsg(UpdateHeader(), " ", transform.To<FLU>()); //viconPose.transform.To<FLU>());

        return messege;
    }

    private HeaderMsg UpdateHeader()
    {
        t++;
        HeaderMsg header = new HeaderMsg(t, UpdateTime(), "map");
        return header;
    }

    private TimeMsg UpdateTime()
    {
        var timeSinceStart = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
        TimeMsg time = new TimeMsg((uint)timeSinceStart.TotalSeconds, (uint)timeSinceStart.TotalMilliseconds);
        return time;
    }
}
