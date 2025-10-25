using StarterAssets;
using UnityEngine;

public class SpikedWoodenClub : MonoBehaviour
{
    public int damage = 100;
    public Enemy enemyScript;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    [Header("Raycast Settings")]
    public Transform originPoint;       // 🔹 Desde aquí parte el Raycast
    public Vector3 castDirection = Vector3.forward;
    public float castDistance = 5f;
    public LayerMask layerMask;

    [Header("BoxCast Settings")]
    public Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    public float boxCastDistance = 0.1f;

    private RaycastHit hitInfo;
    private RaycastHit boxHitInfo;
    private bool hitDetected;
    private bool boxHitDetected;

    void FixedUpdate()
    {
        CheckIfCanHit();

        if (originPoint == null)
            return;

        Vector3 origin = originPoint.position;
        Vector3 direction = originPoint.TransformDirection(castDirection);
        Quaternion orientation = originPoint.rotation;

        // 🔹 Raycast solo como referencia visual (no daña)
        hitDetected = Physics.Raycast(origin, direction, out hitInfo, castDistance, layerMask);

        // 🔹 Punto final del raycast (donde se proyectará el BoxCast)
        Vector3 boxOrigin = hitDetected
            ? hitInfo.point
            : origin + direction * castDistance;

        // 🔹 BoxCast es el que realmente detecta colisiones
        boxHitDetected = Physics.BoxCast(
            boxOrigin,
            boxHalfExtents,
            direction,
            out boxHitInfo,
            orientation,
            boxCastDistance,
            layerMask
        );

        // ✅ Solo el BOXCAST aplica daño
        if (boxHitDetected && enemyScript.isAttacking && canHit)
        {
            Debug.Log($"Impacto con: {boxHitInfo.collider.name}");

            ThirdPersonController player = boxHitInfo.collider.GetComponent<ThirdPersonController>();
            if (player != null)
            {
                player.TakeDamage(damage);
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
        Quaternion orientation = originPoint.rotation;

        // 🔹 Raycast de referencia visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, direction * castDistance);

        // 🔹 BoxCast al final del Raycast (el que realmente detecta)
        Vector3 boxOrigin = hitDetected
            ? hitInfo.point
            : origin + direction * castDistance;

        Gizmos.matrix = Matrix4x4.TRS(boxOrigin, orientation, Vector3.one);
        Gizmos.color = boxHitDetected ? Color.red : new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
    }
}
