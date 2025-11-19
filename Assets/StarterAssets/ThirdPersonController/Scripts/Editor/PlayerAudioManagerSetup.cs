using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace StarterAssets
{
    /// <summary>
    /// Utilidad de Editor para crear automáticamente el PlayerAudioManager en la escena
    /// </summary>
    [InitializeOnLoad]
    public static class PlayerAudioManagerSetup
    {
        static PlayerAudioManagerSetup()
        {
            // Forzar refresco de assets para asegurar que Unity compile los scripts nuevos
            try
            {
                UnityEditor.AssetDatabase.Refresh();
            }
            catch
            {
                // Ignorar si no estamos en un contexto donde AssetDatabase está disponible
            }

            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            // Verificar si ya existe un PlayerAudioManager en la escena
            PlayerAudioManager existingManager = Object.FindObjectOfType<PlayerAudioManager>();

            if (existingManager == null)
            {
                // Preguntar al usuario si desea crear uno
                bool create = EditorUtility.DisplayDialog(
                    "PlayerAudioManager no encontrado",
                    "La escena no tiene un PlayerAudioManager.\n\n¿Deseas crear uno automáticamente?",
                    "Sí, crear",
                    "No, lo haré después"
                );

                if (create)
                {
                    CreatePlayerAudioManager();
                }
            }
        }

        [MenuItem("GameObject/Audio/Player Audio Manager", false, 10)]
        public static void CreatePlayerAudioManager()
        {
            // Crear el GameObject
            GameObject audioManagerObject = new GameObject("PlayerAudioManager");

            // Agregar el componente
            PlayerAudioManager manager = audioManagerObject.AddComponent<PlayerAudioManager>();

            // Seleccionar el objeto para que el usuario lo vea en el Inspector
            Selection.activeGameObject = audioManagerObject;

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("✓ PlayerAudioManager creado correctamente. Ahora configura los AudioClips en el Inspector.");

            // Mostrar mensaje de ayuda
            EditorUtility.DisplayDialog(
                "PlayerAudioManager Creado",
                "El PlayerAudioManager se ha creado exitosamente.\n\n" +
                "SIGUIENTE PASO:\n" +
                "1. Selecciona el GameObject 'PlayerAudioManager' en la jerarquía\n" +
                "2. Arrastra tus AudioClips a los campos correspondientes en el Inspector\n" +
                "3. Guarda la escena (Ctrl+S)\n\n" +
                "Consulta PLAYER_AUDIO_MANAGER_GUIDE.md para más detalles.",
                "Entendido"
            );
        }
    }
}
