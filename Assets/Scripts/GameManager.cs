using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    private Queue<ClienteBT> colaCheckIn = new Queue<ClienteBT>();
    private Queue<ClienteBT> colaSalaEspera = new Queue<ClienteBT>();
    private bool checkInOcupado = false, entrevistaOcupada = false;
    private List<LimpiadorFSM> limpiadores = new List<LimpiadorFSM>();
    private Dictionary<Transform, bool> estadoSalas = new Dictionary<Transform, bool>();

    [Header("Configuraciones")]
    public int maxClientes = 3;
    public int numLimpiadores = 2;
    private int clientesActuales = 0;

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
            estadoSalas[sala] = false; // Todas las salas empiezan limpias

        for (int i = 0; i < 5; i++)
        {
            GameObject perro = Instantiate(perroPrefab, zonaPerros.position, Quaternion.identity);
            GameObject gato = Instantiate(gatoPrefab, zonaGatos.position, Quaternion.identity);
            perrosDisponibles.Add(perro);
            gatosDisponibles.Add(gato);
        }
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
                    clienteScript.OnClienteSalido += ClienteSalido;
                    colaCheckIn.Enqueue(clienteScript);
                    RevisarCheckIn();
                    Debug.Log("Cliente creado exitosamente.");
                }
                else
                {
                    Debug.LogError("❌ ERROR: El prefab de Cliente no tiene el script ClienteBT adjunto.");
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
            if (!estadoSalas[salaSucia]) // Solo ensuciar si está limpia
            {
                estadoSalas[salaSucia] = true;
                Debug.Log("⚠️ La sala " + salaSucia.name + " se ha ensuciado.");
                AsignarLimpiadorASala(salaSucia);
            }
        }
    }

    private void RevisarCheckIn()
    {
        if (!checkInOcupado && colaCheckIn.Count > 0)
        {
            ClienteBT siguienteCliente = colaCheckIn.Dequeue();
            StartCoroutine(ClienteEnCheckIn(siguienteCliente));
        }
    }

    public IEnumerator ClienteEnCheckIn(ClienteBT cliente)
    {
        checkInOcupado = true;
        Debug.Log(cliente.name + " moviéndose al Check-In...");
        yield return cliente.IrA(puntoCheckIn);

        while (cliente.DetectarZonaActual() != "CheckIn")
            yield return null;

        Debug.Log(cliente.name + " llegó al Check-In.");
        yield return new WaitForSeconds(2f);

        checkInOcupado = false;
        cliente.MoverASalaEspera();
        RevisarCheckIn();
    }

    public void ClienteEnSalaEspera(ClienteBT cliente)
    {
        colaSalaEspera.Enqueue(cliente);
        RevisarSalaEspera();
    }

    public void RevisarSalaEspera()
    {
        if (!entrevistaOcupada && colaSalaEspera.Count > 0)
        {
            ClienteBT siguienteCliente = colaSalaEspera.Dequeue();
            StartCoroutine(ClienteEnEntrevista(siguienteCliente));
        }
    }

    public IEnumerator ClienteEnEntrevista(ClienteBT cliente)
    {
        while (entrevistaOcupada)
            yield return null;

        entrevistaOcupada = true;
        Debug.Log(cliente.name + " se mueve a la Entrevista...");
        yield return cliente.IrA(salaEntrevista);

        while (cliente.DetectarZonaActual() != "SalaEntrevista")
            yield return null;

        Debug.Log(cliente.name + " llegó a la Sala de Entrevista.");
        cliente.IniciarEntrevista();
    }

    public void OcupaSalaEntrevista()
    {
        entrevistaOcupada = true;
    }

    public void LiberaSalaEntrevista()
    {
        entrevistaOcupada = false;
        RevisarSalaEspera();
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

        // Seleccionar un limpiador aleatorio entre los que están patrullando
        List<LimpiadorFSM> limpiadoresDisponibles = limpiadores
            .Where(l => l.EstadoActual == LimpiadorFSM.EstadoLimpiador.Patrullando)
            .ToList();

        if (limpiadoresDisponibles.Count > 0)
        {
            LimpiadorFSM limpiadorAsignado = limpiadoresDisponibles[Random.Range(0, limpiadoresDisponibles.Count)];
            limpiadorAsignado.IrALimpiar(sala);
            estadoSalas[sala] = false; // Marcar sala como en proceso de limpieza
        }
    }

    public List<LimpiadorFSM> ObtenerLimpiadores()
    {
        return limpiadores;
    }



    public void SalaLimpia(Transform sala)
    {
        estadoSalas[sala] = false;
        Debug.Log("✅ Sala limpia: " + sala.name);
    }

    void ClienteSalido()
    {
        clientesActuales--;
    }
}
