using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private GameManager gameManager;
    private DetectarZona detectarZona;

    private Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;
    private bool registrado = false, entrevistado = false, aprobado = false, enSalaEspera = false;
    private bool quierePerro;

    public delegate void ClienteSalidoDelegate();
    public event ClienteSalidoDelegate OnClienteSalido;
    private GameObject animalAsignado;
    private static Queue<ClienteBT> colaEspera = new Queue<ClienteBT>();
    private static bool salaEntrevistaOcupada = false;

    public void InicializarCliente(Transform checkIn, Transform espera, Transform entrevista, Transform gatos, Transform perros, Transform check, Transform outRefugio, GameManager manager)
    {
        puntoCheckIn = checkIn;
        salaEspera = espera;
        salaEntrevista = entrevista;
        zonaGatos = gatos;
        zonaPerros = perros;
        checkout = check;
        salida = outRefugio;
        gameManager = manager;
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        detectarZona = GetComponent<DetectarZona>();

        if (agente == null || !agente.isOnNavMesh || gameManager == null)
        {
            Debug.LogError("❌ ERROR: Cliente no está correctamente configurado.");
            return;
        }

        StartCoroutine(EjecutarBehaviourTree());
    }

    public string DetectarZonaActual()
    {
        return detectarZona != null ? detectarZona.zonaActual : "FueraDeZona";
    }

    IEnumerator EjecutarBehaviourTree()
    {
        yield return StartCoroutine(EsperarCheckIn());
        yield return StartCoroutine(MoverASalaEspera());
        yield return StartCoroutine(EsperarEntrevista());
        yield return StartCoroutine(ProcesoEntrevista());

        if (aprobado)
        {
            yield return StartCoroutine(VisitarZonaAdopcion());
            yield return StartCoroutine(IrA(checkout));
        }
        else
        {
            yield return StartCoroutine(IrA(checkout));
        }

        yield return StartCoroutine(IrA(salida));
        SalirDelRefugio();
    }

    IEnumerator EsperarCheckIn()
    {
        yield return gameManager.ClienteEnCheckIn(this);
        yield return StartCoroutine(IrA(puntoCheckIn));
        while (DetectarZonaActual() != "CheckIn")
            yield return null;
        yield return new WaitForSeconds(2f);
        registrado = true;
    }

    public IEnumerator MoverASalaEspera()
    {
        yield return StartCoroutine(IrA(salaEspera));
        while (DetectarZonaActual() != "SalaEspera")
            yield return null;
        enSalaEspera = true;
        colaEspera.Enqueue(this);
        gameManager.RevisarSalaEspera();
    }

    IEnumerator EsperarEntrevista()
    {
        while (colaEspera.Peek() != this || salaEntrevistaOcupada)
            yield return null;

        colaEspera.Dequeue();
        salaEntrevistaOcupada = true;
        yield return StartCoroutine(IrA(salaEntrevista));
        while (DetectarZonaActual() != "SalaEntrevista")
            yield return null;
    }

    public void IniciarEntrevista()
    {
        StartCoroutine(ProcesoEntrevista());
    }

    IEnumerator ProcesoEntrevista()
    {
        if (DetectarZonaActual() != "SalaEntrevista")
        {
            Debug.LogError("❌ ERROR: Cliente intentó realizar la entrevista fuera de la Sala de Entrevista.");
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(3f, 5f));
        aprobado = Random.value > 0.5f;
        quierePerro = Random.value > 0.5f;
        Debug.Log(name + " ha terminado la Entrevista. Aprobado: " + aprobado + ", Quiere Perro: " + quierePerro);
        yield return new WaitForSeconds(1f);
        salaEntrevistaOcupada = false;
        gameManager.RevisarSalaEspera();
    }

    IEnumerator VisitarZonaAdopcion()
    {
        Transform zonaDestino = quierePerro ? zonaPerros : zonaGatos;

        if (zonaDestino == null)
        {
            Debug.LogError("❌ ERROR: La zona de adopción es NULL. Verifica que los puntos están asignados en GameManager.");
            yield break;
        }

        Debug.Log(name + " se mueve a la zona de " + (quierePerro ? "Perros" : "Gatos"));
        yield return StartCoroutine(IrA(zonaDestino));

        while (DetectarZonaActual() != (quierePerro ? "ZonaPerros" : "ZonaGatos"))
            yield return null;

        yield return new WaitForSeconds(Random.Range(3f, 7f));
        animalAsignado = gameManager.AsignarAnimal(quierePerro);

        if (animalAsignado != null)
        {
            Debug.Log(name + " ha adoptado un " + (quierePerro ? "perro" : "gato"));
            animalAsignado.transform.SetParent(transform);
            animalAsignado.transform.localPosition = new Vector3(0.5f, 0, 0);
        }
    }

    public IEnumerator IrA(Transform destino)
    {
        if (destino == null || agente == null || !agente.isOnNavMesh)
        {
            Debug.LogError("❌ ERROR: Destino inválido en ClienteBT.");
            yield break;
        }

        agente.isStopped = false;
        agente.SetDestination(destino.position);
        Debug.Log(name + " moviéndose hacia: " + destino.name + " en posición " + destino.position);

        while (agente.pathPending || agente.remainingDistance > agente.stoppingDistance)
            yield return null;

        agente.isStopped = true;
        Debug.Log(name + " llegó a " + destino.name);
    }

    void SalirDelRefugio()
    {
        OnClienteSalido?.Invoke();
        Destroy(gameObject);
    }
}