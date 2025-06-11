using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animal))]
public class AnimalUS : MonoBehaviour
{
    public enum EstadoAnimal { Idle, Caminando, Comiendo, Jugando, Durmiendo }

    [SerializeField] private EstadoAnimal estado = EstadoAnimal.Idle;
    private NavMeshAgent agente;

    // Rango de necesidades iniciales
    [Header("Valores iniciales")]
    [Tooltip("Valor inicial de hambre (0 = nada, 1 = mucha hambre)")]
    public float hambreMin = 0.2f, hambreMax = 0.8f;
    [Tooltip("Valor inicial de energía (0 = sin energía, 1 = energía completa)")]
    public float energiaMin = 0.2f, energiaMax = 0.8f;
    [Tooltip("Valor inicial de aburrimiento (0 = nada aburrido, 1 = muy aburrido)")]
    public float aburrimientoMin = 0.2f, aburrimientoMax = 0.8f;

    // Necesidades dinámicas
    private float hambre;
    private float energia;
    private float aburrimiento;

    // Curvas de utilidad para suavizar la decisión
    [Header("Curvas de utilidad")]
    public AnimationCurve curvaHambre = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve curvaEnergia = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve curvaAburrimiento = AnimationCurve.Linear(0, 0, 1, 1);

    // Pesos para cada necesidad
    [Header("Pesos de utilidad")]
    public float pesoHambre = 1f;
    public float pesoEnergia = 1f;
    public float pesoAburrimiento = 1f;

    // Referencias a puntos de interés
    private Transform comida, descanso, juego;
    private GameManager gm;
    private Animal.TipoAnimal tipo;
    private Transform destinoActual = null;

    // UI de estado
    private TextMeshProUGUI textoEstado;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        hambre = Random.Range(hambreMin, hambreMax);
        energia = Random.Range(energiaMin, energiaMax);
        aburrimiento = Random.Range(aburrimientoMin, aburrimientoMax);
    }

    void Start()
    {
        tipo = GetComponent<Animal>().tipo;
        gm = FindObjectOfType<GameManager>();
        AsignarPuntos();
        textoEstado = GetComponentInChildren<TextMeshProUGUI>();
        ActualizarTextoEstado();
        StartCoroutine(ActualizarEstado());
    }

    void Update()
    {
        hambre = Mathf.Clamp01(hambre + Time.deltaTime * 0.01f);
        energia = Mathf.Clamp01(energia - Time.deltaTime * 0.005f);
        aburrimiento = Mathf.Clamp01(aburrimiento + Time.deltaTime * 0.008f);
    }

    void AsignarPuntos()
    {
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
    }

    IEnumerator ActualizarEstado()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if (estado == EstadoAnimal.Caminando) continue;

            // Evaluar utilidades usando curvas y pesos
            float uHambre = curvaHambre.Evaluate(hambre) * pesoHambre;
            float uEnergia = curvaEnergia.Evaluate(1f - energia) * pesoEnergia;
            float uAburrimiento = curvaAburrimiento.Evaluate(aburrimiento) * pesoAburrimiento;

            // Selección de la acción con mayor utilidad
            EstadoAnimal seleccion = EstadoAnimal.Comiendo;
            float maxUtil = uHambre;
            if (uEnergia > maxUtil)
            {
                maxUtil = uEnergia;
                seleccion = EstadoAnimal.Durmiendo;
            }
            if (uAburrimiento > maxUtil)
            {
                seleccion = EstadoAnimal.Jugando;
            }

            IniciarAccion(seleccion);
        }
    }

    void IniciarAccion(EstadoAnimal accion)
    {
        Transform dest = null;
        switch (accion)
        {
            case EstadoAnimal.Comiendo: dest = comida; break;
            case EstadoAnimal.Durmiendo: dest = descanso; break;
            case EstadoAnimal.Jugando: dest = juego; break;
        }
        if (dest == null)
        {
            Debug.LogError($"{name}: Punto de {accion} no asignado.");
            return;
        }

        estado = EstadoAnimal.Caminando;
        destinoActual = dest;
        ActualizarTextoEstado();
        agente.isStopped = false;
        agente.SetDestination(dest.position);
        StartCoroutine(IrYEsperar(dest, accion));
    }

    IEnumerator IrYEsperar(Transform destino, EstadoAnimal accionFinal)
    {
        float timeout = 5f, timer = 0f;
        while ((agente.pathPending || agente.remainingDistance > agente.stoppingDistance) && timer < timeout)
        {
            if (agente.pathStatus != NavMeshPathStatus.PathComplete) break;
            timer += Time.deltaTime;
            yield return null;
        }
        agente.isStopped = true;
        estado = accionFinal;
        ActualizarTextoEstado();
        float duracion = accionFinal == EstadoAnimal.Durmiendo ? 30f : 20f;
        yield return new WaitForSeconds(duracion);
        switch (accionFinal)
        {
            case EstadoAnimal.Comiendo: hambre = 0f; break;
            case EstadoAnimal.Durmiendo: energia = 1f; break;
            case EstadoAnimal.Jugando: aburrimiento = 0f; break;
        }
        estado = EstadoAnimal.Idle;
        ActualizarTextoEstado();
    }

    void ActualizarTextoEstado()
    {
        if (textoEstado != null)
            textoEstado.text = estado.ToString();
    }

    
}
