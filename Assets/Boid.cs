using Unity.Burst.CompilerServices;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity;
    public FlockController controller;
    public QuadrotorController QuadController;
    public Vector3 rayDirection;
    public GameObject targetMesh;
    public Vector3 Pose;
    public bool flock;

    private void Awake()
    {
        controller = GameObject.Find("DroneManager").GetComponent< FlockController>();
        QuadController = GetComponent<QuadrotorController>();
    }

    int cubeLayerIndex;
    int layerMask;
    private void Start()
    {
        rayDirection = transform.forward;
        //velocity = Random.insideUnitSphere;
        velocity = Vector3.zero;
        cubeLayerIndex = LayerMask.NameToLayer("Target");
        layerMask = (1 << cubeLayerIndex);
    }

    private void Update()
    {
        flock = controller.flock; 
    }


    public Vector3 VelocitySDF = Vector3.zero;
    public void setVel(Vector3 vec)
    {
         VelocitySDF = vec;
    }

    Vector3 SDF2;
    private void FixedUpdate()
    {

        MoveBoid();
        Pose = transform.position;

        //RAYcast();
       


       
    }


    private void RAYcast()
    {
        RaycastHit hit;

        rayDirection = targetMesh.transform.position - transform.position;

        if (Physics.Raycast(transform.position, rayDirection, out hit, 10f, layerMask))
        {
            print("Found an object ");
            SDF = hit.point;
            SDF2 = hit.normal;
        }
    }

    Vector3 SDF; 

    public void RayObject(GameObject gameObject)
    {
        targetMesh = gameObject; 
    }
    private void MoveBoid()
    {
        Vector3 cohesion = controller.Cohesion(this) - transform.position;
        Vector3 separation = controller.Separation(this) - transform.position;
        Vector3 alignment = controller.Alignment(this) - velocity;


        var c1 = controller.forceCohention;
        var c2 = controller.forceSeparation;
        var c3 = controller.forceAlingnment;
        var c4 = controller.forceSDF;
        var c5 = controller.ROSSDF;



        //velocity += c1 * cohesion + c2 * separation + c3 * alignment + c4 * (SDF - transform.position + SDF2);
        //velocity = Vector3.ClampMagnitude(velocity, controller.maxSpeed);

        if (flock)
        {
            velocity += c1 * cohesion + c2 * separation + c3 * alignment;
            velocity = Vector3.ClampMagnitude(velocity, controller.maxSpeed);
        }
        else
        {
            velocity = VelocitySDF;
        }


        QuadController.applyControl(velocity, 0f);

        /*
        if (velocity != Vector3.zero)
        {
            QuadController.applyControl(velocity,0f);
           // transform.forward = velocity.normalized;
            //transform.position += velocity * Time.deltaTime;
        }     */
    }
}
