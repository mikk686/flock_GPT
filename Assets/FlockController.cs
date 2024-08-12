using UnityEngine;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using RosMessageTypes.LlmSwarmNav;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.Serialization;

public class FlockController : MonoBehaviour
{
    public float neighborRadius = 2.0f;
    public float maxSpeed = 0.1f;
    public List<Boid> boids = new List<Boid>();

    public float forceSeparation = 1f;
    public float forceCohention = 1f;
    public float forceAlingnment = 1f;
    public float forceSDF = 1f;

    public bool flock = false;


    [SerializeField]

    int numberOfDrones = 50;
    public int m_numberOfDrones { get => numberOfDrones; set => numberOfDrones =  value; }

    public GameObject Prefab;
    public List<GameObject> swarm = new();


     public void Flocking()
    {
        flock = !flock;
        Debug.Log(flock);
    }

    public void Spawn()
    {
        int num = 0;
        for (int i = 0; i < Mathf.Sqrt(numberOfDrones); i++)
        {
            for (int j = 0; j < Mathf.Sqrt(numberOfDrones); j++)
            {

                Vector3 spawnPoint = Vector3.zero + Vector3.up * 0.1f + Vector3.right * 0.5f * i + Vector3.forward * 0.5f * j;
                GameObject spawnDrone = Instantiate(Prefab, spawnPoint, Quaternion.identity);
                spawnDrone.name = "drone_" + num.ToString();
                num++;
                //spawnDrone.GetComponent<Communicator>().setNumber((uint)i);
                boids.Add(spawnDrone.GetComponent<Boid>());
                swarm.Add(spawnDrone);

            }
        }
        Allocate();
    }

    public void DeSpawn()
    {

        boids.Clear();

        foreach (GameObject gameObject in swarm)
        {
            Destroy(gameObject);
        }
        swarm.Clear();
    }

    public GameObject Target;
    public void Allocate()
    {
        foreach (Boid boid in boids)
        {
            boid.RayObject(Target);
        }
    }

    public Vector3 Cohesion(Boid boid)
    {
        Vector3 centerMass = Vector3.zero;
        int count = 0;

        foreach (Boid other in boids)
        {
            if (other != boid && IsNeighbor(boid, other))
            {
                centerMass += other.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            centerMass /= count;
            return centerMass;
        }

        //var res = boid.transform.position * forceCohention;
        return boid.transform.position;
    }

    public Vector3 Separation(Boid boid)
    {
        Vector3 separation = Vector3.zero;
        foreach (Boid other in boids)
        {
            if (other != boid && IsNeighbor(boid, other))
            {
                separation += (boid.transform.position - other.transform.position);
            }
        }

        //var res = separation * forceSeparation;
        return separation;
    }

    public Vector3 Alignment(Boid boid)
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        foreach (Boid other in boids)
        {
            if (other != boid && IsNeighbor(boid, other))
            {
                averageVelocity += other.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            averageVelocity /= count;
            return averageVelocity;
        }

        //var res = boid.velocity * forceAlingnment;

        return boid.velocity;
    }

    private bool IsNeighbor(Boid one, Boid two)
    {
        return (one.transform.position - two.transform.position).sqrMagnitude <
               neighborRadius * neighborRadius;
    }



    #region ROS
    public string topicNameSub = "swarm/cmd_vel";
    public string topicNamePub = "swarm/poses";
    ROSConnection ros;
    public float ROSSDF = 1f;

    public void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Vector3StampedArrayMsg>(topicNameSub, setVelCallback);
        ros.RegisterPublisher<Vector3StampedArrayMsg>(topicNamePub);
    }

    public void FixedUpdate()
    {
        ros.Publish(topicNamePub, GetPose());
    }
    Vector3StampedArrayMsg GetPose()
    {   

        var msg = new Vector3StampedArrayMsg();

        Vector3Msg[] msg1 = new Vector3Msg[boids.Count];

        int i = 0;
        foreach (var boid in boids)
        {
            Vector3Msg msg2 = boid.Pose.To<FLU>();
            msg1[i] = msg2;
            i++;
        }

        msg.vector = msg1;
        return msg;
    }

    private void setVelCallback(Vector3StampedArrayMsg msg)
    {
        Debug.Log("get vel");
        uint num = 0;
        List<Vector3> vels = new List<Vector3>();
        foreach (Vector3Msg vector in msg.vector)
        {
            num++;
            var vel = vector.From<FLU>();
            vels.Add(vel);
        }

        SendVel(vels.ToArray());
    }

    void SendVel(Vector3[] vector3s)
    {
        int i = 0;
        foreach (var boid in boids)
        {
            boid.setVel(vector3s[i]);
            i++;
        }
    }


    #endregion
}
