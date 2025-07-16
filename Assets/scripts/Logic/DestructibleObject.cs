using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
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

    [Header("Audio Settings")]
    public AudioClip deathSound;
    public AudioClip hitSound;

    private AudioSource audioSource;

    [Header("Particles")]
    public GameObject breakParticles;

    // UI
    private GameObject canvasGO;
    private TextMeshProUGUI valueText;

    void Start()
    {
        currentHealth = maxHealth;

        // Configuración de Audio
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;

        CreateValueLabel();
        UpdateValueLabel();
        
        // Forzar render al frente para VR/Meta compatibility
        StartCoroutine(ForceRenderOnTopDelayed());

        OnValueChanged?.Invoke(Value);
    }
    
    // Coroutine para asegurar que el material se configure después de que todo esté inicializado
    private System.Collections.IEnumerator ForceRenderOnTopDelayed()
    {
        yield return new WaitForEndOfFrame();
        ForceRenderOnTop();
    }

    void Update()
    {
        if (canvasGO != null && valueText != null)
        {
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Vector3 center = col.bounds.center;
            
            // Para VR/Meta Quest: usar la cámara principal o buscar la cámara VR
            Camera vrCamera = Camera.main;
            if (vrCamera == null)
            {
                // Buscar cámara VR típica de Meta Quest
                vrCamera = FindFirstObjectByType<Camera>();
                if (vrCamera == null) return;
            }
            
            // Posicionar el canvas ligeramente adelante del centro del objeto
            Vector3 targetPosition = center;
            Vector3 directionToCamera = (vrCamera.transform.position - center).normalized;
            targetPosition += directionToCamera * 0.1f; // Mover hacia la cámara
            
            canvasGO.transform.position = targetPosition;
            
            // Billboard completo para VR - el canvas siempre mira hacia la cámara
            Vector3 lookDir = vrCamera.transform.position - canvasGO.transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                // Hacer que el canvas mire hacia la cámara CORRECTAMENTE (sin invertir)
                canvasGO.transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
            }
            
            // Escala adaptativa para VR/Meta Quest
            float distance = Vector3.Distance(vrCamera.transform.position, center);
            float scale = Mathf.Clamp(distance * 0.0005f, 0.0002f, 0.002f); // Escala muy pequeña para VR
            canvasGO.transform.localScale = Vector3.one * scale;
            
            // Mostrar/ocultar basado en si está frente a la cámara
            Vector3 dirToObject = (center - vrCamera.transform.position).normalized;
            float dotProduct = Vector3.Dot(vrCamera.transform.forward, dirToObject);
            
            if (dotProduct > 0.3f && distance < 20f) // Visible si está en el campo de visión y no muy lejos
            {
                valueText.gameObject.SetActive(true);
            }
            else
            {
                valueText.gameObject.SetActive(false);
            }
        }

        if (currentHealth <= 0f)
        {
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            if (breakParticles != null) Instantiate(breakParticles, transform.position, transform.rotation);

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
            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
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
        // Crear Canvas usando World Space - optimizado para Meta Quest/VR
        canvasGO = new GameObject("ValueCanvas");
        canvasGO.transform.SetParent(null); // canvas está fuera de jerarquía
        
        // Configurar Canvas como World Space para VR/Meta Quest
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 32767; // Máximo sorting order
        canvas.overrideSorting = true; // Importante para VR/Meta
        canvas.sortingLayerName = "Default";
        canvas.planeDistance = 0.1f; // Muy cerca de la cámara
        
        // Configurar RectTransform del Canvas para VR
        var canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.3f); // Tamaño en world units
        
        // Posicionar ligeramente adelante del objeto para evitar Z-fighting
        canvasGO.transform.position = transform.position + transform.forward * 0.1f;
        canvasGO.transform.localScale = Vector3.one * 0.001f; // Escala muy pequeña para VR
        
        // Texto
        var textGO = new GameObject("ValueText");
        textGO.transform.SetParent(canvasGO.transform, false);
        valueText = textGO.AddComponent<TextMeshProUGUI>();
        valueText.text = "$ 0";
        valueText.fontSize = 100f; // Tamaño grande para compensar la escala pequeña
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = Color.green;
        
        // Configurar fuente para VR/Meta Quest
        var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("LiberationSans SDF");
        if (defaultFont != null)
        {
            valueText.font = defaultFont;
        }
        
        // Configurar material específico para VR/Meta Quest
        if (valueText.fontSharedMaterial != null)
        {
            // Crear una copia del material para poder modificarlo
            Material vrMaterial = new Material(valueText.fontSharedMaterial);
            vrMaterial.shader = Shader.Find("TextMeshPro/Mobile/Distance Field Overlay");
            
            // FORZAR que se renderice SIEMPRE al frente
            vrMaterial.renderQueue = 5000; // Render queue MUY alto
            vrMaterial.SetInt("_ZTest", 0); // ALWAYS render - ignorar depth
            vrMaterial.SetInt("_ZWrite", 0); // No escribir al depth buffer
            
            // Configuraciones adicionales para forzar render al frente
            if (vrMaterial.HasProperty("_ZTestMode"))
            {
                vrMaterial.SetFloat("_ZTestMode", 0f); // Always render
            }
            if (vrMaterial.HasProperty("_Cull"))
            {
                vrMaterial.SetFloat("_Cull", 0f); // No culling
            }
            
            valueText.fontSharedMaterial = vrMaterial;
        }
        
        // Configurar el componente Text para VR/Meta compatibility
        valueText.fontStyle = FontStyles.Bold; // Más visible en VR
        valueText.textWrappingMode = TextWrappingModes.NoWrap;
        valueText.overflowMode = TextOverflowModes.Overflow;
        
        // Configurar RectTransform del texto
        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        
        // Añadir componente para evitar clipping en VR
        var canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false; // No bloquear raycast en VR
        canvasGroup.interactable = false; // No interactivo
    }

    // Método específico para Meta Quest/VR
    private void ForceRenderOnTop()
    {
        if (valueText != null && canvasGO != null)
        {
            // Para VR/Meta Quest: asegurar configuración correcta
            var canvas = canvasGO.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 32767;
                canvas.overrideSorting = true;
                canvas.renderMode = RenderMode.WorldSpace; // World Space para VR
                canvas.planeDistance = 0.1f; // Muy cerca
            }
            
            // Configurar material para VR/Meta Quest - MUY AGRESIVO
            if (valueText.fontSharedMaterial != null)
            {
                var material = valueText.fontSharedMaterial;
                material.renderQueue = 5000; // Render queue MUY alto
                
                // FORZAR TODAS las configuraciones posibles
                material.SetInt("_ZTest", 0); // ALWAYS render
                material.SetInt("_ZWrite", 0); // No write to depth buffer
                material.SetFloat("_Cull", 0f); // No culling
                
                // Configurar para Meta Quest específicamente
                if (material.HasProperty("_ZTestMode"))
                {
                    material.SetFloat("_ZTestMode", 0f); // Always render
                }
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 1f); // Transparent surface
                }
            }
            
            // Configurar el GameObject para que esté en la capa correcta
            canvasGO.layer = 0; // Default layer
            valueText.gameObject.layer = 0; // Default layer
        }
    }

    // Método que se ejecuta constantemente para FORZAR render al frente
    private void LateUpdate()
    {
        if (valueText != null && valueText.fontSharedMaterial != null)
        {
            // FORZAR en cada frame que se renderice al frente
            valueText.fontSharedMaterial.renderQueue = 5000;
            valueText.fontSharedMaterial.SetInt("_ZTest", 0); // ALWAYS render
            valueText.fontSharedMaterial.SetInt("_ZWrite", 0); // No depth write
        }
    }
}
