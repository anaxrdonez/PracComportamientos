using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UI; // 👈 Añadido para usar Image y Sprite

public class RecepcionistaFSM : MonoBehaviour
{
    public enum Estado { EsperandoCliente, Registrando, Informando }
    private Estado estadoActual = Estado.EsperandoCliente;

    private Queue<ClienteBT> colaClientes = new Queue<ClienteBT>();
    private ClienteBT clienteActual;

    public Transform puntoRecepcion;
    private GameManager gameManager;

    [Header("UI de estado")]
    public Image estadoIcono;                // Imagen del canvas
    public Sprite iconoEsperando;            // Icono para estado Esperando
    public Sprite iconoRegistrando;          // Icono para estado Registrando
    public Sprite iconoInformando;           // Icono para estado Informando

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (puntoRecepcion == null)
            Debug.LogError("❌ puntoRecepcion no asignado en RecepcionistaFSM.");

        ActualizarIconoEstado(); // 👈 Mostrar icono inicial

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
            switch (estadoActual)
            {
                case Estado.EsperandoCliente:
                    if (colaClientes.Count > 0)
                    {
                        clienteActual = colaClientes.Dequeue();
                        estadoActual = Estado.Registrando;
                        ActualizarIconoEstado(); // 👈 Actualiza icono
                    }
                    break;

                case Estado.Registrando:
                    while (clienteActual != null && clienteActual.DetectarZonaActual() != "CheckIn")
                        yield return null;

                    yield return new WaitForSeconds(2f);

                    if (clienteActual != null)
                    {
                        clienteActual.ConfirmarCheckIn();
                        gameManager?.LiberarRecepcion();
                    }

                    estadoActual = Estado.Informando;
                    ActualizarIconoEstado(); // 👈 Actualiza icono
                    break;

                case Estado.Informando:
                    yield return new WaitForSeconds(1f);
                    estadoActual = Estado.EsperandoCliente;
                    ActualizarIconoEstado(); // 👈 Actualiza icono
                    break;
            }

            yield return null;
        }
    }

    private void ActualizarIconoEstado()
    {
        if (estadoIcono == null) return;

        switch (estadoActual)
        {
            case Estado.EsperandoCliente:
                estadoIcono.sprite = iconoEsperando;
                break;
            case Estado.Registrando:
                estadoIcono.sprite = iconoRegistrando;
                break;
            case Estado.Informando:
                estadoIcono.sprite = iconoInformando;
                break;
        }

        estadoIcono.enabled = true;
    }
}
