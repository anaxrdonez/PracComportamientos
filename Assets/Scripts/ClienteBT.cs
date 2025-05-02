// ClienteBT.cs - actualizado con NodoCondicional para adopción
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NodoResultado
{
    Exito,
    Fallo,
    Ejecutando
}

public abstract class NodoBT
{
    public abstract NodoResultado Tick();
}

public class NodoSecuencia : NodoBT
{
    private readonly List<NodoBT> hijos;
    private int indiceActual = 0;

    public NodoSecuencia(List<NodoBT> hijos)
    {
        this.hijos = hijos;
    }

    public override NodoResultado Tick()
    {
        while (indiceActual < hijos.Count)
        {
            var resultado = hijos[indiceActual].Tick();
            if (resultado == NodoResultado.Ejecutando) return NodoResultado.Ejecutando;
            if (resultado == NodoResultado.Fallo)
            {
                indiceActual = 0;
                return NodoResultado.Fallo;
            }
            indiceActual++;
        }
        indiceActual = 0;
        return NodoResultado.Exito;
    }
}

public class NodoAccion : NodoBT
{
    private readonly Func<NodoResultado> accion;

    public NodoAccion(Func<NodoResultado> accion)
    {
        this.accion = accion;
    }

    public override NodoResultado Tick() => accion();
}

public class NodoEsperarZona : NodoBT
{
    private readonly ClienteBT cliente;
    private readonly string zonaObjetivo;

    public NodoEsperarZona(ClienteBT cliente, string zona)
    {
        this.cliente = cliente;
        zonaObjetivo = zona;
    }

    public override NodoResultado Tick()
    {
        return cliente.DetectarZonaActual() == zonaObjetivo ? NodoResultado.Exito : NodoResultado.Ejecutando;
    }
}

public class NodoEsperarCheckInConfirmado : NodoBT
{
    private ClienteBT cliente;

    public NodoEsperarCheckInConfirmado(ClienteBT cliente)
    {
        this.cliente = cliente;
    }

    public override NodoResultado Tick()
    {
        return cliente.CheckInConfirmado() ? NodoResultado.Exito : NodoResultado.Ejecutando;
    }
}

public class NodoColaRecepcion : NodoBT
{
    private readonly ClienteBT cliente;
    private readonly GameManager gameManager;
    private bool registrado = false;

    public NodoColaRecepcion(ClienteBT cliente, GameManager gameManager)
    {
        this.cliente = cliente;
        this.gameManager = gameManager;
    }

    public override NodoResultado Tick()
    {
        if (!registrado)
        {
            gameManager.ClienteARecepcion(cliente);
            registrado = true;
        }
        return gameManager.ClientePuedeSerRegistrado(cliente)
            ? NodoResultado.Exito
            : NodoResultado.Ejecutando;
    }
}

public class NodoEntrevista : NodoBT
{
    private readonly ClienteBT cliente;
    private bool hecha = false;
    private float tiempo;

    public NodoEntrevista(ClienteBT cliente)
    {
        this.cliente = cliente;
    }

    public override NodoResultado Tick()
    {
        if (hecha) return NodoResultado.Exito;
        tiempo += Time.deltaTime;
        if (tiempo >= 4f)
        {
            cliente.RealizarResultadoEntrevista();
            cliente.LiberarSalaEntrevista();  
            hecha = true;
            return NodoResultado.Exito;
        }

        return NodoResultado.Ejecutando;
    }
}

public class NodoAdopcion : NodoBT
{
    private readonly ClienteBT cliente;
    private bool hecho = false;
    private float tiempo;

    public NodoAdopcion(ClienteBT cliente)
    {
        this.cliente = cliente;
    }

    public override NodoResultado Tick()
    {
        if (hecho) return NodoResultado.Exito;
        tiempo += Time.deltaTime;
        if (tiempo >= 5f)
        {
            cliente.AsignarAnimal();
            hecho = true;
            return NodoResultado.Exito;
        }
        return NodoResultado.Ejecutando;
    }
}

public class NodoColaEntrevista : NodoBT
{
    private readonly ClienteBT cliente;
    private readonly GameManager gameManager;
    private bool registrado = false;

    public NodoColaEntrevista(ClienteBT cliente, GameManager gameManager)
    {
        this.cliente = cliente;
        this.gameManager = gameManager;
    }

    public override NodoResultado Tick()
    {
        if (!registrado)
        {
            gameManager.ClienteEnSalaEspera(cliente);
            registrado = true;
        }
        return gameManager.ClientePuedeEntrevistarse(cliente)
            ? NodoResultado.Exito
            : NodoResultado.Ejecutando;
    }
}

public class NodoCondicional : NodoBT
{
    private readonly Func<bool> condicion;
    private readonly NodoBT hijo;

    public NodoCondicional(Func<bool> condicion, NodoBT hijo)
    {
        this.condicion = condicion;
        this.hijo = hijo;
    }

    public override NodoResultado Tick()
    {
        if (!condicion()) return NodoResultado.Exito;
        return hijo.Tick();
    }
}

