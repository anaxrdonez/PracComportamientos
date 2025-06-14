using System.Collections;
using UnityEngine;

public class EntrevistadorFSM : MonoBehaviour
{
    public enum EstadoEntrevistador { Esperando, Entrevistando, DandoResultado }
    public EstadoEntrevistador estadoActual = EstadoEntrevistador.Esperando;

    public Animator animador;
    private ClienteBT clienteActual = null;
    private GameManager gameManager;

    // Referencias a los iconos
    public GameObject iconoAprobado;
    public GameObject iconoDenegado;

    public void Inicializar(GameManager manager)
    {
        gameManager = manager;
    }

    void Start()
    {
        // Asegúrate de ocultarlos al inicio
        if (iconoAprobado != null) iconoAprobado.SetActive(false);
        if (iconoDenegado != null) iconoDenegado.SetActive(false);

        StartCoroutine(FSM());
    }

    public void ClienteLlega(ClienteBT cliente)
    {
        Debug.Log("ClienteLlega llamado en EntrevistadorFSM: " + cliente.name);
        Debug.Log("Estado actual del entrevistador: " + estadoActual);

        if (estadoActual == EstadoEntrevistador.Esperando)
        {
            clienteActual = cliente;
            estadoActual = EstadoEntrevistador.Entrevistando;
            Debug.Log("Entrevistador cambia a estado: Entrevistando");
        }
        else
        {
            Debug.LogWarning("Entrevistador no puede aceptar cliente: no está en estado 'Esperando'");
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
                    if (clienteActual != null)
                    {
                        while (clienteActual.GetComponent<DetectarZona>().zonaActual != "SalaEntrevista")
                            yield return null;

                        animador.Play("Entrevistando");
                        yield return new WaitForSeconds(5f);

                        estadoActual = EstadoEntrevistador.DandoResultado;
                    }
                    break;

                case EstadoEntrevistador.DandoResultado:
                    if (clienteActual != null)
                    {
                        clienteActual.RealizarResultadoEntrevista();

                        bool aprobado = clienteActual.Aprobado();
                        animador.SetInteger("Resultado", aprobado ? 1 : 0);

                        // Mostrar solo el icono correspondiente
                        if (iconoAprobado != null) iconoAprobado.SetActive(aprobado);
                        if (iconoDenegado != null) iconoDenegado.SetActive(!aprobado);

                        yield return new WaitForSeconds(3f); // duración animación + icono

                        // Ocultar ambos iconos
                        if (iconoAprobado != null) iconoAprobado.SetActive(false);
                        if (iconoDenegado != null) iconoDenegado.SetActive(false);

                        clienteActual.LiberarSalaEntrevista();
                        clienteActual = null;
                    }

                    animador.SetInteger("Resultado", -1);
                    estadoActual = EstadoEntrevistador.Esperando;
                    break;
            }

            yield return null;
        }
    }
}
