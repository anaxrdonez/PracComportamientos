using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NodoResultado //Valores que podemos obtener de un nodo
{
    Exito,
    Fallo,
    Ejecutando //sigue en proceso
}

public abstract class NodoBT //base par todos los nodos
{
    public abstract NodoResultado Tick();
}


//---------- NODO SECUENCIA (composite)---------- //
/* 
 * Ejecuta una lista de nodos hijos uno por uno en orden.
 * Si alguno falla, se detiene inmediatamente. 
 * Si todos tienen éxito, la secuencia completa tiene éxito. 
*/
public class NodoSecuencia : NodoBT
{
    private readonly List<NodoBT> hijos; // lista de nodos hijos
    private int indiceActual = 0; // guardará el nodo que se estará ejecutando

    public NodoSecuencia(List<NodoBT> hijos) //constructor (recibe la lista de hijos)
    {
        this.hijos = hijos;
    }

    public override NodoResultado Tick() //métod principal
    {
        while (indiceActual < hijos.Count) //mientras siga habiendo hijos sin procesar
        {
            var resultado = hijos[indiceActual].Tick(); //ejecuta el hijo actual
            // Si el nodo hijo aun no ha terminado se sigue considerando en ejecución.
            // No se avanzará al siguiente hijo
            if (resultado == NodoResultado.Ejecutando) return NodoResultado.Ejecutando; 
            if (resultado == NodoResultado.Fallo) //Si algún hijo fallo falla toda la secuencia.
            {
                indiceActual = 0;
                return NodoResultado.Fallo;
            }
            indiceActual++; //Si hubo éxito se avanza al siguiente nodo hijo.
        }
        indiceActual = 0; // Si todos los hijos han terminado con éxito se resetea  el indexy se devuelve exito
        return NodoResultado.Exito;
    }
}


//---------- NODO ACCIÓN ---------- //
/* 
 * Es un nodo Hoja, no tendrá hijos.
 * Va a ejecutar una acción concreta que le paseremos.
 * (Por ejemplo mover al cliente a un punto)
 */
public class NodoAccion : NodoBT
{
    private readonly Func<NodoResultado> accion; // devolverá ejecutando, éxito o fallo

    public NodoAccion(Func<NodoResultado> accion) //Constructor
    {
        this.accion = accion;
    }

    public override NodoResultado Tick() => accion(); //Ejecuta la acción y devuelve el resultado.
    //Lo usaremos por ejemplo con la acción IrA(sala_X) 
}


//---------- NODO HOJA CON CONDICIÓN ---------- //
/*
 * Evaluará una condición y esperará a que se cumpla.
 */
public class NodoEsperarZona : NodoBT
{
    private readonly ClienteBT cliente; //ref al cliente que se estará moviendo
    private readonly Func<string> obtenerZona; // zona objetivo

    public NodoEsperarZona(ClienteBT cliente, Func<string> zonaDinamica) //Constructor
    {
        this.cliente = cliente;
        this.obtenerZona = zonaDinamica;
    }

    public override NodoResultado Tick()
    {
        //Devuelve la zona actual donde se encuentra el cliente
        //y lo compoara con el valor de obtenerZona()
        return cliente.DetectarZonaActual() == obtenerZona() 
            ? NodoResultado.Exito //Si coinciden --> ha llegado por lo que ÉXITO
            : NodoResultado.Ejecutando; // Si no sigue en camino --> EJECUTANDO
    }
}


//---------- NODO HOJA CON CONDICIÓN  ---------- //
/*
 *  No hace nada activamente, solo espera a que el recepcionista 
 *  cambie una variable en el cliente (checkInCompletado = true).
 */
public class NodoEsperarCheckInConfirmado : NodoBT
{
    private ClienteBT cliente;

    public NodoEsperarCheckInConfirmado(ClienteBT cliente)
    {
        this.cliente = cliente;
    }

    public override NodoResultado Tick() 
    {  
        //Llama a CheckInConfirmado y devuelve el resultado en consecuencia
        return cliente.CheckInConfirmado() ? NodoResultado.Exito : NodoResultado.Ejecutando;
    }
}


