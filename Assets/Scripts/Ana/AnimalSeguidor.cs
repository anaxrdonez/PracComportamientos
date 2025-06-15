using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalSeguidor : MonoBehaviour
{
    private Transform clienteObjetivo;
    private NavMeshAgent agente;

    void Awake()
    {
        // Aseguramos que el agente se inicializa antes de cualquier llamada a AsignarCliente
        agente = GetComponent<NavMeshAgent>();
    }

    IEnumerator SeguirCliente()
    {
        // Si por alguna razón agente no está asignado, detenemos el seguimiento
        if (agente == null)
        {
            Debug.LogError($"[{name}] NavMeshAgent no asignado en SeguirCliente.");
            yield break;
        }

        while (clienteObjetivo != null)
        {
            // Comprobamos que el transform del cliente sigue siendo válido
            if (clienteObjetivo == null)
            {
                yield break;
            }

            // Volvemos a asignar destino solo si es necesario o la ruta se ha interrumpido
            if (!agente.hasPath || agente.remainingDistance < 0.5f || agente.pathStatus != NavMeshPathStatus.PathComplete)
            {
                agente.SetDestination(clienteObjetivo.position);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    public void AsignarCliente(Transform cliente)
    {
        // Aseguramos agente en caso de que Awake no haya corrido aún
        if (agente == null)
            agente = GetComponent<NavMeshAgent>();

        clienteObjetivo = cliente;
        StopAllCoroutines();
        StartCoroutine(SeguirCliente());
    }
}
