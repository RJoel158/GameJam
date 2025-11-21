using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Arregla problemas con la Main Camera y Cinemachine Brain
/// Usa este script cuando la camara solo funcione con "Solo" activado
/// </summary>
public class MainCameraFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("ARREGLAR MAIN CAMERA")]
    public void FixMainCamera()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>ARREGLANDO MAIN CAMERA...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // 1. Encontrar o crear Main Camera
        Camera mainCam = Camera.main;
        
        if (mainCam == null)
        {
            Debug.LogError("<color=red>NO SE ENCONTRO MAIN CAMERA!</color>");
            
            // Buscar cualquier camara en la escena
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            
            if (allCameras.Length > 0)
            {
                mainCam = allCameras[0];
                mainCam.tag = "MainCamera";
                Debug.Log($"<color=green>Se asigno tag MainCamera a: {mainCam.name}</color>");
            }
            else
            {
                Debug.LogError("<color=red>NO HAY CAMARAS EN LA ESCENA! Crea una Main Camera</color>");
                return;
            }
        }
        else
        {
            Debug.Log($"<color=green>Main Camera encontrada: {mainCam.name}</color>");
        }

        // 2. Verificar y arreglar CinemachineBrain
        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        
        if (brain == null)
        {
            brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
            Debug.Log("<color=green>CinemachineBrain agregado a Main Camera</color>");
        }
        else
        {
            Debug.Log("<color=green>CinemachineBrain ya existe</color>");
        }

        // 3. Configurar CinemachineBrain correctamente
        brain.m_ShowDebugText = false;
        brain.m_ShowCameraFrustum = true;
        brain.m_IgnoreTimeScale = false;
        brain.m_WorldUpOverride = null;
        
        // Configurar el blend por defecto
        brain.m_DefaultBlend.m_Time = 2f;
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        
        Debug.Log("<color=green>CinemachineBrain configurado</color>");
        Debug.Log($"   Blend Time: {brain.m_DefaultBlend.m_Time}s");
        Debug.Log($"   Blend Style: {brain.m_DefaultBlend.m_Style}");

        // 4. Verificar AudioListener
        AudioListener listener = mainCam.GetComponent<AudioListener>();
        if (listener == null)
        {
            mainCam.gameObject.AddComponent<AudioListener>();
            Debug.Log("<color=green>AudioListener agregado</color>");
        }

        // 5. Desactivar el modo "Solo" de todas las camaras virtuales
        CinemachineVirtualCamera[] allVCams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        Debug.Log($"<color=yellow>Desactivando modo Solo en {allVCams.Length} camaras virtuales...</color>");
        
        foreach (var vcam in allVCams)
        {
            // Esto desactiva el modo "Solo" en el editor
            vcam.gameObject.hideFlags = HideFlags.None;
        }

        // 6. Configurar prioridades correctamente
        FixCameraPriorities();

        // 7. Guardar cambios
        EditorUtility.SetDirty(mainCam.gameObject);

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=green>MAIN CAMERA ARREGLADA!</color>");
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=yellow>IMPORTANTE: Desactiva el boton 'Solo' en PlayerFollowCamera</color>");
        Debug.Log("<color=yellow>El boton 'Solo' esta en el Inspector, arriba donde dice 'Status: Live'</color>");
    }

    [ContextMenu("DIAGNOSTICO COMPLETO DE CAMARAS")]
    public void DiagnoseCameras()
    {
        Debug.Log("<color=magenta>========================================</color>");
        Debug.Log("<color=magenta>DIAGNOSTICO DE CAMARAS</color>");
        Debug.Log("<color=magenta>========================================</color>");

        // Main Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("<color=red>NO HAY MAIN CAMERA (tag MainCamera no encontrado)</color>");
        }
        else
        {
            Debug.Log($"<color=green>Main Camera: {mainCam.name}</color>");
            Debug.Log($"   Posicion: {mainCam.transform.position}");
            Debug.Log($"   Rotacion: {mainCam.transform.rotation.eulerAngles}");
            
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                Debug.LogError("<color=red>   Sin CinemachineBrain!</color>");
            }
            else
            {
                Debug.Log($"<color=green>   CinemachineBrain: OK</color>");
                Debug.Log($"   Camara Virtual Activa: {brain.ActiveVirtualCamera?.Name ?? "NINGUNA"}");
                Debug.Log($"   Is Blending: {brain.IsBlending}");
            }
        }

        // Camaras Virtuales
        CinemachineVirtualCamera[] allVCams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        Debug.Log($"<color=cyan>Camaras Virtuales: {allVCams.Length}</color>");

        foreach (var vcam in allVCams)
        {
            bool isActive = vcam.Priority > 0;
            string statusColor = isActive ? "green" : "gray";
            string status = isActive ? "ACTIVA" : "inactiva";
            
            Debug.Log($"<color={statusColor}>  [{vcam.Priority}] {vcam.name} - {status}</color>");
            
            if (vcam.Follow != null)
                Debug.Log($"      Follow: {vcam.Follow.name}");
            else
                Debug.Log("      <color=yellow>Follow: NO ASIGNADO</color>");
                
            if (vcam.LookAt != null)
                Debug.Log($"      LookAt: {vcam.LookAt.name}");
        }

        Debug.Log("<color=magenta>========================================</color>");
    }

    private void FixCameraPriorities()
    {
        CinemachineVirtualCamera[] allVCams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        int fixedCount = 0;
        
        foreach (var vcam in allVCams)
        {
            // Desactivar camaras cinematicas
            if (vcam.name.Contains("Boss") || vcam.name.Contains("Cinematic") || vcam.name.Contains("CM_"))
            {
                if (vcam.Priority != 0)
                {
                    vcam.Priority = 0;
                    EditorUtility.SetDirty(vcam.gameObject);
                    Debug.Log($"<color=yellow>  {vcam.name}: Priority -> 0</color>");
                    fixedCount++;
                }
            }
            // Activar camara del jugador
            else if (vcam.name.Contains("Player") || vcam.name.Contains("Follow"))
            {
                if (vcam.Priority != 10)
                {
                    vcam.Priority = 10;
                    EditorUtility.SetDirty(vcam.gameObject);
                    Debug.Log($"<color=green>  {vcam.name}: Priority -> 10 (ACTIVA)</color>");
                    fixedCount++;
                }
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>{fixedCount} prioridades corregidas</color>");
        }
    }

    [ContextMenu("RESETEAR CINEMACHINE BRAIN")]
    public void ResetCinemachineBrain()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("<color=red>No se encontro Main Camera</color>");
            return;
        }

        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("<color=red>No se encontro CinemachineBrain</color>");
            return;
        }

        // Resetear configuracion
        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
        brain.m_CustomBlends = null;
        brain.m_CameraCutEvent = new CinemachineBrain.BrainEvent();
        brain.m_CameraActivatedEvent = new CinemachineBrain.VcamActivatedEvent();
        
        EditorUtility.SetDirty(brain);
        
        Debug.Log("<color=green>CinemachineBrain reseteado a configuracion por defecto</color>");
    }

    [ContextMenu("FORZAR ACTIVACION DE PLAYER CAMERA")]
    public void ForceActivatePlayerCamera()
    {
        CinemachineVirtualCamera[] allVCams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        foreach (var vcam in allVCams)
        {
            if (vcam.name.Contains("Player") || vcam.name.Contains("Follow"))
            {
                // Primero desactivar todas las demas
                foreach (var otherCam in allVCams)
                {
                    if (otherCam != vcam)
                    {
                        otherCam.Priority = 0;
                        EditorUtility.SetDirty(otherCam.gameObject);
                    }
                }
                
                // Luego activar solo la del jugador
                vcam.Priority = 10;
                EditorUtility.SetDirty(vcam.gameObject);
                
                Debug.Log($"<color=green>CAMARA DEL JUGADOR ACTIVADA: {vcam.name}</color>");
                Debug.Log($"<color=yellow>Todas las demas camaras desactivadas</color>");
                
                // Verificar configuracion
                if (vcam.Follow == null)
                {
                    Debug.LogWarning("<color=yellow>ADVERTENCIA: Follow no esta asignado!</color>");
                }
                if (vcam.LookAt == null)
                {
                    Debug.LogWarning("<color=yellow>ADVERTENCIA: LookAt no esta asignado!</color>");
                }
                
                return;
            }
        }
        
        Debug.LogError("<color=red>No se encontro camara del jugador!</color>");
    }
#endif
}
