using UnityEngine;

public class ModelLoader : MonoBehaviour
{
    public GameObject[] modelos;
    private GameObject modeloActual;

    void Start()
    {
        CargarModeloAleatorio();
    }

    void CargarModeloAleatorio()
    {
        if (modelos.Length == 0)
        {
            Debug.LogError("❌ No hay modelos asignados en ModelLoader.");
            return;
        }

        if (modeloActual != null)
        {
            Destroy(modeloActual);
        }

        int index = Random.Range(0, modelos.Length);
        modeloActual = Instantiate(modelos[index], transform);

        // 🔹 Ajustar posición si el modelo aparece muy arriba
        modeloActual.transform.localPosition = new Vector3(0, -1f, 0);  // Ajusta el "-1f" según sea necesario

        // 🔹 Rotarlo para que mire en la dirección correcta
        modeloActual.transform.localRotation = Quaternion.Euler(0, 0, 0); // Ajusta el "180" si sigue mal

        // 🔹 Asegurar que el modelo no afecte la escala
        modeloActual.transform.localScale = Vector3.one;
    }
}
