using UnityEngine;
using System;

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

    void Start()
    {
        currentHealth = maxHealth;
        OnValueChanged?.Invoke(Value);
    }

    void Update()
    {
        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} destroyed.");
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}. Value: {Value}");

        OnValueChanged?.Invoke(Value);
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

    public float GetCurrentValue()
    {
        return Value;
    }
}
