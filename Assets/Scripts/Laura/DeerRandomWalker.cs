using UnityEngine;
using System.Collections;

public class DeerRandomWalker : MonoBehaviour
{
    [Header("Elipse Area")]
    [Tooltip("Radio en el eje X (semi-eje mayor)")]
    public float radiusX = 5f;
    [Tooltip("Radio en el eje Z (semi-eje menor)")]
    public float radiusZ = 3f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento")]
    public float speed = 2f;
    [Tooltip("Velocidad de rotación (grados por segundo)")]
    public float rotateSpeed = 120f;
    [Tooltip("Tiempo mínimo de espera entre destinos")]
    public float waitTimeMin = 1f;
    [Tooltip("Tiempo máximo de espera entre destinos")]
    public float waitTimeMax = 3f;

    private Vector3 centerPos;

    void Start()
    {
        centerPos = transform.position;
        StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // 1) Elegir destino aleatorio dentro de la elipse
            Vector3 target = GetRandomPointInEllipse();

            // 2) Calcular rotación deseada
            Quaternion targetRotation = Quaternion.LookRotation((target - transform.position).normalized, Vector3.up);

            // 3) Girar suavemente en el lugar hasta alinearse (ángulo < 1°)
            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
                yield return null;
            }

            // 4) Moverse hacia el destino mientras se ajusta un poco la rotación
            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                // Ajuste continuo de la rotación durante el movimiento
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            // 5) Pausa aleatoria antes del siguiente destino
            float wait = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(wait);
        }
    }

    private Vector3 GetRandomPointInEllipse()
    {
        float t = Random.Range(0f, Mathf.PI * 2f);
        float u = Random.value + Random.value;
        float r = u > 1 ? 2f - u : u;

        float x = r * radiusX * Mathf.Cos(t);
        float z = r * radiusZ * Mathf.Sin(t);

        return new Vector3(centerPos.x + x, centerPos.y, centerPos.z + z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(centerPos, 0.1f);
            int segments = 64;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float theta = i / (float)segments * 2 * Mathf.PI;
                float x = Mathf.Cos(theta) * radiusX;
                float z = Mathf.Sin(theta) * radiusZ;
                Vector3 p = new Vector3(centerPos.x + x, centerPos.y, centerPos.z + z);
                if (i > 0) Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }
}
