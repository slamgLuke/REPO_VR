using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Collider))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Value Settings")]
    public float fullHealthValue = 100f;
    public float zeroHealthValue = 0f;

    public float Value => Mathf.Lerp(zeroHealthValue, fullHealthValue, currentHealth / maxHealth);

    [Header("Damage Resistance")]
    [Tooltip("Mayor valor = más resistente. Ej: 2 reduce el daño a la mitad.")]
    public float collisionResistance = 1f;

    public event Action<float> OnValueChanged;

    // UI
    private GameObject canvasGO;
    private TextMeshProUGUI valueText;

    void Start()
    {
        currentHealth = maxHealth;

        CreateValueLabel();
        UpdateValueLabel();

        OnValueChanged?.Invoke(Value);
    }

    void Update()
    {
        if (canvasGO != null && Camera.main != null)
        {
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Vector3 center = col.bounds.center;
            Vector3 toCamera = (Camera.main.transform.position - center).normalized;

            float offset = col.bounds.extents.magnitude * 0.15f;
            canvasGO.transform.position = center + toCamera * offset;

            // Billboard horizontal
            Vector3 lookDir = Camera.main.transform.position - canvasGO.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                canvasGO.transform.rotation = Quaternion.LookRotation(lookDir);
                canvasGO.transform.Rotate(0, 180f, 0);
            }

            // 🔒 Escala fija
            canvasGO.transform.localScale = Vector3.one * 0.1f;
        }

        if (currentHealth <= 0f)
        {
            if (canvasGO != null) Destroy(canvasGO);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (canvasGO != null) Destroy(canvasGO);
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > 2f)
        {
            float baseDamage = impactForce * 2f;
            float adjustedDamage = baseDamage / Mathf.Max(collisionResistance, 0.01f); // evitar div/0
            TakeDamage(adjustedDamage);
        }
    }

    void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}. Value: {Value}");

        OnValueChanged?.Invoke(Value);
        UpdateValueLabel();
    }

    public float GetCurrentValue()
    {
        return Value;
    }

    private void UpdateValueLabel()
    {
        if (valueText != null)
        {
            valueText.text = $"${(int)Value}";
        }
    }
    private void CreateValueLabel()
    {
        // Crear Canvas
        canvasGO = new GameObject("ValueCanvas");
        canvasGO.transform.SetParent(null); // canvas está fuera de jerarquía
        // Posición: al frente en local Z
        canvasGO.transform.localPosition = new Vector3(0f, 0f, 0f);
        // Rotación: alineada con el mundo, no inclinada
        canvasGO.transform.localRotation = Quaternion.identity;
        // Escala razonable
        canvasGO.transform.localScale = Vector3.one * 1f;

        // Configurar Canvas
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(10f, 1f);

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fondo (opcional)
        //var bg = new GameObject("Background");
        //bg.transform.SetParent(canvasGO.transform, false);
        //var img = bg.AddComponent<UnityEngine.UI.Image>();
        //img.color = new Color(0, 0, 0, 0F);
        //var bgRect = bg.GetComponent<RectTransform>();
        //bgRect.anchorMin = Vector2.zero;
        //bgRect.anchorMax = Vector2.one;
        //bgRect.offsetMin = Vector2.zero;
        //bgRect.offsetMax = Vector2.zero;

        // Texto
        var textGO = new GameObject("ValueText");
        textGO.transform.SetParent(canvasGO.transform, false);
        valueText = textGO.AddComponent<TextMeshProUGUI>();
        valueText.text = "$ 0";
        valueText.fontSize = 1;                 // ajuste fino
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = Color.green;
        valueText.fontMaterial.renderQueue = 5000; // como Overlay

        Material overlayMat = new Material(Shader.Find("TextMeshPro/Distance Field Overlay"));
        overlayMat.mainTexture = valueText.font.material.mainTexture; // usar textura de la fuente

        valueText.fontSharedMaterial = overlayMat;

        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
    }
}
