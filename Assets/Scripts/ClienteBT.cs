using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private Transform puntoCheckIn, salaEspera, salaEntrevista, zonaJuegos, checkout, salida;
    private bool registrado = false, entrevistado = false, aprobado = false, enZonaJuegos = false, enSalaEspera = false;
    private GameManager gameManager;

    public delegate void ClienteSalidoDelegate();
    public event ClienteSalidoDelegate OnClienteSalido;

    public void InicializarCliente(Transform checkIn, Transform espera, Transform entrevista, Transform juegos, Transform check, Transform outRefugio, GameManager manager)
    {
        puntoCheckIn = checkIn;
        salaEspera = espera;
        salaEntrevista = entrevista;
        zonaJuegos = juegos;
        checkout = check;
        salida = outRefugio;
        gameManager = manager;
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        StartCoroutine(EjecutarBT());
    }

    IEnumerator EjecutarBT()
    {
        // 1. Ir al Check-In
        yield return StartCoroutine(IrA(puntoCheckIn));
        registrado = true;
        Debug.Log(gameObject.name + " ha completado el check-in.");

        // 2. Si la sala de entrevistas está ocupada, esperar en la sala de espera
        if (gameManager.SalaEntrevistaOcupada())
        {
            yield return StartCoroutine(IrA(salaEspera));
            enSalaEspera = true;
            Debug.Log(gameObject.name + " está en la sala de espera.");

            gameManager.AgregarClienteAEspera(this);
            yield break; // Detenemos la ejecución hasta que sea su turno
        }

        // Si la sala está libre, pasar directamente a la entrevista
        IniciarEntrevista();
    }

    public void IniciarEntrevista()
    {
        StartCoroutine(ProcesoEntrevista());
    }

    IEnumerator ProcesoEntrevista()
    {
        yield return StartCoroutine(IrA(salaEntrevista));
        gameManager.OcupaSalaEntrevista(); // Marcar sala ocupada
        entrevistado = true;
        aprobado = Random.value > 0.5f; // 50% de probabilidad de aprobar
        Debug.Log(gameObject.name + " ha terminado la entrevista. Aprobado: " + aprobado);

        yield return new WaitForSeconds(Random.Range(1f, 3f)); // Simular entrevista

        gameManager.LiberaSalaEntrevista(); // Liberar sala para el siguiente cliente

        // 4. Si fue aprobado, ir a la zona de juegos
        if (aprobado)
        {
            yield return StartCoroutine(IrA(zonaJuegos));
            enZonaJuegos = true;
            Debug.Log(gameObject.name + " está jugando con los animales.");
            yield return new WaitForSeconds(Random.Range(3f, 7f)); // Tiempo de juego
        }

        // 5. Ir al Checkout y salir del refugio
        yield return StartCoroutine(IrA(checkout));
        Debug.Log(gameObject.name + " está en checkout.");
        yield return StartCoroutine(IrA(salida));

        SalirDelRefugio();
    }

    IEnumerator IrA(Transform destino)
    {
        agente.SetDestination(destino.position);
        yield return new WaitUntil(() => agente.isOnNavMesh && !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance);
    }

    void SalirDelRefugio()
    {
        OnClienteSalido?.Invoke();
        Debug.Log(gameObject.name + " ha salido del refugio.");
        Destroy(gameObject);
    }
}