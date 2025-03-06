using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject clientePrefab;
    public GameObject perroPrefab, gatoPrefab;

    [Header("Puntos de Referencia")]
    public Transform puntoSpawnClientes;
    public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;

    private Queue<ClienteBT> colaCheckIn = new Queue<ClienteBT>();
    private Queue<ClienteBT> colaSalaEspera = new Queue<ClienteBT>();
    private bool checkInOcupado = false, entrevistaOcupada = false;

    [Header("Configuraciones")]
    public int maxClientes = 3;
    private int clientesActuales = 0;

    private List<GameObject> perrosDisponibles = new List<GameObject>();
    private List<GameObject> gatosDisponibles = new List<GameObject>();

    void Start()
    {
        Debug.Log("Iniciando GameManager...");
        StartCoroutine(GenerarClientes());

        // Generar 5 perros y 5 gatos en la zona de adopción
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

    private void RevisarCheckIn()
    {
        if (!checkInOcupado && colaCheckIn.Count > 0)
        {
            StartCoroutine(ClienteEnCheckIn(colaCheckIn.Peek()));
        }
    }

    public IEnumerator ClienteEnCheckIn(ClienteBT cliente)
    {
        while (checkInOcupado || colaCheckIn.Count == 0 || colaCheckIn.Peek() != cliente)
            yield return null;

        Debug.Log(cliente.name + " moviéndose al Check-In...");
        yield return cliente.IrA(puntoCheckIn);

        while (cliente.DetectarZonaActual() != "CheckIn")
            yield return null;

        Debug.Log(cliente.name + " llegó al Check-In.");
        checkInOcupado = true;
        colaCheckIn.Dequeue();

        yield return new WaitForSeconds(2f); // Simular proceso de Check-In

        checkInOcupado = false;
        cliente.MoverASalaEspera();
        RevisarCheckIn();
    }

    public void ClienteEnSalaEspera(ClienteBT cliente)
    {
        colaSalaEspera.Enqueue(cliente);
        RevisarSalaEspera();
    }

    private void RevisarSalaEspera()
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
            yield return null; // Espera a que la entrevista esté libre

        entrevistaOcupada = true;
        Debug.Log(cliente.name + " se mueve a la Entrevista...");

        yield return cliente.IrA(salaEntrevista); // 🔥 Espera hasta que realmente llegue
        while (cliente.DetectarZonaActual() != "SalaEntrevista")
            yield return null;

        Debug.Log(cliente.name + " llegó a la Sala de Entrevista.");
        cliente.IniciarEntrevista(); // 🔥 La entrevista comienza SOLO cuando el cliente está presente
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
        return null; // No hay animales disponibles
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

    void ClienteSalido()
    {
        clientesActuales--;
    }
}