using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// Script para ayudarte a configurar las cámaras en el Timeline
/// </summary>
public class AutoAddCamerasToTimeline : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("🎬 PREPARAR CÁMARAS Y ABRIR TIMELINE")]
    public void PrepareAndOpenTimeline()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Timeline Helper] PREPARANDO CÁMARAS...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // Verificar Timeline
        GameObject timelineObj = GameObject.Find("BossCinematicTimeline");
        if (timelineObj == null)
        {
            Debug.LogError("<color=red>[Timeline Helper] ❌ No se encontró BossCinematicTimeline!</color>");
            return;
        }

        PlayableDirector director = timelineObj.GetComponent<PlayableDirector>();
        if (director == null || director.playableAsset == null)
        {
            Debug.LogError("<color=red>[Timeline Helper] ❌ Timeline no configurado correctamente!</color>");
            return;
        }

        // Verificar cámaras
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent == null)
        {
            Debug.LogError("<color=red>[Timeline Helper] ❌ No se encontró BossCinematicCameras!</color>");
            return;
        }

        CinemachineVirtualCamera[] cameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        if (cameras.Length == 0)
        {
            Debug.LogError("<color=red>[Timeline Helper] ❌ No hay cámaras virtuales!</color>");
            return;
        }

        Debug.Log($"<color=green>[Timeline Helper] ✅ Timeline encontrado: {director.playableAsset.name}</color>");
        Debug.Log($"<color=green>[Timeline Helper] ✅ {cameras.Length} cámaras encontradas:</color>");
        
        for (int i = 0; i < cameras.Length; i++)
        {
            Debug.Log($"<color=green>   {i+1}. {cameras[i].name}</color>");
        }

        // Verificar Main Camera con Cinemachine Brain
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                Debug.LogWarning("<color=yellow>[Timeline Helper] ⚠️ Main Camera no tiene Cinemachine Brain, agregándolo...</color>");
                brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
                Debug.Log("<color=green>[Timeline Helper] ✅ Cinemachine Brain agregado!</color>");
            }
            else
            {
                Debug.Log("<color=green>[Timeline Helper] ✅ Main Camera tiene Cinemachine Brain</color>");
            }
        }

        // Seleccionar el Timeline para facilitar la edición
        Selection.activeGameObject = timelineObj;
        
        // Abrir ventana de Timeline
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=yellow>[Timeline Helper] 📋 INSTRUCCIONES:</color>");
        Debug.Log("<color=yellow>========================================</color>");
        Debug.Log("<color=white>1. La ventana de Timeline debería abrirse ahora</color>");
        Debug.Log("<color=white>2. Busca el track 'Cinemachine Track' o crea uno:</color>");
        Debug.Log("<color=white>   - Click derecho en el área vacía del Timeline</color>");
        Debug.Log("<color=white>   - Selecciona: Cinemachine Track</color>");
        Debug.Log("");
        Debug.Log("<color=white>3. En la Hierarchy, expande 'BossCinematicCameras'</color>");
        Debug.Log("");
        Debug.Log("<color=white>4. ARRASTRA cada cámara al track en estos tiempos:</color>");
        Debug.Log("<color=cyan>   • CM_BossIntro_Wide          → 0s a 3s</color>");
        Debug.Log("<color=cyan>   • CM_BossIntro_CloseUp       → 3s a 5s</color>");
        Debug.Log("<color=cyan>   • CM_BossIntro_Dramatic      → 5s a 7s</color>");
        Debug.Log("<color=cyan>   • CM_BossIntro_PlayerReaction → 7s a 9s</color>");
        Debug.Log("");
        Debug.Log("<color=white>5. Click Play ▶️ en el Timeline para probar</color>");
        Debug.Log("<color=yellow>========================================</color>");

        EditorUtility.DisplayDialog(
            "Timeline Preparado ✅",
            "¡Todo listo para agregar las cámaras!\n\n" +
            "SIGUIENTE PASO:\n" +
            "1. En el Timeline que se acaba de abrir\n" +
            "2. Si no hay 'Cinemachine Track', créalo (click derecho > Cinemachine Track)\n" +
            "3. Arrastra las cámaras de 'BossCinematicCameras' al track\n" +
            "4. Revisa la Console para ver los tiempos exactos\n\n" +
            "¡Consulta la Console para instrucciones detalladas!",
            "Entendido"
        );
    }

    [ContextMenu("📋 MOSTRAR GUÍA PASO A PASO")]
    public void ShowStepByStepGuide()
    {
        Debug.Log("<color=magenta>========================================</color>");
        Debug.Log("<color=magenta>📋 GUÍA: CÓMO AGREGAR CÁMARAS AL TIMELINE</color>");
        Debug.Log("<color=magenta>========================================</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>PASO 1: Abrir Timeline</color>");
        Debug.Log("<color=white>  • Selecciona 'BossCinematicTimeline' en Hierarchy</color>");
        Debug.Log("<color=white>  • Window > Sequencing > Timeline</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>PASO 2: Crear Cinemachine Track (si no existe)</color>");
        Debug.Log("<color=white>  • Click derecho en área vacía del Timeline</color>");
        Debug.Log("<color=white>  • Selecciona: 'Cinemachine Track'</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>PASO 3: Expandir BossCinematicCameras</color>");
        Debug.Log("<color=white>  • En Hierarchy, busca 'BossCinematicCameras'</color>");
        Debug.Log("<color=white>  • Click en la flecha para expandir</color>");
        Debug.Log("<color=white>  • Verás 4 cámaras: Wide, CloseUp, Dramatic, PlayerReaction</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>PASO 4: ARRASTRAR Cámaras al Timeline</color>");
        Debug.Log("<color=cyan>  CÁMARA 1: CM_BossIntro_Wide</color>");
        Debug.Log("<color=white>    1. Click y MANTÉN en 'CM_BossIntro_Wide'</color>");
        Debug.Log("<color=white>    2. Arrastra hacia la ventana de Timeline</color>");
        Debug.Log("<color=white>    3. Suelta en el Cinemachine Track en el segundo 0</color>");
        Debug.Log("<color=white>    4. Aparecerá un bloque, estira el borde hasta el segundo 3</color>");
        Debug.Log("");
        Debug.Log("<color=cyan>  CÁMARA 2: CM_BossIntro_CloseUp</color>");
        Debug.Log("<color=white>    1. Arrastra al track en el segundo 3</color>");
        Debug.Log("<color=white>    2. Estira hasta el segundo 5</color>");
        Debug.Log("");
        Debug.Log("<color=cyan>  CÁMARA 3: CM_BossIntro_Dramatic</color>");
        Debug.Log("<color=white>    1. Arrastra al segundo 5</color>");
        Debug.Log("<color=white>    2. Estira hasta el segundo 7</color>");
        Debug.Log("");
        Debug.Log("<color=cyan>  CÁMARA 4: CM_BossIntro_PlayerReaction</color>");
        Debug.Log("<color=white>    1. Arrastra al segundo 7</color>");
        Debug.Log("<color=white>    2. Estira hasta el segundo 9</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>PASO 5: Probar</color>");
        Debug.Log("<color=white>  • Click Play ▶️ en la ventana de Timeline</color>");
        Debug.Log("<color=white>  • Deberías ver las cámaras cambiar</color>");
        Debug.Log("");
        Debug.Log("<color=green>¡Eso es todo! Muy simple una vez que lo haces 😊</color>");
        Debug.Log("<color=magenta>========================================</color>");
    }

    [ContextMenu("ℹ️ VERIFICAR ESTADO ACTUAL")]
    public void VerifyCurrentState()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>VERIFICANDO ESTADO DEL SETUP...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        bool allGood = true;

        // Timeline
        GameObject timelineObj = GameObject.Find("BossCinematicTimeline");
        if (timelineObj != null)
        {
            PlayableDirector director = timelineObj.GetComponent<PlayableDirector>();
            if (director != null && director.playableAsset != null)
            {
                Debug.Log($"<color=green>✅ Timeline: {director.playableAsset.name}</color>");
            }
            else
            {
                Debug.LogError("<color=red>❌ Timeline sin PlayableAsset</color>");
                allGood = false;
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ BossCinematicTimeline no encontrado</color>");
            allGood = false;
        }

        // Cámaras
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent != null)
        {
            CinemachineVirtualCamera[] cameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
            Debug.Log($"<color=green>✅ {cameras.Length} cámaras encontradas</color>");
            foreach (var cam in cameras)
            {
                Debug.Log($"   • {cam.name}");
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ BossCinematicCameras no encontrado</color>");
            allGood = false;
        }

        // Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                Debug.Log("<color=green>✅ Main Camera con Cinemachine Brain</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>⚠️ Main Camera sin Cinemachine Brain</color>");
            }
        }

        // BossSpawner
        BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
        if (spawner != null)
        {
            Debug.Log("<color=green>✅ BossSpawner encontrado</color>");
            Debug.Log($"   • Timeline asignado: {(spawner.cinematicTimeline != null ? "SÍ" : "NO")}");
            Debug.Log($"   • Play cinematic: {spawner.playCinematicBeforeSpawn}");
        }

        Debug.Log("<color=cyan>========================================</color>");
        if (allGood)
        {
            Debug.Log("<color=green>✅ Setup básico correcto</color>");
            Debug.Log("<color=yellow>⚠️ Ahora necesitas agregar las cámaras al Timeline manualmente</color>");
            Debug.Log("<color=yellow>   Usa: 'PREPARAR CÁMARAS Y ABRIR TIMELINE'</color>");
        }
        Debug.Log("<color=cyan>========================================</color>");
    }
#endif
}
