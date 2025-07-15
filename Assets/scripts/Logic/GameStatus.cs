using UnityEngine;

public static class GameState
{
    private const string STATUS_KEY = "GAME_STATUS";
    private const string INIT_KEY = "GAME_INITIALIZED";

    public enum Status
    {
        NO_WIN,
        WIN
    }

    /// <summary>
    /// Se asegura de que el estado del juego se inicializa solo una vez.
    /// </summary>
    public static void InitializeIfNeeded()
    {
        if (!PlayerPrefs.HasKey(INIT_KEY))
        {
            Debug.Log("[GameState] Inicializando por primera vez...");
            SetStatus(Status.NO_WIN);
            PlayerPrefs.SetInt(INIT_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Guarda el estado actual del juego.
    /// </summary>
    public static void SetStatus(Status newStatus)
    {
        PlayerPrefs.SetString(STATUS_KEY, newStatus.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Obtiene el estado actual del juego. Por defecto es NO_WIN.
    /// </summary>
    public static Status GetStatus()
    {
        string value = PlayerPrefs.GetString(STATUS_KEY, Status.NO_WIN.ToString());
        if (System.Enum.TryParse(value, out Status status))
            return status;

        return Status.NO_WIN;
    }

    /// <summary>
    /// Limpia todo el estado (por ejemplo, al reiniciar el juego).
    /// </summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(STATUS_KEY);
        PlayerPrefs.DeleteKey(INIT_KEY);
        PlayerPrefs.Save();
    }
}
