using UnityEngine;

public class WinChecker : MonoBehaviour
{
    [Header("Opcional")]
    [Tooltip("Este objeto solo se activa si el jugador ha ganado.")]
    public GameObject winIndicator;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("GAME_INITIALIZED"))
        {
            Debug.Log("Juego iniciado por primera vez. Reiniciando estado.");
            PlayerPrefs.SetString("GAME_STATUS", "NO_WIN");
            PlayerPrefs.SetInt("GAME_INITIALIZED", 1);
            PlayerPrefs.Save();
        }
    }
    void Start()
    {
        string status = PlayerPrefs.GetString("GAME_STATUS", "NO_WIN");
        Debug.Log(status);
        Debug.Log("Estado cargado: " + status);

        if (status == "WIN")
        {
            if (winIndicator != null)
                winIndicator.SetActive(true);
        }
        else
        {
            if (winIndicator != null)
                winIndicator.SetActive(false);
        }
    }
}
