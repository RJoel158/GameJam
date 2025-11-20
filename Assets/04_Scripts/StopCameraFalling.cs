using UnityEngine;
using Cinemachine;
using StarterAssets;

/// <summary>
/// SOLUCION DEFINITIVA para camara cayendo
/// Este script se ejecuta CADA FRAME para mantener la camara en su lugar
/// Arrastralo a un GameObject vacio en la escena
/// </summary>
public class StopCameraFalling : MonoBehaviour
{
    private ThirdPersonController player;
    private Transform cameraTarget;
    private bool isFixed = false;

    void Start()
    {
        // Buscar jugador
        player = FindAnyObjectByType<ThirdPersonController>();

        if (player == null)
        {
            Debug.LogError("[STOP FALLING] No se encontro jugador!");
            enabled = false;
            return;
        }

        Debug.Log($"[STOP FALLING] Jugador encontrado: {player.name}");

        // Arreglar el CameraTarget
        FixCameraTarget();

        // Configurar la camara virtual
        ConfigureVirtualCamera();
    }

    void FixCameraTarget()
    {
        // Si ya tiene CameraTarget asignado
        if (player.CinemachineCameraTarget != null)
        {
            cameraTarget = player.CinemachineCameraTarget.transform;
            Debug.Log($"[STOP FALLING] CameraTarget encontrado: {cameraTarget.name}");
        }
        else
        {
            // Buscar o crear CameraTarget
            Transform existingTarget = player.transform.Find("PlayerCameraRoot");

            if (existingTarget == null)
            {
                // Crear nuevo CameraTarget
                GameObject newTarget = new GameObject("PlayerCameraRoot");
                cameraTarget = newTarget.transform;
                Debug.Log("[STOP FALLING] Creando nuevo CameraTarget");
            }
            else
            {
                cameraTarget = existingTarget;
                Debug.Log("[STOP FALLING] Usando CameraTarget existente");
            }

            player.CinemachineCameraTarget = cameraTarget.gameObject;
        }

        // IMPORTANTE: Hacer que sea hijo del jugador
        if (cameraTarget.parent != player.transform)
        {
            cameraTarget.SetParent(player.transform);
            Debug.Log("<color=green>[STOP FALLING] CameraTarget ahora es hijo del jugador</color>");
        }

        // Posicionarlo correctamente (a la altura de la cabeza)
        cameraTarget.localPosition = new Vector3(0f, 1.375f, 0f);
        cameraTarget.localRotation = Quaternion.identity;

        Debug.Log($"<color=green>[STOP FALLING] CameraTarget posicionado: {cameraTarget.localPosition}</color>");
    }

    void ConfigureVirtualCamera()
    {
        CinemachineVirtualCamera[] cameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var cam in cameras)
        {
            if (cam.name.Contains("Player") || cam.name.Contains("Follow"))
            {
                // Asignar Follow y LookAt al CameraTarget
                cam.Follow = cameraTarget;
                cam.LookAt = cameraTarget;
                cam.Priority = 10;

                Debug.Log($"<color=green>[STOP FALLING] Camara configurada: {cam.name}</color>");
                Debug.Log($"  Follow: {cam.Follow.name}");
                Debug.Log($"  LookAt: {cam.LookAt.name}");

                isFixed = true;
            }
            else if (cam.name.Contains("Boss") || cam.name.Contains("Cinematic") || cam.name.Contains("CM_"))
            {
                // Desactivar otras camaras
                cam.Priority = 0;
            }
        }

        if (isFixed)
        {
            Debug.Log("<color=cyan>====================================</color>");
            Debug.Log("<color=cyan>[STOP FALLING] CAMARA ARREGLADA!</color>");
            Debug.Log("<color=cyan>====================================</color>");
        }
        else
        {
            Debug.LogError("[STOP FALLING] No se encontro camara del jugador!");
        }
    }

    void LateUpdate()
    {
        // Verificar constantemente que el CameraTarget siga siendo hijo del jugador
        if (cameraTarget != null && player != null)
        {
            // Si se desconecto del jugador, reconectarlo
            if (cameraTarget.parent != player.transform)
            {
                Debug.LogWarning("[STOP FALLING] CameraTarget se desconecto! Reconectando...");
                cameraTarget.SetParent(player.transform);
                cameraTarget.localPosition = new Vector3(0f, 1.375f, 0f);
            }

            // Mantener rotacion local en identidad (evitar rotaciones raras)
            if (cameraTarget.localRotation != Quaternion.identity)
            {
                cameraTarget.localRotation = Quaternion.identity;
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("[STOP FALLING] Script destruido");
    }
}
