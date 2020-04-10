using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{
    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public bool pathStale = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private NavMeshAgent navAgent = null;
    private Animator animator = null;
    private float originalMaxSpeed = 0;

    private void SetNextDestination(bool increment)
    {
        if (waypointNetwork == null)
            return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        while (nextWaypointTransform == null)
        {
            // Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
            int nextWaypoint = (currentIndex + incStep >= waypointNetwork.waypoints.Count) ? 0 : currentIndex + incStep;
            nextWaypointTransform = waypointNetwork.waypoints[nextWaypoint];

            if (nextWaypointTransform != null)
            {
                currentIndex = nextWaypoint;
                navAgent.destination = nextWaypointTransform.position;
                return;
            }
        }

        currentIndex++;
    }

    private void Start()
    {
        // Cache NavMeshAgent Reference
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navAgent)
            originalMaxSpeed = navAgent.speed;

        if (waypointNetwork == null)
            return;

        // Set first waypoint
        SetNextDestination(false);
    }

    private void Update()
    {
        int turnOnSpot = 0;

        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStale = navAgent.isPathStale;
        pathStatus = navAgent.pathStatus;

        // Perform cross product on forward vector and desired velocity vector. If both inputs are Unit length
        // the resulting vector's magnitude will be Sin(theta) where theta is the angle between the vectors.
        Vector3 cross = Vector3.Cross(transform.forward, navAgent.desiredVelocity.normalized);

        // If y component is negative it is a negative rotation else a positive rotation
        float horizontal = (cross.y < 0) ? -cross.magnitude : cross.magnitude;

        // Scale into the 2.32 range for our animator
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (navAgent.desiredVelocity.magnitude < 1.0f && Vector3.Angle(transform.forward, navAgent.desiredVelocity) > 10.0f)
        {
            navAgent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            navAgent.speed = originalMaxSpeed;
        }

        // Send the data calculated above into the animator parameters
        animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        animator.SetInteger("TurnOnSpot", turnOnSpot);

        if ((navAgent.remainingDistance <= navAgent.stoppingDistance && !pathPending) || pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }
        else if (navAgent.isPathStale)
        {
            SetNextDestination(false);
        }
    }
}
