using UnityEngine;
using UnityEditor;
using Cinemachine;

#if UNITY_EDITOR
/// <summary>
/// Script para arreglar el problema "No cameras rendering" automáticamente
/// </summary>
public class FixCinematicCameras : MonoBehaviour
{
    [ContextMenu("🔧 FIX: Agregar Cinemachine Brain a Main Camera")]
    public void AddCinemachineBrainToMainCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>[Fix] No se encontró Main Camera!</color>");
            return;
        }

        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        
        if (brain == null)
        {
            brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
            Debug.Log("<color=green>[Fix] ✅ Cinemachine Brain agregado a Main Camera!</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[Fix] Cinemachine Brain ya existe en Main Camera</color>");
        }

        // Configurar el brain
        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
        brain.m_DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Style.EaseInOut, 1.5f);

        Debug.Log("<color=green>[Fix] Cinemachine Brain configurado correctamente!</color>");
    }

    [ContextMenu("🔧 FIX: Configurar todas las Virtual Cameras")]
    public void ConfigureAllVirtualCameras()
    {
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        
        if (camerasParent == null)
        {
            Debug.LogError("<color=red>[Fix] No se encontró BossCinematicCameras!</color>");
            return;
        }

        CinemachineVirtualCamera[] vcams = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        
        if (vcams.Length == 0)
        {
            Debug.LogError("<color=red>[Fix] No se encontraron Virtual Cameras!</color>");
            return;
        }

        foreach (var vcam in vcams)
        {
            vcam.Priority = 10;
            Debug.Log($"<color=green>[Fix] ✅ {vcam.name} - Priority set to 10</color>");
        }

        Debug.Log($"<color=green>[Fix] {vcams.Length} cámaras configuradas!</color>");
    }

    [ContextMenu("🔧 FIX: Verificar Setup Completo")]
    public void VerifyCompleteSetup()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Fix] VERIFICANDO SETUP COMPLETO...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        bool allGood = true;

        // 1. Verificar Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                Debug.Log("<color=green>✅ Main Camera tiene Cinemachine Brain</color>");
            }
            else
            {
                Debug.LogError("<color=red>❌ Main Camera NO tiene Cinemachine Brain!</color>");
                Debug.LogWarning("<color=yellow>   → Usa: 'FIX: Agregar Cinemachine Brain a Main Camera'</color>");
                allGood = false;
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ No se encontró Main Camera!</color>");
            allGood = false;
        }

        // 2. Verificar Virtual Cameras
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent != null)
        {
            CinemachineVirtualCamera[] vcams = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
            if (vcams.Length > 0)
            {
                Debug.Log($"<color=green>✅ Encontradas {vcams.Length} Virtual Cameras</color>");
                foreach (var vcam in vcams)
                {
                    Debug.Log($"   - {vcam.name} (Priority: {vcam.Priority})");
                }
            }
            else
            {
                Debug.LogError("<color=red>❌ No se encontraron Virtual Cameras!</color>");
                allGood = false;
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ No se encontró BossCinematicCameras!</color>");
            allGood = false;
        }

        // 3. Verificar Timeline
        GameObject timelineObj = GameObject.Find("BossCinematicTimeline");
        if (timelineObj != null)
        {
            var director = timelineObj.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (director != null)
            {
                if (director.playableAsset != null)
                {
                    Debug.Log("<color=green>✅ Timeline tiene Playable Asset asignado</color>");
                    Debug.Log($"   - Asset: {director.playableAsset.name}");
                    Debug.Log($"   - Play On Awake: {director.playOnAwake} (debería ser FALSE)");
                    
                    if (director.playOnAwake)
                    {
                        Debug.LogWarning("<color=yellow>⚠️ Play On Awake debería estar en FALSE!</color>");
                    }
                }
                else
                {
                    Debug.LogError("<color=red>❌ Timeline NO tiene Playable Asset!</color>");
                    allGood = false;
                }
            }
            else
            {
                Debug.LogError("<color=red>❌ BossCinematicTimeline NO tiene Playable Director!</color>");
                allGood = false;
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ No se encontró BossCinematicTimeline!</color>");
            allGood = false;
        }

        // 4. Verificar BossSpawner
        BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
        if (spawner != null)
        {
            Debug.Log("<color=green>✅ BossSpawner encontrado</color>");
            
            if (spawner.cinematicTimeline != null)
            {
                Debug.Log($"   - Timeline asignado: {spawner.cinematicTimeline.gameObject.name}");
            }
            else
            {
                Debug.LogError("<color=red>❌ BossSpawner NO tiene Timeline asignado!</color>");
                allGood = false;
            }

            Debug.Log($"   - Play Cinematic Before Spawn: {spawner.playCinematicBeforeSpawn}");
            
            if (!spawner.playCinematicBeforeSpawn)
            {
                Debug.LogWarning("<color=yellow>⚠️ Play Cinematic Before Spawn está en FALSE!</color>");
            }
        }
        else
        {
            Debug.LogError("<color=red>❌ No se encontró BossSpawner!</color>");
            allGood = false;
        }

        Debug.Log("<color=cyan>========================================</color>");
        if (allGood)
        {
            Debug.Log("<color=green>✅ TODO ESTÁ CORRECTO! La cinemática debería funcionar.</color>");
            Debug.Log("<color=yellow>⚠️ IMPORTANTE: Asegúrate de agregar las cámaras al Timeline manualmente!</color>");
            Debug.Log("<color=yellow>   1. Selecciona BossCinematicTimeline</color>");
            Debug.Log("<color=yellow>   2. Window > Sequencing > Timeline</color>");
            Debug.Log("<color=yellow>   3. Arrastra las cámaras al Cinemachine Track</color>");
        }
        else
        {
            Debug.LogError("<color=red>❌ HAY PROBLEMAS! Revisa los errores arriba y usa los comandos de FIX.</color>");
        }
        Debug.Log("<color=cyan>========================================</color>");
    }

    [ContextMenu("🚀 AUTO-FIX: Reparar Todo Automáticamente")]
    public void AutoFixEverything()
    {
        Debug.Log("<color=cyan>[Auto-Fix] Iniciando reparación automática...</color>");
        
        AddCinemachineBrainToMainCamera();
        ConfigureAllVirtualCameras();
        
        Debug.Log("<color=green>[Auto-Fix] Reparación completada!</color>");
        Debug.Log("<color=yellow>[Auto-Fix] Ejecuta 'Verificar Setup Completo' para confirmar.</color>");
    }
}
#endif
