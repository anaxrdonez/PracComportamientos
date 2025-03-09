using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class LimpiadorFSM : MonoBehaviour
{
    public enum EstadoLimpiador { Patrullando, Limpiando, Reponiendo }
    public EstadoLimpiador EstadoActual { get; private set; } = EstadoLimpiador.Patrullando;

    private NavMeshAgent agente;
    private GameManager gameManager;
    private Transform almacen;
    private List<Transform> puntosPatrulla;
    private List<Transform> salas;
    private Transform salaObjetivo;
    private int salasLimpias = 0;

    public void InicializarLimpiador(Transform almacenRef, List<Transform> patrullas, List<Transform> salasReferencias, GameManager manager)
    {
        almacen = almacenRef;
        puntosPatrulla = patrullas;
        salas = salasReferencias;
        gameManager = manager;
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        if (agente == null)
        {
            Debug.LogError("❌ ERROR: NavMeshAgent no asignado en " + name);
            return;
        }
        StartCoroutine(FSM());
    }

    private IEnumerator FSM()
    {
        while (true)
        {
            switch (EstadoActual)
            {
                case EstadoLimpiador.Patrullando:
                    yield return StartCoroutine(Patrullar());
                    break;
                case EstadoLimpiador.Limpiando:
                    yield return StartCoroutine(LimpiarSala());
                    break;
                case EstadoLimpiador.Reponiendo:
                    yield return StartCoroutine(ReponerUtensilios());
                    break;
            }
        }
    }

    private IEnumerator Patrullar()
    {
        while (EstadoActual == EstadoLimpiador.Patrullando)
        {
            Transform destino = puntosPatrulla[Random.Range(0, puntosPatrulla.Count)];
            yield return StartCoroutine(IrA(destino));
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    public void IrALimpiar(Transform sala)
    {
        if (EstadoActual == EstadoLimpiador.Patrullando)
        {
            salaObjetivo = sala;
            EstadoActual = EstadoLimpiador.Limpiando;
        }
    }

    private IEnumerator LimpiarSala()
    {
        yield return StartCoroutine(IrA(salaObjetivo));
        yield return new WaitForSeconds(5f);
        gameManager.SalaLimpia(salaObjetivo);
        salasLimpias++;
        EstadoActual = salasLimpias >= 2 ? EstadoLimpiador.Reponiendo : EstadoLimpiador.Patrullando;
        if (EstadoActual == EstadoLimpiador.Reponiendo)
            salasLimpias = 0;
    }

    private IEnumerator ReponerUtensilios()
    {
        yield return StartCoroutine(IrA(almacen));
        yield return new WaitForSeconds(3f);
        EstadoActual = EstadoLimpiador.Patrullando;
    }

    private IEnumerator IrA(Transform destino)
    {
        if (destino == null || agente == null || !agente.isOnNavMesh)
        {
            Debug.LogError("❌ ERROR: Destino inválido en " + name);
            yield break;
        }

        agente.isStopped = false;
        agente.SetDestination(destino.position);

        while (agente.pathPending || agente.remainingDistance > agente.stoppingDistance)
            yield return null;

        agente.isStopped = true;
    }
}
