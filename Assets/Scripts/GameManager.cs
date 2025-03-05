using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject clientePrefab, limpiadorPrefab;
    public Transform puntoSpawnClientes;
    public Transform[] puntosSpawnLimpiadores;
    public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaJuegos, checkout, salida, almacen;
    public Transform[] puntosPatrulla, salas;

    private int maxClientes = 2, clientesActuales = 0, maxLimpiadores = 1;
    private bool salaEntrevistaOcupada = false; // NUEVO: Variable para saber si hay alguien en la entrevista

    private Queue<ClienteBT> clientesEnEspera = new Queue<ClienteBT>(); // NUEVO: Cola de clientes esperando

    void Start()
    {
        for (int i = 0; i < maxLimpiadores; i++)
        {
            GameObject nuevoLimpiador = Instantiate(limpiadorPrefab, puntosSpawnLimpiadores[i].position, Quaternion.identity);
            nuevoLimpiador.GetComponent<LimpiadorFSM>().InicializarLimpiador(almacen, puntosPatrulla, salas);
        }
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
                clienteScript.InicializarCliente(puntoCheckIn, salaEspera, salaEntrevista, zonaJuegos, checkout, salida, this);
                clientesActuales++;
                clienteScript.OnClienteSalido += ClienteSalido;
            }
        }
    }

    public bool SalaEntrevistaOcupada() // NUEVO: Saber si hay alguien en la entrevista
    {
        return salaEntrevistaOcupada;
    }

    public void OcupaSalaEntrevista() // NUEVO: Marcar la entrevista como ocupada
    {
        salaEntrevistaOcupada = true;
    }

    public void LiberaSalaEntrevista() // NUEVO: Marcar la entrevista como libre y permitir al siguiente cliente entrar
    {
        salaEntrevistaOcupada = false;

        if (clientesEnEspera.Count > 0)
        {
            ClienteBT siguienteCliente = clientesEnEspera.Dequeue();
            siguienteCliente.IniciarEntrevista();
        }
    }

    public void AgregarClienteAEspera(ClienteBT cliente) // NUEVO: Agregar clientes a la sala de espera
    {
        clientesEnEspera.Enqueue(cliente);
    }

    void ClienteSalido()
    {
        clientesActuales--;
    }
}