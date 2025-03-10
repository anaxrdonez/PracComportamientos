using UnityEngine;

public class ModelLoader : MonoBehaviour
{
    [Header("Modelos de Cliente")]
    public GameObject[] modelosCliente; // Array con los 3 modelos diferentes
    private GameObject modeloActual;

    void Start()
    {
        if (modelosCliente.Length == 0)
        {
            Debug.LogError("❌ ERROR: No hay modelos asignados en el ModelLoader");
            return;
        }

        // Seleccionar un modelo aleatorio
        int indice = Random.Range(0, modelosCliente.Length);
        modeloActual = Instantiate(modelosCliente[indice], transform);
        modeloActual.transform.localPosition = Vector3.zero; // Asegurar que el modelo se coloque correctamente
    }
}