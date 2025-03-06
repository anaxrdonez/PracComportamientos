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
    private GameObject animalAsignado; // Animal que el cliente adopta


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

        if (agente == null)
        {
            Debug.LogError("❌ ERROR: Cliente no tiene NavMeshAgent.");
            return;
        }

        if (!agente.isOnNavMesh)
        {
            Debug.LogError("❌ ERROR: Cliente no está sobre un NavMesh. Verifica que el suelo es navegable.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("❌ ERROR: GameManager no asignado.");
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
        }

        yield return StartCoroutine(IrA(checkout));
        yield return StartCoroutine(IrA(salida));

        SalirDelRefugio();
    }

    IEnumerator EsperarCheckIn()
    {
        yield return gameManager.ClienteEnCheckIn(this);
        Debug.Log(name + " se mueve al Check-In");
        yield return StartCoroutine(IrA(puntoCheckIn));

        while (DetectarZonaActual() != "CheckIn")
            yield return null;

        yield return new WaitForSeconds(2f);
        registrado = true;
    }

    public IEnumerator MoverASalaEspera()
    {
        Debug.Log(name + " se mueve a la Sala de Espera");
        yield return StartCoroutine(IrA(salaEspera));

        while (DetectarZonaActual() != "SalaEspera")
            yield return null;

        enSalaEspera = true;
        gameManager.ClienteEnSalaEspera(this);
    }

    IEnumerator EsperarEntrevista()
    {
        yield return gameManager.ClienteEnEntrevista(this);
        Debug.Log(name + " se mueve a la Entrevista");
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
        gameManager.OcupaSalaEntrevista();

        Debug.Log(name + " ha comenzado la entrevista.");
        yield return new WaitForSeconds(Random.Range(3f, 5f)); // 🔥 Simula la entrevista

        aprobado = Random.value > 0.5f;
        quierePerro = Random.value > 0.5f;

        Debug.Log(name + " ha terminado la Entrevista. Aprobado: " + aprobado + ", Quiere Perro: " + quierePerro);

        yield return new WaitForSeconds(1f); // 🔥 Asegura un pequeño retraso antes de continuar

        gameManager.LiberaSalaEntrevista();
    }


    IEnumerator VisitarZonaAdopcion()
    {
        Transform zonaDestino = quierePerro ? zonaPerros : zonaGatos;
        Debug.Log(name + " se mueve a la zona de " + (quierePerro ? "Perros" : "Gatos"));
        yield return StartCoroutine(IrA(zonaDestino));

        while (DetectarZonaActual() != (quierePerro ? "ZonaPerros" : "ZonaGatos"))
            yield return null;

        yield return new WaitForSeconds(Random.Range(3f, 7f));

        // Asignar un animal
        animalAsignado = gameManager.AsignarAnimal(quierePerro);

        if (animalAsignado != null)
        {
            Debug.Log(name + " ha adoptado un " + (quierePerro ? "perro" : "gato"));
            animalAsignado.transform.SetParent(transform); // El animal se pega al cliente
            animalAsignado.transform.localPosition = new Vector3(0.5f, 0, 0); // Lo coloca a su lado
        }

        // Ir al Checkout antes de salir
        yield return StartCoroutine(IrA(checkout));

        while (DetectarZonaActual() != "Checkout")
            yield return null;

        Debug.Log(name + " ha llegado al checkout.");

        // Liberar el animal antes de salir
        gameManager.LiberarAnimal(animalAsignado);
        animalAsignado = null;

        yield return StartCoroutine(IrA(salida));

        SalirDelRefugio();
    }


    public IEnumerator IrA(Transform destino)
    {
        if (destino == null)
        {
            Debug.LogError("❌ ERROR: Destino NULL en ClienteBT. Verifica que todas las posiciones están asignadas en GameManager.");
            yield break;
        }

        if (agente == null)
        {
            Debug.LogError("❌ ERROR: NavMeshAgent es NULL en ClienteBT.");
            yield break;
        }

        if (!agente.isOnNavMesh)
        {
            Debug.LogError("❌ ERROR: Cliente NO está sobre un NavMesh.");
            yield break;
        }

        agente.isStopped = false;
        agente.SetDestination(destino.position);

        Debug.Log(name + " moviéndose hacia: " + destino.name);

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