using UnityEngine;
using Cinemachine;
using StarterAssets;

/// <summary>
/// Arregla el problema de camara cayendo
/// Arrastra este script a un GameObject vacio en la escena
/// </summary>
public class FixFallingCamera : MonoBehaviour
{
    void Start()
    {
        // Buscar el ThirdPersonController
        ThirdPersonController player = FindAnyObjectByType<ThirdPersonController>();

        if (player == null)
        {
            Debug.LogError("[FIX CAMERA] No se encontro ThirdPersonController!");
            return;
        }

        Debug.Log($"[FIX CAMERA] Player encontrado: {player.name}");

        // Verificar CinemachineCameraTarget
        if (player.CinemachineCameraTarget == null)
        {
            Debug.LogError("[FIX CAMERA] CinemachineCameraTarget esta NULL!");

            // Buscar o crear el CameraTarget
            Transform cameraTarget = player.transform.Find("PlayerCameraRoot");

            if (cameraTarget == null)
            {
                // Crear uno nuevo
                GameObject newTarget = new GameObject("PlayerCameraRoot");
                newTarget.transform.SetParent(player.transform);
                newTarget.transform.localPosition = new Vector3(0f, 1.375f, 0f); // Altura de los ojos
                newTarget.transform.localRotation = Quaternion.identity;

                player.CinemachineCameraTarget = newTarget;

                Debug.Log("<color=green>[FIX CAMERA] CameraTarget creado y asignado!</color>");
            }
            else
            {
                player.CinemachineCameraTarget = cameraTarget.gameObject;
                Debug.Log("<color=green>[FIX CAMERA] CameraTarget encontrado y asignado!</color>");
            }
        }
        else
        {
            Debug.Log($"[FIX CAMERA] CameraTarget: {player.CinemachineCameraTarget.name}");

            // Verificar que el target sea hijo del jugador
            if (player.CinemachineCameraTarget.transform.parent != player.transform)
            {
                Debug.LogWarning("[FIX CAMERA] CameraTarget NO es hijo del jugador! Arreglando...");
                player.CinemachineCameraTarget.transform.SetParent(player.transform);
                player.CinemachineCameraTarget.transform.localPosition = new Vector3(0f, 1.375f, 0f);
                Debug.Log("<color=green>[FIX CAMERA] CameraTarget reposicionado!</color>");
            }

            // Resetear rotacion local
            player.CinemachineCameraTarget.transform.localRotation = Quaternion.identity;
        }

        // Buscar la camara virtual del jugador
        CinemachineVirtualCamera[] cameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var cam in cameras)
        {
            if (cam.name.Contains("Player") || cam.name.Contains("Follow"))
            {
                Debug.Log($"[FIX CAMERA] Configurando: {cam.name}");

                // Asignar Follow y LookAt
                if (cam.Follow == null)
                {
                    cam.Follow = player.CinemachineCameraTarget.transform;
                    Debug.Log("<color=green>[FIX CAMERA] Follow asignado!</color>");
                }

                if (cam.LookAt == null)
                {
                    cam.LookAt = player.CinemachineCameraTarget.transform;
                    Debug.Log("<color=green>[FIX CAMERA] LookAt asignado!</color>");
                }

                // Asegurar que tenga prioridad maxima
                cam.Priority = 10;

                Debug.Log($"<color=green>[FIX CAMERA] Camara configurada correctamente!</color>");
                Debug.Log($"  Follow: {cam.Follow?.name ?? "NULL"}");
                Debug.Log($"  LookAt: {cam.LookAt?.name ?? "NULL"}");
                Debug.Log($"  Priority: {cam.Priority}");
            }
            else
            {
                // Desactivar otras camaras
                cam.Priority = 0;
            }
        }

        Debug.Log("<color=cyan>======================================</color>");
        Debug.Log("<color=cyan>[FIX CAMERA] ARREGLO COMPLETO!</color>");
        Debug.Log("<color=cyan>======================================</color>");

        // Destruir este script despues de ejecutarse
        Destroy(this);
    }
}
