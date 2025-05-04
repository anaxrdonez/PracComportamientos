using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject clientePrefab;
    public GameObject perroPrefab, gatoPrefab;
    public GameObject limpiadorPrefab;

    [Header("Puntos de Referencia")]
    public Transform puntoSpawnClientes;
    public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;

    [Header("Puntos de Referencia - Limpiadores")]
    public Transform puntoSpawnLimpiadores;
    public Transform almacen;
    public List<Transform> puntosPatrulla;
    public List<Transform> salas;

    private Queue<ClienteBT> colaSalaEspera = new Queue<ClienteBT>();
    private Queue<ClienteBT> colaRecepcion = new Queue<ClienteBT>();
    private bool entrevistaOcupada = false;
    private bool recepcionOcupada = false;
    private List<LimpiadorFSM> limpiadores = new List<LimpiadorFSM>();
    private Dictionary<Transform, bool> estadoSalas = new Dictionary<Transform, bool>();

    [Header("Configuraciones")]
    public int maxClientes = 3;
    public int numLimpiadores = 2;
    private int clientesActuales = 0;
    private ClienteBT clienteConPermisoEntrevista = null;

    private CameraSwitcher cameraSwitcher;


    [Header("Animales")]
    private List<GameObject> perrosDisponibles = new List<GameObject>();
    private List<GameObject> gatosDisponibles = new List<GameObject>();

    void Start()
    {
        Debug.Log("Iniciando GameManager...");
        StartCoroutine(GenerarClientes());
        GenerarLimpiadores();
        StartCoroutine(ControlSuciedadSalas());

        foreach (var sala in salas)
            estadoSalas[sala] = false;

        for (int i = 0; i < 5; i++)
        {
            GameObject perro = Instantiate(perroPrefab, zonaPerros.position, Quaternion.identity);
            GameObject gato = Instantiate(gatoPrefab, zonaGatos.position, Quaternion.identity);
            perrosDisponibles.Add(perro);
            gatosDisponibles.Add(gato);
        }
        cameraSwitcher = FindObjectOfType<CameraSwitcher>();

    }

    IEnumerator GenerarClientes()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            Debug.Log("Intentando crear un cliente...");

            if (clientesActuales < maxClientes)
            {
                GameObject nuevoCliente = Instantiate(clientePrefab, puntoSpawnClientes.position, Quaternion.identity);
                ClienteBT clienteScript = nuevoCliente.GetComponent<ClienteBT>();

                if (clienteScript != null)
                {
                    clienteScript.InicializarCliente(puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida, this);
                    clientesActuales++;

                    RecepcionistaFSM recepcionista = FindObjectOfType<RecepcionistaFSM>();
                    if (recepcionista != null)
                        recepcionista.ClienteLlega(clienteScript);

                    if (cameraSwitcher != null && clienteScript.ClienteCam != null)
                    {
                        cameraSwitcher.RegistrarCliente(clienteScript.ClienteCam);

                        clienteScript.OnClienteSalido += () =>
                        {
                            if (cameraSwitcher != null && clienteScript.ClienteCam != null)
                                cameraSwitcher.DesregistrarCliente(clienteScript.ClienteCam);
                        };
                    }

                    Debug.Log("Cliente creado y enviado al recepcionista.");
                }

                else
                {
                    Debug.LogError(" ERROR: El prefab de Cliente no tiene el script ClienteBT adjunto.");
                }
            }
            else
            {
                Debug.Log("No se generan más clientes, máximo alcanzado.");
            }
        }
    }

    void GenerarLimpiadores()
    {
        for (int i = 0; i < numLimpiadores; i++)
        {
            GameObject nuevoLimpiador = Instantiate(limpiadorPrefab, puntoSpawnLimpiadores.position, Quaternion.identity);
            LimpiadorFSM limpiadorScript = nuevoLimpiador.GetComponent<LimpiadorFSM>();

            if (limpiadorScript != null)
            {
                limpiadorScript.InicializarLimpiador(almacen, puntosPatrulla, salas, this);

                NavMeshAgent agente = nuevoLimpiador.GetComponent<NavMeshAgent>();
                if (agente != null)
                {
                    agente.avoidancePriority = Random.Range(30, 70);
                }

                limpiadores.Add(limpiadorScript);
            }
        }
    }

    IEnumerator ControlSuciedadSalas()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            Transform salaSucia = salas[Random.Range(0, salas.Count)];
            if (!estadoSalas[salaSucia])
            {
                estadoSalas[salaSucia] = true;
                Debug.Log("⚠️ La sala " + salaSucia.name + " se ha ensuciado.");
                AsignarLimpiadorASala(salaSucia);
            }
        }
    }

    public void ClienteEnSalaEspera(ClienteBT cliente)
    {
        if (!colaSalaEspera.Contains(cliente))
            colaSalaEspera.Enqueue(cliente);
    }

    public bool ClientePuedeEntrevistarse(ClienteBT cliente)
    {
        if (entrevistaOcupada || clienteConPermisoEntrevista != null) return false;
        if (colaSalaEspera.Count == 0 || colaSalaEspera.Peek() != cliente) return false;

        colaSalaEspera.Dequeue();
        entrevistaOcupada = true;
        clienteConPermisoEntrevista = cliente;
        return true;
    }
    public bool ClienteTienePermisoEntrevista(ClienteBT cliente)
    {
        if (clienteConPermisoEntrevista == cliente)
        {
            cliente.OtorgarPermisoEntrevista();
            clienteConPermisoEntrevista = null; // Limpia para el siguiente
            return true;
        }
        return false;
    }


    public void ClienteARecepcion(ClienteBT cliente)
    {
        if (!colaRecepcion.Contains(cliente))
            colaRecepcion.Enqueue(cliente);
    }

    public bool ClientePuedeSerRegistrado(ClienteBT cliente)
    {
        if (recepcionOcupada) return false;
        if (colaRecepcion.Count == 0 || colaRecepcion.Peek() != cliente) return false;

        recepcionOcupada = true;
        colaRecepcion.Dequeue();
        return true;
    }

    public void LiberarRecepcion()
    {
        recepcionOcupada = false;
    }

    public GameObject AsignarAnimal(bool quierePerro)
    {
        if (quierePerro && perrosDisponibles.Count > 0)
        {
            GameObject animal = perrosDisponibles[0];
            perrosDisponibles.RemoveAt(0);
            return animal;
        }
        else if (!quierePerro && gatosDisponibles.Count > 0)
        {
            GameObject animal = gatosDisponibles[0];
            gatosDisponibles.RemoveAt(0);
            return animal;
        }
        return null;
    }

    public void LiberarAnimal(GameObject animal)
    {
        if (animal == null) return;

        Animal animalScript = animal.GetComponent<Animal>();
        if (animalScript.tipo == Animal.TipoAnimal.Perro)
        {
            perrosDisponibles.Add(animal);
        }
        else
        {
            gatosDisponibles.Add(animal);
        }

        animal.transform.position = animalScript.tipo == Animal.TipoAnimal.Perro ? zonaPerros.position : zonaGatos.position;
        animal.transform.SetParent(null);
    }

    public void AsignarLimpiadorASala(Transform sala)
    {
        if (!estadoSalas[sala]) return;

        List<LimpiadorFSM> limpiadoresDisponibles = limpiadores
            .Where(l => l.EstadoActual == LimpiadorFSM.EstadoLimpiador.Patrullando)
            .ToList();

        if (limpiadoresDisponibles.Count > 0)
        {
            LimpiadorFSM limpiadorAsignado = limpiadoresDisponibles[Random.Range(0, limpiadoresDisponibles.Count)];
            limpiadorAsignado.IrALimpiar(sala);
            estadoSalas[sala] = false;
        }
    }

    public void SalaLimpia(Transform sala)
    {
        estadoSalas[sala] = false;
        Debug.Log("✅ Sala limpia: " + sala.name);
    }
    public void LiberarSalaEntrevista()
    {
        entrevistaOcupada = false;
        clienteConPermisoEntrevista = null;
        Debug.Log("🟢 La sala de entrevistas ha sido liberada.");
    }

    public void ClienteSalido()
    {
        clientesActuales--;
        entrevistaOcupada = false;
    }
}