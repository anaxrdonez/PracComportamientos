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
            Debug.LogError("No hay modelos asignados en ModelLoader.");
            return;
        }

        if (modeloActual != null)
        {
            Destroy(modeloActual);
        }

        int index = Random.Range(0, modelos.Length);
        modeloActual = Instantiate(modelos[index], transform);

        modeloActual.transform.localPosition = new Vector3(0, -1f, 0);  

        modeloActual.transform.localRotation = Quaternion.Euler(0, 0, 0); 

        modeloActual.transform.localScale = Vector3.one;
    }
}
