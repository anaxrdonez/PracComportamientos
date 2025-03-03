using UnityEngine;
using UnityEngine.AI;

public class ClientBT : MonoBehaviour
{
    private NavMeshAgent agente;
    public Transform puntoCheckIn;
    public Transform salaEspera;
    public Transform salaEntrevista;
    public Transform zonaJuegos;
    public Transform checkout;
    public Transform salida;

    private bool registrado = false;
    private bool entrevistado = false;
    private bool aprobado = false;
    private bool enZonaJuegos = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        EjecutarBT();
    }

    void EjecutarBT()
    {
        if (!registrado)
        {
            IrA(puntoCheckIn, () => registrado = true);
        }
        else if (!entrevistado)
        {
            IrA(salaEntrevista, () => {
                entrevistado = true;
                aprobado = Random.value > 0.5f; // 50% de probabilidad de aprobación
            });
        }
        else if (aprobado && !enZonaJuegos)
        {
            IrA(zonaJuegos, () => enZonaJuegos = true);
        }
        else if (entrevistado)
        {
            IrA(checkout, () => IrA(salida, () => Destroy(gameObject))); // Cliente sale del refugio
        }
    }

    void IrA(Transform destino, System.Action callback)
    {
        agente.SetDestination(destino.position);
        StartCoroutine(EsperarLlegada(callback));
    }

    System.Collections.IEnumerator EsperarLlegada(System.Action callback)
    {
        yield return new WaitUntil(() => !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance);
        callback.Invoke();
    }
}
