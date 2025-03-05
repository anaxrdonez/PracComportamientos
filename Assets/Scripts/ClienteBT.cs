using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;
    private bool registrado = false, entrevistado = false, aprobado = false, enZonaAdopcion = false, enSalaEspera = false;
    private GameManager gameManager;
    private DetectarZona detectarZona;
    private bool quierePerro; // 🔥 Nueva variable para saber qué quiere adoptar

    public delegate void ClienteSalidoDelegate();
    public event ClienteSalidoDelegate OnClienteSalido;

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

        // 🔥 Asignar aleatoriamente si quiere adoptar un perro o un gato
        quierePerro = Random.value > 0.5f;
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        detectarZona = GetComponent<DetectarZona>();

        if (agente == null)
        {
            Debug.LogError("❌ ERROR: Cliente no tiene un NavMeshAgent. Asegúrate de agregarlo en el prefab.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("❌ ERROR: GameManager no asignado en ClienteBT.");
            return;
        }

        if (detectarZona == null)
        {
            Debug.LogError("❌ ERROR: Cliente no tiene un componente DetectarZona.");
            return;
        }

        StartCoroutine(RealizarCheckIn());
    }

    public IEnumerator RealizarCheckIn()
    {
        if (puntoCheckIn == null)
        {
            Debug.LogError("❌ ERROR: puntoCheckIn es NULL en ClienteBT.");
            yield break;
        }

        yield return StartCoroutine(IrA(puntoCheckIn));

        yield return new WaitUntil(() => detectarZona.zonaActual == "CheckIn");

        yield return new WaitForSeconds(2f); // ⏳ Esperar 2 segundos para completar el Check-In

        registrado = true;
        Debug.Log(gameObject.name + " ha completado el check-in.");

        if (gameManager.SalaEntrevistaOcupada())
        {
            yield return StartCoroutine(MoverASalaEspera());
            gameManager.AgregarClienteAEspera(this);
            yield break;
        }

        IniciarEntrevista();
    }

    public IEnumerator MoverASalaEspera()
    {
        if (salaEspera == null)
        {
            Debug.LogError("❌ ERROR: salaEspera es NULL en ClienteBT.");
            yield break;
        }

        yield return StartCoroutine(IrA(salaEspera));

        yield return new WaitUntil(() => detectarZona.zonaActual == "SalaEspera");

        enSalaEspera = true;
        Debug.Log(gameObject.name + " está en la sala de espera.");
    }

    public IEnumerator SalirDeSalaEspera()
    {
        if (!enSalaEspera) yield break; // Si no está en la sala de espera, no hacer nada

        Debug.Log(gameObject.name + " está dejando la Sala de Espera.");
        enSalaEspera = false;
        yield return new WaitForSeconds(0.5f); // 🔥 Pequeña pausa antes de moverse
    }

    public void IniciarEntrevista()
    {
        StartCoroutine(ProcesoEntrevista());
    }

    public IEnumerator IrAEntrevista()
    {
        if (salaEntrevista == null)
        {
            Debug.LogError("❌ ERROR: salaEntrevista es NULL en ClienteBT.");
            yield break;
        }

        yield return StartCoroutine(IrA(salaEntrevista));

        yield return new WaitUntil(() => detectarZona.zonaActual == "SalaEntrevista");

        Debug.Log(gameObject.name + " ha llegado a la Sala de Entrevista.");
    }

    IEnumerator ProcesoEntrevista()
    {
        yield return StartCoroutine(IrA(salaEntrevista));

        yield return new WaitUntil(() => detectarZona.zonaActual == "SalaEntrevista");

        gameManager.OcupaSalaEntrevista();
        entrevistado = true;

        yield return new WaitForSeconds(Random.Range(3f, 5f)); // ⏳ Simular entrevista

        aprobado = Random.value > 0.5f;
        Debug.Log(gameObject.name + " ha terminado la entrevista. Aprobado: " + aprobado + ", Quiere Perro: " + quierePerro);

        gameManager.LiberaSalaEntrevista();

        if (aprobado)
        {
            yield return StartCoroutine(VisitarZonaAdopcion());
        }

        yield return StartCoroutine(IrA(checkout));
        yield return new WaitUntil(() => detectarZona.zonaActual == "Checkout");

        Debug.Log(gameObject.name + " está en checkout.");
        yield return StartCoroutine(IrA(salida));

        SalirDelRefugio();
    }

    IEnumerator VisitarZonaAdopcion()
    {
        Transform zonaDestino = quierePerro ? zonaPerros : zonaGatos;

        if (zonaDestino == null)
        {
            Debug.LogError("❌ ERROR: La zona de adopción es NULL.");
            yield break;
        }

        yield return StartCoroutine(IrA(zonaDestino));

        string nombreZona = quierePerro ? "ZonaPerros" : "ZonaGatos";
        yield return new WaitUntil(() => detectarZona.zonaActual == nombreZona);

        enZonaAdopcion = true;
        Debug.Log(gameObject.name + " está en " + nombreZona + ".");
        yield return new WaitForSeconds(Random.Range(3f, 7f));
    }

    IEnumerator IrA(Transform destino)
    {
        if (destino == null)
        {
            Debug.LogError("❌ ERROR: El destino es NULL en ClienteBT.");
            yield break;
        }

        if (agente == null)
        {
            Debug.LogError("❌ ERROR: NavMeshAgent es NULL en ClienteBT.");
            yield break;
        }

        agente.SetDestination(destino.position);
        yield return new WaitUntil(() => !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance);
    }

    void SalirDelRefugio()
    {
        OnClienteSalido?.Invoke();
        Debug.Log(gameObject.name + " ha salido del refugio.");
        Destroy(gameObject);
    }
}