using UnityEngine;
using Cinemachine;

public class AddVirtualCameraComponents : MonoBehaviour
{
    [ContextMenu("🎥 AGREGAR COMPONENTES CINEMACHINE A TODAS LAS CÁMARAS")]
    public void AddCinemachineComponents()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Camera Fix] Agregando componentes Cinemachine...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent == null)
        {
            Debug.LogError("<color=red>[Camera Fix] ❌ No se encontró BossCinematicCameras!</color>");
            return;
        }

        // Configuración para cada cámara
        string[] cameraNames = { "CM_BossIntro_Wide", "CM_BossIntro_CloseUp", "CM_BossIntro_Dramatic", "CM_BossIntro_PlayerReact" };
        float[] fovValues = { 60f, 45f, 55f, 50f };

        for (int i = 0; i < cameraNames.Length; i++)
        {
            Transform camTransform = camerasParent.transform.Find(cameraNames[i]);
            if (camTransform == null)
            {
                Debug.LogWarning($"<color=yellow>[Camera Fix] ⚠️ No se encontró {cameraNames[i]}</color>");
                continue;
            }

            GameObject camObj = camTransform.gameObject;
            
            // Verificar si ya tiene el componente
            CinemachineVirtualCamera vcam = camObj.GetComponent<CinemachineVirtualCamera>();
            
            if (vcam == null)
            {
                // Agregar el componente
                vcam = camObj.AddComponent<CinemachineVirtualCamera>();
                Debug.Log($"<color=green>[Camera Fix] ✅ Componente agregado a: {cameraNames[i]}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>[Camera Fix] ⚠️ {cameraNames[i]} ya tiene el componente</color>");
            }

            // Configurar la cámara
            vcam.Priority = 10;
            vcam.m_Lens.FieldOfView = fovValues[i];
            
            // Asegurar que esté activa
            camObj.SetActive(true);

            Debug.Log($"<color=green>[Camera Fix]    • {cameraNames[i]} - FOV: {fovValues[i]}, Priority: {vcam.Priority}</color>");
        }

        // Verificar Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
                Debug.Log("<color=green>[Camera Fix] ✅ Cinemachine Brain agregado a Main Camera</color>");
            }
            
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
            brain.m_ShowDebugText = true; // Para ver qué cámara está activa
            brain.enabled = true;
        }

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=green>[Camera Fix] ✅ ¡COMPONENTES AGREGADOS!</color>");
        Debug.Log("<color=yellow>[Camera Fix] Ahora las cámaras deberían funcionar en el Timeline</color>");
        Debug.Log("<color=cyan>========================================</color>");

#if UNITY_EDITOR
        // Guardar cambios
        UnityEditor.EditorUtility.SetDirty(camerasParent);
        if (mainCam != null) UnityEditor.EditorUtility.SetDirty(mainCam.gameObject);
#endif
    }

    [ContextMenu("ℹ️ VERIFICAR CÁMARAS")]
    public void VerifyCameras()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>VERIFICANDO CÁMARAS...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent == null)
        {
            Debug.LogError("<color=red>❌ BossCinematicCameras no encontrado</color>");
            return;
        }

        CinemachineVirtualCamera[] vcams = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        
        if (vcams.Length == 0)
        {
            Debug.LogError("<color=red>❌ NO HAY componentes CinemachineVirtualCamera!</color>");
            Debug.LogError("<color=red>   Usa: 'AGREGAR COMPONENTES CINEMACHINE A TODAS LAS CÁMARAS'</color>");
        }
        else
        {
            Debug.Log($"<color=green>✅ {vcams.Length} cámaras virtuales encontradas:</color>");
            foreach (var vcam in vcams)
            {
                Debug.Log($"<color=green>   • {vcam.name} - Priority: {vcam.Priority}, FOV: {vcam.m_Lens.FieldOfView}</color>");
            }
        }

        // Verificar Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                Debug.Log($"<color=green>✅ Main Camera tiene Cinemachine Brain (Enabled: {brain.enabled})</color>");
            }
            else
            {
                Debug.LogError("<color=red>❌ Main Camera NO tiene Cinemachine Brain!</color>");
            }
        }

        Debug.Log("<color=cyan>========================================</color>");
    }
}
