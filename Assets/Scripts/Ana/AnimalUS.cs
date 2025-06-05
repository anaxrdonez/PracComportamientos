using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animal))]
public class AnimalUS : MonoBehaviour
{
    enum EstadoAnimal { Idle, Caminando, Comiendo, Jugando, Durmiendo }

    private EstadoAnimal estado = EstadoAnimal.Idle;
    private NavMeshAgent agente;

    // Necesidades
    float hambre = 0f;
    float energia = 1f;
    float aburrimiento = 0.5f;

    // Referencias
    private Transform comida, descanso, juego;
    private GameManager gm;
    private Animal.TipoAnimal tipo;
    private Transform destinoActual = null;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        tipo = GetComponent<Animal>().tipo;
        gm = FindObjectOfType<GameManager>();

        // Asignar puntos según tipo
        if (tipo == Animal.TipoAnimal.Perro)
        {
            comida = gm.puntoComidaPerros;
            descanso = gm.puntoDescansoPerros;
            juego = gm.puntoJuegoPerros;
        }
        else
        {
            comida = gm.puntoComidaGatos;
            descanso = gm.puntoDescansoGatos;
            juego = gm.puntoJuegoGatos;
        }

        StartCoroutine(ActualizarEstado());
    }

    void Update()
    {
        hambre += Time.deltaTime * 0.01f;
        energia -= Time.deltaTime * 0.005f;
        aburrimiento += Time.deltaTime * 0.008f;

        hambre = Mathf.Clamp01(hambre);
        energia = Mathf.Clamp01(energia);
        aburrimiento = Mathf.Clamp01(aburrimiento);
    }

    IEnumerator ActualizarEstado()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            if (estado == EstadoAnimal.Caminando) continue;

            // Evaluar utilidades
            Dictionary<string, float> utilidades = new Dictionary<string, float>
            {
                { "Comer", hambre },
                { "Dormir", 1f - energia },
                { "Jugar", aburrimiento }
            };

            string accion = utilidades.OrderByDescending(kv => kv.Value).First().Key;
            EjecutarAccion(accion);
        }
    }

    void EjecutarAccion(string accion)
    {
        switch (accion)
        {
            case "Comer":
                if (comida != null)
                    StartCoroutine(IrYEsperar(comida, EstadoAnimal.Comiendo));
                break;
            case "Dormir":
                if (descanso != null)
                    StartCoroutine(IrYEsperar(descanso, EstadoAnimal.Durmiendo));
                break;
            case "Jugar":
                if (juego != null)
                    StartCoroutine(IrYEsperar(juego, EstadoAnimal.Jugando));
                break;
        }
    }

    IEnumerator IrYEsperar(Transform destino, EstadoAnimal nuevoEstado)
    {
        estado = EstadoAnimal.Caminando;
        destinoActual = destino;

        agente.SetDestination(destino.position);
        agente.isStopped = false;

        while (agente.pathPending || agente.remainingDistance > agente.stoppingDistance)
        {
            yield return null;
        }

        agente.isStopped = true;
        estado = nuevoEstado;

        float duracion = Random.Range(3f, 6f);
        yield return new WaitForSeconds(duracion);

        // Reinicio de valores según la acción realizada
        switch (nuevoEstado)
        {
            case EstadoAnimal.Comiendo:
                hambre = 0f;
                break;
            case EstadoAnimal.Durmiendo:
                energia = 1f;
                break;
            case EstadoAnimal.Jugando:
                aburrimiento = 0f;
                break;
        }

        estado = EstadoAnimal.Idle;
    }
}