public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private DetectarZona detectarZona;
    private GameManager gameManager;

    [Header("Puntos")] public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;

    private bool registrado = false, entrevistado = false, aprobado = false, enSalaEspera = false;
    private bool quierePerro = false;
    private GameObject animalAsignado;
    private bool checkInCompletado = false;

    private NodoBT arbol;
    private NodoResultado estadoActual = NodoResultado.Ejecutando;
    private Transform destinoActual = null;

    public string DetectarZonaActual() => detectarZona != null ? detectarZona.zonaActual : "FueraDeZona";

    public void InicializarCliente(Transform checkIn, Transform espera, Transform entrevista, Transform gatos, Transform perros, Transform check, Transform outRefugio, GameManager manager)
    {
        puntoCheckIn = checkIn;
        salaEspera = espera;
        salaEntrevista = entrevista;
        zonaGatos = gatos;
        zonaPerros = perros;
        checkout = check;
        salida = outRefugio;
        gameManager = manager;
    }

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        detectarZona = GetComponent<DetectarZona>();
        ConstruirArbol();
    }

    void Update()
    {
        if (estadoActual == NodoResultado.Ejecutando && arbol != null)
        {
            estadoActual = arbol.Tick();
        }
    }

    void ConstruirArbol()
    {
        NodoBT checkInSecuencia = new NodoSecuencia(new List<NodoBT> {
            new NodoColaRecepcion(this, gameManager),
            new NodoAccion(() => IrA(puntoCheckIn)),
            new NodoEsperarZona(this, "CheckIn"),
            new NodoEsperarCheckInConfirmado(this)
        });

        NodoBT irEspera = new NodoAccion(() => IrA(salaEspera));
        NodoBT esperarSala = new NodoEsperarZona(this, "SalaEspera");

        NodoBT registroCola = new NodoColaEntrevista(this, gameManager);

        NodoBT irEntrevista = new NodoAccion(() => IrA(salaEntrevista));
        NodoBT esperarEntrevista = new NodoEsperarZona(this, "SalaEntrevista");

        NodoBT entrevista = new NodoEntrevista(this);

        NodoBT adopcion = new NodoSecuencia(new List<NodoBT> {
            new NodoAccion(() => IrA(quierePerro ? zonaPerros : zonaGatos)),
            new NodoEsperarZona(this, quierePerro ? "ZonaPerros" : "ZonaGatos"),
            new NodoAdopcion(this)
        });

        NodoBT irCheckout = new NodoAccion(() => IrA(checkout));
        NodoBT irSalida = new NodoAccion(() => IrA(salida, SalirDelRefugio));

        var pasos = new List<NodoBT> {
            checkInSecuencia,
            irEspera, esperarSala,
            registroCola,
            irEntrevista, esperarEntrevista,
            entrevista,
            new NodoCondicional(() => aprobado, adopcion),
            irCheckout,
            irSalida
        };

        arbol = new NodoSecuencia(pasos);
    }

    NodoResultado IrA(Transform destino, Action onLlegada = null)
    {
        if (destino == null || agente == null || !agente.isOnNavMesh) return NodoResultado.Fallo;

        if (destinoActual != destino)
        {
            agente.isStopped = false;
            agente.SetDestination(destino.position);
            destinoActual = destino;
            return NodoResultado.Ejecutando;
        }

        if (agente.pathPending || agente.remainingDistance > agente.stoppingDistance)
            return NodoResultado.Ejecutando;

        agente.isStopped = true;
        destinoActual = null;
        onLlegada?.Invoke();
        return NodoResultado.Exito;
    }

    public void RealizarResultadoEntrevista()
    {
        aprobado = UnityEngine.Random.value > 0.5f;
        quierePerro = UnityEngine.Random.value > 0.5f;
        entrevistado = true;
        Debug.Log($"{name} entrevistado. Aprobado: {aprobado}, QuierePerro: {quierePerro}");
    }
    public void LiberarSalaEntrevista()
    {
        gameManager?.LiberarSalaEntrevista();
    }

    public void AsignarAnimal()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager no asignado en ClienteBT");
            return;
        }

        animalAsignado = gameManager.AsignarAnimal(quierePerro);
        if (animalAsignado != null)
        {
            animalAsignado.transform.SetParent(transform);
            animalAsignado.transform.localPosition = new Vector3(0.5f, 0, 0);
            Debug.Log(name + " ha adoptado un " + (quierePerro ? "perro" : "gato"));
        }
        else
        {
            Debug.LogWarning("No hay animales disponibles para asignar a " + name);
        }
    }

    public void ConfirmarCheckIn()
    {
        checkInCompletado = true;
        Debug.Log($"🟢 {name} recibió confirmación de check-in.");
    }

    public bool CheckInConfirmado()
    {
        Debug.Log($"🔍 {name} verifica check-in confirmado: {checkInCompletado}");
        return checkInCompletado;
    }

    void SalirDelRefugio()
    {
        gameManager?.LiberarRecepcion();
        gameManager?.ClienteSalido();
        Destroy(gameObject);
    }

    public bool EstaEnSalaEspera() => enSalaEspera;
    public bool EstaEntrevistado() => entrevistado;
}