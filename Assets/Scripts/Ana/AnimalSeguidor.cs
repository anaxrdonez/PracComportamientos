using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]
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
        if (agente == null) yield break;
        while (clienteObjetivo != null)
        {
            agente.SetDestination(clienteObjetivo.position);
            yield return null;    // en lugar de WaitForSeconds
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
