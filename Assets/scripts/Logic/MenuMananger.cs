using System;
using Fusion;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject trophyObject; // El objeto trofeo en la UI o mundo

    void Start()
    {
        Debug.Log( PlayerPrefs.GetString("GAME_STATUS", ""));
        if (PlayerPrefs.GetString("GAME_STATUS","") == "WIN")
            {
                trophyObject.SetActive(true);
            }
        else
            {
                trophyObject.SetActive(false);
            }
    }
}
