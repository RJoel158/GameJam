using UnityEngine;
using Cinemachine;

/// <summary>
/// SOLUCION FINAL: Fuerza que PlayerFollowCamera sea la camara activa
/// Arrastra este script a PlayerFollowCamera directamente
/// </summary>
public class ForcePlayerCamera : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("[FORCE CAMERA] Este script debe estar en un objeto con CinemachineVirtualCamera!");
            return;
        }

        // FORZAR maxima prioridad
        virtualCamera.Priority = 100;

        Debug.Log($"<color=green>[FORCE CAMERA] {gameObject.name} - Priority forzada a 100</color>");
    }

    void Start()
    {
        // Desactivar TODAS las otras camaras virtuales
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var cam in allCameras)
        {
            if (cam != virtualCamera)
            {
                cam.Priority = 0;
                Debug.Log($"[FORCE CAMERA] Desactivada: {cam.name}");
            }
        }

        // Verificar configuracion
        if (virtualCamera.Follow == null)
        {
            Debug.LogError("<color=red>[FORCE CAMERA] Follow NO esta asignado! Asignalo manualmente</color>");
        }
        else
        {
            Debug.Log($"<color=green>[FORCE CAMERA] Follow: {virtualCamera.Follow.name}</color>");
        }

        if (virtualCamera.LookAt == null)
        {
            Debug.LogWarning("<color=yellow>[FORCE CAMERA] LookAt NO esta asignado (opcional)</color>");
        }
        else
        {
            Debug.Log($"<color=green>[FORCE CAMERA] LookAt: {virtualCamera.LookAt.name}</color>");
        }

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[FORCE CAMERA] ACTIVACION COMPLETA</color>");
        Debug.Log("<color=cyan>========================================</color>");
    }

    void Update()
    {
        // Mantener prioridad maxima constantemente
        if (virtualCamera.Priority != 100)
        {
            virtualCamera.Priority = 100;
        }
    }
}
