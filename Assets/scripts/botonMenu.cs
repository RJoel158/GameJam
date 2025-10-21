using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para usar botones UI

public class botonMenu : MonoBehaviour
{
    public Button playButton; // Referencia al bot�n Play

    void Start()
    {
        // Verifica si el bot�n fue asignado
        if (playButton != null)
        {
            playButton.onClick.AddListener(CargarEscena);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el bot�n Play en el Inspector.");
        }
    }

    void CargarEscena()
    {
        SceneManager.LoadScene("OpenWorldScene");
    }
}
