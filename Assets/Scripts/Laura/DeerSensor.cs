// DeerSensor.cs  (idéntico para TigerSensor.cs cambiando tags y enum si se desea)
using UnityEngine;

public class DeerSensor : MonoBehaviour
{
    public enum SensorType { LeftEye, RightEye, Ear }
    public SensorType sensorType;

    private DeerAI ai;

    void Awake()
    {
        ai = GetComponentInParent<DeerAI>();
        if (ai == null)
            Debug.LogError("No se ha encontrado DeerAI en el padre.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tiger"))
            ai.OnSensorEnter(sensorType, other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tiger"))
            ai.OnSensorExit(sensorType, other.transform);
    }
}
