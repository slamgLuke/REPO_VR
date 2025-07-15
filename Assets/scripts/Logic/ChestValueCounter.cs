using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ChestValueCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI totalValueText;

    [Header("Fade Config")]
    [SerializeField] private Image fadeScreen; // Imagen negra en pantalla
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private float targetValueToWin = 1000f;

    private List<DestructibleObject> detectedObjects = new List<DestructibleObject>();
    private bool gameEnded = false;

    void Start()
    {
        // Solo establecer como NO_WIN si no es una transición de victoria
        if (PlayerPrefs.GetString("GAME_STATUS", "") != "WIN")
        {
            PlayerPrefs.SetString("GAME_STATUS", "NO_WIN");
            PlayerPrefs.Save();
        }

        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("El collider no estaba marcado como trigger. Se activará automáticamente.");
            col.isTrigger = true;
        }

        if (fadeScreen != null)
        {
            fadeScreen.color = new Color(0, 0, 0, 0); // transparente al inicio
        }

        UpdateTotalValue();
    }

    private void OnDestroy()
    {
        foreach (var obj in detectedObjects)
        {
            if (obj != null)
            {
                obj.OnValueChanged -= HandleObjectValueChanged;
            }
        }
        detectedObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null && !detectedObjects.Contains(destructible))
        {
            detectedObjects.Add(destructible);
            destructible.OnValueChanged += HandleObjectValueChanged;
            UpdateTotalValue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null && detectedObjects.Contains(destructible))
        {
            destructible.OnValueChanged -= HandleObjectValueChanged;
            detectedObjects.Remove(destructible);
            UpdateTotalValue();
        }
    }

    private void HandleObjectValueChanged(float newValue)
    {
        UpdateTotalValue();
    }

    private void UpdateTotalValue()
    {
        float currentTotal = detectedObjects.Sum(obj => obj.Value);

        if (totalValueText != null)
        {
            totalValueText.text = $"${(int)currentTotal}";
        }

        if (!gameEnded && currentTotal >= targetValueToWin)
        {
            gameEnded = true;
            StartCoroutine(FadeAndLoadMenu());
        }
    }

    private IEnumerator FadeAndLoadMenu()
    {
        if (fadeScreen == null)
        {
            Debug.LogError("Falta el Image de fadeScreen.");
            yield break;
        }

        float timer = 0f;
        fadeScreen.gameObject.SetActive(true);

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeScreen.color = Color.black;

        // GUARDA que el jugador ganó antes de cargar el menú
        PlayerPrefs.SetString("GAME_STATUS", "WIN");
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(menuSceneName);
    }
}
