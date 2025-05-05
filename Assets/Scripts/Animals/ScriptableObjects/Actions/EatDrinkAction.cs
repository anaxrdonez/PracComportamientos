using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;

[CreateAssetMenu(menuName = "UtilityAI/Actions/EatDrinkAction")]
public class EatDrinkAction : AIAction
{
    [Tooltip("Tiempo que la mascota se queda quieta en el comedero antes de comer y desaparecer la comida (segundos)")]
    public float idleDuration = 2f;

    private bool isEating = false;
    private GameObject currentFood;

    public override void Initialize(Context context)
    {
        if (!string.IsNullOrEmpty(targetTag))
        {
            context.sensor.targetTags.Add(targetTag);
            Debug.Log($"[EatDrinkAction] Initialize: registrado targetTag='{targetTag}'.");
        }
    }

    public override void Execute(Context context)
    {
        var agent = context.agent;
        if (isEating)
        {
            Debug.Log("[EatDrinkAction] Execute: ya está comiendo, salto ejecución.");
            return;
        }

        var target = context.sensor.GetClosestTarget(targetTag);
        if (target == null)
        {
            Debug.Log($"[EatDrinkAction] Execute: no encontré ningún objeto con tag '{targetTag}'.");
            return;
        }

        // Moverse hacia el comedero
        agent.isStopped = false;
        context.target = target;
        context.agent.SetDestination(target.position);
        Debug.Log($"[EatDrinkAction] Execute: moviéndose hacia '{targetTag}' en {target.position}.");

        // Comprobar llegada
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isEating = true;
            agent.isStopped = true;
            currentFood = target.gameObject;
            Debug.Log("[EatDrinkAction] Execute: ¡llegué al comedero! Inicio idle para comer.");

            context.brain.StartCoroutine(ResumeEating(context, agent));
        }
    }

    private IEnumerator ResumeEating(Context context, NavMeshAgent agent)
    {
        Debug.Log($"[EatDrinkAction] ResumeEating: esperando {idleDuration} s antes de comer.");
        yield return new WaitForSeconds(idleDuration);

        /*// Destruir la comida
        if (currentFood != null)
        {
            Debug.Log("[EatDrinkAction] ResumeEating: destruyendo comida.");
            Destroy(currentFood);
        }*/

        // Resetear hambre
        Debug.Log("[EatDrinkAction] ResumeEating: reseteando hambre en Context.");
        context.SetData("hunger", 0f);

        // Reactivar movimiento
        agent.isStopped = false;
        isEating = false;
        currentFood = null;

        Debug.Log("[EatDrinkAction] ResumeEating: comida completada, reanudo movimiento.");
    }
}
