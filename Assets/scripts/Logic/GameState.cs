using UnityEngine;

public static class GameState
{
    private static bool initialized = false;

    public enum Status
    {
        NO_WIN,
        WIN
    }

    private static Status currentStatus = Status.NO_WIN;

    /// <summary>
    /// Se asegura de que el estado del juego se inicializa solo una vez por ejecución.
    /// </summary>
    public static void InitializeIfNeeded()
    {
        if (!initialized)
        {
            Debug.Log("[GameState] Inicializando por primera vez...");
            currentStatus = Status.NO_WIN;
            initialized = true;
        }
    }

    /// <summary>
    /// Establece el estado del juego.
    /// </summary>
    public static void SetStatus(Status newStatus)
    {
        currentStatus = newStatus;
    }

    /// <summary>
    /// Obtiene el estado actual del juego.
    /// </summary>
    public static Status GetStatus()
    {
        return currentStatus;
    }

    /// <summary>
    /// Reinicia el estado interno (no es persistente).
    /// </summary>
    public static void ResetAll()
    {
        initialized = false;
        currentStatus = Status.NO_WIN;
    }
}
