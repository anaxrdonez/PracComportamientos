using UnityEngine;

public class DeerSensor : MonoBehaviour
{
    public enum SensorType { LeftEye, RightEye, Ear }
    public SensorType sensorType;

    private DeerAI ai;

    void Awake()
    {
        ai = GetComponentInParent<DeerAI>();
        Debug.Assert(ai != null, "Falta DeerAI en el padre");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tiger"))
        {
            // Dibuja una línea desde el sensor hasta el tigre durante 1 segundo
            Debug.DrawLine(
                transform.position,            // Origen: la posición del sensor
                other.transform.position,      // Destino: la posición del tigre
                Color.red,                     // Color de la línea
                1f                             // Duración en segundos
            );

            Debug.Log($"[DeerSensor:{sensorType}] ¡Detectó un tigre! {other.name}");
            ai.OnSensorEnter(sensorType, other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tiger"))
        {
            Debug.Log($"[DeerSensor:{sensorType}] Tigre perdió contacto: {other.name}");
            ai.OnSensorExit(sensorType, other.transform);
        }
    }
}
