using UnityEngine;
using UnityEngine.AI;

public class EnemyController : Character
{
    [Header("Attack Settings")]
    public float detectionRange = 25f;
    public float attackRange = 10f;
    public float attackDamage;
    [Tooltip("The projectile prefab the enemy will shoot.")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("How many shots the enemy can fire per second.")]
    [SerializeField] private float fireRate = 1f;

    private Transform targetPlayer;
    private NavMeshAgent agent;
    private float nextFireTime = 1f;

    [SerializeField] private AudioClip shootSFX_1;
    [SerializeField] private AudioClip shootSFX_2;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
            return;
        }

        agent.stoppingDistance = attackRange;
    }

    void Update()
    {
        if (agent.stoppingDistance != attackRange)
        {
            Debug.LogError("WARNING: stoppingDistance has been changed! Is now " + agent.stoppingDistance);
        }

        FindClosestPlayer();

        if (targetPlayer == null)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        //Debug.Log(gameObject.name + " | Distance: " + distanceToPlayer.ToString("F2") + " | Attack Range: " + attackRange);

        if (distanceToPlayer > attackRange)
        {
            // STATE: CHASE
            agent.isStopped = false;
            agent.SetDestination(targetPlayer.position);
        }
        else
        {
            // STATE: ATTACK
            agent.isStopped = true;

            Vector3 lookDirection = (targetPlayer.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Shoot();
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
            // Skip dead players
            Character character = player.GetComponent<Character>();
            if (character != null && character.isDead)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        targetPlayer = (closestPlayer != null) ? closestPlayer.transform : null;
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab not assigned on " + gameObject.name);
            return;
        }
        Vector3 spawnPos = transform.position + transform.forward * 1.5f;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, transform.rotation);

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.archetype = Archetype.Enemy;
            p.Init(transform.forward);

            if (shootSFX_1 != null && shootSFX_2 != null)
            {
                AudioManager.Instance.PlayRandomClip(shootSFX_1, shootSFX_2);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
