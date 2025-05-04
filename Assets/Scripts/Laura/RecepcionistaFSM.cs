using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class RecepcionistaFSM : MonoBehaviour
{
    public enum Estado { EsperandoCliente, Registrando, Informando }
    private Estado estadoActual = Estado.EsperandoCliente;

    private Queue<ClienteBT> colaClientes = new Queue<ClienteBT>();
    private ClienteBT clienteActual;

    public Transform puntoRecepcion;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (puntoRecepcion == null)
        {
            //Debug.LogError("❌ puntoRecepcion no asignado en RecepcionistaFSM.");
        }

        StartCoroutine(FSM());
    }

    public void ClienteLlega(ClienteBT nuevoCliente)
    {
        Debug.Log("📥 Cliente llegó a la recepción.");
        colaClientes.Enqueue(nuevoCliente);
    }

    private IEnumerator FSM()
    {
        while (true)
        {
            Debug.Log($"📌 Estado actual: {estadoActual}");

            switch (estadoActual)
            {
                case Estado.EsperandoCliente:
                    if (colaClientes.Count > 0)
                    {
                        clienteActual = colaClientes.Dequeue();
                        Debug.Log("🎯 Cliente dequeued y listo para registrar");
                        estadoActual = Estado.Registrando;
                    }
                    break;

                case Estado.Registrando:
                    Debug.Log("✍️ Esperando que el cliente llegue a CheckIn...");
                    while (clienteActual != null && clienteActual.DetectarZonaActual() != "CheckIn")
                    {
                        yield return null;
                    }

                    Debug.Log("📍 Cliente ha llegado a CheckIn (confirmado por zona)");
                    yield return new WaitForSeconds(2f); // Simula tiempo de registro

                    if (clienteActual != null)
                    {
                        Debug.Log("✅ Confirmando check-in del cliente");
                        clienteActual.ConfirmarCheckIn();
                        gameManager?.LiberarRecepcion(); // 🔓 Liberar aquí la recepción
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ clienteActual es null en Registrando");
                    }

                    estadoActual = Estado.Informando;
                    break;

                case Estado.Informando:
                    Debug.Log("📋 Informando al cliente.");
                    yield return new WaitForSeconds(1f);
                    estadoActual = Estado.EsperandoCliente;
                    break;
            }
            yield return null;
        }
    }
}