//---------- NODO HOJA CON CONDICIÓN  ---------- //
/*
 * Su propósito es esperar en una cola, pero no realiza una 
 * acción activa como moverse o cambiar algo visual.
 */
public class NodoColaRecepcion : NodoBT
{
    private readonly ClienteBT cliente;
    private readonly GameManager gameManager; //para acceder a colas y estado recepción
    private bool registrado = false;

    public NodoColaRecepcion(ClienteBT cliente, GameManager gameManager)
    {
        this.cliente = cliente;
        this.gameManager = gameManager;
    }

    public override NodoResultado Tick()
    {
        if (!registrado) //Si el cliente no ha sido encolado se encola
        {
            gameManager.ClienteARecepcion(cliente);
            registrado = true; // y se marca como registrado para que no vuelva a encolarse
        }
        //Cada vez que se evalua al nodo preguntará al gamemanager si se puede hacer el checkin al cliente,
        // para ello debe ser el primero en la cola y la recepción estar libre
        return gameManager.ClientePuedeSerRegistrado(cliente)
            ? NodoResultado.Exito
            : NodoResultado.Ejecutando;
    }
}


//---------- NODO ACCIÓN ---------- //
/*
 * Representa el momento en que el cliente está dentro de la sala de entrevistas.
 * Simula que la entrevista dura unos segundos.
 * Al finalizar, determina si el cliente ha sido aprobado y qué tipo de animal desea adoptar.
 * Además libera la sala para el siguiente cliente.s
 */
public class NodoEntrevista : NodoBT
{
        private readonly ClienteBT cliente;

        public NodoEntrevista(ClienteBT cliente)
        {
            this.cliente = cliente;
        }

        public override NodoResultado Tick()
        {
            // Espera pasiva: el EntrevistadorFSM gestiona la entrevista.
            // Este nodo sólo comprueba si ya ha finalizado.
            return cliente.EstaEntrevistado()
                ? NodoResultado.Exito
                : NodoResultado.Ejecutando;
        }

}
public class NodoAvisarEntrevistador : NodoBT
{
    private readonly ClienteBT cliente;
    private bool notificado = false;

    public NodoAvisarEntrevistador(ClienteBT cliente)
    {
        this.cliente = cliente;
    }

    public override NodoResultado Tick()
    {
        if (notificado) return NodoResultado.Exito;

        EntrevistadorFSM entrevistador = GameObject.FindObjectOfType<EntrevistadorFSM>();
        if (entrevistador != null)
        {
            entrevistador.ClienteLlega(cliente);
            Debug.Log(" Entrevistador notificado por el cliente: " + cliente.name);
            notificado = true;
            return NodoResultado.Exito;
        }

        return NodoResultado.Fallo;
    }
}



//---------- NODO ACCIÓN ---------- //
/*
 * Simula un tiempo de espera mientras el cliente explora la zona de adopción.
 * Una vez pasa ese tiempo (5 segundos), el cliente adopta un animal disponible (perro o gato).
 */
public class NodoAdopcion : NodoBT //Igual que el nodo entrevista
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


//---------- NODO DECOR CONDCIONAL ---------- //
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
            Debug.Log("📥 Cliente registrado en cola de entrevista: " + cliente.name);
        }

        bool puede = gameManager.ClientePuedeEntrevistarse(cliente);
        Debug.Log($"🔄 Cliente {cliente.name} intentando obtener permiso para entrevista. Puede: {puede}");

        if (puede)
        {
            cliente.OtorgarPermisoEntrevista();
            Debug.Log("✅ Cliente recibió permiso para entrevista: " + cliente.name);
            return NodoResultado.Exito;
        }

        return NodoResultado.Ejecutando;
    }


}


//---------- NODO CONDCIONAL ---------- //
public class NodoCondicional : NodoBT
{
    private readonly Func<bool> condicion; // condición a evaluar
    private readonly NodoBT hijo; // el nodo que se ejecutará solo si la condición es verdadera

    public NodoCondicional(Func<bool> condicion, NodoBT hijo)
    {
        this.condicion = condicion;
        this.hijo = hijo;
    }

