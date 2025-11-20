using UnityEngine;
using Cinemachine;

/// <summary>
/// SOLUCION RAPIDA: Arrastra este script a tu Main Camera
/// Se ejecuta automaticamente al iniciar el juego
/// </summary>
public class QuickCameraFix : MonoBehaviour
{
    void Start()
    {
        // Buscar todas las camaras virtuales
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        Debug.Log($"[CAMERA FIX] Encontradas {allCameras.Length} camaras virtuales");

        foreach (var cam in allCameras)
        {
            // Desactivar TODAS las camaras cinematicas
            if (cam.name.Contains("Boss") ||
                cam.name.Contains("Cinematic") ||
                cam.name.Contains("CM_") ||
                cam.name.Contains("Intro"))
            {
                cam.Priority = 0;
                Debug.Log($"[CAMERA FIX] Desactivada: {cam.name}");
            }
            // Activar SOLO la camara del jugador
            else if (cam.name.Contains("Player") ||
                     cam.name.Contains("Follow"))
            {
                cam.Priority = 10;
                Debug.Log($"[CAMERA FIX] ACTIVADA: {cam.name} (Priority: 10)");
            }
        }

        Debug.Log("<color=green>[CAMERA FIX] Configuracion aplicada!</color>");
    }
}
