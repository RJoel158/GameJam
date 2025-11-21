using System.Collections;
using UnityEngine;

/// <summary>
/// Al recibir el evento de campamento limpiado, hace que el campamento (y su hijo marcador)
/// desciendan suavemente hacia el suelo y, una vez por debajo del terreno, se destruyan
/// tras un pequeño retardo para liberar memoria.
/// 
/// Colocar este script en un GameObject manager en la escena o en cada campamento (opciones).
/// </summary>
public class CampClearSinkDestroy : MonoBehaviour
{
    [Tooltip("Velocidad de descenso en unidades/segundo")] public float sinkSpeed = 2f;
    [Tooltip("Cuánto por debajo del suelo debe llegar antes de considerar destruido (unidades)")] public float belowGroundOffset = 1.0f;
    [Tooltip("Tiempo en segundos desde que cruza el umbral hasta destruir el GameObject (por defecto 5s)")] public float destroyDelayAfterSink = 5f;
    [Tooltip("Nombre del child marcador dentro del campamento que también debe moverse (por defecto 'Marquer')")]
    public string markerChildName = "Marquer";
    [Tooltip("Si true, mueve TODO el GameObject del campamento; si false, únicamente mueve el child marcador")]
    public bool moveWholeCamp = true;
    [Tooltip("Si true, el script destruirá el GameObject del campamento; si false, lo desactivará")]
    public bool destroyCamp = true;

    [Tooltip("Si true, desactiva todos los Colliders del campamento mientras se hunde (recomendado)")]
    public bool disableCollidersDuringSink = true;
    [Tooltip("LayerMask para detectar el suelo via Raycast. Por defecto todo (-1)")]
    public LayerMask groundMask = ~0;

    void OnEnable()
    {
        EnemyCamp.OnCampCleared += HandleCampCleared;
    }

    void OnDisable()
    {
        EnemyCamp.OnCampCleared -= HandleCampCleared;
    }

    void HandleCampCleared(EnemyCamp camp)
    {
        if (camp == null) return;

        // Si este script está en el mismo GameObject del campamento, ejecutamos directamente.
        // En cualquier otro caso, lanzamos la coroutine usando el transform del camp que fue pasado.
        StartCoroutine(SinkAndDestroyRoutine(camp));
    }

    IEnumerator SinkAndDestroyRoutine(EnemyCamp camp)
    {
        // We'll create a visual container to move visuals only. This avoids physics/collider
        // issues on the original root. We will reparent visual children under the container.
        Transform sinkContainer = new GameObject(camp.name + "_SinkContainer").transform;
        sinkContainer.position = camp.transform.position;
        sinkContainer.rotation = camp.transform.rotation;
        // Collect visual transforms (renderers) to reparent
        var visuals = camp.GetComponentsInChildren<Renderer>(true);
        if (visuals != null && visuals.Length > 0)
        {
            foreach (var r in visuals)
            {
                // preserve world position when reparenting
                r.transform.SetParent(sinkContainer, true);
            }
        }
        else
        {
            // fallback: move whole camp if no visuals found
            sinkContainer.SetParent(camp.transform.parent, true);
            sinkContainer.position = camp.transform.position;
        }
        Transform targetTransform = sinkContainer;

        // disable animators on visuals so they don't fight the movement
        var animators = sinkContainer.GetComponentsInChildren<Animator>(true);
        foreach (var a in animators)
        {
            a.enabled = false;
        }

        // obtain ground position under the camp
        float targetGroundY;
        Vector3 origin = camp.transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        bool foundGround = false;

        // Disable potential physics/navigation components that can fight transform changes
        var rbRoot = camp.GetComponent<Rigidbody>();
        if (rbRoot != null)
        {
            rbRoot.isKinematic = true;
        }
        var childRigidbodies = camp.GetComponentsInChildren<Rigidbody>(true);
        foreach (var cr in childRigidbodies)
        {
            cr.isKinematic = true;
        }
        UnityEngine.AI.NavMeshAgent nav = camp.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.enabled = false;
        var cc = camp.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Optionally disable colliders so the object can move through the terrain collider
        Collider[] colliders = null;
        bool[] colliderWasEnabled = null;
        if (disableCollidersDuringSink)
        {
            colliders = camp.GetComponentsInChildren<Collider>(true);
            if (colliders != null && colliders.Length > 0)
            {
                colliderWasEnabled = new bool[colliders.Length];
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliderWasEnabled[i] = colliders[i].enabled;
                    colliders[i].enabled = false;
                }
            }
        }

        // Primero intentar raycast hacia abajo
        if (Physics.Raycast(origin, Vector3.down, out hit, 200f, groundMask))
        {
            targetGroundY = hit.point.y;
            foundGround = true;
        }
        else if (Terrain.activeTerrain != null)
        {
            // Si hay Terrain en escena, muestreamos la altura del mismo
            targetGroundY = Terrain.activeTerrain.SampleHeight(camp.transform.position) + Terrain.activeTerrain.GetPosition().y;
            foundGround = true;
        }
        else
        {
            // fallback: simplemente bajar cierta distancia (5 unidades)
            targetGroundY = camp.transform.position.y - 5f;
            foundGround = false;
        }

        if (!foundGround)
        {
            Debug.LogWarning($"CampClearSinkDestroy: ground not found under camp '{camp.name}'. Using fallback Y={targetGroundY}");
        }

        float destroyTargetY = targetGroundY - belowGroundOffset;

        // mientras no haya alcanzado el objetivo, traducir hacia abajo
        bool reached = false;
        while (!reached)
        {
            if (targetTransform == null) break;

            float step = sinkSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(targetTransform.position, new Vector3(targetTransform.position.x, destroyTargetY, targetTransform.position.z), step);
            // Apply the position. If the camp has a Rigidbody it was set to kinematic; moving the transform is OK.
            targetTransform.position = newPos;

            if (targetTransform.position.y <= destroyTargetY + 0.01f)
            {
                reached = true;
                break;
            }

            yield return null;
        }

        // Al alcanzar el objetivo, esperamos el delay y destruimos/desactivamos
        yield return new WaitForSeconds(destroyDelayAfterSink);

        if (targetTransform != null)
        {
            // Si movimos sólo el child y queremos destruir el camp, mantener consistencia.
            if (!moveWholeCamp && destroyCamp)
            {
                // destruir el root del campamento
                if (camp != null)
                {
                    Destroy(camp.gameObject);
                }
            }
            else
            {
                if (moveWholeCamp)
                {
                    if (destroyCamp)
                        Destroy(camp.gameObject);
                    else
                        camp.gameObject.SetActive(false);
                }
                else
                {
                    // we moved visuals into the sinkContainer - destroy it
                    Destroy(targetTransform.gameObject);
                }
            }
        }

        // If we disabled colliders and didn't destroy the camp, restore previous states
        if (!destroyCamp && disableCollidersDuringSink && colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = colliderWasEnabled != null ? colliderWasEnabled[i] : true;
            }
        }

        // If we created a sinkContainer and destroyed it above, nothing else to do.
        // If we moved visuals but didn't destroy root, reparent back to original if needed.
        // (optional) -- leaving container destroyed/restored is OK depending on destroyCamp flag.
    }
}
