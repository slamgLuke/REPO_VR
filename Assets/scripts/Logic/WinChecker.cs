using UnityEngine;

public class WinChecker : MonoBehaviour
{
    public GameObject winIndicator;

    void Start()
    {
        GameState.InitializeIfNeeded();

        if (GameState.GetStatus() == GameState.Status.WIN)
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
