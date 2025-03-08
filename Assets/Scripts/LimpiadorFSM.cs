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
    private List<Transform> salas;
    private Transform salaObjetivo;

    public void InicializarLimpiador(Transform almacenRef, List<Transform> salasReferencias, GameManager manager)
    {
        almacen = almacenRef;
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

        if (salas == null || salas.Count == 0 || almacen == null)
        {
            Debug.LogError("❌ ERROR: No se han asignado salas o almacén en " + name);
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
                    yield return StartCoroutine(LimpiarSala(salaObjetivo));
                    break;

                case EstadoLimpiador.Reponiendo:
                    yield return StartCoroutine(ReponerUtensilios());
                    break;
            }

            yield return null;
        }
    }

    private IEnumerator Patrullar()
    {
        while (EstadoActual == EstadoLimpiador.Patrullando)
        {
            Transform destino = salas[Random.Range(0, salas.Count)];
            yield return StartCoroutine(IrA(destino));

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    public void IrALimpiar(Transform sala)
    {
        if (EstadoActual == EstadoLimpiador.Limpiando) return;

        salaObjetivo = sala;
        EstadoActual = EstadoLimpiador.Limpiando;
    }

    private IEnumerator LimpiarSala(Transform sala)
    {
        Debug.Log(name + " moviéndose a limpiar " + sala.name);
        yield return StartCoroutine(IrA(sala));

        Debug.Log(name + " limpiando " + sala.name);
        yield return new WaitForSeconds(5f);

        gameManager.SalaLimpia(sala);
        EstadoActual = EstadoLimpiador.Reponiendo;
    }

    private IEnumerator ReponerUtensilios()
    {
        Debug.Log(name + " yendo al almacén a reponer");
        yield return StartCoroutine(IrA(almacen));

        Debug.Log(name + " reponiendo utensilios...");
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
