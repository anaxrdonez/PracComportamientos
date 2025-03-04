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

    private int maxClientes = 8, clientesActuales = 0, maxLimpiadores = 3;

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
                clienteScript.InicializarCliente(puntoCheckIn, salaEspera, salaEntrevista, zonaJuegos, checkout, salida);
                clientesActuales++;
                clienteScript.OnClienteSalido += ClienteSalido;
            }
        }
    }

    void ClienteSalido()
    {
        clientesActuales--;
    }
}