    public override NodoResultado Tick()
    {
        /*
         * Si la condición NO se cumple, el nodo se considera completado con éxito 
         * sin hacer nada. se omite el nodo hijo, pero no se detiene el árbol.
         */
        if (!condicion()) return NodoResultado.Exito;
        return hijo.Tick(); //Si la condición sí se cumple, se evalúa el nodo hijo.
    }
}


public class ClienteBT : MonoBehaviour
{
    private NavMeshAgent agente;
    private DetectarZona detectarZona;
    private GameManager gameManager;

    [Header("Puntos")] public Transform puntoCheckIn, salaEspera, salaEntrevista, zonaGatos, zonaPerros, checkout, salida;

    private bool entrevistado = false, aprobado = false, enSalaEspera = false;
    private bool quierePerro = false;
    private GameObject animalAsignado;
    private bool checkInCompletado = false;

    private NodoBT arbol;
    private NodoResultado estadoActual = NodoResultado.Ejecutando;
    private Transform destinoActual = null;

    public bool Aprobado() => aprobado; //apto para adoptar

    public string DetectarZonaActual() => detectarZona != null ? detectarZona.zonaActual : "FueraDeZona";
    public Camera ClienteCam => GetComponentInChildren<Camera>();
    public event System.Action OnClienteSalido; //para cuando el cliente ha salido del refugio
    private bool permisoEntrevista = false;

    // Getter y setter
    public void OtorgarPermisoEntrevista()
    {
        permisoEntrevista = true;
    }

    public bool TienePermisoEntrevista()
    {
        return permisoEntrevista;
    }

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
        //-------- 1. PROCESO DE CHECK-IN --------//
        NodoBT checkInSecuencia = new NodoSecuencia(new List<NodoBT> {
            new NodoColaRecepcion(this, gameManager), //encolar
            new NodoAccion(() => IrA(puntoCheckIn)), //ir fisicamente al checkin
            new NodoEsperarZona(this, () => "CheckIn"), //esperar a estar en el checkin
            new NodoEsperarCheckInConfirmado(this) //esperar a que el recepcionista confirme
        });

        //-------- 2. IR A SALA DE ESPERA --------//
        NodoBT irEspera = new NodoAccion(() => IrA(salaEspera));
        NodoBT esperarSala = new NodoEsperarZona(this, () => "SalaEspera");

        //-------- 3. COLA PARA ENTREVISTA --------//
        NodoBT registroCola = new NodoColaEntrevista(this, gameManager);

        //-------- 4. IR A SALA ENTREVISTA Y NOTIFICAR --------//
        NodoBT irEntrevista = new NodoCondicional(
               () => TienePermisoEntrevista(),
               new NodoAccion(() => IrA(salaEntrevista))
           );
        NodoBT esperarEntrevista = new NodoEsperarZona(this, () => "SalaEntrevista");
        NodoBT avisarEntrevistador = new NodoAvisarEntrevistador(this);
        NodoBT entrevista = new NodoEntrevista(this);

        //-------- 5. ADOPCIÓN (SOLO SI HA APROBADO) --------//
        NodoBT adopcion = new NodoSecuencia(new List<NodoBT> {
            new NodoAccion(() => IrA(quierePerro ? zonaPerros : zonaGatos)),
            new NodoEsperarZona(this, () => quierePerro ? "ZonaPerros" : "ZonaGatos"),
            new NodoAdopcion(this)
        });

        //-------- 6. CHECK-OUT Y SALIDA --------//
        NodoBT irCheckout = new NodoAccion(() => IrA(checkout));
        NodoBT irSalida = new NodoAccion(() => IrA(salida, SalirDelRefugio));

        // Se juntan todos los nodos anteriores en un único NodoSecuencia
        var pasos = new List<NodoBT> {
            checkInSecuencia,
            irEspera, esperarSala,
            registroCola,
            irEntrevista, esperarEntrevista,avisarEntrevistador,
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
        OnClienteSalido?.Invoke();
        Destroy(gameObject);
    }

    public bool EstaEnSalaEspera() => enSalaEspera;
    public bool EstaEntrevistado() => entrevistado;

    public void LiberarSalaEntrevista()
    {
        gameManager?.LiberarSalaEntrevista();
    }
}