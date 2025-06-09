using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalSeguidor : MonoBehaviour
{
    private Transform clienteObjetivo;
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void AsignarCliente(Transform cliente)
    {
        clienteObjetivo = cliente;
    }

    void Update()
    {
        if (clienteObjetivo != null)
        {
            agente.SetDestination(clienteObjetivo.position);
        }
    }
}