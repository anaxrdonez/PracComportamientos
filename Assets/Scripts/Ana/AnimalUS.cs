using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animal))]
[RequireComponent(typeof(ChangeSprite))]  // Asegura que exista el componente para cambiar sprites
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

    // Curvas de utilidad
    [Header("Curvas de utilidad (preseleccionadas)")]
    public AnimationCurve curvaHambre;
    public AnimationCurve curvaEnergia;
    public AnimationCurve curvaAburrimiento;

    // Pesos para cada necesidad
    [Header("Pesos de utilidad (aleatorios)")]
    [Range(0.5f, 1f)] public float pesoHambre;
    [Range(0.5f, 1f)] public float pesoEnergia;
    [Range(0.5f, 1f)] public float pesoAburrimiento;

    // Sprites para cada estado
    [Header("Sprites de estado")]
    public Sprite walkSprite;
    public Sprite eatSprite;
    public Sprite playSprite;
    public Sprite sleepSprite;

    // Referencia al componente que cambia sprites
    private ChangeSprite changeSprite;

    // Referencias a puntos de interés
    private Transform comida, descanso, juego;
    private GameManager gm;
    private Animal.TipoAnimal tipo;

    [SerializeField] private Animator animator;
    // Valores que queremos usar:
    const float IDLE_V = 0f, MOVE_V = 1f;
    const float WALK_S = 0f, RUN_S = 1f;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        changeSprite = GetComponent<ChangeSprite>();

        hambre = Random.Range(hambreMin, hambreMax);
        energia = Random.Range(energiaMin, energiaMax);
        aburrimiento = Random.Range(aburrimientoMin, aburrimientoMax);
        RandomizeCurvasYPesos();
    }

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator no asignado en " + name);

        tipo = GetComponent<Animal>().tipo;
        gm = FindObjectOfType<GameManager>();
        AsignarPuntos();
        DecidirYMover();
        StartCoroutine(ActualizarEstado());
    }

    void Update()
    {
        hambre = Mathf.Clamp01(hambre + Time.deltaTime * 0.01f);
        energia = Mathf.Clamp01(energia - Time.deltaTime * 0.005f);
        aburrimiento = Mathf.Clamp01(aburrimiento + Time.deltaTime * 0.008f);
    }

    void DecidirYMover()
    {
        float uH = curvaHambre.Evaluate(hambre) * pesoHambre;
        float uE = curvaEnergia.Evaluate(1f - energia) * pesoEnergia;
        float uA = curvaAburrimiento.Evaluate(aburrimiento) * pesoAburrimiento;

        EstadoAnimal seleccion = EstadoAnimal.Comiendo;
        float maxU = uH;
        if (uE > maxU) { maxU = uE; seleccion = EstadoAnimal.Durmiendo; }
        if (uA > maxU) { seleccion = EstadoAnimal.Jugando; }

        IniciarAccion(seleccion);
    }

    void RandomizeCurvasYPesos()
    {
        curvaHambre = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(Random.Range(0.3f, 0.6f), Random.Range(0.4f, 0.9f)),
            new Keyframe(1f, 1f)
        );
        curvaEnergia = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(Random.Range(0.4f, 0.7f), Random.Range(0.2f, 0.5f)),
            new Keyframe(1f, 0f)
        );
        curvaAburrimiento = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(Random.Range(0.2f, 0.5f), Random.Range(0.5f, 1f)),
            new Keyframe(Random.Range(0.7f, 0.9f), Random.Range(0.2f, 0.6f)),
            new Keyframe(1f, 1f)
        );
        pesoHambre = Random.Range(0.5f, 1f);
        pesoEnergia = Random.Range(0.5f, 1f);
        pesoAburrimiento = Random.Range(0.5f, 1f);
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
            DecidirYMover();
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

        // Cambiar a estado Caminando y sprite de caminar
        estado = EstadoAnimal.Caminando;
        animator.SetFloat("Vert", MOVE_V);
        // elegir caminata
        animator.SetFloat("State", WALK_S); animator.SetFloat("Vert", IDLE_V);


        agente.isStopped = false;
        agente.SetDestination(dest.position);
        StartCoroutine(IrYEsperar(dest, accion));
    }

    IEnumerator IrYEsperar(Transform destino, EstadoAnimal accionFinal)
    {
        float timeout = 2f, timer = 0f;
        while ((agente.pathPending || agente.remainingDistance > agente.stoppingDistance) && timer < timeout)
        {
            if (agente.pathStatus != NavMeshPathStatus.PathComplete) break;
            timer += Time.deltaTime;
            yield return null;
        }
        agente.isStopped = true;
        estado = accionFinal;

        // Cambiar sprite según la acción final
        switch (accionFinal)
        {
            case EstadoAnimal.Comiendo:
                changeSprite.ActualizarSprite(eatSprite);
                animator.SetFloat("Vert", IDLE_V);

                hambre = 0f;
                break;
            case EstadoAnimal.Durmiendo:
                changeSprite.ActualizarSprite(sleepSprite);
                animator.SetFloat("Vert", IDLE_V);

                energia = 1f;
                break;
            case EstadoAnimal.Jugando:
                changeSprite.ActualizarSprite(playSprite);
                animator.SetFloat("Vert", MOVE_V);
                animator.SetFloat("State", RUN_S);

                aburrimiento = 0f;
                break;
        }

        float duracion = (accionFinal == EstadoAnimal.Durmiendo) ? 30f : 20f;
        yield return new WaitForSeconds(duracion);

        // Volver a Idle con sprite de caminar
        estado = EstadoAnimal.Idle;
        changeSprite.ActualizarSprite(walkSprite);
        animator.SetFloat("Vert", MOVE_V);
        // elegir caminata
        animator.SetFloat("State", WALK_S);
    }
}
