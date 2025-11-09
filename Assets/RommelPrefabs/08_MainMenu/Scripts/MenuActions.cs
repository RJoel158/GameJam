using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void NewGame()
{
    Debug.Log("Insertar lógica New Game aquí");
    if (CinematicManager.Instance != null)
    {
        CinematicManager.Instance.PlayCinematicThenLoad();
    }
    else
    {
        if (FadeTransition.Instance != null)
            FadeTransition.Instance.FadeToScene("LoadingScreen_1");
        else
            SceneManager.LoadScene("LoadingScreen_1");
    }
}




    public void LoadGame()
{
    Debug.Log("Insertar lógica Load Game aquí");

    // Comprueba que exista guardado
    if (SaveManager.Instance == null)
    {
        Debug.LogWarning("SaveManager no encontrado en la escena. Asegúrate de que existe un SaveManager en MainMenu.");
        return;
    }

    if (!SaveManager.Instance.HasSaveFile())
    {
        Debug.Log("No hay guardado. NewGame o mostrar mensaje al usuario.");
        return;
    }

    // Indica al SaveManager que aplique el guardado cuando la escena OpenWorldSceneMerged esté cargada
    SaveManager.Instance.loadOnNextOpenWorld = true;

    // Haz la transición hacia la pantalla de carga (si existe FadeTransition) o carga directamente LoadingScreen_1
    if (FadeTransition.Instance != null)
    {
        FadeTransition.Instance.FadeToScene("LoadingScreen_1");
    }
    else
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScreen_1");
    }
}


    public void Options()
    {
        Debug.Log("Insertar lógica Options aquí");
        // abrir panel de opciones
    }

    public void Exit()
    {
        Debug.Log("Insertar lógica Exit aquí");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
