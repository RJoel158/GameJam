using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] ThirdPersonController thirdPersonController;
    public int damage = 10;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    [Header("Raycast Settings")]
    public Transform originPoint;
    public Vector3 castDirection = Vector3.forward;
    public float castDistance = 5f;
    public LayerMask layerMask;

    private RaycastHit hitInfo;
    private bool hitDetected;

    void FixedUpdate()
    {
        if (originPoint == null)
            return;

        CheckIfCanHit();

        Vector3 origin = originPoint.position;
        Vector3 direction = originPoint.TransformDirection(castDirection);

        hitDetected = Physics.Raycast(origin, direction, out hitInfo, castDistance, layerMask);

        if (hitDetected && thirdPersonController.isAttacking && canHit)
        {
            Debug.Log($"Impacto con: {hitInfo.collider.name}");

            Enemy enemy = hitInfo.collider.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                canHit = false;
            }
        }
    }

    void CheckIfCanHit()
    {
        if (!canHit)
        {
            hitTimer += Time.deltaTime;

            if (hitTimer >= timeBwtHit)
            {
                canHit = true;
                hitTimer = 0;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (originPoint == null)
            return;

        Vector3 origin = originPoint.position;
        Vector3 direction = originPoint.TransformDirection(castDirection);

        Gizmos.color = hitDetected ? Color.red : Color.green;

        // Dibuja el raycast
        Gizmos.DrawRay(origin, direction * castDistance);

        // Dibuja un pequeño cubo en el punto de impacto si lo hay
        if (hitDetected)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(hitInfo.point, Vector3.one * 0.2f);
        }
    }

}
