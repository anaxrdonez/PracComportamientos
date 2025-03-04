using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private Transform puntoCheckIn, salaEspera, salaEntrevista, zonaJuegos, checkout, salida;
    private bool registrado = false, entrevistado = false, aprobado = false, enZonaJuegos = false;

    public delegate void ClienteSalidoDelegate();
    public event ClienteSalidoDelegate OnClienteSalido;

    public void InicializarCliente(Transform checkIn, Transform espera, Transform entrevista, Transform juegos, Transform check, Transform outRefugio)
    {
        puntoCheckIn = checkIn;
        salaEspera = espera;
        salaEntrevista = entrevista;
        zonaJuegos = juegos;
        checkout = check;
        salida = outRefugio;
    }

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
                aprobado = Random.value > 0.5f;
            });
        }
        else if (aprobado && !enZonaJuegos)
        {
            IrA(zonaJuegos, () => enZonaJuegos = true);
        }
        else if (entrevistado)
        {
            IrA(checkout, () => IrA(salida, () => SalirDelRefugio()));
        }
    }

    void IrA(Transform destino, System.Action callback)
    {
        agente.SetDestination(destino.position);
        StartCoroutine(EsperarLlegada(callback));
    }

    IEnumerator EsperarLlegada(System.Action callback)
    {
        yield return new WaitUntil(() => agente.isOnNavMesh && !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance);
        callback.Invoke();
    }

    void SalirDelRefugio()
    {
        OnClienteSalido?.Invoke();
        Destroy(gameObject);
    }
}