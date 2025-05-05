using UnityEngine;
using UtilityAI;

[CreateAssetMenu(menuName = "UtilityAI/Actions/IdleAction")]
public class IdleAIAction : AIAction
{
    public override void Execute(Context context)
    {
        var agent = context.agent;

        // Detenemos por completo al agente
        agent.isStopped = true;
        agent.ResetPath();  // opcional, limpia cualquier ruta pendiente

  
        // AnimationController detectará que velocity = 0 y pondrá Speed = 0 → Idle
    }
}
