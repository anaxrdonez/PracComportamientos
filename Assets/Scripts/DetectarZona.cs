using UnityEngine;

public class DetectarZona : MonoBehaviour
{
    public string zonaActual = "Fuera de zona";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = other.gameObject.name;
            Debug.Log(gameObject.name + " entr� a la zona: " + zonaActual);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = "Fuera de zona";
            Debug.Log(gameObject.name + " sali� de la zona.");
        }
    }
}