using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour
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
    private float smoothAngle = 0;

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

        navAgent.updateRotation = false;

        if (waypointNetwork == null)
            return;

        // Set first waypoint
        SetNextDestination(false);
    }

    private void Update()
    {
        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStale = navAgent.isPathStale;
        pathStatus = navAgent.pathStatus;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;
        smoothAngle = Mathf.MoveTowardsAngle(smoothAngle, angle, 80.0f * Time.deltaTime);

        float speed = localDesiredVelocity.z;

        animator.SetFloat("Angle", smoothAngle);
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        if (navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            Quaternion lookRotation = Quaternion.LookRotation(navAgent.desiredVelocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
        }

        if ((navAgent.remainingDistance <= navAgent.stoppingDistance && !pathPending) || pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }
        else if (navAgent.isPathStale)
        {
            SetNextDestination(false);
        }
    }

    private void OnAnimatorMove()
    {
        //transform.rotation = animator.rootRotation;
        navAgent.velocity = animator.deltaPosition / Time.deltaTime;
    }
}
