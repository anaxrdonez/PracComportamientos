using UnityEngine;

public class DetectarZona : MonoBehaviour
{
    public string zonaActual = "Fuera de zona";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = other.gameObject.name;
            Debug.Log(gameObject.name + " entró a la zona: " + zonaActual);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = "Fuera de zona";
            Debug.Log(gameObject.name + " salió de la zona.");
        }
    }
}