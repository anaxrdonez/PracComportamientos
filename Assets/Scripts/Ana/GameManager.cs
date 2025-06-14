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

    [Header("Puntos de Referencia - Animales")]
    [Header("Zonas de Perros")]
    public Transform puntoComidaPerros;
    public Transform puntoDescansoPerros;
    public Transform puntoJuegoPerros;

    [Header("Zonas de Gatos")]
    public Transform puntoComidaGatos;
    public Transform puntoDescansoGatos;
    public Transform puntoJuegoGatos;

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

    //tabla de compatibilidad para asignar animales a clientes
    private Dictionary<(string cliente, string animal), float> compatibilidad = new()
{
    { ("Cariñoso", "Cariñoso"), 1f },
    { ("Cariñoso", "Tranquilo"), 0.8f },
    { ("Cariñoso", "Activo"), 0.6f },

    { ("Activo", "Activo"), 1f },
    { ("Activo", "Cariñoso"), 0.7f },
    { ("Activo", "Tranquilo"), 0.4f },

    { ("Independiente", "Tranquilo"), 1f },
    { ("Independiente", "Activo"), 0.6f },
    { ("Independiente", "Cariñoso"), 0.3f },

};

    void Start()
    {
        Debug.Log("Iniciando GameManager...");
        StartCoroutine(GenerarClientes());
        GenerarLimpiadores();
        StartCoroutine(ControlSuciedadSalas());

        foreach (var sala in salas)
            estadoSalas[sala] = false;

        for (int i = 0; i < 3; i++)
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
                    clienteScript.personalidadCliente = ObtenerPersonalidadAleatoriaCliente();


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


    public string ObtenerPersonalidadAleatoriaCliente()
    {
        string[] opciones = { "Cariñoso", "Activo", "Independiente" };
        return opciones[UnityEngine.Random.Range(0, opciones.Length)];
    }
    void GenerarLimpiadores()
    {
        for (int i = 0; i < numLimpiadores; i++)
        {
            GameObject nuevoLimpiador = Instantiate(limpiadorPrefab, puntoSpawnLimpiadores.position, Quaternion.identity);
            LimpiadorFSM limpiadorScript = nuevoLimpiador.GetComponent<LimpiadorFSM>();

            if (limpiadorScript != null)
            {
                limpiadorScript.InicializarLimpiador(puntosPatrulla, 3f, almacen, this);

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

            //Notificar al entrevistador directamente
            EntrevistadorFSM entrevistador = FindObjectOfType<EntrevistadorFSM>();
            if (entrevistador != null)
            {
                entrevistador.ClienteLlega(cliente);
                Debug.Log(" Entrevistador notificado de la llegada del cliente.");
            }

            clienteConPermisoEntrevista = null;
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

    public GameObject AsignarAnimal(bool quierePerro, string personalidadCliente)
    {
        List<GameObject> candidatos = quierePerro ? perrosDisponibles : gatosDisponibles;

        if (candidatos.Count == 0) return null;

        GameObject mejorAnimal = null;
        float mejorCompatibilidad = -1f;

        foreach (var animal in candidatos)
        {
            Animal animalInstance = animal.GetComponent<Animal>();
            if (animalInstance == null) continue;

            string personalidadAnimal = animalInstance.personalidadAnimal;
            float compat = CalcularCompatibilidad(personalidadCliente, personalidadAnimal);

            if (compat > mejorCompatibilidad)
            {
                mejorCompatibilidad = compat;
                mejorAnimal = animal;
            }
        }

        if (mejorAnimal != null)
        {
            candidatos.Remove(mejorAnimal);
            return mejorAnimal;
        }

        return null;
    }


    public float CalcularCompatibilidad(string cliente, string animal)
    {
        if (compatibilidad.TryGetValue((cliente, animal), out float valor))
            return valor;
        return 0.5f;
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