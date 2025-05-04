using System.Collections;
using UnityEngine;

public class EntrevistadorFSM : MonoBehaviour
{
    public enum EstadoEntrevistador { Esperando, Entrevistando, DandoResultado }
    public EstadoEntrevistador estadoActual = EstadoEntrevistador.Esperando;

    public Animator animador;
    private ClienteBT clienteActual = null;
    private GameManager gameManager;

    public void Inicializar(GameManager manager)
    {
        gameManager = manager;
    }

    void Start()
    {
        StartCoroutine(FSM());
    }

    public void ClienteLlega(ClienteBT cliente)
    {
        Debug.Log("📨 ClienteLlega llamado en EntrevistadorFSM: " + cliente.name);
        Debug.Log("🔎 Estado actual del entrevistador: " + estadoActual);

        if (estadoActual == EstadoEntrevistador.Esperando)
        {
            clienteActual = cliente;
            estadoActual = EstadoEntrevistador.Entrevistando;
            Debug.Log("✅ Entrevistador cambia a estado: Entrevistando");
        }
        else
        {
            Debug.LogWarning("⚠️ Entrevistador no puede aceptar cliente: no está en estado 'Esperando'");
        }
    }



    private IEnumerator FSM()
    {
        while (true)
        {
            switch (estadoActual)
            {
                case EstadoEntrevistador.Esperando:
                    animador.Play("Esperando");
                    yield return null;
                    break;

                case EstadoEntrevistador.Entrevistando:
                    // Esperar a que el cliente llegue realmente a la sala
                    if (clienteActual != null)
                    {
                        // Espera hasta que el cliente esté en la zona correcta
                        while (clienteActual.GetComponent<DetectarZona>().zonaActual != "SalaEntrevista")
                        {
                            yield return null;
                        }

                        // Una vez dentro, ahora sí comienza la entrevista
                        animador.Play("Entrevistando");
                        yield return new WaitForSeconds(5f); // duración de la animación

                        estadoActual = EstadoEntrevistador.DandoResultado;
                    }
                    break;


                case EstadoEntrevistador.DandoResultado:
                    if (clienteActual != null)
                    {
                        clienteActual.RealizarResultadoEntrevista(); // calcula si es apto

                        // Aquí lanzamos la animación correspondiente mediante el parámetro
                        animador.SetInteger("Resultado", clienteActual.Aprobado() ? 1 : 0);

                        yield return new WaitForSeconds(2f); // espera a que termine la animación

                        clienteActual.LiberarSalaEntrevista();
                        clienteActual = null;
                    }

                    // Reseteamos el parámetro para la próxima vez
                    animador.SetInteger("Resultado", -1);

                    estadoActual = EstadoEntrevistador.Esperando;
                    break;
            }

            yield return null;
        }
    }
}
