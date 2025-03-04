using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class LimpiadorFSM : MonoBehaviour
{
    public enum EstadoLimpiador { Patrullando, IrALimpiar, Limpiando, Reponiendo }
    public EstadoLimpiador estadoActual;
    private NavMeshAgent agente;

    private Transform almacen;
    private List<Transform> puntosPatrulla = new List<Transform>();
    private List<Transform> salas = new List<Transform>();
    private Dictionary<Transform, int> salasSucias = new Dictionary<Transform, int>();

    private int puntoActual = 0;

    public void InicializarLimpiador(Transform store, Transform[] patrulla, Transform[] habitaciones)
    {
        almacen = store;
        puntosPatrulla.AddRange(patrulla);
        salas.AddRange(habitaciones);
        foreach (var sala in salas)
        {
            salasSucias[sala] = 0;
        }
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        estadoActual = EstadoLimpiador.Patrullando;
        IrA(puntosPatrulla[puntoActual]);
    }

    void Update()
    {
        switch (estadoActual)
        {
            case EstadoLimpiador.Patrullando:
                if (HaLlegado() && puntosPatrulla.Count > 0)
                {
                    puntoActual = (puntoActual + 1) % puntosPatrulla.Count;
                    IrA(puntosPatrulla[puntoActual]);
                }
                foreach (var sala in salasSucias)
                {
                    if (sala.Value >= 3)
                    {
                        estadoActual = EstadoLimpiador.IrALimpiar;
                        IrA(sala.Key);
                        break;
                    }
                }
                break;
            case EstadoLimpiador.IrALimpiar:
                if (HaLlegado())
                {
                    estadoActual = EstadoLimpiador.Limpiando;
                    Invoke("FinalizarLimpieza", 3f);
                }
                break;
            case EstadoLimpiador.Limpiando:
                break;
            case EstadoLimpiador.Reponiendo:
                if (HaLlegado())
                {
                    estadoActual = EstadoLimpiador.Patrullando;
                    IrA(puntosPatrulla[puntoActual]);
                }
                break;
        }
    }

    void IrA(Transform destino)
    {
        agente.SetDestination(destino.position);
    }

    bool HaLlegado()
    {
        return agente.isOnNavMesh && !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance;
    }

    void FinalizarLimpieza()
    {
        foreach (var sala in salasSucias.Keys)
        {
            salasSucias[sala] = 0;
        }
        estadoActual = EstadoLimpiador.Reponiendo;
        IrA(almacen);
    }
}