using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : Character
{
    public float detectionRange = 15f;
    public float attackRange = 2f;

    private Transform targetPlayer;
    private NavMeshAgent agent;
    private float distanceToPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        FindClosestPlayer();

        if (targetPlayer != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();

                if (distanceToPlayer <= attackRange)
                {
                    AttackPlayer();
                }
            }
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            targetPlayer = closestPlayer.transform;
        }
        else
        {
            targetPlayer = null; // No players found safety check
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(targetPlayer.position);
    }

    void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(targetPlayer);

        // Attack Logic, working on it

        Debug.Log("Attacking " + targetPlayer.name);
    }
}