using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject clientePrefab;
    public Transform puntoSpawnClientes, puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;

    private Queue<ClienteBT> colaCheckIn = new Queue<ClienteBT>();
    private Queue<ClienteBT> colaSalaEspera = new Queue<ClienteBT>();

    private bool checkInOcupado = false, entrevistaOcupada = false;

    void Start()
    {
        StartCoroutine(GenerarClientes());
    }

    IEnumerator GenerarClientes()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));

            GameObject nuevoCliente = Instantiate(clientePrefab, puntoSpawnClientes.position, Quaternion.identity);
            ClienteBT clienteScript = nuevoCliente.GetComponent<ClienteBT>();
            clienteScript.InicializarCliente(puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida, this);

            colaCheckIn.Enqueue(clienteScript);
            RevisarCheckIn();
        }
    }

    public IEnumerator ClienteEnCheckIn(ClienteBT cliente)
    {
        while (checkInOcupado || colaCheckIn.Count == 0 || colaCheckIn.Peek() != cliente)
            yield return null;

        Debug.Log(cliente.name + " moviéndose al Check-In...");
        yield return cliente.IrA(puntoCheckIn);

        while (cliente.DetectarZonaActual() != "CheckIn") // 🔥 Usar una función para acceder a detectarZona
            yield return null;

        Debug.Log(cliente.name + " llegó al Check-In. Ahora será atendido.");
        checkInOcupado = true;
        colaCheckIn.Dequeue();

        yield return new WaitForSeconds(2f); // Simular proceso de Check-In

        checkInOcupado = false;
        cliente.MoverASalaEspera();
        RevisarCheckIn();
    }



    private void RevisarCheckIn()
    {
        if (!checkInOcupado && colaCheckIn.Count > 0)
        {
            StartCoroutine(ClienteEnCheckIn(colaCheckIn.Peek()));
        }
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
            yield return null;

        entrevistaOcupada = true;
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
}