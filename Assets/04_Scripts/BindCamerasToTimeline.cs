using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Playables;
using Cinemachine;

public class BindCamerasToTimeline : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("🔧 BINDEAR CÁMARAS AL TIMELINE")]
    public void BindCameras()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Bind] Vinculando cámaras al Timeline...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // Buscar Timeline
        GameObject timelineObj = GameObject.Find("BossCinematicTimeline");
        if (timelineObj == null)
        {
            Debug.LogError("<color=red>[Bind] ❌ No se encontró BossCinematicTimeline!</color>");
            return;
        }

        PlayableDirector director = timelineObj.GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogError("<color=red>[Bind] ❌ No hay PlayableDirector!</color>");
            return;
        }

        // Buscar Main Camera y agregar/verificar Cinemachine Brain
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
                Debug.Log("<color=green>[Bind] ✅ Cinemachine Brain agregado a Main Camera</color>");
            }
            else
            {
                Debug.Log("<color=green>[Bind] ✅ Main Camera ya tiene Cinemachine Brain</color>");
            }

            // Configurar el brain
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
            brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
            
            // Activar el brain si está desactivado
            brain.enabled = true;
        }
        else
        {
            Debug.LogError("<color=red>[Bind] ❌ No se encontró Main Camera!</color>");
            return;
        }

        // Buscar cámaras
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent == null)
        {
            Debug.LogError("<color=red>[Bind] ❌ No se encontró BossCinematicCameras!</color>");
            return;
        }

        CinemachineVirtualCamera[] cameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        if (cameras.Length == 0)
        {
            Debug.LogError("<color=red>[Bind] ❌ No hay cámaras virtuales!</color>");
            return;
        }

        Debug.Log($"<color=green>[Bind] ✅ Encontradas {cameras.Length} cámaras</color>");

        // Activar todas las cámaras
        foreach (var cam in cameras)
        {
            cam.gameObject.SetActive(true);
            cam.Priority = 10;
            Debug.Log($"<color=green>[Bind]    • {cam.name} - Priority: {cam.Priority}</color>");
        }

        // Guardar cambios
        EditorUtility.SetDirty(director);
        if (mainCam != null) EditorUtility.SetDirty(mainCam.gameObject);
        foreach (var cam in cameras)
        {
            EditorUtility.SetDirty(cam.gameObject);
        }

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=green>[Bind] ✅ PROCESO COMPLETADO!</color>");
        Debug.Log("<color=yellow>[Bind] AHORA HAZ ESTO MANUALMENTE:</color>");
        Debug.Log("<color=white>1. Abre Window > Sequencing > Timeline</color>");
        Debug.Log("<color=white>2. Selecciona BossCinematicTimeline en Hierarchy</color>");
        Debug.Log("<color=white>3. En el Timeline, mira el track de cámaras</color>");
        Debug.Log("<color=white>4. Haz doble-click en cada bloque de cámara</color>");
        Debug.Log("<color=white>5. En Inspector, busca 'Virtual Camera'</color>");
        Debug.Log("<color=white>6. Arrastra la cámara correspondiente de Hierarchy</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // Abrir Timeline
        Selection.activeGameObject = timelineObj;
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");

        EditorUtility.DisplayDialog(
            "Cámaras Preparadas ✅",
            "Las cámaras están configuradas.\n\n" +
            "ÚLTIMO PASO:\n" +
            "1. En el Timeline que se abrió\n" +
            "2. Haz DOBLE-CLICK en cada bloque de cámara\n" +
            "3. En el Inspector, arrastra la cámara correspondiente al campo 'Virtual Camera'\n\n" +
            "O prueba con: 'ARREGLO ALTERNATIVO' si esto no funciona.",
            "Entendido"
        );
    }

    [ContextMenu("⚡ ARREGLO ALTERNATIVO - Recrear Timeline")]
    public void RecreateTimelineBinding()
    {
        Debug.Log("<color=magenta>[Arreglo] Intentando arreglo alternativo...</color>");

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("<color=red>[Arreglo] No se encontró Main Camera!</color>");
            return;
        }

        // Asegurar Cinemachine Brain
        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
        }

        brain.m_ShowDebugText = true; // Activar debug
        brain.enabled = true;

        Debug.Log("<color=green>[Arreglo] ✅ Cinemachine Brain configurado con debug</color>");

        // Activar todas las cámaras con mayor prioridad
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent != null)
        {
            CinemachineVirtualCamera[] cameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
            
            // Desactivar todas primero
            foreach (var cam in cameras)
            {
                cam.Priority = 0;
            }

            // Activar solo la primera para test
            if (cameras.Length > 0)
            {
                cameras[0].Priority = 100;
                cameras[0].gameObject.SetActive(true);
                Debug.Log($"<color=green>[Arreglo] ✅ Cámara de prueba activada: {cameras[0].name}</color>");
                Debug.Log($"<color=yellow>[Arreglo] Deberías ver esta cámara AHORA en Game view</color>");
                
                EditorUtility.SetDirty(cameras[0].gameObject);
            }

            EditorUtility.SetDirty(mainCam.gameObject);
        }

        Debug.Log("<color=magenta>[Arreglo] Si ves la cámara en Game view, el sistema funciona!</color>");
        Debug.Log("<color=yellow>[Arreglo] El problema es el binding del Timeline.</color>");
    }

    [ContextMenu("🎮 TEST: Ver cámara en Game View")]
    public void TestCameraInGameView()
    {
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent == null) return;

        CinemachineVirtualCamera[] cameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        
        if (cameras.Length > 0)
        {
            // Desactivar todas
            foreach (var cam in cameras)
            {
                cam.Priority = 0;
            }

            // Activar la primera con alta prioridad
            cameras[0].Priority = 999;
            
            Debug.Log($"<color=green>[Test] Cámara {cameras[0].name} activada con prioridad 999</color>");
            Debug.Log($"<color=yellow>[Test] Mira la GAME VIEW - deberías ver desde esta cámara</color>");
            Debug.Log($"<color=yellow>[Test] Si funciona, el problema es solo el Timeline binding</color>");
        }
    }
#endif
}
