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
            ThirdPersonController player = collision.gameObject.GetComponent<ThirdPersonController>();

            // ARREGLO: Solo hacer daño si el jugador NO está muerto
            if (player != null && !player.dead && !player.death)
            {
                Debug.Log("PlayerDetected - Taking Damage");
                player.TakeDamage(damage);
            }
            else
            {
                Debug.Log("PlayerDetected - But player is already dead");
            }

            Destroy(gameObject);
        }
    }
}
