// TigerSensor.cs
using UnityEngine;

public class TigerSensor : MonoBehaviour
{
    private TigerAI ai;
    void Awake()
    {
        ai = GetComponentInParent<TigerAI>();
        if (ai == null) Debug.LogError("Falta TigerAI en el padre.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeerBody"))
            ai.OnSeePrey(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DeerBody"))
            ai.OnLosePrey(other.transform);
    }
}
