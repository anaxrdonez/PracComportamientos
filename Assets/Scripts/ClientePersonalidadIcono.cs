using UnityEngine;

public class ClientePersonalidadIcono : MonoBehaviour
{
    public GameObject iconoCariñoso;
    public GameObject iconoTranquilo;
    public GameObject iconoActivo;

    public void MostrarIconoCliente(string personalidad)
    {
        iconoCariñoso?.SetActive(false);
        iconoTranquilo?.SetActive(false);
        iconoActivo?.SetActive(false);

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
                Debug.LogWarning($" Personalidad desconocida en cliente: {personalidad}");
                break;
        }
    }
}
