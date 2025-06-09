using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LimpiadorFSM : MonoBehaviour
{
    public enum EstadoLimpiador { Inactivo, Patrullando, Limpiando }

    public EstadoLimpiador EstadoActual = EstadoLimpiador.Inactivo;
    public List<Transform> puntosPatrulla;
    public float tiempoLimpieza = 3f;

    private NavMeshAgent agente;
    private Transform objetivoActual;
    private Coroutine rutinaActual;
    private ClienteEstadoUI estadoUI;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        estadoUI = GetComponentInChildren<ClienteEstadoUI>();

        if (puntosPatrulla == null || puntosPatrulla.Count == 0)
        {
            GameObject puntosGO = GameObject.Find("PuntosPatrulla");
            if (puntosGO != null)
            {
                puntosPatrulla = new List<Transform>();
                foreach (Transform t in puntosGO.transform)
                {
                    puntosPatrulla.Add(t);
                }
            }
        }

        CambiarEstado(EstadoLimpiador.Patrullando);
    }

    public void InicializarLimpiador(List<Transform> patrullas, float tiempoLimpieza, Transform objetivoInicial, GameManager gameManager)
    {
        this.puntosPatrulla = patrullas;
        this.tiempoLimpieza = tiempoLimpieza;
        this.objetivoActual = objetivoInicial;
    }

    public void IrALimpiar(Transform objetivo)
    {
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        objetivoActual = objetivo;
        rutinaActual = StartCoroutine(RutinaIrALimpiar());
    }

    private IEnumerator RutinaIrALimpiar()
    {
        EstadoActual = EstadoLimpiador.Limpiando;

        MostrarEstado("Yendo a limpiar zona...");
        yield return StartCoroutine(SaludarSiClienteCerca());
        yield return StartCoroutine(IrA(objetivoActual));

        MostrarEstado("Limpiando zona...");
        yield return StartCoroutine(SaludarSiClienteCerca());
        yield return new WaitForSeconds(tiempoLimpieza);

        MostrarEstado("Zona limpia. Reanudando patrulla...");
        yield return new WaitForSeconds(1f);

        CambiarEstado(EstadoLimpiador.Patrullando);
    }

    void CambiarEstado(EstadoLimpiador nuevoEstado)
    {
        if (rutinaActual != null) StopCoroutine(rutinaActual);

        EstadoActual = nuevoEstado;
        switch (EstadoActual)
        {
            case EstadoLimpiador.Patrullando:
                rutinaActual = StartCoroutine(Patrullar());
                break;
            case EstadoLimpiador.Limpiando:
                rutinaActual = StartCoroutine(Limpiar());
                break;
        }
    }

    private IEnumerator Patrullar()
    {
        EstadoActual = EstadoLimpiador.Patrullando;

        while (EstadoActual == EstadoLimpiador.Patrullando)
        {
            yield return StartCoroutine(SaludarSiClienteCerca());

            if (puntosPatrulla == null || puntosPatrulla.Count == 0)
            {
                MostrarEstado("Sin puntos de patrulla");
                yield return null;
                continue;
            }

            Transform destino = puntosPatrulla[Random.Range(0, puntosPatrulla.Count)];
            MostrarEstado("Patrullando hacia punto...");
            yield return StartCoroutine(IrA(destino));
            yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
        }
    }

    private IEnumerator Limpiar()
    {
        EstadoActual = EstadoLimpiador.Limpiando;

        MostrarEstado("Limpiando zona...");
        yield return StartCoroutine(SaludarSiClienteCerca());
        yield return new WaitForSeconds(tiempoLimpieza);

        MostrarEstado("Zona limpia. Reanudando patrulla...");
        yield return new WaitForSeconds(1f);

        CambiarEstado(EstadoLimpiador.Patrullando);
    }

    private IEnumerator IrA(Transform destino)
    {
        if (destino == null || agente == null || !agente.isOnNavMesh) yield break;

        agente.isStopped = false;
        agente.SetDestination(destino.position);

        while (agente.pathPending || agente.remainingDistance > agente.stoppingDistance)
        {
            yield return StartCoroutine(SaludarSiClienteCerca());
            yield return null;
        }

        agente.isStopped = true;
    }

    private IEnumerator SaludarSiClienteCerca()
    {
        Transform cliente = ObtenerClienteCercano();
        if (cliente != null)
        {
            MostrarEstado("¡Hola!");
            Vector3 mirar = new Vector3(cliente.position.x, transform.position.y, cliente.position.z);
            transform.LookAt(mirar);
            yield return new WaitForSeconds(0.3f);
            MostrarEstado(""); // Oculta el saludo
        }
    }


    private bool ClienteCerca(float radio = 10f)
    {
        Collider[] colisiones = Physics.OverlapSphere(transform.position, radio);
        foreach (var col in colisiones)
        {
            if (col.CompareTag("Cliente"))
            {
                return true;
            }
        }
        return false;
    }

    private void MostrarEstado(string mensaje)
    {
        estadoUI?.ActualizarTexto(mensaje);
    }

    private Transform ObtenerClienteCercano(float radio = 5f)
    {
        Collider[] colisiones = Physics.OverlapSphere(transform.position, radio);
        Debug.Log($"🧠 Limpiador [{name}] revisando colisiones. Detectadas: {colisiones.Length}");

        foreach (var col in colisiones)
        {
            Debug.Log($"➡️ Detectado: {col.name}, Tag: {col.tag}");

            if (col.CompareTag("Cliente"))
            {
                Debug.Log($"✅ Cliente detectado: {col.name}");
                return col.transform;
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}