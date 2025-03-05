using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject clientePrefab, limpiadorPrefab;
    public Transform puntoSpawnClientes;
    public Transform[] puntosSpawnLimpiadores;
    public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida, almacen;
    public Transform[] puntosPatrulla, salas;

    private int maxClientes = 2, clientesActuales = 0, maxLimpiadores = 1;
    private bool salaEntrevistaOcupada = false;
    private Queue<ClienteBT> clientesEnEspera = new Queue<ClienteBT>(); // Cola de clientes esperando entrevista

    void Start()
    {
        // Generar limpiadores
        for (int i = 0; i < maxLimpiadores; i++)
        {
            GameObject nuevoLimpiador = Instantiate(limpiadorPrefab, puntosSpawnLimpiadores[i].position, Quaternion.identity);
            nuevoLimpiador.GetComponent<LimpiadorFSM>().InicializarLimpiador(almacen, puntosPatrulla, salas);
        }

        // Generar clientes
        StartCoroutine(GenerarClientes());
    }

    IEnumerator GenerarClientes()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));

            if (clientesActuales < maxClientes)
            {
                GameObject nuevoCliente = Instantiate(clientePrefab, puntoSpawnClientes.position, Quaternion.identity);
                ClienteBT clienteScript = nuevoCliente.GetComponent<ClienteBT>();
                clienteScript.InicializarCliente(puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida, this);
                clientesActuales++;
                clienteScript.OnClienteSalido += ClienteSalido;

                // El Cliente siempre empieza por hacer Check-In
                StartCoroutine(ClienteHacerCheckIn(clienteScript));
            }
        }
    }

    IEnumerator ClienteHacerCheckIn(ClienteBT cliente)
    {
        yield return cliente.RealizarCheckIn();

        if (!salaEntrevistaOcupada)
        {
            cliente.IniciarEntrevista();
        }
        else
        {
            StartCoroutine(EnviarClienteASalaEspera(cliente));
        }
    }

    IEnumerator EnviarClienteASalaEspera(ClienteBT cliente)
    {
        yield return cliente.MoverASalaEspera();
        AgregarClienteAEspera(cliente);
    }

    public void AgregarClienteAEspera(ClienteBT cliente)
    {
        clientesEnEspera.Enqueue(cliente);
        RevisarSalaDeEspera(); // 🔥 Si la Entrevista está libre, mover al siguiente cliente
    }

    public bool SalaEntrevistaOcupada()
    {
        return salaEntrevistaOcupada;
    }

    public void OcupaSalaEntrevista()
    {
        salaEntrevistaOcupada = true;
    }

    public void LiberaSalaEntrevista()
    {
        salaEntrevistaOcupada = false;

        if (clientesEnEspera.Count > 0)
        {
            ClienteBT siguienteCliente = clientesEnEspera.Dequeue();
            StartCoroutine(MoverClienteAEntrevista(siguienteCliente));
        }
    }

    // 🔥 Si hay clientes en la Sala de Espera y la Entrevista está libre, el primero pasa automáticamente.
    private void RevisarSalaDeEspera()
    {
        if (!salaEntrevistaOcupada && clientesEnEspera.Count > 0)
        {
            ClienteBT siguienteCliente = clientesEnEspera.Dequeue();
            StartCoroutine(MoverClienteAEntrevista(siguienteCliente));
        }
    }

    private IEnumerator MoverClienteAEntrevista(ClienteBT cliente)
    {
        yield return cliente.SalirDeSalaEspera();
        yield return cliente.IrAEntrevista();
        cliente.IniciarEntrevista();
    }

    void ClienteSalido()
    {
        clientesActuales--;
    }
}