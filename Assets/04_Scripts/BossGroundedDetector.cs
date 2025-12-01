using UnityEngine;

/// <summary>
/// Detecta cuando el boss está en el suelo y actualiza el parámetro Grounded del Animator
/// </summary>
public class BossGroundedDetector : MonoBehaviour
{
    [Header("Ground Detection")]
    [Tooltip("Distancia para detectar el suelo")]
    public float groundCheckDistance = 0.5f;

    [Tooltip("Layer del suelo")]
    public LayerMask groundLayer = -1; // -1 = todo

    [Tooltip("Offset desde el centro del boss para el raycast")]
    public float groundCheckOffset = 0.1f;

    [Header("Debug")]
    public bool showDebugRays = true;

    private Animator animator;
    private bool wasGrounded = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[BossGroundedDetector] No Animator found on boss!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Inicialmente en el suelo
        if (animator != null)
        {
            animator.SetBool("Grounded", true);
            wasGrounded = true;
        }
    }

    private void Update()
    {
        if (animator == null) return;

        bool isGrounded = CheckGrounded();

        // Solo actualizar si cambió el estado
        if (isGrounded != wasGrounded)
        {
            animator.SetBool("Grounded", isGrounded);

            if (isGrounded)
            {
                Debug.Log("<color=green>[BossGroundedDetector] Boss GROUNDED (landed on ground)</color>");
            }
            else
            {
                Debug.Log("<color=yellow>[BossGroundedDetector] Boss AIRBORNE (left ground)</color>");
            }

            wasGrounded = isGrounded;
        }
    }

    private bool CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;

        // Raycast hacia abajo
        bool hit = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);

        if (showDebugRays)
        {
            Color rayColor = hit ? Color.green : Color.red;
            Debug.DrawRay(origin, Vector3.down * groundCheckDistance, rayColor);
        }

        return hit;
    }

    // Método público para forzar el estado
    public void ForceGrounded(bool grounded)
    {
        if (animator != null)
        {
            animator.SetBool("Grounded", grounded);
            wasGrounded = grounded;
            Debug.Log($"<color=cyan>[BossGroundedDetector] Force set Grounded = {grounded}</color>");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el rango de detección en el editor
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, 0.1f);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
