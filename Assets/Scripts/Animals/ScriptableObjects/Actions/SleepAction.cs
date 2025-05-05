using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;

[CreateAssetMenu(menuName = "UtilityAI/Actions/SleepAction")]
public class SleepAction : AIAction
{
    [Tooltip("Velocidad de caminar hacia la cama (m/s)")]
    public float walkSpeed = 1f;

    [Tooltip("Tiempo que la mascota permanece en la cama (segundos)")]
    public float idleDuration = 5f;

    // Flags internas
    private bool isSleeping;
    private bool speedOverridden;
    private float originalSpeed;

    // Por defecto, targetTag = "Bed"
    void Reset() => targetTag = "Bed";

    public override void Initialize(Context context)
    {
        if (!string.IsNullOrEmpty(targetTag))
            context.sensor.targetTags.Add(targetTag);
    }

    public override void Execute(Context context)
    {
        var agent = context.agent;
        if (isSleeping) return;

        var target = context.sensor.GetClosestTarget(targetTag);
        if (target == null) return;

        // Al iniciar, forzamos caminar
        if (!speedOverridden)
        {
            originalSpeed = agent.speed;
            agent.speed = walkSpeed;
            speedOverridden = true;
        }

        // Caminar hacia la cama
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Si ha llegado (distancia < 0.5m)
        if (Vector3.Distance(agent.transform.position, target.position) < 0.5f)
        {
            isSleeping = true;
            agent.isStopped = true;  // Speed = 0 → Idle en tu Blend Tree

            // Arrancar la espera de "dormir"
            context.brain.StartCoroutine(ResumeSleeping(context, agent));
        }
    }

    private IEnumerator ResumeSleeping(Context context, NavMeshAgent agent)
    {
        // Permanecer en Idle el tiempo definido
        yield return new WaitForSeconds(idleDuration);

        // Aplicar descanso
        var needs = agent.GetComponent<PetNeeds>();
        if (needs != null)
            needs.Rest(1f);

        // Resetear nivel de sueño en el Context
        context.SetData("sleep", 0f);

        // Restaurar velocidad y reanudar movimiento
        agent.speed = originalSpeed;
        speedOverridden = false;
        isSleeping = false;
        agent.isStopped = false;
    }
}
