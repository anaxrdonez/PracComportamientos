using UnityEngine;
using System;

public class DetectarZona : MonoBehaviour
{
    public string zonaActual = "Fuera de zona";

    public event Action<string> OnZonaCambiada;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("❌ ERROR: " + gameObject.name + " no tiene un Collider. Agrega uno y marca 'Is Trigger'.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("⚠️ ADVERTENCIA: " + gameObject.name + " tiene un Collider, pero no está marcado como 'Trigger'.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = other.gameObject.name;
            Debug.Log(gameObject.name + " entró a la zona: " + zonaActual);
            OnZonaCambiada?.Invoke(zonaActual);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Zona") && zonaActual != other.gameObject.name)
        {
            zonaActual = other.gameObject.name;
            Debug.Log(gameObject.name + " sigue en la zona: " + zonaActual);
            OnZonaCambiada?.Invoke(zonaActual);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Zona"))
        {
            zonaActual = "Fuera de zona";
            Debug.Log(gameObject.name + " salió de la zona.");
            OnZonaCambiada?.Invoke(zonaActual);
        }
    }
}