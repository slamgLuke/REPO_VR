using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class ChestValueCounter : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Arrastra aquí el objeto TextMeshPro que mostrará el valor total.")]
    [SerializeField] private TextMeshProUGUI totalValueText;

    // Lista para mantener un registro de los objetos detectados dentro del trigger
    private List<DestructibleObject> detectedObjects = new List<DestructibleObject>();

    void Start()
    {
        // Asegurarse de que el collider es un trigger al inicio
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("El collider en " + gameObject.name + " no está marcado como 'Is Trigger'. Se activará automáticamente.", this);
            col.isTrigger = true;
        }
        UpdateTotalValue(); // Actualizar el texto a 0 al inicio
    }

    private void OnDestroy()
    {
        // Es una buena práctica desuscribirse de todos los eventos al destruir el objeto
        // para evitar fugas de memoria (memory leaks).
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
        // Intentamos obtener el script DestructibleObject del objeto que entró
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();

        // Si el objeto tiene el script y no está ya en nuestra lista...
        if (destructible != null && !detectedObjects.Contains(destructible))
        {
            // Lo añadimos a la lista
            detectedObjects.Add(destructible);

            // Nos suscribimos a su evento OnValueChanged.
            // Esto es clave para que el total se actualice si el objeto recibe daño (y pierde valor)
            // MIENTRAS está dentro del detector.
            destructible.OnValueChanged += HandleObjectValueChanged;

            Debug.Log(destructible.name + " entró en el detector.");
            UpdateTotalValue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();

        // Si el objeto que sale tiene el script y está en nuestra lista...
        if (destructible != null && detectedObjects.Contains(destructible))
        {
            // Nos desuscribimos del evento para no seguir escuchando cambios
            destructible.OnValueChanged -= HandleObjectValueChanged;

            // Lo quitamos de la lista
            detectedObjects.Remove(destructible);

            Debug.Log(destructible.name + " salió del detector.");
            UpdateTotalValue();
        }
    }

    // Este método se llamará cada vez que el valor de un objeto DENTRO del detector cambie.
    private void HandleObjectValueChanged(float newValue)
    {
        Debug.Log("El valor de un objeto cambió. Recalculando total.");
        UpdateTotalValue();
    }

    // Calcula la suma de los valores de todos los objetos en la lista y actualiza el texto
    private void UpdateTotalValue()
    {
        // Usamos LINQ para sumar los valores de forma concisa y segura.
        // `obj => obj.Value` obtiene el valor actual de cada objeto.
        float currentTotal = detectedObjects.Sum(obj => obj.Value);

        if (totalValueText != null)
        {
            // Actualizamos el texto con el formato deseado
            totalValueText.text = $"${(int)currentTotal}";
        }
    }
}