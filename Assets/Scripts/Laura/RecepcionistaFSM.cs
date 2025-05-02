using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecepcionistaFSM : MonoBehaviour
{
    public enum Estado { EsperandoCliente, Registrando, Informando, LlamandoASalaEspera }
    private Estado estadoActual = Estado.EsperandoCliente;

    private Queue<ClienteBT> colaClientes = new Queue<ClienteBT>();
    private ClienteBT clienteActual;

    void Start()
    {
        StartCoroutine(FSM());
    }

    public void ClienteLlega(ClienteBT nuevoCliente)
    {
        colaClientes.Enqueue(nuevoCliente);
    }

    private IEnumerator FSM()
    {
        while (true)
        {
            switch (estadoActual)
            {
                case Estado.EsperandoCliente:
                    if (colaClientes.Count > 0)
                    {
                        clienteActual = colaClientes.Dequeue();
                        estadoActual = Estado.Registrando;
                    }
                    break;

                case Estado.Registrando:
                    yield return StartCoroutine(clienteActual.IrA(clienteActual.transform)); // espera que llegue al check-in
                    yield return new WaitForSeconds(2f); // simula tiempo de registro
                    estadoActual = Estado.Informando;
                    break;

                case Estado.Informando:
                    Debug.Log("📋 Cliente registrado. Informando al cliente sobre el proceso.");
                    yield return new WaitForSeconds(1f);
                    estadoActual = Estado.LlamandoASalaEspera;
                    break;

                case Estado.LlamandoASalaEspera:
                    Debug.Log("➡️ Enviando cliente a la sala de espera.");
                    StartCoroutine(clienteActual.MoverASalaEspera());
                    estadoActual = Estado.EsperandoCliente;
                    break;
            }

            yield return null;
        }
    }
}
