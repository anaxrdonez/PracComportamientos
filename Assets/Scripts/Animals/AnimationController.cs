using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UtilityAI
{
    public class AnimationController : MonoBehaviour
    {
        Animator animator;
        NavMeshAgent agent;

        // hashes de parámetros
        readonly int speedHash = Animator.StringToHash("State2");
        readonly int stateHash = Animator.StringToHash("State");

        // smoothing
        float currentSpeed;
        float speedVelocity;
        public float smoothTime = 0.3f;

        // a partir de qué velocidad consideras correr
        [Tooltip("Velocidad mínima para cambiar a correr (State=1)")]
        public float runThreshold = 1.5f;

        Dictionary<string, int> triggerHashes = new Dictionary<string, int>();

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();
        }

        public void Trigger(string triggerName)
        {
            if (!triggerHashes.TryGetValue(triggerName, out var h))
            {
                h = Animator.StringToHash(triggerName);
                triggerHashes[triggerName] = h;
            }
            animator.SetTrigger(h);
        }

        void Update()
        {
            // 1) calcula velocidad suavizada
            float targetSpeed = agent.velocity.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);

            // 2) actualiza Speed → controla Idle vs BlendTree locomoción
            animator.SetFloat(speedHash, 1);

            // 3) actualiza State → dentro de tu BlendTree anidado: walk (0) vs run (1)
            int stateValue = currentSpeed > runThreshold ? 1 : 0;
            animator.SetFloat(stateHash, stateValue);
        }
    }
}
