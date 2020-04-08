using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{
    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public bool pathStale = false;

    public AnimationCurve animationCurve = new AnimationCurve();

    private NavMeshAgent navAgent = null;
     
    private void SetNextDestination(bool increment)
    {
        if (waypointNetwork == null)
            return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        while (nextWaypointTransform == null)
        {
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
        navAgent = GetComponent<NavMeshAgent>();

        if (waypointNetwork == null)
            return;

        SetNextDestination(false);
    }

    private void Update()
    {
        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStale = navAgent.isPathStale;

        if (navAgent.remainingDistance <= navAgent.stoppingDistance && !pathPending)
        {
            SetNextDestination(true);
        }
        else if (navAgent.isPathStale)
        {

        }
    }
}
