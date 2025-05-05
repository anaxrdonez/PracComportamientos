using UnityEngine;
using UnityEngine.AI;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/MoveToTargetAction")]
    public class MoveToTargetAIAction : AIAction
    {
        [Tooltip("Tag del objetivo que el agente debe perseguir")]
        public string targetTag;

        public override void Initialize(Context context)
        {
            // Registrar el tag para que el Sensor lo detecte
            context.sensor.targetTags.Add(targetTag);
        }

        public override void Execute(Context context)
        {
            // Obtener el objetivo más cercano con el tag especificado
            Transform target = context.sensor.GetClosestTarget(targetTag);
            if (target == null)
                return;

            // Enviar al agente hacia la posición del objetivo
            context.agent.SetDestination(target.position);
        }
    }
}
