using UnityEngine;
using UnityEngine.AI;

public class EnemyPrototype : MonoBehaviour
{
    public float lookRadius = 4f;

    public GameObject player;
    NavMeshAgent agent;
    public bool followPlayer = false;
    public float distance = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(player.transform.position, transform.position);

        followPlayer = distance <= lookRadius;

        if (followPlayer)
        {
            agent.SetDestination(player.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
