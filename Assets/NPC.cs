using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public Transform[] waitPoints;
    public float waitTime = 2f;

    private int currentWaypoint = 0;
    private float waitTimer = 0f;

    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (waitPoints.Length > 0)
        {
            agent.SetDestination(waitPoints[currentWaypoint].position);
        }
    }

    void Update()
    {
        if (waitPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                GoToNextWaypoint();
            }
        }

        animator.SetFloat(
            "Speed",
            agent.velocity.magnitude / agent.speed
        );
    }

    void GoToNextWaypoint()
    {
        currentWaypoint++;

        if (currentWaypoint >= waitPoints.Length)
        {
            currentWaypoint = 0;
        }

        agent.SetDestination(waitPoints[currentWaypoint].position);
    }
}