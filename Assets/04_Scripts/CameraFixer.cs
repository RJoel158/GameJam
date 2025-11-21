using UnityEngine;
using Cinemachine;
using StarterAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script para diagnosticar y reparar problemas de cámara
/// Usa este script cuando la cámara se vuelve loca o no sigue al jugador
/// </summary>
public class CameraFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Diagnóstico")]
    [Tooltip("Muestra información de debug en la consola")]
    public bool showDebugInfo = true;

    [Header("Reparación Automática")]
    [Tooltip("Resetea las prioridades de las cámaras")]
    public bool fixCameraPriorities = true;
    
    [Tooltip("Resetea la rotación del CinemachineCameraTarget")]
    public bool resetCameraTargetRotation = true;
    
    [Tooltip("Verifica y corrige el Input del jugador")]
    public bool fixPlayerInput = true;

    [ContextMenu("REPARAR CAMARA AHORA")]
    public void FixCameraIssues()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>INICIANDO REPARACION DE CAMARA...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        int issuesFixed = 0;

        // 1. Diagnosticar y arreglar prioridades de cámaras
        if (fixCameraPriorities)
        {
            issuesFixed += FixCameraPrioritiesIssue();
        }

        // 2. Resetear rotación del CameraTarget
        if (resetCameraTargetRotation)
        {
            issuesFixed += ResetCameraTargetRotation();
        }

        // 3. Verificar Input del jugador
        if (fixPlayerInput)
        {
            issuesFixed += CheckPlayerInput();
        }

        // 4. Verificar configuración de Cinemachine Brain
        issuesFixed += CheckCinemachineBrain();

        // 5. Limpiar referencias nulas
        issuesFixed += CleanupNullReferences();

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log($"<color=green>REPARACION COMPLETA: {issuesFixed} problema(s) arreglado(s)</color>");
        Debug.Log("<color=cyan>========================================</color>");
        
        if (issuesFixed == 0)
        {
            Debug.Log("<color=yellow>No se detectaron problemas. Si la camara sigue fallando:</color>");
            Debug.Log("<color=yellow>  1. Verifica la sensibilidad del mouse en PlayerInput</color>");
            Debug.Log("<color=yellow>  2. Revisa que solo haya UNA camara principal con tag 'MainCamera'</color>");
            Debug.Log("<color=yellow>  3. Asegurate de que el CinemachineCameraTarget este asignado</color>");
        }
    }

    [ContextMenu("DIAGNOSTICO COMPLETO")]
    public void DiagnoseCamera()
    {
        Debug.Log("<color=magenta>========================================</color>");
        Debug.Log("<color=magenta>DIAGNOSTICO DE CAMARA</color>");
        Debug.Log("<color=magenta>========================================</color>");

        // Buscar ThirdPersonController
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogError("<color=red>No se encontro ThirdPersonController en la escena</color>");
            return;
        }

        Debug.Log($"<color=green>ThirdPersonController encontrado: {playerController.name}</color>");

        // Verificar CinemachineCameraTarget
        if (playerController.CinemachineCameraTarget == null)
        {
            Debug.LogError("<color=red>CinemachineCameraTarget NO esta asignado!</color>");
        }
        else
        {
            Debug.Log($"<color=green>CinemachineCameraTarget: {playerController.CinemachineCameraTarget.name}</color>");
            Debug.Log($"   Posición: {playerController.CinemachineCameraTarget.transform.position}");
            Debug.Log($"   Rotación: {playerController.CinemachineCameraTarget.transform.rotation.eulerAngles}");
        }

        // Listar todas las cámaras virtuales
        var allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        Debug.Log($"<color=cyan>Camaras Virtuales encontradas: {allCameras.Length}</color>");
        
        foreach (var cam in allCameras)
        {
            string activeText = cam.Priority > 0 ? "<color=green>ACTIVA</color>" : "<color=gray>inactiva</color>";
            Debug.Log($"  - {cam.name} - Priority: {cam.Priority} [{activeText}]");
            
            // Verificar si sigue al jugador
            if (cam.Follow != null)
            {
                Debug.Log($"    Follow: {cam.Follow.name}");
            }
            else
            {
                Debug.Log($"    <color=yellow>Follow no asignado</color>");
            }

            if (cam.LookAt != null)
            {
                Debug.Log($"    LookAt: {cam.LookAt.name}");
            }
        }

        // Verificar Main Camera
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>No se encontro Main Camera (tag 'MainCamera')</color>");
        }
        else
        {
            Debug.Log($"<color=green>Main Camera: {mainCamera.name}</color>");
            
            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                Debug.LogError("<color=red>Main Camera no tiene CinemachineBrain!</color>");
            }
            else
            {
                Debug.Log($"<color=green>CinemachineBrain configurado</color>");
                Debug.Log($"   Camara Activa: {brain.ActiveVirtualCamera?.Name ?? "NINGUNA"}");
                Debug.Log($"   Blend Time: {brain.m_DefaultBlend.m_Time}s");
            }
        }

        // Verificar Input
        var playerInput = playerController.GetComponent<StarterAssetsInputs>();
        if (playerInput == null)
        {
            Debug.LogError("<color=red>StarterAssetsInputs no encontrado!</color>");
        }
        else
        {
            Debug.Log($"<color=green>StarterAssetsInputs encontrado</color>");
        }

        Debug.Log("<color=magenta>========================================</color>");
    }

    private int FixCameraPrioritiesIssue()
    {
        int fixedCount = 0;
        var allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        Debug.Log($"<color=yellow>Revisando {allCameras.Length} camaras virtuales...</color>");

        // Primero, desactivar TODAS las cámaras cinemáticas
        foreach (var cam in allCameras)
        {
            if (cam.name.Contains("BossIntro") || cam.name.Contains("CM_Boss") || cam.name.Contains("Cinematic"))
            {
                if (cam.Priority != 0)
                {
                    cam.Priority = 0;
                    EditorUtility.SetDirty(cam);
                    Debug.Log($"<color=green>{cam.name}: Priority -> 0 (desactivada)</color>");
                    fixedCount++;
                }
            }
        }

        // Luego, asegurar que la cámara del jugador tenga prioridad máxima
        foreach (var cam in allCameras)
        {
            if (cam.name.Contains("Player") || cam.name.Contains("Follow") || cam.name.Contains("TPCamera"))
            {
                if (cam.Priority != 10)
                {
                    cam.Priority = 10;
                    EditorUtility.SetDirty(cam);
                    Debug.Log($"<color=green>{cam.name}: Priority -> 10 (ACTIVADA)</color>");
                    fixedCount++;
                }
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>Se corrigieron {fixedCount} prioridades de camara</color>");
        }

        return fixedCount;
    }

    private int ResetCameraTargetRotation()
    {
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning("<color=yellow>No se encontro ThirdPersonController</color>");
            return 0;
        }

        if (playerController.CinemachineCameraTarget == null)
        {
            Debug.LogError("<color=red>CinemachineCameraTarget no esta asignado!</color>");
            Debug.Log("<color=yellow>SOLUCION: En el Inspector del jugador, asigna el GameObject 'PlayerCameraRoot' o similar</color>");
            return 0;
        }

        // Resetear rotación del target
        var target = playerController.CinemachineCameraTarget.transform;
        var oldRotation = target.rotation.eulerAngles;
        
        // Resetear pitch (X) a 0, mantener yaw (Y) del jugador
        target.rotation = Quaternion.Euler(0f, playerController.transform.eulerAngles.y, 0f);
        
        Debug.Log($"<color=green>CameraTarget rotacion reseteada</color>");
        Debug.Log($"   Antes: {oldRotation}");
        Debug.Log($"   Despues: {target.rotation.eulerAngles}");

        EditorUtility.SetDirty(target.gameObject);
        return 1;
    }

    private int CheckPlayerInput()
    {
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController == null) return 0;

        var input = playerController.GetComponent<StarterAssetsInputs>();
        if (input == null)
        {
            Debug.LogError("<color=red>StarterAssetsInputs no encontrado en el jugador!</color>");
            return 0;
        }

        Debug.Log("<color=green>Input del jugador funcionando correctamente</color>");
        
        // Verificar si LockCameraPosition está activado (podría causar problemas)
        if (playerController.LockCameraPosition)
        {
            Debug.LogWarning("<color=yellow>LockCameraPosition esta ACTIVADO - esto previene el movimiento de camara</color>");
            Debug.Log("<color=yellow>   Desactiva esta opcion en ThirdPersonController si quieres mover la camara</color>");
        }

        return 0;
    }

    private int CheckCinemachineBrain()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>No hay Main Camera en la escena!</color>");
            Debug.Log("<color=yellow>SOLUCION: Asigna el tag 'MainCamera' a tu camara principal</color>");
            return 0;
        }

        var brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("<color=red>Main Camera no tiene CinemachineBrain!</color>");
            Debug.Log("<color=yellow>SOLUCION: Anade el componente CinemachineBrain a la Main Camera</color>");
            return 0;
        }

        Debug.Log($"<color=green>CinemachineBrain configurado correctamente</color>");
        Debug.Log($"   Camara Activa: {brain.ActiveVirtualCamera?.Name ?? "NINGUNA"}");
        
        if (brain.ActiveVirtualCamera == null)
        {
            Debug.LogWarning("<color=yellow>No hay camara virtual activa!</color>");
            Debug.Log("<color=yellow>   Asegurate de que al menos una CinemachineVirtualCamera tenga Priority > 0</color>");
        }

        return 0;
    }

    private int CleanupNullReferences()
    {
        int fixedCount = 0;
        
        // Buscar CinematicControllers con referencias nulas
        var cinematicControllers = FindObjectsByType<CinematicController>(FindObjectsSortMode.None);
        
        foreach (var controller in cinematicControllers)
        {
            bool hasNulls = false;
            
            if (controller.playerCamera == null && controller.cinematicCameras.Length > 0)
            {
                Debug.LogWarning($"<color=yellow>{controller.name}: playerCamera es null</color>");
                hasNulls = true;
            }

            if (hasNulls)
            {
                Debug.Log($"<color=yellow>Revisa las referencias en: {controller.name}</color>");
            }
        }

        return fixedCount;
    }

    [ContextMenu("Encontrar y Asignar Camara del Jugador")]
    public void FindAndAssignPlayerCamera()
    {
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogError("<color=red>No se encontro ThirdPersonController</color>");
            return;
        }

        var allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        foreach (var cam in allCameras)
        {
            if (cam.name.Contains("Player") || cam.name.Contains("Follow") || cam.name.Contains("TPCamera"))
            {
                Debug.Log($"<color=green>Camara del jugador encontrada: {cam.name}</color>");
                Debug.Log($"   Priority actual: {cam.Priority}");
                
                if (cam.Follow == null)
                {
                    Debug.LogWarning($"<color=yellow>{cam.name} no tiene Follow asignado!</color>");
                    Debug.Log("<color=yellow>Asigna el GameObject del jugador o su CameraTarget al campo 'Follow'</color>");
                }
                
                if (cam.LookAt == null)
                {
                    Debug.LogWarning($"<color=yellow>{cam.name} no tiene LookAt asignado!</color>");
                    Debug.Log("<color=yellow>Asigna el CinemachineCameraTarget al campo 'LookAt'</color>");
                }
            }
        }
    }

    [ContextMenu("Resetear TODA la configuracion de camara")]
    public void ResetAllCameraSettings()
    {
        Debug.Log("<color=red>========================================</color>");
        Debug.Log("<color=red>RESETEO COMPLETO DE CAMARA</color>");
        Debug.Log("<color=red>========================================</color>");

        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController != null)
        {
            // Resetear valores del ThirdPersonController
            playerController.TopClamp = 70f;
            playerController.BottomClamp = -30f;
            playerController.CameraAngleOverride = 0f;
            playerController.LockCameraPosition = false;
            
            EditorUtility.SetDirty(playerController);
            Debug.Log("<color=green>ThirdPersonController reseteado</color>");
        }

        // Resetear todas las cámaras
        FixCameraPrioritiesIssue();
        ResetCameraTargetRotation();

        Debug.Log("<color=green>RESETEO COMPLETO</color>");
    }
#endif
}
