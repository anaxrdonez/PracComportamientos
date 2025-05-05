using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;

[CreateAssetMenu(menuName = "UtilityAI/Actions/BathroomAction")]
public class BathroomAction : AIAction
{
    [Tooltip("Prefab de caca que se instanciará al hacer pipí")]
    public GameObject poopPrefab;

    [Tooltip("Tiempo que la mascota se queda quieta tras usar el baño (segundos)")]
    public float idleDuration = 2f;

    [Tooltip("Velocidad a la que corre la mascota hacia el baño (m/s)")]
    public float runSpeed = 5f;

    private string dynamicTag;
    private bool isUsingBathroom;
    private bool speedOverridden;
    private float originalSpeed;

    public override void Initialize(Context context)
    {
        dynamicTag = context.Species == Species.Cat ? "BathroomCat" : "BathroomDog";
        context.sensor.targetTags.Add(dynamicTag);
    }

    public override void Execute(Context context)
    {
        var agent = context.agent;
        if (isUsingBathroom) return;

        // Al iniciar el acercamiento, guardamos y sobrescribimos la velocidad
        if (!speedOverridden)
        {
            originalSpeed = agent.speed;
            agent.speed = runSpeed;
            speedOverridden = true;
        }

        var target = context.sensor.GetClosestTarget(dynamicTag);
        if (target == null) return;

        // Corre hacia el baño
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Al llegar
        if (Vector3.Distance(agent.transform.position, target.position) < 0.5f)
        {
            isUsingBathroom = true;
            agent.isStopped = true;

            context.brain.StartCoroutine(CompleteBathroom(agent, target.position, context));
        }
    }

    private IEnumerator CompleteBathroom(NavMeshAgent agent, Vector3 position, Context context)
    {
        // Espera mientras "hace pipí"
        yield return new WaitForSeconds(idleDuration);

        // Instanciar la caca
        if (poopPrefab != null)
            GameObject.Instantiate(poopPrefab, position, Quaternion.identity);

        // Resetear la necesidad en el contexto
        context.SetData("bathroom", 0f);

        // Restaurar velocidad original y reanudar movimiento
        agent.speed = originalSpeed;
        speedOverridden = false;
        agent.isStopped = false;
        isUsingBathroom = false;
    }
}
