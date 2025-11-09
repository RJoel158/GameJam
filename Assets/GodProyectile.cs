using StarterAssets;
using UnityEngine;

public class GodProyectile : MonoBehaviour
{
    public float moveSpeed = 6;
    public float timeToDestroy = 5;
    public int damage = 50;
    public bool playerBullet = false;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && !playerBullet)
        {
            Debug.Log("PlayerDetected");
            ThirdPersonController player = collision.gameObject.GetComponent<ThirdPersonController>();
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
