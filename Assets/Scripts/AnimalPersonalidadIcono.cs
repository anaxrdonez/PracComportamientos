using UnityEngine;

public class AnimalPersonalidadIcono : MonoBehaviour
{
    public GameObject iconoCariñoso;
    public GameObject iconoTranquilo;
    public GameObject iconoActivo;

    void Start()
    {
        // Ocultar todos al principio
        iconoCariñoso?.SetActive(false);
        iconoTranquilo?.SetActive(false);
        iconoActivo?.SetActive(false);

        string personalidad = GetComponent<Animal>().personalidadAnimal;

        // Activar el correspondiente
        switch (personalidad)
        {
            case "Cariñoso":
                iconoCariñoso?.SetActive(true);
                break;
            case "Tranquilo":
                iconoTranquilo?.SetActive(true);
                break;
            case "Activo":
                iconoActivo?.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Personalidad desconocida en animal: {personalidad}");
                break;
        }
    }
}
