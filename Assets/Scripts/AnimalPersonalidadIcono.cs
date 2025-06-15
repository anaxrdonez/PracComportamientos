using UnityEngine;

public class AnimalPersonalidadIcono : MonoBehaviour
{
    public GameObject iconoCariñoso;
    public GameObject iconoTranquilo;
    public GameObject iconoActivo;

    public void MostrarIconoAnimal(string personalidad)
    {
        // Ocultar todos al principio
        iconoCariñoso?.SetActive(false);
        iconoTranquilo?.SetActive(false);
        iconoActivo?.SetActive(false);


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
        Debug.Log($"Iconos asignados en {name} -> Cariñoso: {iconoCariñoso != null}, Tranquilo: {iconoTranquilo != null}, Activo: {iconoActivo != null}");

    }
}
